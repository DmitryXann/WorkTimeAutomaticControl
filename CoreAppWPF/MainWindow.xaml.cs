//Work Time Automatic Control StopWatch use Google Spreadsheets to save your work information in the cloud.
//Copyright (C) 2013  Tomayly Dmitry
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.  If not, see http://www.gnu.org/licenses/.
//
//Google APIs Client Library for .NET license: http://www.apache.org/licenses/LICENSE-2.0 .
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using Google.GData.Client;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow(DataEntities.WorkEntity workInfo)
		{
			_workInfo = workInfo;
			InitializeComponent();

			_sendLocalSavedWorkReportsbackgroundWorker = new BackgroundWorker {WorkerReportsProgress = true};
			_sendLocalSavedWorkReportsbackgroundWorker.ProgressChanged += SendLocalSavedWorkReportsbackgroundWorker_ProgressChanged;
			_sendLocalSavedWorkReportsbackgroundWorker.DoWork += SendLocalSavedWorkReportsbackgroundWorker_DoWork;
			_sendLocalSavedWorkReportsbackgroundWorker.RunWorkerCompleted += SendLocalSavedWorkReportsbackgroundWorker_RunWorkerCompleted;
		}

		private const string WORK_DESCRIPTION_WHEN_FORM_CLOSING_MISSING = @"User forgot to enter work description";

		private DataEntities.UserEntity _userInfo;
		private DataEntities.WorkEntity _workInfo;

		private bool _exceptionOccurs;
		private bool _workDescriptionTextBoxTextChanged;
		private bool _reportSended;

		private CrashHandler _crashHandler;
		private IEnumerable<DataEntities.WorkEntity> _crashReports;
		private readonly BackgroundWorker _sendLocalSavedWorkReportsbackgroundWorker;

		#region Methods

		/// <summary>
		/// Reports work info and handles exception
		/// </summary>
		/// <param name="throwCloudException">show clout exception user message boxes</param>
		/// <returns>true - data sended to the cloud successfully, false - otherwise</returns>
		private bool SendInfoToCloud(DataEntities.WorkEntity workInfo, bool throwCloudException = true)
		{
			try
			{
				new CloudWorker(_userInfo.CloudDBUserData, _userInfo.Multiplayer.ToString(),
					_userInfo.WorkReportSpreadsheetName, _userInfo.WorkReportHistorySpreadsheetName).SendWorkUpdate(workInfo);

				return true;
			}
			catch (CaptchaRequiredException captchaRequiredException) //try to enter correct data, exception stop thows after several false attempts
			{
				MessageBox.Show("Cloud access require captcha, re-enter needed data and enter captcha.", 
								DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);

				ActivateSettiingsForm(captchaRequiredException);
				SendInfoToCloud(_workInfo); 

				return true;
			}
			catch (InvalidCredentialsException) //Invalid user data, exception stop thows after several false attempts
			{
				MessageBox.Show("Cloud access failed, login or password incorrect, re-enter correct login and password and try again.", 
								DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);

				ActivateSettiingsForm();
				SendInfoToCloud(_workInfo);

				return true;
			}
			catch (Exception exception)
			{
				if (throwCloudException)
					MessageBox.Show(string.Format("Cloud access failed with exception: \n{0}.\nAll your data saved locally and will be saved in the cloud when cloud will be available.", exception.Message), 
									DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				UpdateCrashReportRegistryEntry(true);

				try
				{
					new CrashHandler().SaveCrashReport(workInfo);
				}
				catch (Exception crashReportException)
				{
					if (MessageBox.Show(string.Format("Save crash report information to file failed with exception: {0}, delete corrupted file?", crashReportException.Message),
										DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
					{
						try
						{
							CrashHandler.DeleteCorruptedCrashReportFile();
							UpdateCrashReportRegistryEntry(false);

						}
						catch (Exception)
						{
							MessageBox.Show(string.Format("Deleting crash report information file failed with exception: {0}, try again later.", crashReportException.Message), 
											DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
						}
					}
					else
					{
						MessageBox.Show(@"Corrupted crash report file will open in text editor automatically.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Warning);

						CrashHandler.OpenCrashReportFileInDefaultTextEditor();

						MessageBox.Show(@"Press ""YES"" when you finishing corrupted file editing.
Default file structure:
<?xml version=""1.0"" encoding=""utf-8""?>
<!--This is auto generated file, do not modify-->
<WorkTimeAutomaticControlCrashReport>
	<WorkTime WorkTime=""YYYY-MM-DDTHH:MM:SS"">Work description</WorkTime>
</WorkTimeAutomaticControlCrashReport>", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK,
						                MessageBoxImage.Warning);
						try
						{
							new CrashHandler().SaveCrashReport(workInfo);
							UpdateCrashReportRegistryEntry(true);
						}
						catch (Exception secondCrashReportException)
						{
							if (MessageBox.Show(string.Format(@"Save crash report information file failed with exception: {0}, delete corrupted file?", secondCrashReportException.Message),
												DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
							{
								try
								{
									CrashHandler.DeleteCorruptedCrashReportFile();
									UpdateCrashReportRegistryEntry(false);
								}
								catch (Exception)
								{
									MessageBox.Show(string.Format("Deleting crash report information file failed with exception: {0}, try again later.",
														crashReportException.Message), DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
								}
							}
							else
							{
								try
								{
									MessageBox.Show(@"Corrupted crash report file will be moved on your desktop",
										            DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Warning);

									CrashHandler.MoveCrashedReportToDescktop();
									UpdateCrashReportRegistryEntry(false);

								}
								catch (Exception)
								{
									MessageBox.Show(string.Format("Moving crash report information failed with exception: {0}, try again later.",
										            crashReportException.Message), DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
								}
							}
						}
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Update crash registry flag
		/// </summary>
		/// <param name="crashOccured"></param>
		private void UpdateCrashReportRegistryEntry(bool crashOccured)
		{
			try
			{
				RegistryWork.SaveCrashReport(crashOccured);
			}
			catch (Exception regException)
			{
				MessageBox.Show(string.Format("Save crash report failed with exception: \n{0}.", regException.Message)
					, DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				ActivateSettiingsForm();
				RegistryWork.SaveCrashReport(crashOccured);
			}
		}

		/// <summary>
		/// Load app settings
		/// </summary>
		private void LoadSettings()
		{
			try
			{
				_userInfo = RegistryWork.GetData();
				_userInfo.CloudDBUserData.Login = Cryptography.GetDecryptedMessage(_userInfo.CloudDBUserData.Login);
				_userInfo.CloudDBUserData.Password = Cryptography.GetDecryptedMessage(_userInfo.CloudDBUserData.Password);

				CurrentStatustoolStripStatusLabel.Content = string.Format("Welcome {0}, your time multiplier: {1}, earlier crash occurred: {2}", 
																			_userInfo.CloudDBUserData.Login, _userInfo.Multiplayer, _userInfo.Crash);
				_exceptionOccurs = false;

				if (_userInfo.Crash)
				{
					_crashHandler = new CrashHandler();
					_crashReports = _crashHandler.GetAllCrashes();
					CurrentStatustoolStripStatusLabel.Visibility = Visibility.Collapsed;

					SavedWorkReportsSendingStatustoolStripProgressBar.Maximum = _crashReports.Count();
					SavedWorkReportsSendingStatustoolStripProgressBar.Visibility = Visibility.Visible;
					SendingReportstoolStripStatusLabel.Visibility = Visibility.Visible;

					SettingsMenuItem.IsEnabled = false;
					WorkDescriptionTextBox.IsEnabled = false;
					SavedWorkReportsSendingStatustoolStripProgressBar.Maximum = _crashReports.Count();

					_sendLocalSavedWorkReportsbackgroundWorker.RunWorkerAsync();
				}
				else
				{
					CurrentStatustoolStripStatusLabel.Visibility = Visibility.Visible;
					SettingsMenuItem.IsEnabled = true;
					WorkDescriptionTextBox.IsEnabled = true;
				}
			}
			catch (Exception exception)
			{
				_exceptionOccurs = true;
				MessageBox.Show(string.Format("Loading settings failed with exception: \n{0} \nRecomended to reSet Settings.", exception.Message), DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, 
								MessageBoxButton.OK, MessageBoxImage.Error);
				ActivateSettiingsForm();
			}
		}

		/// <summary>
		/// Show app settings dialogue
		/// </summary>
		/// <param name="captchaRequiredException"></param>
		private void ActivateSettiingsForm(CaptchaRequiredException captchaRequiredException = null)
		{
			var settingsWindow = new SettingsWindow(captchaRequiredException, _userInfo);
			settingsWindow.Closed += SettingsForm_FormClosed;
			settingsWindow.ShowDialog(this);
		}
		#endregion
		
		#region Buttons
		private void ReportButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(WorkDescriptionTextBox.Text))
			{
				MessageBox.Show(string.Format("Work description can`t be empty."), DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			if ((DateTime.Now.Hour - _workInfo.WorkTime.Hour) < 0)
				if (MessageBox.Show(string.Format("You probably work starting from yesterday, use yesterday date?"), @"Work Time Automatic Control Suggestion", MessageBoxButton.YesNo, 
					MessageBoxImage.Question) == MessageBoxResult.Yes)
					if (_workInfo.WorkTime.Date.Day > 1)
						_workInfo = new DataEntities.WorkEntity(
							new DateTime(_workInfo.WorkTime.Year,
										 _workInfo.WorkTime.Month,
										 _workInfo.WorkTime.Day - 1,
										 _workInfo.WorkTime.Hour,
										 _workInfo.WorkTime.Minute,
										 _workInfo.WorkTime.Second),
							_workInfo.WorkDescription, _workInfo.Project);

			_workInfo.WorkDescription = WorkDescriptionTextBox.Text;
			Hide();
			SendInfoToCloud(_workInfo);
			_reportSended = true;
			Close();
		}
		#endregion

		#region TextBoxEvents
		private void WorkDescriptionTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			_workDescriptionTextBoxTextChanged = true;
			ReportButton.IsEnabled = !_exceptionOccurs;
			WorkDescriptionTextBox.Clear();
		}

		private void WorkDescriptionTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				ReportButton_Click(sender, e);
		}
		#endregion

		#region Menu
		private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow().ShowDialog(this);
		}

		private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ActivateSettiingsForm();
		}
		#endregion

		#region BackGroundWorkerEvents
		private void SendLocalSavedWorkReportsbackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			SavedWorkReportsSendingStatustoolStripProgressBar.Value++;
		}

		private void SendLocalSavedWorkReportsbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (_crashHandler.NumberOfCrashReports() == 0)
			{
				RegistryWork.SaveCrashReport(false);
				return;
			}

			for (var counter = 0; counter < _crashReports.Count(); counter++)
			{
				if (SendInfoToCloud(_crashReports.ElementAt(counter), false))
					_crashHandler.DeleteCrashReportReccords(_crashReports.ElementAt(counter));
				_sendLocalSavedWorkReportsbackgroundWorker.ReportProgress(counter);
			}

			if (_crashHandler.NumberOfCrashReports() == 0)
				RegistryWork.SaveCrashReport(false);
		}

		private void SendLocalSavedWorkReportsbackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			SavedWorkReportsSendingStatustoolStripProgressBar.Visibility = Visibility.Collapsed;
			SendingReportstoolStripStatusLabel.Visibility = Visibility.Collapsed;

			CurrentStatustoolStripStatusLabel.Visibility = Visibility.Visible;

			SettingsMenuItem.IsEnabled = true;
			WorkDescriptionTextBox.IsEnabled = true;
		}
		#endregion

		#region FormEvents
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
			LoadSettings();
		}

		private void SettingsForm_FormClosed(object sender, EventArgs e)
		{
			LoadSettings();
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (_reportSended)
				return;
			_workInfo.WorkDescription = string.IsNullOrEmpty(WorkDescriptionTextBox.Text)
			                            || (WorkDescriptionTextBox.Text.Count() < 1)
			                            || !_workDescriptionTextBoxTextChanged
			                            	? WORK_DESCRIPTION_WHEN_FORM_CLOSING_MISSING
			                            	: WorkDescriptionTextBox.Text;
			Hide();
			SendInfoToCloud(_workInfo);
		}
		#endregion
	}
}
