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
using System.Linq;
using Microsoft.Win32;
using System.IO;

namespace WorkTimeAutomaticControl
{
	internal static class RegistryWork
	{
		private const string DEFAULT_REGISTRY_PATH_PLACE_HOLDER = @"Software";
		private const string DEFAULT_REGISTRY_PATH_SOFTEARE_KEY = @"WorkTimeAutomaticControl";
		private const string DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS = @"Settings";
		private const string DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME = "DBLogin";
		private const string DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME = "DBPassword";
		private const string DEFAULT_REGISTRY_BUG_TRACK_LOGIN_KEY_NAME = "BTLogin";
		private const string DEFAULT_REGISTRY_BUG_TRACK_PASSWORD_KEY_NAME = "BTPassword";
		private const string DEFAULT_REGISTRY_USE_BUG_TRACK_KEY_NAME = "UseBT";
		private const string DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME = "TMultiplier";
		private const string DEFAULT_REGISTRY_CRASH_KEY_NAME = "Crash";
	
		internal static void SaveData(DataEntities.UserEntity dataContainer, bool useBugTrack = true)
		{
			if (string.IsNullOrEmpty(dataContainer.CloudDBUserData.Login)
				|| string.IsNullOrEmpty(dataContainer.CloudDBUserData.Password)
				|| (string.IsNullOrEmpty(dataContainer.BugTrackUserData.Login) && useBugTrack)
				|| (string.IsNullOrEmpty(dataContainer.BugTrackUserData.Password) && useBugTrack))
				throw new ArgumentException(string.Format("All fields must be filled."));

			var settingsRegKey = Registry.CurrentUser.OpenSubKey(DEFAULT_REGISTRY_PATH_PLACE_HOLDER, RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (settingsRegKey == null)
				throw new Exception(string.Format("No such place holder, place holder: {0}.", DEFAULT_REGISTRY_PATH_PLACE_HOLDER));
			settingsRegKey.CreateSubKey(DEFAULT_REGISTRY_PATH_SOFTEARE_KEY + Path.DirectorySeparatorChar + DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS);
			settingsRegKey.Close();

			var valueSettingsRegKey = Registry.CurrentUser.OpenSubKey(
				DEFAULT_REGISTRY_PATH_PLACE_HOLDER
				+ Path.DirectorySeparatorChar
				+ DEFAULT_REGISTRY_PATH_SOFTEARE_KEY
				+ Path.DirectorySeparatorChar
				+ DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS
				, RegistryKeyPermissionCheck.ReadWriteSubTree);

			if (valueSettingsRegKey == null)
				throw new Exception(string.Format("Create key failure."));

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME, dataContainer.CloudDBUserData.Login);
			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME, dataContainer.CloudDBUserData.Password);

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_USE_BUG_TRACK_KEY_NAME, useBugTrack.ToString());

			if (useBugTrack)
			{
				valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_BUG_TRACK_LOGIN_KEY_NAME, dataContainer.BugTrackUserData.Login);
				valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_BUG_TRACK_PASSWORD_KEY_NAME, dataContainer.BugTrackUserData.Password);
			}
			valueSettingsRegKey.SetValue(DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME, dataContainer.WorkReportSpreadsheetName);
			valueSettingsRegKey.SetValue(DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME, dataContainer.WorkReportHistorySpreadsheetName);

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME, dataContainer.Multiplayer.ToString());
			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME, dataContainer.Crash);

			valueSettingsRegKey.Close();
		}

		internal static DataEntities.UserEntity GetData()
		{
			var regKeyPath = DEFAULT_REGISTRY_PATH_PLACE_HOLDER
				+ Path.DirectorySeparatorChar
				+ DEFAULT_REGISTRY_PATH_SOFTEARE_KEY
				+ Path.DirectorySeparatorChar
				+ DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS;

			var valueSettingsRegKey = Registry.CurrentUser.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree);

			if (valueSettingsRegKey == null)
				throw new Exception(string.Format("Key not found, key: {0}.", regKeyPath));
			var regKeys = valueSettingsRegKey.GetValueNames();

			if (!regKeys.Contains(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME)
				|| !regKeys.Contains(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME)
				|| !regKeys.Contains(DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME))
				throw new Exception(string.Format("Keys not found."));

			var dblogin = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME) as string;
			var dbpassword = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME) as string;
			var useYouTrack = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_USE_BUG_TRACK_KEY_NAME) as string;
			var workReportSpreadsheetName = valueSettingsRegKey.GetValue(DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME) as string;
			var workReportHistorySpreadsheetName = valueSettingsRegKey.GetValue(DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME) as string;
			var multiplierFromRegistry = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME) as string;
			var crash = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME) as string;

			if (string.IsNullOrEmpty(dblogin)
				|| string.IsNullOrEmpty(dbpassword)
				|| string.IsNullOrEmpty(useYouTrack)
				|| string.IsNullOrEmpty(multiplierFromRegistry)
				|| string.IsNullOrEmpty(crash))
				throw new Exception(string.Format("Values is empty."));

			int multiplier;
			if (!int.TryParse(multiplierFromRegistry, out multiplier))
				throw new Exception(string.Format("Incorrect value, value: {0}.", multiplierFromRegistry));

			bool useBugTrackValue;
			if (!bool.TryParse(useYouTrack, out useBugTrackValue))
				throw new Exception(string.Format("Incorrect value, value: {0}.", useYouTrack));

			var btLogin = string.Empty;
			var btPassword = string.Empty;

			if (useBugTrackValue)
			{
				btLogin = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME) as string;
				btPassword = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME) as string;

				if (string.IsNullOrEmpty(btLogin)
				|| string.IsNullOrEmpty(btPassword))
					throw new Exception(string.Format("Values is empty."));
			}
			valueSettingsRegKey.Close();

			bool crashValue;
			if (!bool.TryParse(crash, out crashValue))
				throw new Exception(string.Format("Incorrect value, value: {0}.", crash));

			if (useBugTrackValue)
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword)
					, new DataEntities.UserPrivateData(btLogin, btPassword), multiplier, crashValue, workReportSpreadsheetName, workReportHistorySpreadsheetName);

			if (string.IsNullOrEmpty(workReportSpreadsheetName) && string.IsNullOrEmpty(workReportHistorySpreadsheetName))
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword)
					, new DataEntities.UserPrivateData(), multiplier, crashValue);

			if (string.IsNullOrEmpty(workReportHistorySpreadsheetName))
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword)
					, new DataEntities.UserPrivateData(), multiplier, crashValue, workReportSpreadsheetName);

			if (string.IsNullOrEmpty(workReportHistorySpreadsheetName))
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword)
					, new DataEntities.UserPrivateData(), multiplier, crashValue, workReportHistorySpreadsheetName: workReportHistorySpreadsheetName);
			return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword)

					, new DataEntities.UserPrivateData(), multiplier, crashValue, workReportSpreadsheetName, workReportHistorySpreadsheetName);
		}

		internal static void SaveCrashReport(bool chrasHappend)
		{
			var valueSettingsRegKey = Registry.CurrentUser.OpenSubKey(
				DEFAULT_REGISTRY_PATH_PLACE_HOLDER
				+ Path.DirectorySeparatorChar
				+ DEFAULT_REGISTRY_PATH_SOFTEARE_KEY
				+ Path.DirectorySeparatorChar
				+ DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS
				, RegistryKeyPermissionCheck.ReadWriteSubTree);

			if (valueSettingsRegKey == null)
				throw new Exception(string.Format("Open key failure."));

			var crash = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME) as string;

			if (crash == null)
				throw new Exception(string.Format("Crash values not found."));

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME, chrasHappend.ToString());

			valueSettingsRegKey.Close();
		}
	}
}
