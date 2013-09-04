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
using Google.GData.Client;
using Google.GData.Spreadsheets;
using WorkTimeAutomaticControl.Exceptions;

namespace WorkTimeAutomaticControl
{
	/// <summary>
	/// Handle google spreadsheet creation and data storing
	/// </summary>
	internal sealed class CloudWorker
	{
		private const string APPLICATION_NAME = "WorkTimeAutomaticControl";

		private static Google.GData.Documents.DocumentsService _documentService;

		private readonly SpreadsheetsService _spreadsheetService;
		private readonly string _multiplier;
		private readonly string _workReportSpreadsheetName;
		private readonly string _workReportPreviousMonthSpreadsheetName;


		/// <summary>
		/// Cloud worker initialization
		/// </summary>
		/// <exception cref="ArgumentException">Multiplier cant be empty or null.</exception>
		internal CloudWorker(DataEntities.UserPrivateData userInfo, string multiplier, string workReportSpreadsheetName = DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME, 
			string workReportHistorySpreadsheetName = DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME)
		{
			//throw new CloudWorkerException("olol");

			_documentService = new Google.GData.Documents.DocumentsService(APPLICATION_NAME);
			_documentService.setUserCredentials(userInfo.Login, userInfo.Password);

			_documentService.QueryClientLoginToken();

			_spreadsheetService = new SpreadsheetsService(APPLICATION_NAME);
			_spreadsheetService.setUserCredentials(userInfo.Login, userInfo.Password);

			if (string.IsNullOrEmpty(multiplier))
				throw new ArgumentException("Multiplier cant be empty or null.");

			_workReportSpreadsheetName = string.IsNullOrEmpty(workReportSpreadsheetName)
											? DefaultConst.DEFAULT_WORK_REPORT_SPREADHEET_NAME
			                             	: workReportSpreadsheetName;

			_workReportPreviousMonthSpreadsheetName = string.IsNullOrEmpty(workReportHistorySpreadsheetName)
														? DefaultConst.DEFAULT_WORK_REPORT_HISTORY_SPREADHEET_NAME
														: workReportHistorySpreadsheetName;
			_multiplier = multiplier;
		}

		/// <summary>
		/// Sends work info to google docs
		/// </summary>
		/// <exception cref="CloudWorkerException"></exception>
		internal void SendWorkUpdate(DataEntities.WorkEntity workInfo)
		{
			var spreadsheetURI = NewSpreadsheet(_workReportSpreadsheetName);

			var entity = _spreadsheetService.Query(new CellQuery(spreadsheetURI)).Entries;
			var entityCount = entity.Count;
			var numbeerOfRow = (uint)((entityCount / 4) + 2);
			DateTime lastDate;

			if ((numbeerOfRow > 2) && (DateTime.TryParse(((CellEntry)entity[entityCount - 4]).Cell.Value, out lastDate)))
			{
				if (lastDate.Month != workInfo.WorkTime.Month)
				{
					var previousMonthSpreadsheetURI = NewSpreadsheet(_workReportPreviousMonthSpreadsheetName);
					var previousMonthEntityCount = _spreadsheetService.Query(new CellQuery(previousMonthSpreadsheetURI)).Entries.Count;
					var previousMonthEntity = (uint)((previousMonthEntityCount / 4) + 2);

					var row = 4;
					while (row < entityCount)
					{
						for (uint item = 1; item <= 4; item++)
							EditCell(previousMonthSpreadsheetURI, ((CellEntry)entity[(row++)]).Cell.Value, previousMonthEntity, item, ref previousMonthEntityCount);
						previousMonthEntity++;
					}

					for (uint workSpreadsheetRow = 3; workSpreadsheetRow < numbeerOfRow; workSpreadsheetRow++)
					{
						for (uint item = 1; item <= 4; item++)
							EditCell(spreadsheetURI, string.Empty, workSpreadsheetRow, item, ref entityCount, true);
					}
					numbeerOfRow = 3;
				}
			}
			else
				if (entityCount != 4)
					throw new CloudWorkerException("Incorrect spreadsheet structure.");

			EditCell(spreadsheetURI, workInfo.WorkTime.ToString("d"), numbeerOfRow, 1, ref entityCount);
			EditCell(spreadsheetURI, string.IsNullOrEmpty(workInfo.Project)
				? "Enter addition project info"
				: workInfo.Project, numbeerOfRow, 2, ref entityCount);
			EditCell(spreadsheetURI, workInfo.WorkDescription, numbeerOfRow, 3, ref entityCount);
			EditCell(spreadsheetURI, workInfo.WorkTime.ToString("HH:mm"), numbeerOfRow, 4, ref entityCount);
		}

		/// <summary>
		/// Creates new spreadsheet with needed name
		/// </summary>
		/// <param name="documentName">spreadsheet name</param>
		/// <returns>spreadsheet URI</returns>
		private string NewSpreadsheet(string documentName)
		{
			var newSpreadsheet = _documentService.Query(new Google.GData.Documents.DocumentsListQuery())
				.Entries.Any(el => el.Title.Text == documentName);

			if (!newSpreadsheet)
			{
				var entry = new Google.GData.Documents.DocumentEntry
				            	{
				            		Title = {Text = documentName},
				            		IsSpreadsheet = true
				            	};

				entry.Categories.Add(Google.GData.Documents.DocumentEntry.SPREADSHEET_CATEGORY);
				_documentService.Insert(Google.GData.Documents.DocumentsListQuery.documentsBaseUri, entry);
			}

			var spreadsheetURI = _spreadsheetService.Query(new WorksheetQuery(_spreadsheetService.Query(new SpreadsheetQuery()).Entries
																	.Single(el => el.Title.Text == documentName)
																	.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, 
																	AtomLink.ATOM_TYPE).HRef.Content))
				.Entries.Select(el => el.Links.FindService(GDataSpreadsheetsNameTable.CellRel, AtomLink.ATOM_TYPE))
				.First().HRef.Content;

			if (!newSpreadsheet)
			{
				var currentEntityCount = 0;
				EditCell(spreadsheetURI, "Sum (HH:MM):", 1, 5, ref currentEntityCount);
				EditCell(spreadsheetURI, "=SUM(D1:D10000)", 1, 6, ref currentEntityCount);

				EditCell(spreadsheetURI, "Sum (Money:Money):", 2, 5, ref currentEntityCount);
				EditCell(spreadsheetURI, "=SUM(D1:D10000)*" + _multiplier, 2, 6, ref currentEntityCount);
			}

			return spreadsheetURI;
		}

		/// <summary>
		/// Edit single cell
		/// </summary>
		private void EditCell(string spreadsheetURI, string inputValue, uint row, uint column, ref int entityCount, bool deleteItem = false)
		{
			do //Extra logic to ensure, that cell is created, because sometimes it need to be done several times
				do
					_spreadsheetService.Insert(new Uri(spreadsheetURI), new CellEntry
					                                                    	{
					                                                    		Cell = new CellEntry.CellElement
					                                                    		       	{
					                                                    		       		InputValue = inputValue,
					                                                    		       		Row = row,
					                                                    		       		Column = column
					                                                    		       	}
					                                                    	});
				while (_spreadsheetService.Query(new CellQuery(spreadsheetURI)).Entries.Count == entityCount);
			while (_spreadsheetService.Query(new CellQuery(spreadsheetURI)).Entries.Count == entityCount);

			if (deleteItem)
				entityCount--;
			else
				entityCount++;
		}

		/// <summary>
		/// Checks user data connecting to google
		/// </summary>
		/// <returns>true - if all ok, false - otherwise</returns>
		/// <exception cref="CaptchaRequiredException"></exception>
		/// <exception cref="InvalidCredentialsException"></exception>
		internal static bool CheckUserData(DataEntities.UserPrivateData userInfo)
		{
			_documentService = new Google.GData.Documents.DocumentsService(APPLICATION_NAME);
			_documentService.setUserCredentials(userInfo.Login, userInfo.Password);
			_documentService.QueryClientLoginToken();

			return true;
		}

		/// <summary>
		/// Process captcha
		/// </summary>
		/// <returns>true - if all ok, false - otherwise</returns>
		/// <exception cref="CaptchaRequiredException"></exception>
		/// <exception cref="InvalidCredentialsException"></exception>
		internal static bool HandleCaptcha(CaptchaRequiredException captchaRequiredException, DataEntities.UserPrivateData userInfo, string captchaAnswer)
		{
			if (_documentService == null)
				throw new ArgumentNullException(
					string.Format(@"Google document service == null, that's means, that you try to use this method before using 
internal static bool CheckUserData(DataEntities.UserPrivateData userInfo).
First use internal static bool CheckUserData(DataEntities.UserPrivateData userInfo) and, 
if CaptchaRequiredException was thrown, use 
this(internal static bool HandleCaptcha(CaptchaRequiredException captchaRequiredException, DataEntities.UserPrivateData userInfo, string captchaAnswer)) method."));

			_documentService.setUserCredentials(userInfo.Login, userInfo.Password);

			var requestFactory = (GDataGAuthRequestFactory)_documentService.RequestFactory;

			requestFactory.CaptchaAnswer = captchaAnswer;
			requestFactory.CaptchaToken = captchaRequiredException.Token;

			_documentService.QueryClientLoginToken();

			return true;
		}
	}
}
