//Work Time Automatic Control StopWatch use Google Spreadsheets to save your work information in the cloud.
//Copyright (C) 2012  Tomayly Dmitry
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
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Interaction logic for WorkTimeAutomaticControlAboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		private const string ABOUT_GNU = "http://www.gnu.org/licenses/licenses.html#GPL";

		public AboutWindow()
		{
			InitializeComponent();
		}

		#region Menu
		private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
		{
			AppendTextToAboutTextBox(true);
		}

		private void LicenseMenuItem_Click(object sender, RoutedEventArgs e)
		{
			AppendTextToAboutTextBox(false);
		}
		#endregion

		#region Buttons
		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Process.Start(ABOUT_GNU);
		}
		#endregion

		#region Methods
		private void AppendTextToAboutTextBox(bool showAbout)
		{
			AboutRichTextBox.Document.Blocks.Clear();

			AboutRichTextBox.AppendText(showAbout ? Properties.Resources.About : Properties.Resources.License);
			//AboutRichTextBox.SelectionStart = 0;
			AboutRichTextBox.ScrollToHome();
		}
		#endregion
		
		#region FormEvents
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
			AppendTextToAboutTextBox(true);
		}
		#endregion
	}
}
