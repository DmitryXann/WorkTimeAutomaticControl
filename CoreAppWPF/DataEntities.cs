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

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Represents all data storing entities
	/// </summary>
	public static class DataEntities
	{
		/// <summary>
		/// Represents single data entity
		/// </summary>
		public struct UserEntity
		{
			internal UserPrivateData CloudDBUserData;

			internal int Multiplayer;
			internal string WorkReportSpreadsheetName;
			internal string WorkReportHistorySpreadsheetName;
			internal bool Crash;

			internal UserEntity(UserPrivateData cloudDBUserData, int multiplayer, bool crash = false, 
				string workReportSpreadsheetName = "WorkReport", string workReportHistorySpreadsheetName = "WorkReportHistory")
			{
				CloudDBUserData = cloudDBUserData;

				Multiplayer = multiplayer;

				WorkReportSpreadsheetName = workReportSpreadsheetName;
				WorkReportHistorySpreadsheetName = workReportHistorySpreadsheetName;

				Crash = crash;				
			}
		}

		/// <summary>
		/// Stores login password pair
		/// </summary>
		internal struct UserPrivateData
		{
			internal string Login;
			internal string Password;
			
			internal UserPrivateData(string login, string password)
			{
				Login = login;
				Password = password;
			}
		}

		/// <summary>
		/// Stores single work report
		/// </summary>
		public struct WorkEntity
		{
			internal DateTime WorkTime;
			internal string WorkDescription;
			internal string Project;

			internal WorkEntity(DateTime workTime, string workDescription = null, string project = null)
			{
				Project = project;
				WorkTime = workTime;
				WorkDescription = workDescription;
			}
		}
	}
}
