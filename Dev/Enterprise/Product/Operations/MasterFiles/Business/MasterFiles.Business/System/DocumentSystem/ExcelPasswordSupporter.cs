using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class ExcelPasswordSupporter
	{
		public ExcelPasswordSupporter(BusinessObject parent)
		{
			if (parent == null)
			{
				throw new ArgumentException("parent can't be null");
			}
			Parent = parent;
		}

		readonly BusinessObject Parent;

		#region excel password

		StmDataForExcelPassword StmDataForExcelPasswordOpening
		{
			get
			{
				return stmDataForExcelPasswordOpening ??= GetStmDataViaPasswordType(ExcelPasswordConstants.ExcelPasswordForOpening);
			}
		}

		StmDataForExcelPassword stmDataForExcelPasswordOpening;

		StmDataForExcelPassword StmDataForExcelPasswordModifying
		{
			get
			{
				return stmDataForExcelPasswordModifying ??= GetStmDataViaPasswordType(ExcelPasswordConstants.ExcelPasswordForModifying);
			}
		}

		StmDataForExcelPassword stmDataForExcelPasswordModifying;

		StmDataForExcelPassword GetStmDataViaPasswordType(string passwordType)
		{
			var query = new ZQuery(StmDataSchema.SD_Owner, Parent.PK);
			query.AddToFilter(StmDataSchema.SD_Name, passwordType);
			var stmDataForExcelPassword = Parent.Factory.LoadTop1<StmDataForExcelPassword>(query);
			if (stmDataForExcelPassword != null)
			{
				stmDataForExcelPassword.logAction = AddExcelPasswordChangedEventLog;
			}
			return stmDataForExcelPassword;
		}

		TwoWayEncoder Encoder
		{
			get
			{
				return encoder ??= TwoWayEncoder.NewWithStandardInitialisationVector();
			}
		}

		TwoWayEncoder encoder;

		public ZString ExcelPasswordForOpening
		{
			get
			{
				if(excelPasswordForOpening != null)
				{
					return excelPasswordForOpening;
				}

				if (StmDataForExcelPasswordOpening != null)
				{
					excelPasswordForOpening = Encoder.Decrypt(Encoding.UTF8.GetString(StmDataForExcelPasswordOpening.SD_BinaryValue));
				}
				else
				{
					excelPasswordForOpening = string.Empty;
				}
				return excelPasswordForOpening;
			}
			set
			{
				if (excelPasswordForOpening != value)
				{
					excelPasswordForOpening = value;

					if (StmDataForExcelPasswordOpening == null)
					{
						stmDataForExcelPasswordOpening = CreateNewStmDataForExcelPassword(ExcelPasswordConstants.ExcelPasswordForOpening);
					}
					stmDataForExcelPasswordOpening.SD_BinaryValue = Encoding.UTF8.GetBytes(Encoder.Encrypt(excelPasswordForOpening));
				}
			}
		}

		string excelPasswordForOpening;

		public ZString ExcelPasswordForModifying
		{
			get
			{
				if (excelPasswordForModifying != null)
				{
					return excelPasswordForModifying;
				} 

				if (StmDataForExcelPasswordModifying != null)
				{
					excelPasswordForModifying = Encoder.Decrypt(Encoding.UTF8.GetString(StmDataForExcelPasswordModifying.SD_BinaryValue));
				}
				else
				{
					excelPasswordForModifying = string.Empty;
				}
				return excelPasswordForModifying;
			}
			set
			{
				if (excelPasswordForModifying != value)
				{
					excelPasswordForModifying = value;

					if (StmDataForExcelPasswordModifying == null)
					{
						stmDataForExcelPasswordModifying = CreateNewStmDataForExcelPassword(ExcelPasswordConstants.ExcelPasswordForModifying);
					}
					stmDataForExcelPasswordModifying.SD_BinaryValue = Encoding.UTF8.GetBytes(Encoder.Encrypt(excelPasswordForModifying));
				}
			}
		}

		string excelPasswordForModifying;

		StmDataForExcelPassword CreateNewStmDataForExcelPassword(string passwordType)
		{
			var stmDataForExcelPassword = Parent.Factory.New<StmDataForExcelPassword>();
			stmDataForExcelPassword.SD_Name = passwordType;
			stmDataForExcelPassword.SD_Owner = Parent.PK;
			stmDataForExcelPassword.logAction = AddExcelPasswordChangedEventLog;

			return stmDataForExcelPassword;
		}

		public void DeleteStmDataIfExcelPasswordIsEmpty()
		{
			if (stmDataForExcelPasswordOpening != null && ExcelPasswordForOpening.IsEmpty)
			{
				stmDataForExcelPasswordOpening.Delete();
				stmDataForExcelPasswordOpening = null;
			}

			if (stmDataForExcelPasswordModifying != null && ExcelPasswordForModifying.IsEmpty)
			{
				stmDataForExcelPasswordModifying.Delete();
				stmDataForExcelPasswordModifying = null;
			}
		}

		#region excel password changed event

		void AddExcelPasswordChangedEventLog(ZString passwordType, ZString changeType)
		{
			if (Parent is OrgContact orgcontact)
			{
				orgcontact.Logs.AddNew(AutoEvents.ExcelPasswordChanged, $"Org Contact = {orgcontact.PK} {orgcontact.OC_ContactName} Excel {passwordType} Password was {changeType}, user = {Env.CurrentUser.PK}", ZDateTimeOffset.Now);
			}
		}

		#endregion

		#endregion
	}
}
