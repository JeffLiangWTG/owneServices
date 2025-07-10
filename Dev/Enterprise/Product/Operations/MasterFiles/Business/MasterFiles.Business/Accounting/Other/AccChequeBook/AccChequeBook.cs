using System;
using System.Data;
using CargoWise.BrandManager;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoAccChequeBook.Schema.AK_Desc)]
	public class AccChequeBook : AutoAccChequeBook, IDocManagerSupport, IDataVersionLoggingSupported, IEDocsParsingSupport
	{
		public AccChequeBook(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static class CalculatedFieldNames
		{
			public const string AK_Calc_CurrentNoString = "AK_Calc_CurrentNoString";
			public const string AK_Calc_StartNoString = "AK_Calc_StartNoString";
			public const string AK_Calc_LastNoString = "AK_Calc_LastNoString";
		}

		#region Property Override

		#region AK_AB
		[List("AK_ABList")]
		public override ZGuid AK_AB
		{
			get { return base.AK_AB; }
			set
			{
				if (value != AK_AB)
				{
					base.AK_AB = value;
					aK_AB_ReadOnly = null;
					if (BankAccount != null && !BankAccount.AB_GB.IsEmpty)
					{
						AK_GB = BankAccount.AB_GB;
					}
				}
			}
		}

		protected bool AK_AB_ReadOnly
		{
			get
			{
				if (aK_AB_ReadOnly == null)
				{
					aK_AB_ReadOnly = IsReferencedByJobCharge() || IsReferencedByAccHotCheque();
				}
				return aK_AB_ReadOnly.Value;
			}
		}

		bool? aK_AB_ReadOnly;

		#endregion

		#region AK_GB
		[List("AK_GBList")]
		public override ZGuid AK_GB
		{
			get
			{
				return base.AK_GB;
			}
		}

		protected bool AK_GB_ReadOnly
		{
			get { return (BankAccount != null && !BankAccount.AB_GB.IsEmpty); }
		}

		#endregion

		#region AK_StartNo

		public override ZDecimal AK_StartNo
		{
			get { return base.AK_StartNo; }
			set
			{
				base.AK_StartNo = value;
				if (!AK_StartNoInfo.HasErrors())
				{
					AK_CurrentNo = AK_StartNo;
				}
			}
		}

		#endregion

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("76c3079d-c52a-4269-aac5-d888a30a655f", "Check Book");
			}
		}

		#endregion

		#region AK_AutoPrintCheque

		public override ZBool AK_AutoPrintCheque
		{
			get
			{
				return base.AK_AutoPrintCheque;
			}
			set
			{
				base.AK_AutoPrintCheque = value;
				if (!value)
				{
					AK_SQ = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region AK_SQ
		[List("Lookups.Printers")]
		public override ZGuid AK_SQ
		{
			get
			{
				return base.AK_SQ;
			}
			set
			{
				base.AK_SQ = value;
			}
		}
		#endregion

		#region AK_SQInfo

		protected bool AK_SQ_ReadOnly
		{
			get
			{
				return !AK_AutoPrintCheque;
			}
		}

		#endregion

		#endregion

		#region Calculated Fields

		[BusinessObjectTestExclude]
		public ZString AK_Calc_CurrentNoString
		{
			get { return Validation.ValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, AK_CurrentNo.ToString()); }
			set
			{
				ZDecimal noParsed;
				ZDecimal.TryParse(value, out noParsed);
				AK_CurrentNo = noParsed;
			}
		}

		public ZPropertyInfo AK_Calc_CurrentNoStringInfo
		{
			get { return GetWrappedZPropertyInfo(CalculatedFieldNames.AK_Calc_CurrentNoString, x => AK_CurrentNoInfo); }
		}

		[BusinessObjectTestExclude]
		public ZString AK_Calc_StartNoString
		{
			get { return Validation.ValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, AK_StartNo.ToString()); }
			set
			{
				ZDecimal noParsed;
				ZDecimal.TryParse(value, out noParsed);
				AK_StartNo = noParsed;
			}
		}

		public ZPropertyInfo AK_Calc_StartNoStringInfo
		{
			get { return GetWrappedZPropertyInfo(CalculatedFieldNames.AK_Calc_StartNoString, x => AK_StartNoInfo); }
		}

		[BusinessObjectTestExclude]
		public ZString AK_Calc_LastNoString
		{
			get { return Validation.ValidationHelper.PadChequeDigitsWithLeadingZeros(BankAccount, AK_LastNo.ToString()); }
			set
			{
				ZDecimal noParsed;
				ZDecimal.TryParse(value, out noParsed);
				AK_LastNo = noParsed;
			}
		}

		public ZPropertyInfo AK_Calc_LastNoStringInfo
		{
			get { return GetWrappedZPropertyInfo(CalculatedFieldNames.AK_Calc_LastNoString, x => AK_LastNoInfo); }
		}

		public ZBool IsAutoPrint
		{
			get
			{
				if (AK_AutoPrintCheque)
				{
					if (BankAccount.ChequeTemplate != null)
					{
						return ZBool.True;
					}
				}
				return ZBool.False;
			}
		}

		#endregion

		#region Lists

		#region Bank Account List

		public AccBankAccountCollection AK_ABList
		{
			get
			{
				if (fBankAccountList == null)
				{
					ZGuid filterGuid = Env.CurrentCompany.PK;
					if (filterGuid.IsValid)
					{
						ZQuery filter = new ZQuery(AccBankAccountSchema.AB_GC, SQLComparisonOperator.Equal, filterGuid);
						fBankAccountList = new AccBankAccountCollection(Factory, filter);
					}
					else
					{
						fBankAccountList = new AccBankAccountCollection(Factory);
					}
				}
				return fBankAccountList;
			}
		}

		#endregion

		#region GlbBranch List

		public GlbBranchCollection AK_GBList
		{
			get
			{
				if (fGlbBranchList == null)
				{
					ZGuid filterGuid = Env.CurrentCompany.PK;
					if (filterGuid.IsValid)
					{
						ZQuery branchesFilter = new ZQuery(GlbBranchSchema.GB_GC, filterGuid);
						fGlbBranchList = new GlbBranchCollection(Factory, branchesFilter);
					}
					else
					{
						fGlbBranchList = new GlbBranchCollection(Factory);
					}
				}

				return fGlbBranchList;
			}
		}

		#endregion

		#endregion

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged
		{
			get { return AccountingMasterFilesRegistry.Instance.CheckBookDataVersionAutoLogging.Value; }
		}

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region Implementation

		protected AccBankAccountCollection fBankAccountList;
		protected GlbBranchCollection fGlbBranchList;
		#region IsReferencedByJobCharge

		public bool IsReferencedByJobCharge()
		{
			ZQuery filter = new ZQuery(JobChargeSchema.JR_AK, PK);
			JobCharge charge = (JobCharge)Factory.LoadTop1(typeof(JobCharge), filter);
			return charge != null;
		}

		#endregion

		#region IsReferencedByAccHotCheque

		public bool IsReferencedByAccHotCheque()
		{
			var query = new ZQuery(AccHotChequeSchema.AQ_AK, PK);
			return Factory.ExistsInDatabase(AccHotChequeSchema.Constants.TableName, query);
		}

		#endregion

		#region GetReferencedCompany

		// gets the company referenced by this cheque book
		public GlbCompany GetReferencedCompany()
		{
			ZQuery filter = new ZQuery(AccBankAccountSchema.PK, AK_AB);
			AccBankAccount account = (AccBankAccount)Factory.LoadTop1(typeof(AccBankAccount), filter);
			if (account != null)
			{
				if (account.AB_GC.IsValid)
				{
					ZQuery filter2 = new ZQuery(GlbCompanySchema.PK, account.AB_GC);
					GlbCompany company = (GlbCompany)Factory.LoadTop1(typeof(GlbCompany), filter2);

					if (company != null)
					{
						return company;
					}
				}
			}
			return null;
		}

		#endregion

		#region StartNoOverlapsExistingRanges

		public bool StartNoOverlapsExistingRanges()
		{
			ZQuery filter = new ZQuery(AccChequeBookSchema.PK, SQLComparisonOperator.NotEqual, PK);
			filter.AddToFilter(JoinCondition.And, AccChequeBookSchema.AK_AB, SQLComparisonOperator.Equal, AK_AB);
			BusinessObject[] chequeBooks = Factory.Load(typeof(AccChequeBook), filter);

			bool isEvenNumberOfEndPointsLessThanCurrent = true;
			for (int i = 0; i < chequeBooks.Length; i++)
			{
				AccChequeBook chequeBook = chequeBooks[i] as AccChequeBook;
				if (chequeBook.AK_StartNo <= AK_StartNo)
				{
					isEvenNumberOfEndPointsLessThanCurrent = !isEvenNumberOfEndPointsLessThanCurrent;
				}
				if (chequeBook.AK_LastNo < AK_StartNo)
				{
					isEvenNumberOfEndPointsLessThanCurrent = !isEvenNumberOfEndPointsLessThanCurrent;
				}
			}
			return !isEvenNumberOfEndPointsLessThanCurrent;
		}
		#endregion

		#region LastNoOverlapsExistingRanges

		public bool LastNoOverlapsExistingRanges()
		{
			ZQuery filter = new ZQuery(AccChequeBookSchema.PK, SQLComparisonOperator.NotEqual, PK);
			filter.AddToFilter(JoinCondition.And, AccChequeBookSchema.AK_AB, SQLComparisonOperator.Equal, AK_AB);
			BusinessObject[] chequeBooks = Factory.Load(typeof(AccChequeBook), filter);

			bool isEvenNumberOfEndPointsLessThanCurrent = false; // we include our own start point bringing the total to 1
			for (int i = 0; i < chequeBooks.Length; i++)
			{
				AccChequeBook chequeBook = chequeBooks[i] as AccChequeBook;
				if (chequeBook.AK_StartNo >= AK_StartNo && chequeBook.AK_LastNo <= AK_LastNo)
				{
					if (!StartNoOverlapsExistingRanges())
					{
						return true;
					}
				}

				if (chequeBook.AK_StartNo <= AK_LastNo)
				{
					isEvenNumberOfEndPointsLessThanCurrent = !isEvenNumberOfEndPointsLessThanCurrent;
				}
				if (chequeBook.AK_LastNo < AK_LastNo)
				{
					isEvenNumberOfEndPointsLessThanCurrent = !isEvenNumberOfEndPointsLessThanCurrent;
				}
			}

			return isEvenNumberOfEndPointsLessThanCurrent;
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (AK_GB.IsEmpty)
			{
				AK_GB = GlbBranch.CurrentBranch.PK; //do this before base call to avoid creating new GlbCompany
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		#endregion

		#region Utility Methods

		public bool IsChequeInBook(ZDecimal chequeNumber)
		{
			return chequeNumber >= AK_StartNo && chequeNumber <= AK_LastNo;
		}

		public bool HasChequeBeenPosted(ZDecimal chequeNumber)
		{
			ZQuery mainFilter = new ZQuery(AccTransactionHeaderSchema.AH_AB, SQLComparisonOperator.Equal, AK_AB);
			mainFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.Equal, chequeNumber.ToString());
			mainFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_ReceiptType, SQLComparisonOperator.Equal, ZArchitecture.Core.ReceiptTypes.Cheque);
			mainFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);
			mainFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.Equal, Branch.GB_GC);

			ZQuery transactionTypeFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.DirectPayment);
			transactionTypeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Payment);
			mainFilter.AddToFilter(transactionTypeFilter);

			return Factory.Load(typeof(AccTransactionHeader), mainFilter).Length > 0;
		}

		public static void UpdateCurrentNumber(ZGuid pK, ZDecimal chequeNumber)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccChequeBook newChequeBook = newFactory.Load<AccChequeBook>(pK);
			if (newChequeBook != null && chequeNumber.IsInteger && !newChequeBook.HasChequeBeenPosted(chequeNumber) && newChequeBook.IsChequeInBook(chequeNumber))
			{
				newChequeBook.AK_CurrentNo = chequeNumber;
				newFactory.Save();
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.ChequeBook);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region  WarningSamePrinter

		public string MessageIfChequeBookUsesSamePrinter()
		{
			string message = string.Empty;
			Validation.ValidateAK_SQ();
			foreach (INotification warning in AK_SQInfo.Notifications.GetWarnings())
			{
				if (warning.Message.StartsWith(AccChequeBook.WarningSamePrinterMessageStart))
				{
					BusinessObject chequeBookPrinter = (BusinessObject)Factory.Load<Enterprise.Integration.DocumentEngine.IStmPrintQueue>(AK_SQ);
					message = AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(((ZString)chequeBookPrinter[StmPrintQueueSchema.SQ_QueueName]), AK_CurrentNo);
				}
			}
			return message;
		}

		public void AddWarningSamePrinter(ZPropertyInfo info)
		{
			string showwarning = "";
			Validation.ValidateAK_SQ();
			foreach (INotification warning in this.AK_SQInfo.Notifications.GetWarnings())
			{
				if (warning.Message.StartsWith(AccChequeBook.WarningSamePrinterMessageStart))
				{
					showwarning = warning.Message.Replace(AccChequeBook.WarningSamePrinterMessageStart, AccChequeBook.WarningChequeBookWithSamePrinterMessageStart);
				}
			}
			if (!string.IsNullOrEmpty(showwarning))
			{
				info.AddWarning(showwarning);
			}
		}
		public static string WarningSamePrinterMessageCaption
		{
			get { return Res.GetString("106058b5-12ee-49a5-b913-8babba48135c", "Check book uses printer assigned with another check book"); }
		}

		public static string WarningSamePrinterMessageStart
		{
			get { return Res.GetString("07750698-8f74-4595-8730-c14fa251de2f", @"This print queue is in use by the following check books:") + "\r\n"; }
		}
		public static string WarningSamePrinterMessageEnd
		{
			get { return "\r\n" + Res.GetString("ec130fb7-6d0b-4384-800f-055a2936cb69", @"{0} does not support automated check printing when one printer is used for multiple checks.
When posting payments for check books using the automatic print function, {0} controls the allocation of check numbers at the point of posting to ensure that check numbers are always allocated AND printed in the correct sequence. In this case, {0} does not allow users to enter a check number manually.
If you do elect to use a single printer for multiple check books, users will be required to manually control the process of ensuring the following BEFORE posting payments:

	1.	That no other user should post check payments using this or the above check books
	2.	That the check paper in the printer is for the correct check book
	3.	That the check number of the check paper in the printer is the same as the 'Current No.' of the selected check book (press F3 on the payment's check book field to inspect this value)", BrandingFactory.Instance.ProductName) + "\r\n"; }
		}
		public static string WarningChequeBookWithSamePrinterMessageStart
		{
			get { return Res.GetString("5b78bc8c-9976-4927-835d-a6d6525c2743", @"This check book uses a print queue that is in use by the following check books:") + "\r\n"; }
		}
		public static string WarningPaymentWithChequeBookWithSamePrinterMessage(ZString printerQueueName, ZDecimal chequeBookNumber)
		{
			return Res.GetString("7ac88ddb-8c8e-4687-a175-fa57b193f012", @"This payment has been entered using a check book that uses the following print queue:
{0:G}

This print queue is also used by other check books.
{1} does not support automated check printing when one printer is used for multiple check books.
If you press 'OK' to continue, {1} will allocate the check number to the next check number available.
Before pressing 'OK' to continue, you will need to ensure the following:

	1.	That no other user should post check payments for check that use this print queue
	2.	That the check paper in the printer is for the correct check book
	3.	That the check number of the check paper in the printer is the same as the 'Current No.' of the selected check book.

The 'Current' check number for the selected check book is currently '{2}'.
If multiple users post checks using this check book, this check number may already be allocated by the time you press 'OK' to continue.", printerQueueName, BrandingFactory.Instance.ProductName, chequeBookNumber) + "\r\n";
		}

		#endregion
	}
}
