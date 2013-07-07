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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace WorkTimeAutomaticControl
{
	internal sealed class CrashHandler
	{
		private const string DEFAULT_XML_COMMENT = "This is auto generated file, do not modify";
		private const string DEFAULT_XML_PARENT = "WorkTimeAutomaticControlCrashReport";
		private const string DEFAULT_XML_WORK_TIME_ELEMENT_NAME = "WorkTime";
		private const string DEFAULT_XML_FILE_NAME = "CrashReport.wtac";

		private XDocument _crashReport;
		private readonly bool _crashReportFileNotFound;

		private readonly string _fullFilePath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(CrashHandler)).CodeBase).Substring(6) + Path.DirectorySeparatorChar +
		                                        DEFAULT_XML_FILE_NAME;
		internal CrashHandler()
		{
			_crashReportFileNotFound = File.Exists(_fullFilePath);
			if (_crashReportFileNotFound)
				try
				{
					_crashReport = XDocument.Load(_fullFilePath);
				}
				catch (Exception)
				{
					throw new FileLoadException(string.Format("CrashReport file load fail, incorrect file structure or file can`t be opened."));
				}
		}

		internal void SaveCrashReport(DataEntities.WorkEntity inputData)
		{
			if (!_crashReportFileNotFound)
			{
				_crashReport = new XDocument(
					new XComment(DEFAULT_XML_COMMENT),
					new XElement(DEFAULT_XML_PARENT, 
						new XElement(DEFAULT_XML_WORK_TIME_ELEMENT_NAME,
							new XAttribute(DEFAULT_XML_WORK_TIME_ELEMENT_NAME, inputData.WorkTime)
							,inputData.WorkDescription)));
				_crashReport.Save(_fullFilePath);
			}
			else
			{
				var parentElement = _crashReport.Root;
				if (parentElement == null)
					throw new FileLoadException(string.Format("CrashReport Load fail, incorrect file structure."));
				if (!GetAllCrashes().Any(el => el.WorkTime == inputData.WorkTime))
					parentElement.Add(
					new XElement(DEFAULT_XML_WORK_TIME_ELEMENT_NAME,
							new XAttribute(DEFAULT_XML_WORK_TIME_ELEMENT_NAME, inputData.WorkTime)
							,inputData.WorkDescription));
				_crashReport.Save(_fullFilePath);
			}
		}

		internal IEnumerable<DataEntities.WorkEntity> GetAllCrashes()
		{
			if (!_crashReportFileNotFound)
				throw new FileNotFoundException(string.Format("File not found, file path: {0}", _fullFilePath));
			return new List<DataEntities.WorkEntity>(_crashReport.Descendants().Elements(DEFAULT_XML_WORK_TIME_ELEMENT_NAME).Select(el => new DataEntities.WorkEntity(DateTime.Parse((string)el.Attribute(DEFAULT_XML_WORK_TIME_ELEMENT_NAME)), el.Value)));
		}

		internal int NumberOfCrashReports()
		{
			if (!_crashReportFileNotFound)
				throw new FileNotFoundException(string.Format("File not found, file path: {0}.", _fullFilePath));

			var parentElement = _crashReport.Root;
			if (parentElement == null)
				throw new FileLoadException(string.Format("CrashReport Load fail, incorrect file structure."));

			return _crashReport.Descendants().Elements(DEFAULT_XML_WORK_TIME_ELEMENT_NAME).Count();
		}

		internal void DeleteCrashReportReccords(DataEntities.WorkEntity inputData)
		{
			if (!_crashReportFileNotFound)
				throw new FileNotFoundException(string.Format("File not found, file path: {0}.", _fullFilePath));

			var parentElement = _crashReport.Root;
			if (parentElement == null)
				throw new FileLoadException(string.Format("CrashReport Load fail, incorrect file structure."));

			var selectedCrashReport = _crashReport.Descendants().Elements(DEFAULT_XML_WORK_TIME_ELEMENT_NAME).Where(
				el =>
				DateTime.Parse((string) el.Attribute(DEFAULT_XML_WORK_TIME_ELEMENT_NAME)) == inputData.WorkTime &&
				el.Value == inputData.WorkDescription);

			if (selectedCrashReport.Count() != 1)
				throw new FileLoadException(string.Format("CrashReport incorrect file structure."));
			selectedCrashReport.First().Remove();
			_crashReport.Save(_fullFilePath);
		}

		internal static void DeleteCorruptedCrashReportFile()
		{
			File.Delete(Environment.CurrentDirectory + Path.DirectorySeparatorChar + DEFAULT_XML_FILE_NAME);
		}

		internal static void OpenCrashReportFileInDefaultTextEditor()
		{
			new Process
			{
				StartInfo =
				{
					FileName =
					Environment.CurrentDirectory + Path.DirectorySeparatorChar + DEFAULT_XML_FILE_NAME,
					UseShellExecute = true
				}
			}.Start();
		}

		internal static void MoveCrashedReportToDescktop()
		{
			File.Move((Path.GetDirectoryName(Assembly.GetAssembly(typeof(CrashHandler)).CodeBase).Substring(6) + Path.DirectorySeparatorChar +
												DEFAULT_XML_FILE_NAME), Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)+ Path.DirectorySeparatorChar + "CORRUPTED_" + DEFAULT_XML_FILE_NAME);
		}
	}
}
