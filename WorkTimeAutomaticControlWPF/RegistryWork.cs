//Work Time Automatic Control StopWatch use Google Spreadsheets to save your work information in the cloud.
//Copyright (C) 2013 Tomayly Dmitry
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
using WorkTimeAutomaticControl.Exceptions;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Handles application settings storing in registry 
	/// </summary>
	internal static class RegistryWork
	{
		private const string DEFAULT_REGISTRY_PATH_PLACE_HOLDER = @"Software";
		private const string DEFAULT_REGISTRY_PATH_SOFTEARE_KEY = @"WorkTimeAutomaticControl";
		private const string DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS = @"Settings";
		private const string DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME = "DBLogin";
		private const string DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME = "DBPassword";
		private const string DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME = "TMultiplier";
		private const string DEFAULT_REGISTRY_CRASH_KEY_NAME = "Crash";
		
		/// <summary>
		/// Save settings in registry
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="RegystryWorkException"></exception>
		internal static void SaveData(DataEntities.UserEntity dataContainer)
		{
			if (string.IsNullOrEmpty(dataContainer.CloudDBUserData.Login) || string.IsNullOrEmpty(dataContainer.CloudDBUserData.Password))
				throw new ArgumentException("All fields must be filled.");

			var settingsRegKey = Registry.CurrentUser.OpenSubKey(DEFAULT_REGISTRY_PATH_PLACE_HOLDER, RegistryKeyPermissionCheck.ReadWriteSubTree);
			if (settingsRegKey == null)
				throw new RegystryWorkException(string.Format("No such place holder, place holder: {0}.", DEFAULT_REGISTRY_PATH_PLACE_HOLDER));

			settingsRegKey.CreateSubKey(DEFAULT_REGISTRY_PATH_SOFTEARE_KEY + Path.DirectorySeparatorChar + DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS);
			settingsRegKey.Close();

			var valueSettingsRegKey = Registry.CurrentUser.OpenSubKey(DEFAULT_REGISTRY_PATH_PLACE_HOLDER +
			                                                          Path.DirectorySeparatorChar +
			                                                          DEFAULT_REGISTRY_PATH_SOFTEARE_KEY +
			                                                          Path.DirectorySeparatorChar +
			                                                          DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS,
			                                                          RegistryKeyPermissionCheck.ReadWriteSubTree);

			if (valueSettingsRegKey == null)
				throw new RegystryWorkException("Create key failure.");

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME, dataContainer.CloudDBUserData.Login);
			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME, dataContainer.CloudDBUserData.Password);

			valueSettingsRegKey.SetValue(DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME, dataContainer.WorkReportSpreadsheetName);
			valueSettingsRegKey.SetValue(DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME, dataContainer.WorkReportHistorySpreadsheetName);

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME, dataContainer.Multiplayer.ToString());
			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME, dataContainer.Crash);

			valueSettingsRegKey.Close();
		}

		/// <summary>
		/// Gets application settings from registry
		/// </summary>
		/// <exception cref="RegystryWorkException"></exception>
		internal static DataEntities.UserEntity GetData()
		{
			var regKeyPath = DEFAULT_REGISTRY_PATH_PLACE_HOLDER + 
				Path.DirectorySeparatorChar + 
				DEFAULT_REGISTRY_PATH_SOFTEARE_KEY + 
				Path.DirectorySeparatorChar + 
				DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS;

			var valueSettingsRegKey = Registry.CurrentUser.OpenSubKey(regKeyPath, RegistryKeyPermissionCheck.ReadSubTree);

			if (valueSettingsRegKey == null)
				throw new RegystryWorkException(string.Format("Key not found, key: {0}.", regKeyPath));
			var regKeys = valueSettingsRegKey.GetValueNames();

			if (!regKeys.Contains(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME) || 
				!regKeys.Contains(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME) || 
				!regKeys.Contains(DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME))
				throw new RegystryWorkException("Keys not found.");

			var dblogin = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_DB_LOGIN_KEY_NAME) as string;
			var dbpassword = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_DB_PASSWORD_KEY_NAME) as string;
			var workReportSpreadsheetName = valueSettingsRegKey.GetValue(DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME) as string;
			var workReportHistorySpreadsheetName = valueSettingsRegKey.GetValue(DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME) as string;
			var multiplierFromRegistry = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_MULTIPLIER_KEY_NAME) as string;
			var crash = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME) as string;

			if (string.IsNullOrEmpty(dblogin) || 
				string.IsNullOrEmpty(dbpassword) || 
				string.IsNullOrEmpty(multiplierFromRegistry) || 
				string.IsNullOrEmpty(crash))
				throw new RegystryWorkException("Values is empty.");

			int multiplier;
			if (!int.TryParse(multiplierFromRegistry, out multiplier))
				throw new RegystryWorkException(string.Format("Incorrect value, value: {0}.", multiplierFromRegistry));

			valueSettingsRegKey.Close();

			bool crashValue;
			if (!bool.TryParse(crash, out crashValue))
				throw new RegystryWorkException(string.Format("Incorrect value, value: {0}.", crash));

			if (string.IsNullOrEmpty(workReportSpreadsheetName) && string.IsNullOrEmpty(workReportHistorySpreadsheetName))
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword), multiplier, crashValue);

			if (string.IsNullOrEmpty(workReportHistorySpreadsheetName))
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword), multiplier, crashValue, workReportSpreadsheetName);

			if (string.IsNullOrEmpty(workReportHistorySpreadsheetName))
				return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword), multiplier, 
					crashValue, workReportHistorySpreadsheetName: workReportHistorySpreadsheetName);

			return new DataEntities.UserEntity(new DataEntities.UserPrivateData(dblogin, dbpassword), multiplier, crashValue, workReportSpreadsheetName, workReportHistorySpreadsheetName);
		}

		/// <summary>
		/// Saves application crash report flag (unable to connect to cloud in previous app run)
		/// </summary>
		/// <exception cref="RegystryWorkException"></exception>
		internal static void SaveCrashReport(bool chrasHappend)
		{
			var valueSettingsRegKey = Registry.CurrentUser.OpenSubKey(DEFAULT_REGISTRY_PATH_PLACE_HOLDER +
			                                                          Path.DirectorySeparatorChar +
			                                                          DEFAULT_REGISTRY_PATH_SOFTEARE_KEY +
			                                                          Path.DirectorySeparatorChar +
			                                                          DEFAULT_REGISTRY_PATH_SOFTWARE_SETTINGS,
			                                                          RegistryKeyPermissionCheck.ReadWriteSubTree);

			if (valueSettingsRegKey == null)
				throw new RegystryWorkException("Open key failure.");

			var crash = valueSettingsRegKey.GetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME) as string;

			if (crash == null)
				throw new RegystryWorkException("Crash values not found.");

			valueSettingsRegKey.SetValue(DEFAULT_REGISTRY_CRASH_KEY_NAME, chrasHappend.ToString());

			valueSettingsRegKey.Close();
		}
	}
}
