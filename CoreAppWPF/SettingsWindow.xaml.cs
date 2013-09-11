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
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Google.GData.Client;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Interaction logic for WorkTimeAutomaticControlSettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		private const short DEFAULT_WINDOWS_HEIGTH_WITHOUT_CAPTCHA = 293;
		private const short DEFAULT_WINDOWS_HEIGTH_WITH_CAPTCHA = 422;

		private CaptchaRequiredException _captchaRequiredException;
		private DataEntities.UserEntity _userInfo;

		private bool _googleLoginTextBoxValid;
		private bool _googlePasswordTextBboxValid;
		private bool _multiplierTextBoxValid;
		private bool _captchaRequire;
		private bool _captchaTextBoxValid;
		private bool _logiFailed;

		public SettingsWindow(CaptchaRequiredException captchaRequiredException = null, DataEntities.UserEntity userInfo = new DataEntities.UserEntity())
		{
			_captchaRequiredException = captchaRequiredException;
			_userInfo = userInfo;
			_captchaRequire = _captchaRequiredException != null;

			InitializeComponent();
		}

		#region Buttons
		private void SaseButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(LoginTextBox.Text) || 
				string.IsNullOrEmpty(PasswordmaskedTextBox.Password) || 
				string.IsNullOrEmpty(MultiplierTextBox.Text))
			{
				MessageBox.Show(@"All fields must be filled.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			int multiplier;
			if (!int.TryParse(MultiplierTextBox.Text, out multiplier))
			{
				MessageBox.Show(@"Multiplier must be a digit.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			var userPrivateData = new DataEntities.UserPrivateData(LoginTextBox.Text, PasswordmaskedTextBox.Password);

			try
			{
				if (_captchaRequire)
				{
					if (string.IsNullOrEmpty(CaptchaTextBox.Text))
					{
						MessageBox.Show(@"reCaptcha can`t be empty.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

					if (_captchaRequiredException != null)
						CloudWorker.HandleCaptcha(_captchaRequiredException, userPrivateData, CaptchaTextBox.Text);
					else
					{
						MessageBox.Show(@"Unexpected error.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
						return;
					}

				}
				else
					CloudWorker.CheckUserData(userPrivateData);
			}	
			catch (CaptchaRequiredException captchaRequiredException)
			{
				_captchaRequiredException = captchaRequiredException;

				try
				{
					var pictureResponse = WebRequest.Create(captchaRequiredException.Url).GetResponse();

					var imageSource = new BitmapImage();
					imageSource.BeginInit();
					imageSource.StreamSource = pictureResponse.GetResponseStream();
					imageSource.EndInit();

					CaptchaImage.Source = imageSource;
				}
				catch (Exception)
				{
					CaptchaImage.Source = System.Windows.Interop.Imaging
					                            .CreateBitmapSourceFromHBitmap(Properties.Resources.error.GetHbitmap(), IntPtr.Zero,
					                                                           Int32Rect.Empty,
					                                                           BitmapSizeOptions.FromEmptyOptions());
					MessageBox.Show("Google Account Captcha receive fail, enter this reCaptcha: \"GACRFETRC\".",
					                DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally
				{
					AutentificationFailReport();
					MakeCaptchaFormTransformation(true);
					CaptchaTextBox.Focus();
				}
			}
			catch (Exception)
			{
				AutentificationFailReport();
				MakeCaptchaFormTransformation();
				LoginTextBox.Focus();

				MessageBox.Show(@"Google Account login fail, enter correct login and password.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);

				return;
			}

			RegistryWork.SaveData(
				new DataEntities.UserEntity(new DataEntities.UserPrivateData(Cryptography.GetCryptedMessage(LoginTextBox.Text), Cryptography.GetCryptedMessage(PasswordmaskedTextBox.Password)),
				                            multiplier,
				                            workReportSpreadsheetName: string.IsNullOrEmpty(WorkReportSpreadsheetNameTextBox.Text)
					                                                       ? DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME
					                                                       : WorkReportSpreadsheetNameTextBox.Text,
				                            workReportHistorySpreadsheetName:
					                            string.IsNullOrEmpty(HistorySpreadsheetNameTextBox.Text)
						                            ? DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME
						                            : HistorySpreadsheetNameTextBox.Text));
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion

		#region Methods
		private void MakeCaptchaFormTransformation(bool useCaptcha = false)
		{
			Height = useCaptcha
			         	? DEFAULT_WINDOWS_HEIGTH_WITH_CAPTCHA
			         	: DEFAULT_WINDOWS_HEIGTH_WITHOUT_CAPTCHA;

			var visibility = useCaptcha ? Visibility.Visible : Visibility.Collapsed;

			CaptchaTextBox.Visibility = visibility;
			CaptchaAsterixLabel.Visibility = visibility;
			CaptchaLabel.Visibility = visibility;
			CaptchaToolStripStatusLabel.Visibility = visibility;
			CaptchaImage.Visibility = visibility;
			CaptchaTextBox.TabIndex = useCaptcha ? 5 : 15;

			_captchaRequire = useCaptcha;
		}

		private void AutentificationFailReport(bool reportAboutFail = true)
		{
			GoogleAccountLoginAsteriskLabel.Foreground = Brushes.Red;
			GoogleAccountPasswordAsteriskLabel.Foreground = Brushes.Red;
			PasswordmaskedTextBox.Clear();

			OktoolStripStatusLabel.Visibility = !reportAboutFail ? Visibility.Visible : Visibility.Collapsed;
			AsterisktoolStripStatusLabel.Visibility = reportAboutFail ? Visibility.Visible : Visibility.Collapsed;

			_googleLoginTextBoxValid = !reportAboutFail;
			_googlePasswordTextBboxValid = !reportAboutFail;
			_logiFailed = reportAboutFail;
		}

		private void TryToActivateOkMessage()
		{
			var currentStatus = (!_googleLoginTextBoxValid || !_googlePasswordTextBboxValid || !_multiplierTextBoxValid || !(!_captchaRequire || _captchaTextBoxValid));

			var visibility = currentStatus ? Visibility.Visible : Visibility.Collapsed;
			AsterisktoolStripStatusLabel.Visibility = visibility;
			OktoolStripStatusLabel.Visibility = !currentStatus ? Visibility.Visible : Visibility.Collapsed;
			if (_captchaRequire)
				CaptchaToolStripStatusLabel.Visibility = visibility;
		}
		#endregion

		#region TextBoxEvents
		#region LostFocus
		private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(LoginTextBox.Text))
				return;
			_googleLoginTextBoxValid = true;
			GoogleAccountLoginAsteriskLabel.Foreground = Brushes.Green;

			TryToActivateOkMessage();
		}

		private void PasswordmaskedTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(PasswordmaskedTextBox.Password))
				return;
			_googlePasswordTextBboxValid = true;
			GoogleAccountPasswordAsteriskLabel.Foreground = Brushes.Green;

			if ((_captchaRequire || _logiFailed) && _multiplierTextBoxValid)
				SaveSettingsButton.Focus();

			TryToActivateOkMessage();
		}

		private void MultiplierTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(MultiplierTextBox.Text) || MultiplierTextBox.Text.Any(el => !char.IsDigit(el)))
				return;
			_multiplierTextBoxValid = true;

			HourMultiplierAsteriskLabel.Foreground = Brushes.Green;
			TryToActivateOkMessage();
		}

		private void CaptchaTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(CaptchaTextBox.Text)) return;
			_captchaTextBoxValid = true;
			CaptchaAsterixLabel.Foreground = Brushes.Green;

			 PasswordmaskedTextBox.Focus();
		}
		#endregion

		#region GotFocus
		private void LoginTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			_googleLoginTextBoxValid = false;
			GoogleAccountLoginAsteriskLabel.Foreground = Brushes.Red;
			TryToActivateOkMessage();
		}

		private void PasswordmaskedTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			_googlePasswordTextBboxValid = false;
			PasswordmaskedTextBox.Clear();
			GoogleAccountPasswordAsteriskLabel.Foreground = Brushes.Red;
			TryToActivateOkMessage();
		}

		private void MultiplierTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			_multiplierTextBoxValid = false;
			HourMultiplierAsteriskLabel.Foreground = Brushes.Red;
			TryToActivateOkMessage();
		}

		private void CaptchaTextBox_GotFocus(object sender, RoutedEventArgs e)
		{
			_captchaTextBoxValid = false;
			CaptchaTextBox.Clear();
			CaptchaAsterixLabel.Foreground = Brushes.Red;
			TryToActivateOkMessage();
		}
		#endregion

		#region KeyDown
		private void WorkReportSpreadsheetNameTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;
			HistorySpreadsheetNameTextBox.Focus();
		}

		private void HistorySpreadsheetNameTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;
			LoginTextBox.Focus();
		}

		private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;
			PasswordmaskedTextBox.Focus();
		}

		private void PasswordmaskedTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			if (_captchaRequire)
				SaveSettingsButton.Focus();
			else
				MultiplierTextBox.Focus();
		}

		private void MultiplierTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;

			if (_captchaRequire)
				CaptchaTextBox.Focus();
			else
				SaveSettingsButton.Focus();
		}

		private void CaptchaTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter)
				return;
			SaveSettingsButton.Focus();
		}
		#endregion
		#endregion

		#region FormEvents
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			GlassHelper.ExtendGlassFrame(this, new Thickness(-1));

			WorkReportSpreadsheetNameTextBox.Text = string.IsNullOrEmpty(_userInfo.WorkReportSpreadsheetName)
				                                        ? DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME
				                                        : _userInfo.WorkReportSpreadsheetName;

			HistorySpreadsheetNameTextBox.Text = string.IsNullOrEmpty(_userInfo.WorkReportHistorySpreadsheetName)
				                                     ? DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME
				                                     : _userInfo.WorkReportHistorySpreadsheetName;

			LoginTextBox.Text = _userInfo.CloudDBUserData.Login;
			PasswordmaskedTextBox.Password = _userInfo.CloudDBUserData.Password;
			MultiplierTextBox.Text = _userInfo.Multiplayer.ToString();
		}
		#endregion

		public void ShowDialog(Window parent)
		{
			Owner = parent;
			ShowDialog();
		}
	}
}
