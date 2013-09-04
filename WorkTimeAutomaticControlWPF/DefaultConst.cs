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
namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Stores application const, that shared between some classes
	/// </summary>
	internal static class DefaultConst
	{
		internal const string DEFAULT_WORK_REPORT_SPREADHEET_NAME = "WorkReport";
		internal const string DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME = "WorkReportHistory";
		internal const string ERROR_MESSAGE_HEADER_OF_WINDOW = @"Work Time Automatic Control Error";
	}
}
