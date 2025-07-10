using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	internal class StmDataForExcelPassword : StmData
	{
		public StmDataForExcelPassword(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Action<ZString, ZString> logAction;

		public override void OnSaving()
		{
			base.OnSaving();
			logAction?.Invoke(PasswordType, (NoResString)"Changed");
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				logAction?.Invoke(PasswordType, (NoResString)"Removed");
			}
			base.Delete();
		}

		#region UniqueIndexFailureHandlers

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				yield return new ExcelPasswordUniqueIndexFailureHandler(this);
			}
		}

		public ZString PasswordType => SD_Name == ExcelPasswordConstants.ExcelPasswordForModifying ? (NoResString)"Modify" : (NoResString)"Open";

		public class ExcelPasswordUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public ExcelPasswordUniqueIndexFailureHandler(StmDataForExcelPassword stmDataForExcelPasswordInMemory)
			{
				StmDataForExcelPasswordInMemory = stmDataForExcelPasswordInMemory;
			}
			readonly StmDataForExcelPassword StmDataForExcelPasswordInMemory;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return StmDataSchema.Constants.Indexes.NR_UC__SD_Name_SD_Owner_SD_DepartmentGuid; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier.ReportInformation(
					Res.GetString("5EDC1919-D48E-48C2-91C1-A9205FC63786", "An Excel {0} Password for this staff/contact was added by another user and already exists in database. Please reload the form.", StmDataForExcelPasswordInMemory.PasswordType),
					Res.GetString("A7105506-B00F-4AA7-B502-EF5410A52D9F", "Excel {0} Password already exists.", StmDataForExcelPasswordInMemory.PasswordType));
			}
		}

		#endregion
	}

	public static class ExcelPasswordConstants
	{
		public const string ExcelPasswordForModifying = "ExcelPasswordForModifying";
		public const string ExcelPasswordForOpening = "ExcelPasswordForOpening";
	}
}
