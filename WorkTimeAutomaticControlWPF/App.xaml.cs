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
using System;
using System.Windows;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ProcessSturtUp(e.Args);
			Shutdown();
		}

		protected void ProcessSturtUp(string[] args)
		{
			if (args.Length == 0)
			{
				MessageBox.Show("No args found. \nWork of Work Time Automatic Control terminated.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			DateTime dateTime;
			if (!DateTime.TryParse(args[0], out dateTime))
			{
				MessageBox.Show("Incorrect args. \nWork of Work Time Automatic Control terminated.", DefaultConst.ERROR_MESSAGE_HEADER_OF_WINDOW, MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			new MainWindow(
				new DataEntities.WorkEntity(new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
				                                         dateTime.Hour, dateTime.Minute, dateTime.Second)))
				.ShowDialog();
		}
	}
}
