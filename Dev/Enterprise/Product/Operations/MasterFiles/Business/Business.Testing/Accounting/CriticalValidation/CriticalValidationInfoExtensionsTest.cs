using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static System.FormattableString;
using static Enterprise.MasterFiles.Business.Testing.AccTransactionMatchLinkCriticalValidationTest;

namespace Enterprise.MasterFiles.Business.Accounting.CriticalValidation.Testing
{
	sealed class CriticalValidationInfoExtensionsTest : TestCaseWithFactory
	{
		#region TestLoadRelatedJobCharge

		public void TestLoadRelatedJobCharge_WIP()
		{
			AssertLoadRelatedJobCharge(TransactionLineTypes.WIP, apSide: false);
		}

		public void TestLoadRelatedJobCharge_ACR()
		{
			AssertLoadRelatedJobCharge(TransactionLineTypes.Accrual, apSide: true);
		}

		public void TestLoadRelatedJobCharge_CST()
		{
			AssertLoadRelatedJobCharge(TransactionLineTypes.Cost, apSide: false);
			AssertLoadRelatedJobCharge(TransactionLineTypes.Cost, apSide: true);
		}

		public void TestLoadRelatedJobCharge_REV()
		{
			AssertLoadRelatedJobCharge(TransactionLineTypes.Revenue, apSide: false);
			AssertLoadRelatedJobCharge(TransactionLineTypes.Revenue, apSide: true);
		}

		void AssertLoadRelatedJobCharge(string lineType, bool apSide)
		{
			AccTransactionLines line = null;
			AssertNull(line.LoadRelatedJobCharge());

			line = Factory.New<AccTransactionLines>();
			line.AL_LineType = lineType;

			var charge = Factory.New<JobCharge>();
			if (apSide)
			{
				charge.JR_AL_APLine = line.PK;
			}
			else
			{
				charge.JR_AL_ARLine = line.PK;
			}
			charge.JR_JH = ZGuid.NewZGuid();
			AssertNull(line.LoadRelatedJobCharge());

			line.AL_JH = charge.JR_JH;
			AssertEquals(charge, line.LoadRelatedJobCharge());
		}

		#endregion

		[TestDate(1971, 9, 18)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestGetTransactionHeaderInfo()
		{
			ZDateTime invoiceDate = new ZDateTime(2009, 11, 20);
			AccTransactionHeader invoice = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), new Guid("E66A7FED-B46B-4322-BDF3-616D341CF2ED"));
			invoice.FillWithValidTestData();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_InvoiceDate = invoiceDate;
			invoice.AH_PostDate = invoiceDate.AddDays(10);
			invoice.AH_TransactionNum = "123";
			invoice.AH_OSTotal = -200M;
			invoice.AH_InvoiceAmount = -100M;
			invoice.AH_OutstandingAmount = -100M;
			invoice.AH_ExchangeRate = 0.7M;
			invoice.AH_RX_NKTransactionCurrency = "NZD";
			invoice.AH_OH = Factory.New<OrgHeader>().PK;
			invoice.Header.FillWithValidTestData();
			invoice.Header.OH_Code = "ORG";

			string expectedInfo = string.Format("Header: PK = {0}, Ledger = AR, Transaction Type = INV, Invoice Date = 20-Nov-09 00:00:00, Post Date = 30-Nov-09 00:00:00, Invoice Amount = -100, GST Amount = 0, OS Total = -200, Exchange Rate = 0.7, Currency = NZD, Outstanding Amount = -100, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 123, Job PK = 00000000-0000-0000-0000-000000000000, Organization = ORG, Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.", invoice.PK);
			AssertEquals("Info should be as expected.", expectedInfo, invoice.GetTransactionHeaderInfo());

			Factory.Save();
			invoice.AH_TransactionType = TransactionTypes.CreditNote;
			invoice.AH_TransactionNum = "CRD125";
			invoice.AH_InvoiceAmount = -100.01M;
			invoice.AH_OutstandingAmount = -99M;
			expectedInfo = string.Format(@"Header: PK = {0}, Ledger = AR, Transaction Type = CRD, Invoice Date = 20-Nov-09 00:00:00, Post Date = 30-Nov-09 00:00:00, Invoice Amount = -100.01, GST Amount = 0, OS Total = -200, Exchange Rate = 0.7, Currency = NZD, Outstanding Amount = -99, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = CRD125, Job PK = 00000000-0000-0000-0000-000000000000, Organization = ORG, Is In DB = Yes, Is Deleted = No, Has Changes = Yes, Business Contexts = None, System Create Time = " + ZDateTime.BrettsBirthday.ToAUString() + @", System Create User = E, System Last Edit Time = " + ZDateTime.BrettsBirthday.ToAUString() + @", System Last Edit User = E.
	Fields with changes: AH_InvoiceAmount (-100, -100.01), AH_OutstandingAmount (-100, -99), AH_TransactionNum (123, CRD125), AH_TransactionType (INV, CRD).", invoice.PK);
			AssertEquals("Info for saved object should be with changed fields.", expectedInfo, invoice.GetTransactionHeaderInfo());

			invoice.AH_SystemCreateTimeUtc = ZDateTime.Empty;
			invoice.AH_SystemCreateUser = ZString.Empty;
			invoice.AH_SystemLastEditTimeUtc = ZDateTime.Empty;
			invoice.AH_SystemLastEditUser = ZString.Empty;
			expectedInfo = string.Format(@"Header: PK = {0}, Ledger = AR, Transaction Type = CRD, Invoice Date = 20-Nov-09 00:00:00, Post Date = 30-Nov-09 00:00:00, Invoice Amount = -100.01, GST Amount = 0, OS Total = -200, Exchange Rate = 0.7, Currency = NZD, Outstanding Amount = -99, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = CRD125, Job PK = 00000000-0000-0000-0000-000000000000, Organization = ORG, Is In DB = Yes, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
	Fields with changes: AH_InvoiceAmount (-100, -100.01), AH_OutstandingAmount (-100, -99), AH_SystemCreateTimeUtc (18-Sep-71 00:00:00, ), AH_SystemCreateUser (E, ), AH_SystemLastEditTimeUtc (18-Sep-71 00:00:00, ), AH_SystemLastEditUser (E, ), AH_TransactionNum (123, CRD125), AH_TransactionType (INV, CRD).", invoice.PK);
			AssertEquals("If created/last-edit info is blank, do not show it", expectedInfo, invoice.GetTransactionHeaderInfo());

			var invoiceNotInDb = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), new Guid("74596F9D-9837-4B1C-BF64-5475D739E4AD"));
			invoice.FillWithValidTestData();
			invoice.AH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			expectedInfo = string.Format(@"Header: PK = {0}, Ledger = , Transaction Type = , Invoice Date = , Post Date = , Invoice Amount = 0, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = , Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = No, Business Contexts = None.", invoiceNotInDb.PK);
			AssertEquals("If created/last-edit info exist but not yet committed to Db, do not show it", expectedInfo, invoiceNotInDb.GetTransactionHeaderInfo());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestGetTransactionLineInfo()
		{
			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.FillWithValidTestData();
			ZDateTime invoiceDate = new ZDateTime(2009, 11, 20);
			line.AL_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			line.ChargeCode.AC_Code = "QWE";
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_PostDate = invoiceDate;
			line.AL_ReverseDate = invoiceDate.AddDays(10);
			line.AL_OSAmount = -200.01M;
			line.AL_LineAmount = -100M;
			line.AL_GSTVAT = -10M;
			line.AL_ExchangeRate = 0.7M;
			line.AL_RX_NKTransactionCurrency = "USD";
			line.AL_PostToGL = "D";
			line.AL_ReverseToGL = "B";
			line.AL_OH = Factory.New<OrgHeader>().PK;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.Header.FillWithValidTestData();
			line.Header.OH_Code = "ORG";
			line.AL_IsFinalCharge = true;
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job.FillWithValidTestData();
			line.AL_JH = job.PK;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			line.GLHeader.AG_AccountNum = "1.1.1.1";
			AccTransactionHeader header = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), new Guid("4521d20a-5a64-4f7e-8601-a9aff454c3e5"));
			header.FillWithValidTestData();
			line.AL_AH = header.PK;
			var expectedInfo = $"Line: PK = {line.PK}, Charge Code = QWE, GL Account = 1.1.1.1, Type = WIP, OS Amount = -200.01, Local Amount = -100, GST = -10, Tax Rate = , Tax Class = , Exchange Rate = 0.7, Currency = USD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 30-Nov-09 00:00:00, Post To GL = D, Reverse To GL = B, Header PK = 4521d20a-5a64-4f7e-8601-a9aff454c3e5, Job PK = 084172b9-3656-41d5-b18c-1a73179a8f9b, Organization = ORG, Revenue Recognition Type = IMM, Is In DB = No, Is Final = Yes, Sub Accounts = , Has Changes = Yes.";

			AssertEquals("Info should be as expected.", expectedInfo, line.GetTransactionLineInfo());

			Factory.Save();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_LineAmount = -99M;
			line.AL_GSTVAT = -9.9M;
			expectedInfo = $@"Line: PK = {line.PK}, Charge Code = QWE, GL Account = 1.1.1.1, Type = CST, OS Amount = -200.01, Local Amount = -99, GST = -9.9, Tax Rate = , Tax Class = , Exchange Rate = 0.7, Currency = USD, Post Date = 20-Nov-09 00:00:00, Reverse Date = 30-Nov-09 00:00:00, Post To GL = D, Reverse To GL = B, Header PK = 4521d20a-5a64-4f7e-8601-a9aff454c3e5, Job PK = 084172b9-3656-41d5-b18c-1a73179a8f9b, Organization = ORG, Revenue Recognition Type = IMM, Is In DB = Yes, Is Final = Yes, Sub Accounts = , Has Changes = Yes.
	Fields with changes: AL_GSTVAT (-10, -9.9), AL_LineAmount (-100, -99), AL_LineType (WIP, CST).";

			AssertEquals("Info for saved object should be with changed fields.", expectedInfo, line.GetTransactionLineInfo());
		}

		[TestDate(2016, 12, 12)]
		public void TestRevenueRecognitionInfo()
		{
			foreach (bool createD3Record in new bool[] { false, true })
			{
				var line = Factory.NewWithValidTestData<AccTransactionLines>();
				line.FillWithValidTestData();

				var job = Factory.NewJobForTesting<JobHeader>();
				job.FillWithValidTestData();
				line.AL_JH = job.PK;

				if (createD3Record)
				{
					var methodInfo = line.Job.GetType().GetMethod("GetOrCreateJobChargeRevRecognition", BindingFlags.NonPublic | BindingFlags.Instance);
					methodInfo.Invoke(line.Job, new object[] { new ZString("JCL"), ZDateTime.Today });
				}

				var expectedRevRecognitionMessage = @"Revenue Recognition info:

";
				if (createD3Record)
				{
					var methodInfo = line.Job.GetType().GetProperty("RevenueRecognitionCollection", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
					var bizOs = (methodInfo.GetValue(line.Job, null) as IEnumerable).Cast<BusinessObject>().ToArray();
					var pk = new ZGuid(bizOs[0][JobChargeRevRecognitionSchema.PK]);
					expectedRevRecognitionMessage += string.Format("PK: {0}, D3_JH: {1}, D3_RecognitionType : JCL, D3_RecognitionDate : 12-Dec-16 00:00:00, Is In DB = No, Has Changes = Yes", pk, line.Job.PK);
				}
				else
				{
					expectedRevRecognitionMessage += "No D3 record is created for this Job";
				}

				AssertEquals("Revenue Recognition Info Text", expectedRevRecognitionMessage, line.Job.GetJobRevenueRecognitionInfo());
			}
		}

		#region TestGetAllPropertyValuesForDelete
		class DummyBuinessObjectForDelete : DummyBusinessObject
		{
			public DummyBuinessObjectForDelete(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString TemporaryPropertyForDelete
			{
				get
				{
					if (IsDeleted)
					{
						throw new DeletedRowInaccessibleException();
					}
					else
					{
						return temporaryPropertyForDelete;
					}
				}
				set
				{
					temporaryPropertyForDelete = value;
				}
			}
			ZString temporaryPropertyForDelete;

			public virtual ZPropertyInfo TemporaryPropertyForDeleteInfo
			{
				get { return GetZPropertyInfo(nameof(TemporaryPropertyForDelete)); }
			}
		}

		public void TestGetAllPropertyValuesForDelete()
		{
			var bizoForDelete = Factory.NewWithValidTestData<DummyBuinessObjectForDelete>();
			bizoForDelete.Z0_Description = "DefaultValueForZ0_Description";
			bizoForDelete.TemporaryPropertyForDelete = "ValueForTemporaryProperty";
			Factory.Save();
			var propertyValues = bizoForDelete.GetAllPropertyValues();
			AssertContains("Should contain generic info",
$@"	PK = {bizoForDelete.PK}
	Type = DummyBuinessObjectForDelete
	Types around row = DummyBuinessObjectForDelete
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None", propertyValues);
			AssertContains("should contains temporary property", "TemporaryPropertyForDelete = ValueForTemporaryProperty", propertyValues);

			bizoForDelete.Delete();
			bizoForDelete.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			AssertNoExceptionThrown("Should not get exception for temporary property", () =>
			{
				propertyValues = bizoForDelete.GetAllPropertyValues();
			});

			AssertContains("Should contain generic info",
$@"	PK = {bizoForDelete.PK}
	Type = DummyBuinessObjectForDelete
	Types around row = DummyBuinessObjectForDelete
	Factory Instance = {Factory._Instance}
	IsDeleted = True
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateDueToAnyChange)", propertyValues);
			AssertContains("should contains Persistent property's original value", "Z0_Description = DefaultValueForZ0_Description", propertyValues);
			AssertNotContains("should not contains temporary property", "TemporaryPropertyForDelete", propertyValues);
		}
		#endregion

		#region TestGetAllPropertyValuesWithAdditionalPropertyValues

		class DummyBuinessObjectForAdditionalProperty : DummyBusinessObject, IAdditionalPropertyValuesProvider
		{
			public DummyBuinessObjectForAdditionalProperty(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZString IAdditionalPropertyValuesProvider.GetAdditionalProperyValues()
			{
				return "DummyAdditionalProperty = Test";
			}
		}

		public void TestGetAllPropertyValuesWithAdditionalPropertyValues()
		{
			var bizoForAdditionalProperty = Factory.NewWithValidTestData<DummyBuinessObjectForAdditionalProperty>();
			Factory.Save();

			bizoForAdditionalProperty.Z0_Description = "Changed Description";
			var propertyValues = bizoForAdditionalProperty.GetAllPropertyValues();

			var genericInfo = $@"PK = {bizoForAdditionalProperty.PK}
	Type = DummyBuinessObjectForAdditionalProperty
	Types around row = DummyBuinessObjectForAdditionalProperty
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None
";
			var propertyInfo = "Properties:";
			var additionalPropertyInfo = "DummyAdditionalProperty = Test";
			var changesPropertyInfo = "Fields with changes:";

			AssertContainsInOrder("Should contain messages in order", propertyValues, genericInfo, propertyInfo, additionalPropertyInfo, changesPropertyInfo);
		}

		#endregion

		public void TestGetAllPropertyValues()
		{
			ZDateTime invoiceDate = new ZDateTime(2009, 11, 20);
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			charge.JR_AC = Factory.New<AccChargeCode>().PK;
			charge.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			var propertyValues = charge.GetAllPropertyValues();
			AssertContains("Should contain generic info",
$@"	PK = {charge.PK}
	Type = Charge
	Types around row = Charge
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = True
	IsDeleting = False

Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateDueToAnyChange)

"
				, propertyValues);
			AssertContains("Should contain info about data refresh update",
@"

Setting SkipDataRefreshBusUpdateDueToAnyChange Charge Fields With changes info = 
DataRefreshBusUpdateSkipped: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.

Properties:
", propertyValues);
			AssertContains("Should contain PK and Code",
@"
 JR_GB = 27a55065-ac88-4ec3-8bed-e575e79172cb (BNE)
 JR_GB_CostTaxBranch = 00000000-0000-0000-0000-000000000000
 JR_GB_InternalBranch = 00000000-0000-0000-0000-000000000000
 JR_GB_SellTaxBranch = 00000000-0000-0000-0000-000000000000
 JR_GC = 878d7aca-ffc3-49fc-9710-969ca0c0f2ac (EDI)
 JR_GE = 86bb1c22-0865-4685-996e-d56cbd136491 (BRN)", propertyValues);
			AssertContains("Should contain fields with changes",
@"

	Fields with changes: JR_AC", propertyValues);
		}

		[DisableZeroExchangeRateOverriding]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestGetJobChargeInfo()
		{
			ZDateTime invoiceDate = new ZDateTime(2009, 11, 20);
			JobCharge charge = (JobCharge)Factory.New(typeof(JobCharge), new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			charge.FillWithValidTestData();
			charge.JR_JH = new ZGuid("6bdda203-cc3a-4f9d-b994-035356f5c606");
			charge.JR_AC = Factory.New<AccChargeCode>().PK;
			charge.ChargeCode.AC_Code = "QWE";
			charge.ChargeCode.AC_ChargeType = "MRG";
			charge.JR_OrderReference = "REF1";
			charge.JR_ProductQuantity = 77M;
			charge.JR_RX_NKCostCurrency = "EUR";
			charge.JR_OSCostExRate = 0.5M;
			charge.JR_OSCostAmt = 100M;
			charge.JR_OSCostGSTAmt = 10M;
			charge.JR_OSCostWHTAmt = 5M;
			charge.JR_APInvoiceNum = "1234";
			charge.JR_APInvoiceDate = invoiceDate;
			charge.JR_PaymentDate = invoiceDate.AddDays(10);
			charge.JR_PaymentType = "EFT";
			charge.JR_AK = new ZGuid("f40f5010-047c-40ff-8943-818fcbb52307");
			charge.JR_AB = new ZGuid("c412ebc4-b819-4f86-9c14-3f207db3a4dd");
			charge.JR_ChequeNo = "789";
			charge.JR_AL_APLine = Factory.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c")).PK;
			charge.JR_OH_SellAccount = Factory.New<OrgHeader>().PK;
			charge.SellAccount.OH_Code = "SELLORG";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_InvoiceType = "FIN";
			charge.JR_OSSellAmt = 150M;
			charge.JR_OSSellExRate = 0.3333M;
			charge.JR_OSSellWHTAmt = 7.5M;
			charge.JR_E6 = new ZGuid("86d97e38-4bd1-47a5-82f0-ca84d95f02e1");
			charge.JR_AL_ARLine = Factory.New(typeof(AccTransactionLines), new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f")).PK;
			charge.ARLine.AL_LineType = TransactionLineTypes.Revenue;
			charge.ARLine.AL_RX_NKTransactionCurrency = charge.JR_RX_NKSellCurrency;
			charge.ARLine.AL_OSAmount = 165;
			charge.ARLine.AL_GSTVAT = 15;
			charge.JR_AL_CFXLine = new ZGuid("d239fd7f-7f22-4ff5-96bd-560cf1a1c47b");
			charge.JR_E6_GatewaySellHeader = new ZGuid("d239ef0c-08e5-4667-986c-dc0c50a784b3");
			string expectedInfo = "Charge: PK = 3dca8b18-7a6e-4d8e-92ac-2a1605633721, Type = Charge, Charge Type = , Job PK = 6bdda203-cc3a-4f9d-b994-035356f5c606, Job Number = , Charge Code = QWE, Charge Code Type = MRG, Cost Account = , OS Cost Amount = 100, Local Cost Amount = 200, OS Cost Exchange Rate = 0.5, Cost GST is Overridden = No, OS Cost GST Amount = 10, OS Cost WHT Amount = 5, AP Invoice # = 1234, AP Invoice Date = 20-Nov-09 00:00:00, Supplier Cost Reference = , Payment Date = 30-Nov-09 00:00:00, Payment Type = EFT, Cheque # = 789, Cheque Book = f40f5010-047c-40ff-8943-818fcbb52307, Cheque Book Code = , Bank Account = c412ebc4-b819-4f86-9c14-3f207db3a4dd, Is Cost Posted = No, AP Line = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Sell Account = SELLORG, OS Sell Exchange Rate = 0.3333, OS Sell Amount = 150, Local Sell Amount = 450.05, OS Sell GST Amount = 15, OS Sell WHT Amount = 7.5, Is Revenue Posted = Yes, AR Line = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, CFX Line = d239fd7f-7f22-4ff5-96bd-560cf1a1c47b, Invoice Type = FIN, Order Reference = REF1, OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 77, Consol Cost = 86d97e38-4bd1-47a5-82f0-ca84d95f02e1, Gateway Sell Header = d239ef0c-08e5-4667-986c-dc0c50a784b3, Cost Currency = EUR, Sell Currency = USD, Is In DB = No, Has Changes = Yes, Is Saved By Factory = No, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 450.05, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.";

			AssertEquals("Info should be as expected", expectedInfo, charge.GetJobChargeInfo());

			charge = (JobCharge)new BusinessObjectFactory().New(typeof(JobCharge), new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			charge.FillWithValidTestData();
			charge.ChargeCode.AC_ChargeType = "MRG";
			charge.JR_OSCostAmt = 100M;
			charge.JR_LocalCostAmt = 100M;
			charge.JR_OSSellWHTAmt = 7.5M;
			AccChequeBook chequeBook = (AccChequeBook)charge.Factory.New(typeof(AccChequeBook), new Guid("f40f5010-047c-40ff-8943-818fcbb52307"));
			chequeBook.FillWithValidTestData();
			charge.JR_AK = chequeBook.PK;
			charge.Factory.Save();
			charge.JR_LocalCostAmt = 22M;
			charge.JR_OSCostAmt = 22M;
			charge.JR_OSSellWHTAmt = 7.01M;
			charge.JR_AK = new ZGuid("6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972");
			expectedInfo = string.Format(
@"Charge: PK = 3dca8b18-7a6e-4d8e-92ac-2a1605633721, Type = Charge, Charge Type = , Job PK = {0}, Job Number = M52G2I7VKQEWLBIZMOPBJVRGSIXLTKEXIZB, Charge Code = 6GQVHOGKXL, Charge Code Type = MRG, Cost Account = , OS Cost Amount = 22, Local Cost Amount = 22, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = {1}, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 100, Local Sell Amount = 100, OS Sell GST Amount = 0, OS Sell WHT Amount = 7.01, Is Revenue Posted = No, AR Line = {2}, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = Yes, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 100, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
	Fields with changes: JR_AgentDeclaredCostAmt (100, 22), JR_AK (f40f5010-047c-40ff-8943-818fcbb52307, 6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972), JR_ChequeNo (0, ), JR_LocalCostAmt (100, 22), JR_OSCostAmt (100, 22), JR_OSSellWHTAmt (7.5, 7.01).",
				charge.JR_JH, charge.JR_AL_APLine, charge.JR_AL_ARLine);
			AssertEquals("Info for saved object should be with changed fields.", expectedInfo, charge.GetJobChargeInfo());

			charge.JR_RX_NKSellInvoiceCurrency = "USD";
			expectedInfo = string.Format(
@"Charge: PK = 3dca8b18-7a6e-4d8e-92ac-2a1605633721, Type = Charge, Charge Type = , Job PK = {0}, Job Number = M52G2I7VKQEWLBIZMOPBJVRGSIXLTKEXIZB, Charge Code = 6GQVHOGKXL, Charge Code Type = MRG, Cost Account = , OS Cost Amount = 22, Local Cost Amount = 22, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = {1}, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 100, Local Sell Amount = 100, OS Sell GST Amount = 0, OS Sell WHT Amount = 7.01, Is Revenue Posted = No, AR Line = {2}, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = Yes, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = USD, OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 100, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = None.
	Fields with changes: JR_AgentDeclaredCostAmt (100, 22), JR_AK (f40f5010-047c-40ff-8943-818fcbb52307, 6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972), JR_ChequeNo (0, ), JR_LocalCostAmt (100, 22), JR_OSCostAmt (100, 22), JR_OSSellWHTAmt (7.5, 7.01), JR_RX_NKSellInvoiceCurrency (, USD).",
				charge.JR_JH, charge.JR_AL_APLine, charge.JR_AL_ARLine);
			AssertEquals("Info for charge wih Sell Invoice Currency.", expectedInfo, charge.GetJobChargeInfo());

			charge.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			expectedInfo = string.Format(
@"Charge: PK = 3dca8b18-7a6e-4d8e-92ac-2a1605633721, Type = Charge, Charge Type = , Job PK = {0}, Job Number = M52G2I7VKQEWLBIZMOPBJVRGSIXLTKEXIZB, Charge Code = 6GQVHOGKXL, Charge Code Type = MRG, Cost Account = , OS Cost Amount = 22, Local Cost Amount = 22, OS Cost Exchange Rate = 1, Cost GST is Overridden = No, OS Cost GST Amount = 0, OS Cost WHT Amount = 0, AP Invoice # = , AP Invoice Date = , Supplier Cost Reference = , Payment Date = , Payment Type = , Cheque # = , Cheque Book = 6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = {1}, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 100, Local Sell Amount = 100, OS Sell GST Amount = 0, OS Sell WHT Amount = 7.01, Is Revenue Posted = No, AR Line = {2}, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = 00000000-0000-0000-0000-000000000000, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = Yes, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = , Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = USD, OS Sell Invoice Amt = 0, Local Sell Invoice Amt = 100, Sell Invoice Ex Rate = 0, CFX Amt = 0, Cost Tax Date = , Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = BizObj Level : (SkipDataRefreshBusUpdateDueToAnyChange).
	Fields with changes: JR_AgentDeclaredCostAmt (100, 22), JR_AK (f40f5010-047c-40ff-8943-818fcbb52307, 6fcbb1f4-0a36-47b2-ad51-8bf7f94ff972), JR_ChequeNo (0, ), JR_LocalCostAmt (100, 22), JR_OSCostAmt (100, 22), JR_OSSellWHTAmt (7.5, 7.01), JR_RX_NKSellInvoiceCurrency (, USD).",
				charge.JR_JH, charge.JR_AL_APLine, charge.JR_AL_ARLine);

			var skipDataRefreshBusInfo = @"Setting SkipDataRefreshBusUpdateDueToAnyChange Charge Fields With changes info = 
DataRefreshBusUpdateSkipped:";

			AssertContains("Info for charge with a BuinessContext.", expectedInfo, charge.GetJobChargeInfo());
			AssertContains("Info for charge with a BuinessContext.", skipDataRefreshBusInfo, charge.GetJobChargeInfo());
			charge.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);

			var newFactory = new BusinessObjectFactory();
			var newLoadCharge = newFactory.Load<JobCharge>(charge.PK);
			newLoadCharge.JR_LocalSellAmt = 45.20M;
			charge.JR_LocalSellAmt = 5165.69M;
			newFactory.Save();

			AssertContains("Info for charge with a BuinessContext.", skipDataRefreshBusInfo, charge.GetJobChargeInfo());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestGetJobInfo()
		{
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

			shipment["ConsigneePK"] = new ZGuid("A6DF8C11-2AE4-4B52-B1F1-16F31F40A78E");
			shipment["ConsignorPK"] = new ZGuid("79EF1AB0-7F48-4A1C-89ED-29B3D019EF15");
			shipment[JobShipmentSchema.JS_RL_NKOrigin.Name] = "AUSYD";
			shipment[JobShipmentSchema.JS_RL_NKDestination.Name] = "NZAKL";
			shipment[JobShipmentSchema.JS_TransportMode.Name] = "";
			shipment[JobShipmentSchema.JS_PackingMode.Name] = "";
			shipment[JobShipmentSchema.JS_E_DEP.Name] = DateTime.Today;
			shipment[JobShipmentSchema.JS_IsShipping.Name] = "";
			shipment[JobShipmentSchema.JS_ShipmentType.Name] = "";
			shipment[JobShipmentSchema.JS_INCO.Name] = "FOB";
			shipment[JobShipmentSchema.JS_UniqueConsignRef.Name] = "234232";

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_Code = "TSLCL1";
			localClient.OH_IsCreditor = true;
			localClient.OH_IsDebtor = true;

			var localClientAddress = (OrgAddress)Factory.New(typeof(OrgAddress), new Guid("38918B17-B446-4184-A553-AD714D7EAA7D"));
			localClient.Addresses.Add(localClientAddress);
			localClientAddress.OA_Address1 = "Local Client Address 1";

			var localClient2 = Factory.NewWithValidTestData<OrgHeader>();
			localClient2.OH_Code = "TSLCL2";
			localClient2.OH_IsCreditor = false;
			localClient2.OH_IsDebtor = false;

			var localClientAddress2 = (OrgAddress)Factory.New(typeof(OrgAddress), new Guid("E205F484-979C-4110-8A0F-FA47188C90E3"));
			localClient2.Addresses.Add(localClientAddress2);
			localClientAddress2.OA_Address1 = "Local Client Address 2";

			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_Code = "TSOVR1";
			overseasAgent.OH_IsCreditor = false;
			overseasAgent.OH_IsDebtor = false;

			var overseasAgentAddress = (OrgAddress)Factory.New(typeof(OrgAddress), new Guid("338F43CD-98A8-4BA3-BF42-D653F32D3399"));
			overseasAgent.Addresses.Add(overseasAgentAddress);
			overseasAgentAddress.OA_Address1 = "Overseas Agent Address 1";

			var overseasAgent2 = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent2.OH_Code = "TSOVR2";
			overseasAgent2.OH_IsCreditor = true;
			overseasAgent2.OH_IsDebtor = true;

			var overseasAgentAddress2 = (OrgAddress)Factory.New(typeof(OrgAddress), new Guid("CDD24ACF-DCD3-420F-9CBD-A512BD2F64CB"));
			overseasAgent2.Addresses.Add(overseasAgentAddress2);
			overseasAgentAddress2.OA_Address1 = "Overseas Agent Address 2";

			Factory.Save();

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f"));
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = ((IJobHeaderParent)shipment).JobNumber;
			job.JH_OA_LocalChargesAddr = localClientAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgentAddress.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			job.InitializeParentFromGenericJobWithoutSettingDefaults();
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			jobCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			string expectedInfo = "Job: Job Number = 234232, PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Parent Table Code = JS, Parent Table Name = JobShipment, ControllerID = JobShipment, Shipment Incoterm = FOB, Local Client Code = TSLCL1 (AR: Yes, AP: Yes), Local Client Address = Local Client Address 1, Overseas Agent Code = TSOVR1 (AR: No, AP: No), Overseas Agent Address = Overseas Agent Address 1, Is In DB = No, Has Changes = Yes.";
			AssertEquals("Info should be as expected.", expectedInfo, job.GetJobInfo());

			Factory.Save();

			job.JH_OA_LocalChargesAddr = localClientAddress2.PK;
			job.JH_OA_AgentCollectAddr = overseasAgentAddress2.PK;

			expectedInfo = @"Job: Job Number = 234232, PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Parent Table Code = JS, Parent Table Name = JobShipment, ControllerID = JobShipment, Shipment Incoterm = FOB, Local Client Code = TSLCL2 (AR: No, AP: No), Local Client Address = Local Client Address 2, Overseas Agent Code = TSOVR2 (AR: Yes, AP: Yes), Overseas Agent Address = Overseas Agent Address 2, Is In DB = Yes, Has Changes = Yes.
	Fields with changes: JH_OA_AgentCollectAddr (338f43cd-98a8-4ba3-bf42-d653f32d3399, cdd24acf-dcd3-420f-9cbd-a512bd2f64cb), JH_OA_LocalChargesAddr (38918b17-b446-4184-a553-ad714d7eaa7d, e205f484-979c-4110-8a0f-fa47188c90e3).";
			AssertEquals("Info should be as expected with changes.", expectedInfo, job.GetJobInfo());
		}

		public void TestGetOrganizationInfo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSLCL1";
			org.OH_IsCreditor = false;
			org.OH_IsDebtor = true;

			string expectedInfo = "TSLCL1 (AR: Yes, AP: No)";
			AssertEquals("Info should be as expected.", expectedInfo, org.GetOrganizationInfo(GlbCompany.CurrentCompany.PK));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestGetPostedCostInfo()
		{
			BusinessObject shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("5d7b0429-948f-46ec-8219-5dfbd4bfe21f"));
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_JobNum = "234232";
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			job.InitializeParentFromGenericJobWithoutSettingDefaults();
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			jobCharge.JR_AC = Env.Registry.FreightChargeCode;
			jobCharge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			jobCharge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AccTransactionHeader invoice1 = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("e66a7fed-b46b-4322-bdf3-616d341cf2ed"));
			invoice1.FillWithValidTestData();
			invoice1.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice1.AH_TransactionType = TransactionTypes.Invoice;
			invoice1.AH_InvoiceAmount = -100m;
			invoice1.AH_OutstandingAmount = 0M;

			AccTransactionLines line1 = (AccTransactionLines)Factory.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line1.AL_JH = job.PK;
			line1.AL_AH = invoice1.PK;

			AccTransactionHeader invoice2 = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("33cac2bc-2c5a-4c1f-9753-78d5f4af7a7a"));
			invoice2.FillWithValidTestData();
			invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice2.AH_TransactionType = TransactionTypes.Invoice;
			invoice2.AH_InvoiceAmount = -100m;
			invoice2.AH_OutstandingAmount = 0M;

			AccTransactionLines line2 = (AccTransactionLines)Factory.New(typeof(AccTransactionLines), new Guid("E66A7FED-B46B-4322-BDF3-616D341CF2ED"));
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line2.AL_AH = invoice2.PK;

			jobCharge.JR_AL_APLine = line1.PK;
			jobCharge.JR_AL_ARLine = line2.PK;

			string expectedInfo = @"Posted Cost:
Cost Header: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Ledger = AP, Transaction Type = INV, Invoice Date = 04-Mar-04 00:00:00, Post Date = , Invoice Amount = -100, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 0CBSU6DGVDNI2TP1RBLLN7XKNXVXACC4CNGTGR, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Cost Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = , Type = CST, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.";

			AssertEquals(expectedInfo, jobCharge.GetPostedCostInfo());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "TSLCL1";
			org.OH_IsCreditor = false;
			org.OH_IsDebtor = false;

			expectedInfo = @"Posted Cost:
Cost Header: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Ledger = AP, Transaction Type = INV, Invoice Date = 04-Mar-04 00:00:00, Post Date = , Invoice Amount = -100, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 0CBSU6DGVDNI2TP1RBLLN7XKNXVXACC4CNGTGR, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = No, Is Deleted = No, Has Changes = Yes, Business Contexts = None.
Cost Line: PK = 6c1c3709-8498-4ea3-95ba-fee57c41b00c, Charge Code = , GL Account = , Type = CST, OS Amount = 0, Local Amount = 0, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 0, Currency = , Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Job PK = 5d7b0429-948f-46ec-8219-5dfbd4bfe21f, Organization = , Revenue Recognition Type = , Is In DB = No, Is Final = No, Sub Accounts = , Has Changes = Yes.";

			AssertEquals(expectedInfo, jobCharge.GetPostedCostInfo());
		}

		public void TestGetJobChargeInternalFieldsInfo()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = jobCharge.JR_JH_InternalJob = job.PK;
			jobCharge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("Info should be as expected.",
				$"InternalFields: InternalJob = {jobCharge.InternalJob?.JH_JobNum}, InternalBranch = {jobCharge.InternalBranch?.GB_Code}, InternalDepartment = {jobCharge.InternalDept?.GE_Code}"
				, jobCharge.GetJobChargeInternalFieldsInfo());
		}

		#region AccTransactionMatchLink

		public void TestGetMatchLinkInfo()
		{
			ZDateTime matchDate = new ZDateTime(2009, 11, 20);

			AccTransactionHeader invoice = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("e66a7fed-b46b-4322-bdf3-616d341cf2ed"));
			invoice.FillWithValidTestData();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_InvoiceAmount = -100m;
			invoice.AH_OutstandingAmount = 0M;

			AccTransactionMatchLink link = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			link.AP_MatchGroupNum = "M000111";
			link.AP_MatchDate = matchDate;
			link.AP_AH = invoice.PK;
			link.AP_Amount = -100m;

			string expectedInfo = "Match Link: Group Number = M000111, Amount = -100, OS Amount = 0, Match Date = 20-Nov-09 00:00:00, Transaction PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Is In DB = No, Has Changes = Yes.";
			AssertEquals("Info for unsaved MatchLink", expectedInfo, link.GetMatchLinkInfo());

			DummyMatchLinkGroup group = new DummyMatchLinkGroup(Factory);
			group.Add(link);

			Factory.Save();

			expectedInfo = "Match Link: Group Number = M000111, Amount = -100, OS Amount = 0, Match Date = 20-Nov-09 00:00:00, Transaction PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Is In DB = Yes, Has Changes = No.";
			AssertEquals("Info for saved MatchLink", expectedInfo, link.GetMatchLinkInfo());

			link.AP_Reason = "NA";
			expectedInfo = @"Match Link: Group Number = M000111, Amount = -100, OS Amount = 0, Match Date = 20-Nov-09 00:00:00, Transaction PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Is In DB = Yes, Has Changes = Yes.
	Fields with changes: AP_Reason (, NA).";
			AssertEquals("Info for saved MatchLink with changes", expectedInfo, link.GetMatchLinkInfo());
		}

		public void TestGetTransactionHeaderMatchLinkInfos()
		{
			ZDateTime matchDate1 = new ZDateTime(2009, 11, 20);
			ZDateTime matchDate2 = new ZDateTime(2010, 11, 20);

			AccTransactionHeader invoice = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("C21AA9DF-2B40-4EBE-89E5-F2DCEB6728EE"));
			invoice.FillWithValidTestData();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_InvoiceAmount = -200m;
			invoice.AH_OutstandingAmount = 0M;

			AccTransactionMatchLink link1 = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("2517E854-E33C-4A4A-A93E-C92CF9CB3425"));
			link1.AP_MatchGroupNum = "M000111";
			link1.AP_MatchDate = matchDate1;
			link1.AP_AH = invoice.PK;
			link1.AP_Amount = -100m;

			AccTransactionMatchLink link2 = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("9F681D28-3A8F-4782-9AE8-6EDC8B400D2F"));
			link2.AP_MatchGroupNum = "M000112";
			link2.AP_MatchDate = matchDate1;
			link2.AP_AH = invoice.PK;
			link2.AP_Amount = -100m;

			AccTransactionMatchLink[] matchlinks = { link1, link2 };

			var expectedInfo = @"

Related Match Links:
Match Link: Group Number = M000111, Amount = -100, OS Amount = 0, Match Date = 20-Nov-09 00:00:00, Transaction PK = c21aa9df-2b40-4ebe-89e5-f2dceb6728ee, Is In DB = No, Has Changes = Yes.
Match Link: Group Number = M000112, Amount = -100, OS Amount = 0, Match Date = 20-Nov-09 00:00:00, Transaction PK = c21aa9df-2b40-4ebe-89e5-f2dceb6728ee, Is In DB = No, Has Changes = Yes.
";
			AssertEquals("Info for matchlinks", expectedInfo, matchlinks.GetTransactionHeaderMatchLinkInfos());
		}

		public void TestGetTransactionHeaderMatchLinkInfosAndUnmatchDeletionInfos()
		{
			var date = new ZDateTime(2022, 9, 27);
			var creator = ObjectFactory.Get<ITransactionCreator>();

			var invoice1 = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			invoice1.AH_InvoiceAmount = 50m;
			invoice1.AH_InvoiceDate = date;
			invoice1.AH_PostDate = date;

			var link1 = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("AD94CC2A-A518-4A81-8029-16CE1D7F6879"));
			link1.AP_MatchGroupNum = "M000111";
			link1.AP_MatchDate = date;
			link1.AP_AH = invoice1.PK;
			link1.AP_Amount = 25m;
			invoice1.AH_OutstandingAmount = 25M;

			var group = new MatchLinkGroupForTest(Factory);
			group.Add(link1);
			AssertNoExceptionThrown(() => Factory.Save());

			var invoice2 = creator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			invoice2.AH_InvoiceAmount = 200m;

			var link2 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			link2.AP_MatchGroupNum = "M000111";
			link2.AP_MatchDate = date;
			link2.AP_AH = invoice2.PK;
			link2.AP_Amount = 100m;

			var link3 = Factory.NewWithValidTestData<AccTransactionMatchLink>();
			link3.AP_MatchGroupNum = "M000112";
			link3.AP_MatchDate = date;
			link3.AP_AH = invoice2.PK;
			link3.AP_Amount = 100m;

			AccTransactionMatchLink[] matchlinks = { link2, link3 };

			var expectedInfo = $@"

Related Match Links:
Match Link Factory Instance Number = {Factory._Instance}.
----------------------
Match Link: Group Number = M000111, Amount = 100, OS Amount = 0, Match Date = 27-Sep-22 00:00:00, Transaction PK = {invoice2.PK}, Is In DB = No, Has Changes = Yes.

MatchLinkDeletionInfo: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
>> Other Match Links In This Group:
Match Link: Group Number = M000111, Amount = 25, OS Amount = 0, Match Date = 27-Sep-22 00:00:00, Transaction PK = {invoice1.PK}, Is In DB = Yes, Has Changes = No.

MatchLinkDeletionInfo: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.

Relative Transaction Header:
Header: PK = {invoice1.PK}, Ledger = AR, Transaction Type = INV, Invoice Date = 27-Sep-22 00:00:00, Post Date = 27-Sep-22 00:00:00, Invoice Amount = 100, GST Amount = 0, OS Total = 100, Exchange Rate = 1, Currency = AUD, Outstanding Amount = 75, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = 00001000, Job PK = 00000000-0000-0000-0000-000000000000, Organization = ZOrg01, Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = {invoice1.AH_SystemCreateTimeUtc.ToAUString()}, System Create User = E, System Last Edit Time = {invoice1.AH_SystemLastEditTimeUtc.ToAUString()}, System Last Edit User = E.

>> ----- End of Other Match Links In This Group -----------------
Match Link: Group Number = M000112, Amount = 100, OS Amount = 0, Match Date = 27-Sep-22 00:00:00, Transaction PK = {invoice2.PK}, Is In DB = No, Has Changes = Yes.

MatchLinkDeletionInfo: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
>> ----- End of Other Match Links In This Group -----------------
----- End of Related Match Links -----------------
";
			AssertEquals("Info for matchlinks", expectedInfo, matchlinks.GetTransactionHeaderMatchLinkInfosAndUnmatchDeletionInfos());
		}

		#endregion

		[TestDate(2016, 11, 14, 00, 00, 00)]
		public void TestReportExtraMsgWhenOutStandingAmountIsNotValid()
		{
			var matchDate = new ZDateTime(2016, 11, 14);

			var invoice1 = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("C21AA9DF-2B40-4EBE-89E5-F2DCEB6728EE"));
			invoice1.FillWithValidTestData();
			invoice1.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = TransactionTypes.Invoice;
			invoice1.AH_InvoiceAmount = -100m;
			invoice1.AH_OutstandingAmount = 0M;

			var invoice2 = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("B6F13C9F-AB3D-4739-BBC3-ED2270D3D51D"));
			invoice2.FillWithValidTestData();
			invoice2.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice2.AH_TransactionType = TransactionTypes.Journal;
			invoice2.AH_InvoiceAmount = 50m;
			invoice2.AH_OutstandingAmount = 0M;

			var invoice3 = Factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("8D33250C-CD5E-4309-9C4A-8F822415A404"));
			invoice3.FillWithValidTestData();
			invoice3.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice3.AH_TransactionType = TransactionTypes.Journal;
			invoice3.AH_InvoiceAmount = 50m;
			invoice3.AH_OutstandingAmount = 0M;

			var matchLinkGroup = new DummyMatchLinkGroup(Factory);

			var link1 = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("2517E854-E33C-4A4A-A93E-C92CF9CB3425"));
			link1.AP_MatchGroupNum = "M000111";
			link1.AP_MatchDate = matchDate;
			link1.AP_AH = invoice1.PK;
			link1.AP_Amount = -100m;

			var link2 = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("9F681D28-3A8F-4782-9AE8-6EDC8B400D2F"));
			link2.AP_MatchGroupNum = "M000111";
			link2.AP_MatchDate = matchDate;
			link2.AP_AH = invoice2.PK;
			link2.AP_Amount = 50m;

			var link3 = Factory.NewWithPrimaryKey<AccTransactionMatchLink>(new Guid("8D9D5A32-985D-46C8-BD69-64D2C6983122"));
			link3.AP_MatchGroupNum = "M000111";
			link3.AP_MatchDate = matchDate;
			link3.AP_AH = invoice3.PK;
			link3.AP_Amount = 50m;

			matchLinkGroup.Add(link1);
			matchLinkGroup.Add(link2);
			matchLinkGroup.Add(link3);
			Factory.Save();

			AccTransactionMatchLink[] matchlinks = { link1 };

			var expectedInfo = @"

Related Match Links:
Match Link: Group Number = M000111, Amount = -100, OS Amount = 0, Match Date = 14-Nov-16 00:00:00, Transaction PK = c21aa9df-2b40-4ebe-89e5-f2dceb6728ee, Is In DB = Yes, Has Changes = No.
Other Match Links In This Group:
Match Link: Group Number = M000111, Amount = 50, OS Amount = 0, Match Date = 14-Nov-16 00:00:00, Transaction PK = b6f13c9f-ab3d-4739-bbc3-ed2270d3d51d, Is In DB = Yes, Has Changes = No.
Header: PK = b6f13c9f-ab3d-4739-bbc3-ed2270d3d51d, Ledger = AR, Transaction Type = JNL, Invoice Date = 04-Mar-04 00:00:00, Post Date = , Invoice Amount = 50, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = GH65IK0XI8F508L5IGIF8IFID0B2GW7D4LEOYP, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 14-Nov-16 00:00:00, System Create User = E, System Last Edit Time = 14-Nov-16 00:00:00, System Last Edit User = E.
Match Link: Group Number = M000111, Amount = 50, OS Amount = 0, Match Date = 14-Nov-16 00:00:00, Transaction PK = 8d33250c-cd5e-4309-9c4a-8f822415a404, Is In DB = Yes, Has Changes = No.
Header: PK = 8d33250c-cd5e-4309-9c4a-8f822415a404, Ledger = AR, Transaction Type = JNL, Invoice Date = 04-Mar-04 00:00:00, Post Date = , Invoice Amount = 50, GST Amount = 0, OS Total = 0, Exchange Rate = 1, Currency = , Outstanding Amount = 0, Fully Paid Date =  , Is Canceled = No, Is Aggregated = N, Transaction Number = YRWEXGHGMM5YPRBSX1JSEQLVGN5OQQLMJLQQSE, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Is In DB = Yes, Is Deleted = No, Has Changes = No, Business Contexts = None, System Create Time = 14-Nov-16 00:00:00, System Create User = E, System Last Edit Time = 14-Nov-16 00:00:00, System Last Edit User = E.

";

			AssertEquals("Info for matchlinks", expectedInfo, matchlinks.GetTransactionHeaderMatchLinkInfos());
		}

		[DisableZeroExchangeRateOverriding]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestLoadRelatedJobChargeExtentionMethod()
		{
			AccTransactionLines line1 = (AccTransactionLines)Factory.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c"));
			line1.AL_GB = GlbBranch.CurrentBranch.PK;
			line1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line1.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			line1.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AccTransactionLines line2 = (AccTransactionLines)Factory.New(typeof(AccTransactionLines), new Guid("E66A7FED-B46B-4322-BDF3-616D341CF2ED"));
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line2.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line2.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AccTransactionHeader header2 = (AccTransactionHeader)Factory.New(typeof(AccTransactionHeader), new Guid("c412ebc4-b819-4f86-9c14-3f207db3a4dd"));
			header2.FillWithValidTestData();
			header2.AH_Ledger = LedgerTypes.AccountsPayable;
			header2.AH_TransactionType = TransactionTypes.Invoice;
			line2.AL_AH = header2.PK;

			var job1 = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("084172b9-3656-41d5-b18c-1a73179a8f9b"));
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.JH_JobNum = "1234";
			job1.FillWithValidTestData();
			line1.AL_JH = job1.PK;
			line2.AL_JH = job1.PK;

			line1.AL_ExchangeRate = 1m;

			JobCharge charge = (JobCharge)Factory.New(typeof(JobCharge), new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"));
			charge.JR_GC = GlbCompany.CurrentCompany.PK;
			charge.JR_AL_ARLine = line1.PK;
			charge.JR_AL_APLine = line2.PK;
			charge.FillWithValidTestData();
			line1.AL_AC = charge.JR_AC;
			line2.AL_AC = charge.JR_AC;
			charge.SetChargeValuesFromLinkedAPLineForTests();
			charge.SetChargeValuesFromLinkedARLineForTests();
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSSellExRate = 1m;

			Factory.Save();

			AssertEquals("The extension method should retrieve the related job charge", new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"), line1.LoadRelatedJobCharge().PK);
			AssertEquals("The extension method should retrieve the related job charge", new Guid("3dca8b18-7a6e-4d8e-92ac-2a1605633721"), line2.LoadRelatedJobCharge().PK);
		}

		public void TestExtraInfoForUserMessage_JobChargeAndConsolCost()
		{
			var charge = Factory.NewWithPrimaryKey<JobCharge>(new Guid("f8a8e869-ce2a-4049-a9df-d0b7a7e79f4e"));
			var consolCost = Factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("a12986b4-036c-4cb2-a0e7-0c96438a46dd"));
			FillChargeWithConsolCostAndAllCorrectInvoiceDetails(Factory, charge, consolCost);

			bool isExistingDataWithoutChange;
			MultilingualString mismatchInfo = CriticalValidationInfoExtensions.GetConsolCostAndChargeInvoiceDetailsDifference(charge, consolCost as IJobConsolCost, out isExistingDataWithoutChange);
			Assert("Shouldn't be any Mismatch", mismatchInfo.IsEmpty);
			AssertEquals("This is not existing data", false, isExistingDataWithoutChange);

			charge.JR_APInvoiceNum = "7775";
			charge.JR_APInvoiceDate = ZDateTime.BrettsBirthday;
			ExceptionReporterTestListener.Instance.Clear();
			charge.JR_PaymentDate = ZDateTime.BrettsBirthday;
			charge.CostAccount.OH_Code = "TEST";
			charge.JR_CostReference = "XYZ";

			var differentTaxRate = Factory.New<AccTaxRate>();
			differentTaxRate.AT_Code = "TAX1";
			charge.JR_AT_CostGSTRate = differentTaxRate.PK;

			var differentTaxClass = Factory.New<AccInvMsg>();
			differentTaxClass.A9_Code = "MSG1";
			charge.JR_A9_CostVATClass = differentTaxClass.PK;

			mismatchInfo = CriticalValidationInfoExtensions.GetConsolCostAndChargeInvoiceDetailsDifference(charge, consolCost as IJobConsolCost, out isExistingDataWithoutChange);
			string expectedMsg = @"
Mismatched fields are listed below -->
				
Field Name:    Values in Charge    |    Values in Consol Cost
---------------------------------------------------------------------------------------------------------------------------
Invoice Number:    7775    |    111
Invoice Date:    18-Sep-1971 12:00:00.0000    |    05-Jan-2010 12:00:00.0000
Payment Date:    18-Sep-1971 12:00:00.0000    |    10-Jan-2010 12:00:00.0000
Supplier Cost Reference:    XYZ    |    ABC
GST Rate:    TAX1    |    <empty>
Tax Class:    MSG1    |    <empty>";
			AssertContains("Mismatched Properties", expectedMsg, mismatchInfo.GetUnresolvedString());
			AssertContains("Mismatched Properties", expectedMsg, mismatchInfo.ToString());
			AssertEquals("This is not existing data", false, isExistingDataWithoutChange);
		}

		public void TestExistingDataWithoutChange_JobChargeAndConsolCost()
		{
			SuspendCriticalValidationAttribute.IsActive = true;

			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_Code = "TAX1";
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Code = "TAX2";

			var taxClass1 = Factory.NewWithValidTestData<AccInvMsg>();
			taxClass1.A9_Code = "MSG1";
			var taxClass2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxClass2.A9_Code = "MSG2";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "Org1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Org2";

			var taxBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			taxBranch1.GB_Code = "TB1";
			var taxBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			taxBranch2.GB_Code = "TB2";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.FillWithValidTestData();

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10m;
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 10m;

			charge.JR_APInvoiceNum = "111";
			charge.JR_APInvoiceDate = new ZDateTime(2022, 1, 11);
			charge.JR_PaymentDate = new ZDateTime(2022, 1, 11);
			charge.JR_OH_CostAccount = org1.PK;
			charge.JR_CostReference = "Reference1";
			charge.JR_CostSupplyType = "LOC";
			charge.JR_AT_CostGSTRate = taxRate1.PK;
			charge.JR_CostTaxDate = new ZDate(2022, 1, 11);
			charge.JR_A9_CostVATClass = taxClass1.PK;
			charge.JR_GB_CostTaxBranch = taxBranch1.PK;
			charge.FillWithValidTestData();

			var consolCost = Factory.New(ObjectFactory.GetType<IJobConsolCost>(), Guid.NewGuid());
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode.Name] = charge.JR_AC;
			consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10m;

			consolCost[JobConsolCostSchema.E6_InvoiceNum.Name] = "222";
			consolCost[JobConsolCostSchema.E6_InvoiceDate.Name] = new ZDateTime(2022, 1, 12);
			consolCost[JobConsolCostSchema.E6_PaymentDate.Name] = new ZDateTime(2022, 1, 12);
			consolCost[JobConsolCostSchema.E6_OH_Creditor.Name] = org2.PK;
			consolCost[JobConsolCostSchema.E6_CostReference.Name] = "Reference2";
			consolCost[JobConsolCostSchema.E6_SupplyType.Name] = "LOA";
			consolCost[JobConsolCostSchema.E6_AT_TaxRate.Name] = taxRate2.PK;
			consolCost[JobConsolCostSchema.E6_TaxDate.Name] = new ZDate(2022, 1, 12);
			consolCost[JobConsolCostSchema.E6_A9_VATClass.Name] = taxClass2.PK;
			consolCost[JobConsolCostSchema.E6_GB_CostTaxBranch.Name] = taxBranch2.PK;
			consolCost[JobConsolCostSchema.E6_AH_APInvoice.Name] = Factory.NewWithValidTestData<AccTransactionHeader>().PK;
			consolCost.FillWithValidTestData();

			charge.JR_E6 = consolCost.PK;
			Factory.Save();
			SuspendCriticalValidationAttribute.IsActive = false;

			bool isExistingDataWithoutChange;
			var mismatchInfo = CriticalValidationInfoExtensions.GetConsolCostAndChargeInvoiceDetailsDifference(charge, consolCost as IJobConsolCost, out isExistingDataWithoutChange);
			string expectedMsg = @"
Mismatched fields are listed below -->
				
Field Name:    Values in Charge    |    Values in Consol Cost
---------------------------------------------------------------------------------------------------------------------------
Invoice Number:    111    |    222
Invoice Date:    11-Jan-2022 12:00:00.0000    |    12-Jan-2022 12:00:00.0000
Payment Date:    11-Jan-2022 12:00:00.0000    |    12-Jan-2022 12:00:00.0000
Creditor:    Org1    |    Org2
Supplier Cost Reference:    Reference1    |    Reference2
GST Rate:    TAX1    |    TAX2
Tax Date:    11-Jan-22 00:00:00    |    12-Jan-22 00:00:00
Tax Class:    MSG1    |    MSG2
Supply Type:    LOC    |    LOA
Tax Branch:    TB1    |    TB2
Charge Cost:Not Posted    |    Consol Cost:Posted";
			AssertContains("Mismatched Properties", expectedMsg, mismatchInfo.GetUnresolvedString());
			AssertContains("Mismatched Properties", expectedMsg, mismatchInfo.ToString());
			AssertEquals("This is existing data", true, isExistingDataWithoutChange);

			consolCost[JobConsolCostSchema.E6_InvoiceNum.Name] = "333";
			mismatchInfo = CriticalValidationInfoExtensions.GetConsolCostAndChargeInvoiceDetailsDifference(charge, consolCost as IJobConsolCost, out isExistingDataWithoutChange);
			expectedMsg = @"
Mismatched fields are listed below -->
				
Field Name:    Values in Charge    |    Values in Consol Cost
---------------------------------------------------------------------------------------------------------------------------
Invoice Number:    111    |    333
Invoice Date:    11-Jan-2022 12:00:00.0000    |    12-Jan-2022 12:00:00.0000
Payment Date:    11-Jan-2022 12:00:00.0000    |    12-Jan-2022 12:00:00.0000
Creditor:    Org1    |    Org2
Supplier Cost Reference:    Reference1    |    Reference2
GST Rate:    TAX1    |    TAX2
Tax Date:    11-Jan-22 00:00:00    |    12-Jan-22 00:00:00
Tax Class:    MSG1    |    MSG2
Supply Type:    LOC    |    LOA
Tax Branch:    TB1    |    TB2
Charge Cost:Not Posted    |    Consol Cost:Posted";
			AssertContains("Mismatched Properties", expectedMsg, mismatchInfo.GetUnresolvedString());
			AssertContains("Mismatched Properties", expectedMsg, mismatchInfo.ToString());
			AssertEquals("This is not existing data", false, isExistingDataWithoutChange);
		}

		JobCharge FillChargeWithConsolCostAndAllCorrectInvoiceDetails(BusinessObjectFactory factory, JobCharge charge, BusinessObject consolCost)
		{
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge.FillWithValidTestData();

			var job = factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("7dd1291f-ed46-4c34-8714-5d35e717cda5"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10m;

			consolCost.FillWithValidTestData();
			var consol = factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new Guid("40b5446d-6066-4ed1-b9c1-8ca588a2a43d"));
			consol.FillWithValidTestData();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID.Name] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode.Name] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			charge.JR_E6 = consolCost.PK;

			var invoice = factory.NewWithPrimaryKey<AccTransactionHeader>(new Guid("6d935954-0e05-4fab-a93b-4f344ed529de"));
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice.AH_PostDate = ZDateTime.Now;
			invoice.AH_InvoiceDate = ZDateTime.Now;

			var line = factory.NewWithPrimaryKey<AccTransactionLines>(new Guid("684f6900-562a-450d-8845-b14087c27ae4"));
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line.AL_ExchangeRate = 1m;
			line.AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.FillWithValidTestData();
			line.AL_OSAmount = line.AL_LineAmount = -10m;
			line.AL_AH = invoice.PK;
			charge.JR_AL_APLine = line.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice] = invoice.PK;

			charge.JR_APInvoiceNum = "111";
			charge.JR_APInvoiceDate = new ZDateTime(2010, 01, 05);
			charge.JR_OH_CostAccount = factory.NewWithValidTestData<OrgHeader>().PK;
			charge.JR_PaymentDate = new ZDateTime(2010, 01, 10);
			charge.JR_CostReference = "ABC";

			consolCost[JobConsolCostSchema.E6_InvoiceNum.Name] = charge.JR_APInvoiceNum;
			consolCost[JobConsolCostSchema.E6_InvoiceDate.Name] = charge.JR_APInvoiceDate;
			consolCost[JobConsolCostSchema.E6_PaymentDate.Name] = charge.JR_PaymentDate;
			consolCost[JobConsolCostSchema.E6_OH_Creditor.Name] = charge.JR_OH_CostAccount;
			consolCost[JobConsolCostSchema.E6_CostReference.Name] = charge.JR_CostReference;
			consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10m;

			return charge;
		}

		class DummyMatchLinkGroup : AccTransactionMatchLinkCollection, ISupportCriticalValidation
		{
			public DummyMatchLinkGroup(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ICriticalValidation CriticalValidation
			{
				get { return new DummyCriticalValidation(); }
			}

			public void SetConflictWithCriticalFieldsBusinessContext()
			{
				throw new NotImplementedException();
			}
		}

		class DummyCriticalValidation : ICriticalValidation
		{
			public void RegisterOnSavingCheck()
			{ }
			public void RunOnSavingCheck()
			{ }
			public void RunDeletedObjectOnSavingCheck()
			{ }
			public void RunAfterSavingCheck()
			{ }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		public void TestGetFieldsWithChangesInfo()
		{
			JobChargeWithNonPersistentPropertyForTest charge = Factory.New<JobChargeWithNonPersistentPropertyForTest>();
			charge.FillWithValidTestData();
			charge.JR_LocalSellAmt = 450.05M;
			charge.JR_OSSellAmt = 450.05M;
			charge.JR_InvoiceType = "FIN";
			charge.JR_RX_NKCostCurrency = "AUD";
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("6bdda203-cc3a-4f9d-b994-035356f5c606"));
			job.FillWithValidTestData();
			charge.JR_JH = job.PK;
			AccBankAccount bankAccount = (AccBankAccount)Factory.New(typeof(AccBankAccount), new Guid("c412ebc4-b819-4f86-9c14-3f207db3a4dd"));
			bankAccount.FillWithValidTestData();
			charge.JR_AB = bankAccount.PK;
			Factory.Save();

			Assert("Precondition: bizo is not changed.", !charge.HasChanges);
			AssertEquals("Info should be empty for not changed object.", string.Empty, charge.GetFieldsWithChangesInfo());

			charge.JR_LocalSellAmt = 450.07M;
			charge.JR_InvoiceType = "CUR";
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_JH = ZGuid.Empty;
			charge.JR_AL_APLine = Factory.New(typeof(AccTransactionLines), new Guid("6c1c3709-8498-4ea3-95ba-fee57c41b00c")).PK;
			charge.JR_AB = new ZGuid("f40f5010-047c-40ff-8943-818fcbb52307");

			Assert("Precondition: we have nonpersistent property.", !charge.TestAmountInfo.IsPersistent);
			Assert("Precondition: the nonpersistent property is not changed.", !charge.TestAmountInfo.HasChanges);
			charge.TestAmount = 11M;
			Assert("ProstCondition: HasChanges for a nonpersistent property is always false.", !charge.TestAmountInfo.HasChanges);

			string expectedInfo = @"
	Fields with changes: JR_AB (c412ebc4-b819-4f86-9c14-3f207db3a4dd, f40f5010-047c-40ff-8943-818fcbb52307), JR_AL_APLine (00000000-0000-0000-0000-000000000000, 6c1c3709-8498-4ea3-95ba-fee57c41b00c), JR_InvoiceType (FIN, CUR), JR_JH (6bdda203-cc3a-4f9d-b994-035356f5c606, 00000000-0000-0000-0000-000000000000), JR_LocalSellAmt (450.05, 450.07), JR_RX_NKCostCurrency (AUD, USD).";
			AssertEquals("Info should contain only persistant properties.", expectedInfo, charge.GetFieldsWithChangesInfo());
		}

		public void TestGetParentCollectionsInfo()
		{
			var lines = new AccTransactionLinesCollection(Factory);
			var line1 = lines.AddNew();
			var actualStr = line1.GetParentCollectionsInfo();
			var expectStr = Invariant(
$@"Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.MasterFiles.Business.AccTransactionLinesCollection
	Element Type = Enterprise.MasterFiles.Business.AccTransactionLines
	Factory Instance = {lines.Factory._Instance}
	Hash Code = {lines.GetHashCode()}
	Contains bizo = True
	Has Changes = False
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True");

			AssertEquals(expectStr, actualStr);
		}

		public void TestGetBusinessObjectGenericInfo()
		{
			var line = Factory.New(typeof(AccTransactionLines), new Guid("6092cf2f-86fc-4997-afcc-4ba69d9606c4"));
			var actualStr = line.GetBusinessObjectGenericInfo();
			var expectStr = Invariant(
$@"	PK = 6092cf2f-86fc-4997-afcc-4ba69d9606c4
	Type = AccTransactionLines
	Types around row = AccTransactionLines
	Factory Instance = {line.Factory._Instance}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None
");

			AssertEquals(expectStr, actualStr);
		}

		public void TestAreDateTimesEqualSafe()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();

			Assert("Values should be equal because they haven't been set yet (invalid), and yet...", CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_DateInfo, dummy2.Z0_DateInfo));
			Assert("Values should be equal because they haven't been set yet (invalid), and yet...", CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_SmallDateTimeInfo, dummy2.Z0_SmallDateTimeInfo));

			dummy1.Z0_Date = ZDateTime.BrettsBirthday;
			Assert("Values should not be equal because only one has been set, and yet...", !CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_DateInfo, dummy2.Z0_DateInfo));

			dummy2.Z0_Date = ZDateTime.BrettsBirthday;
			Assert("Values should be equal because both were set to the same value, and yet...", CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_DateInfo, dummy2.Z0_DateInfo));
			Assert("Values should not be equal because only one has been set, and yet...", !CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_DateInfo, dummy2.Z0_SmallDateTimeInfo));

			dummy1.Z0_SmallDateTime = new ZDateTime(2015, 1, 1, 1, 1, 1, 1);
			Assert("Values should not be equal because only one has been set, and yet...", !CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_SmallDateTimeInfo, dummy2.Z0_SmallDateTimeInfo));
			Assert("Values should not be equal because they have clearly different values, and yet...", !CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_SmallDateTimeInfo, dummy2.Z0_DateInfo));

			dummy2.Z0_SmallDateTime = new ZDateTime(2015, 1, 1, 1, 1, 2, 3);
			Assert("Values should be equal when compared as smalldatetimes (truncating the seconds/milliseconds), and yet...", CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_SmallDateTimeInfo, dummy2.Z0_SmallDateTimeInfo));

			dummy1.Z0_Date = new ZDateTime(2015, 1, 1, 1, 1, 1, 4);
			Assert("Values should not be equal because the date property will not be converted to smalldatetime, and yet...", !CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy1.Z0_DateInfo, dummy2.Z0_SmallDateTimeInfo));

			dummy2.Z0_Date = dummy2.Z0_SmallDateTime;
			Assert("Values should be equal even without smalldatetime conversion, and yet...", CriticalValidationInfoExtensions.AreDateTimesEqualSafe(dummy2.Z0_DateInfo, dummy2.Z0_SmallDateTimeInfo));
		}

		public void TestGetFieldsListWithChanges()
		{
			var job = Factory.NewJobWithPrimaryKeyForTesting<JobHeader>(new Guid("260ed833-8bab-41ac-83ef-a4cd6049cd24"));
			job.FillWithValidTestData();

			JobChargeWithNonPersistentPropertyForTest charge = Factory.New<JobChargeWithNonPersistentPropertyForTest>();
			charge.FillWithValidTestData();
			charge.JR_JH = job.PK;
			charge.JR_LocalSellAmt = 100M;
			charge.JR_OSSellAmt = 100M;
			charge.JR_RX_NKCostCurrency = "AUD";
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_JH = job.PK;
			Factory.Save();

			Assert("Precondition: bizo is not changed.", !charge.HasChanges);
			AssertEquals("Fields list should be empty for not changed object.", 0, charge.GetFieldsListWithChanges().Count());

			charge.JR_LocalSellAmt = 150M;
			charge.JR_RX_NKCostCurrency = "USD";
			AssertEquals("Fields list should contain changed properties count.", 2, charge.GetFieldsListWithChanges().Count());

			Factory.Save();

			Assert("Precondition: bizo is not changed.", !job.HasChanges);

			job.JH_JobNum = "1234";
			AssertEquals("Fields list should contain changed properties count.", 1, job.GetFieldsListWithChanges().Count());
		}

		class JobChargeWithNonPersistentPropertyForTest : JobCharge
		{
			public JobChargeWithNonPersistentPropertyForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZDecimal TestAmount
			{
				get { return TestAmount_innerValue; }
				set { SetNonPersistentPropertyValue(TestAmountInfo, ref TestAmount_innerValue, value); }
			}
			ZDecimal TestAmount_innerValue;

			public ZPropertyInfo TestAmountInfo
			{
				get { return GetZPropertyInfo(nameof(TestAmount)); }
			}

			public override ZDecimal JR_OSCostGSTAmt_Calc { get => JR_OSCostGSTAmt; set => JR_OSCostGSTAmt = value; }

			public override bool CanReautorate(CostSell costOrSell, params ZString[] adapterIDs)
			{
				throw new NotImplementedException();
			}

			public override ILocation CostPlaceOfSupplyLocation => null;

			public override ILocation SellPlaceOfSupplyLocation => null;
		}

		[TestDate(2018, 11, 12)]
		public void TestGetInvoiceApprovalInfoFromAccTransactionHeader()
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();

			var request = Factory.NewWithPrimaryKey<GenApprovalRequest>(new Guid("e66a7fed-b46b-4322-bdf3-616d341cf2ed"));
			request.FillWithValidTestData();
			request.XP_ParentID = invoice.PK;
			request.XP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			string expectedInfo = Invariant($@"Approval Request: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Approval Status = APP, Approval Type = , Approval Date = , Reason Code = , Reason Description = , Request ID = , Create Date = 06-May-98 00:00:00, Create User = E");

			AssertEquals("Info should be as expected.", expectedInfo, invoice.GetInvoiceApprovalInfo());

			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;

			expectedInfo = Invariant($@"Approval Request: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Approval Status = PST, Approval Type = , Approval Date = , Reason Code = , Reason Description = , Request ID = , Create Date = 06-May-98 00:00:00, Create User = E");

			AssertNotEquals("The information for request should be obtained from the database, status should be APP before save.", expectedInfo, invoice.GetInvoiceApprovalInfo());

			Factory.Save();
			AssertEquals("Info should be as expected.", expectedInfo, invoice.GetInvoiceApprovalInfo());
		}

		[TestDate(2018, 11, 12)]
		public void TestGetInvoiceApprovalInfoFromGenApprovalRequest()
		{
			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			var request = Factory.NewWithPrimaryKey<GenApprovalRequest>(new Guid("e66a7fed-b46b-4322-bdf3-616d341cf2ed"));
			request.FillWithValidTestData();
			request.XP_ParentID = invoice.PK;
			request.XP_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			string expectedInfo = Invariant($@"Approval Request: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Approval Status = APP, Approval Type = , Approval Date = , Reason Code = , Reason Description = , Request ID = , Create Date = 06-May-98 00:00:00, Create User = E");
			AssertEquals("Info should be as expected.", expectedInfo, request.GetInvoiceApprovalInfo());

			expectedInfo = Invariant($@"Approval Request: PK = e66a7fed-b46b-4322-bdf3-616d341cf2ed, Approval Status = PST, Approval Type = , Approval Date = , Reason Code = , Reason Description = , Request ID = , Create Date = 06-May-98 00:00:00, Create User = E");
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
			AssertEquals("Info should be as expected.", expectedInfo, request.GetInvoiceApprovalInfo());
		}
	}
}
