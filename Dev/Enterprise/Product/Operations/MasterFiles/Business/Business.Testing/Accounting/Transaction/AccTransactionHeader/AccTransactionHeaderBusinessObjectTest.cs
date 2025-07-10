using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTransactionHeader))]
	public class AccTransactionHeaderBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		#region Data Refresh Bus Update Test Cases

		[SuspendCriticalValidation]
		public virtual void TestShouldApplyDataRefreshBusUpdate()
		{
			var expectedType = GetExpectedBusinessObjectType();
			var transactionSubscriber = (AccTransactionHeader)Factory.NewWithValidTestData(expectedType);
			ForceSavingByFactoryIfApplicable(transactionSubscriber);
			Factory.Save();

			var factoryOfPublisher = new BusinessObjectFactory();
			var transactionPublisher = factoryOfPublisher.Load<AccTransactionHeader>(transactionSubscriber.PK);

			var dataRefreshBusUpdateDeciderMock = new Mock<IDataRefreshBusUpdateActionDecider>();

			AccTransactionHeader passedSubscriber = null, passedPublisher = null;
			ZPropertyInfo[] passedStrictPropertyInfo = null;
			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTransactionHeader>(), It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo[]>()))
				.Returns(true)
				.Callback<DataRefreshAction, BusinessObject, BusinessObject, ZPropertyInfo[]>((action, subscriber, publisher, strictPropertyInfo) =>
					{
						if (subscriber.GetType() == expectedType) //to overcome multiple instances around the row
						{
							passedSubscriber = (AccTransactionHeader)subscriber;
							passedPublisher = (AccTransactionHeader)publisher;
							passedStrictPropertyInfo = strictPropertyInfo;
						}
					});
			ObjectFactory.Substitute(dataRefreshBusUpdateDeciderMock.Object);

			transactionPublisher.AH_Desc = "some other description";
			factoryOfPublisher.Save();

			dataRefreshBusUpdateDeciderMock.Verify(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTransactionHeader>(), It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo[]>()));
			AssertEquals("Subscriber passed into data refresh bus decider.", transactionSubscriber, passedSubscriber);
			AssertEquals("Publisher passed into data refresh bus decider.", transactionPublisher, passedPublisher);

			var currentPropertiesWithStrictConcurrency = transactionSubscriber.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.IsPersistent && info.ConcurrencyPolicy == ConcurrencyPolicy.Strict);
			var typeName = GetExpectedBusinessObjectType().Name;
			var errorMessage = $"Concurrency policy of some {typeName} Properties have changed. Please update list of strict concurrency property infos passed into data refresh bus decider in {typeName} class.";
			AssertContainsExactElementsInAnyOrder(errorMessage, currentPropertiesWithStrictConcurrency, passedStrictPropertyInfo);
		}

		[SuspendCriticalValidation]
		public virtual void TestCanApplyDataRefresh_SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged_BusinessContext()
		{
			var (transactionSubscriber, transactionPublisher, dataRefreshBusUpdateDeciderMock) = SetUpForCanApplyDataRefreshTests();

			dataRefreshBusUpdateDeciderMock.Setup(x => x.IsStrictPropertyChangedOnSubscriberOrPublisher(transactionPublisher, It.IsAny<ZPropertyInfo[]>())).Returns(true);
			AssertEquals("Pre-condition : Publisher not deleted", false, transactionPublisher.IsDeleted);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertEquals("When publisher not deleted and data refresh bus will update strict properties", expected: true, transactionSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));

			transactionSubscriber.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged);
			dataRefreshBusUpdateDeciderMock.Setup(x => x.IsStrictPropertyChangedOnSubscriberOrPublisher(transactionPublisher, It.IsAny<ZPropertyInfo[]>())).Returns(false);
			AssertEquals("Pre-condition : Publisher not deleted", false, transactionPublisher.IsDeleted);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertEquals("When publisher not deleted and data refresh bus is will NOT update strict properties", expected: false, transactionSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));

			transactionPublisher.Delete();
			AssertEquals("Pre-condition : Publisher deleted", true, transactionPublisher.IsDeleted);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertEquals("When publisher not deleted and data refresh bus is will NOT update strict properties", expected: true, transactionSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));
		}

		[SuspendCriticalValidation]
		public virtual void TestUnmanagedRegisteredEditableChildObjectForDataRefresh_DependencyOn_ShouldApplyDataRefreshBusUpdate()
		{
			var (transactionSubscriber, transactionPublisher, dataRefreshBusUpdateDeciderMock) = SetUpForCanApplyDataRefreshTests();

			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTransactionHeader>(), It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo[]>()))
				.Returns(true);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertUnmanagedRegisteredEditableChildObjectForDataRefresh(transactionSubscriber, isUnmanaged: false);

			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTransactionHeader>(), It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo[]>()))
				.Returns(false);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertUnmanagedRegisteredEditableChildObjectForDataRefresh(transactionSubscriber, isUnmanaged: true);
		}

		[SuspendCriticalValidation]
		public virtual void TestUnmanagedRegisteredEditableChildObjectForDataRefresh_Context_SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged()
		{
			var (transactionSubscriber, transactionPublisher, dataRefreshBusUpdateDeciderMock) = SetUpForCanApplyDataRefreshTests();

			dataRefreshBusUpdateDeciderMock.Setup(x => x.IsStrictPropertyChangedOnSubscriberOrPublisher(It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo>(), It.IsAny<ZPropertyInfo>())).Returns(false);
			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<AccTransactionHeader>(), It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo[]>()))
				.Returns(true);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertEquals("Post-Condition: SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged context", expected: false, transactionSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));
			AssertUnmanagedRegisteredEditableChildObjectForDataRefresh(transactionSubscriber, isUnmanaged: false);

			transactionSubscriber.RemoveContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged);
			dataRefreshBusUpdateDeciderMock.Setup(x => x.IsStrictPropertyChangedOnSubscriberOrPublisher(It.IsAny<AccTransactionHeader>(), It.IsAny<ZPropertyInfo>(), It.IsAny<ZPropertyInfo>())).Returns(true);
			((ICanApplyDataRefresh)transactionSubscriber).CanApplyDataRefresh(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, transactionPublisher);
			AssertEquals("Post-Condition: SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged context", expected: true, transactionSubscriber.HasContext(BusinessContext.SkipDataRefreshBusUpdateAsLedgerOrTransactionTypeIsCriticallyChanged));
			AssertUnmanagedRegisteredEditableChildObjectForDataRefresh(transactionSubscriber, isUnmanaged: true);
		}

		protected virtual void AssertUnmanagedRegisteredEditableChildObjectForDataRefresh(AccTransactionHeader invoice, bool isUnmanaged)
		{
			Assert(true);
		}

		(AccTransactionHeader transactionSubscriber, AccTransactionHeader transcationPublisher, Mock<IDataRefreshBusUpdateActionDecider> dataRefreshBusUpdateActionDecidermock) SetUpForCanApplyDataRefreshTests()
		{
			var expectedType = GetExpectedBusinessObjectType();
			var subscriber = (AccTransactionHeader)Factory.NewWithValidTestData(expectedType);
			ForceSavingByFactoryIfApplicable(subscriber);
			Factory.Save();

			var factoryOfPublisher = new BusinessObjectFactory();
			var publisher = factoryOfPublisher.Load<AccTransactionHeader>(subscriber.PK);

			var dataRefreshBusUpdateDeciderMock = new Mock<IDataRefreshBusUpdateActionDecider>();
			ObjectFactory.Substitute(dataRefreshBusUpdateDeciderMock.Object);

			return (subscriber, publisher, dataRefreshBusUpdateDeciderMock);
		}

		#endregion

		public void TestZDecimalsHaveCorrectDecimalPlacesAccTransactionHeader()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.NewWithValidTestData<AccTransactionHeader>();

			var localList = new List<string>
			{
				nameof(header.AH_OutstandingAmount),
				nameof(header.AH_GSTAmount),
				nameof(header.AH_InvoiceAmount),
				nameof(header.AH_WithholdingTax)
			};

			var osList = new List<string>
			{
				nameof(header.AH_OSTotal)
			};

			var exList = new List<string>
			{
				nameof(header.AH_ExchangeRate)
			};

			var tester = new DecimalPlacesAttributeTester(header);
			tester.CheckLocalCurrency(localList, nameof(header.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(header.OSCurrencyDecimals), nameof(header.AH_RX_NKTransactionCurrency), header);
			tester.CheckExchangeRate(exList, nameof(header.ExchangeRateDecimalPlaces));
		}

		public void TestTransactionHeaderLedgerIsCompatibleWithTransactionHeaderType()
		{
			var header = GetNewBusinessObject() as AccTransactionHeader;
			AssertNotNull(header);
			if (!header.GetType().IsAssignableFrom(typeof(AccTransactionHeader)))
			{
				Assert("The ledger is empty", !header.AH_Ledger.IsEmpty);
				Assert(string.Format("The transaction type is empty when ledger={0}", header.AH_Ledger), !header.AH_TransactionType.IsEmpty);
				Assert(string.Format("The ledger '{0}' is not compatible with transaction type '{1}'", header.AH_Ledger, header.AH_TransactionType), AccTransactionHeaderCompatibilityMatrix.IsLedgerCompatibleWithTransactionType(header.AH_Ledger, header.AH_TransactionType));
			}
		}

		[TestDate(2012, 11, 11)]
		public void TestComplianceSequence()
		{
			SetupForSave();

			AccComplianceSequence complianceSequence1 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence1.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence1.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			complianceSequence1.XD_Code = "AAA";
			complianceSequence1.XD_SequenceClass = "TXI";
			complianceSequence1.XD_Prefix = "xyz-222";
			complianceSequence1.XD_StartNumber = 1;
			complianceSequence1.XD_EndNumber = 99;
			complianceSequence1.XD_MaximumNumberDigits = 4;
			complianceSequence1.XD_NextNumber = 25;
			complianceSequence1.XD_IsActive = true;
			complianceSequence1.XD_ExpiryDate = new ZDateTime(2012, 12, 12);

			AccComplianceSequence complianceSequence2 = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence2.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence2.XD_SequenceClass = "TXI";
			complianceSequence2.XD_Prefix = "abc-001";
			complianceSequence2.XD_StartNumber = 100;
			complianceSequence2.XD_EndNumber = 199;
			complianceSequence2.XD_MaximumNumberDigits = 4;
			complianceSequence2.XD_NextNumber = 25;
			complianceSequence2.XD_IsActive = false;

			Factory.Save();

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_ComplianceSubType = "TXI";
			header.AH_TransactionReference = "";
			AssertEquals("should find sequence base on subtype", complianceSequence1.PK, header.ComplianceSequence.PK);

			header.AH_TransactionReference = "abc-0010120";
			AssertEquals("should find sequence base on transaction reference", complianceSequence2.PK, header.ComplianceSequence.PK);
		}

		[TestDate(2012, 11, 11)]
		public void TestComplianceSequenceFromTransactionComplianceBook()
		{
			SetupForSave();

			var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
			complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			complianceSequence.XD_SequenceClass = "TXI";
			complianceSequence.XD_Prefix = "abc-001";
			complianceSequence.XD_StartNumber = 100;
			complianceSequence.XD_EndNumber = 199;
			complianceSequence.XD_MaximumNumberDigits = 4;
			complianceSequence.XD_NextNumber = 25;
			complianceSequence.XD_IsActive = false;

			Factory.Save();

			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_GB = GlbBranch.CurrentBranch.PK;
			header.AH_ComplianceSubType = "";
			header.AH_TransactionReference = "";

			AssertNull(header.ComplianceSequence);

			header.AH_XD_ComplianceBook = complianceSequence.PK;

			AssertEquals("Should find sequence base on transaction compliance book", complianceSequence.PK, header.ComplianceSequence.PK);
		}

		public void TestComplianceSubTypeList()
		{
			AssertComplianceSubTypeList(Constants.CountryCodes.Peru, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "TXI", "DSB", "TCD", "TCR", "TBO", "CAE", "CMA", "HON", "MQR", "NXI", "OTR", "SSP", "NCR", "NCD", "TBC" } });
			AssertComplianceSubTypeList(Constants.CountryCodes.VietNam, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "EXI", "TXI" } });
			AssertComplianceSubTypeList(Constants.CountryCodes.Indonesia, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "TXI", "BKP", "JKP", "T01", "T02", "T03", "T04", "T05", "T06", "T07", "T08", "T09" } });
			AssertComplianceSubTypeList(Constants.CountryCodes.Poland, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "TXI", "TCD", "DCR", "DSB" } });
			AssertComplianceSubTypeList(Constants.CountryCodes.Taiwan, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "TXI", "TDP", "TXE", "TXP", "TXC", "TXS", "TCR", "NTI", "NTC", "TCD", "TDC", "TDI", "TSD", "TSX", "TCE", "XCL", "ZNG" } });
			AssertComplianceSubTypeList(Constants.CountryCodes.Guatemala, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "TXI", "TCR", "TCD", "XCL", "XCR" } });
			AssertComplianceSubTypeList(Constants.CountryCodes.Honduras, new ExpectedComplianceSubTypeValues() { ValueList = new List<string> { "TXI", "TCR", "TCD", "XCL" } });

			AssertComplianceSubTypeList(
				Constants.CountryCodes.Italy,
				new ExpectedComplianceSubTypeValues()
				{
					Ledger = LedgerTypes.AccountsPayable,
					ValueList = new List<string> { "API", "APS", "INT", "INI", "APV", "XAP", "G28", "XLP" }
				},
				new ExpectedComplianceSubTypeValues()
				{
					Ledger = LedgerTypes.AccountsReceivable,
					ValueList = new List<string> { "ARI", "ARS", "ARE", "ARN", "ARV", "G26", "XCL" }
				}
			);

			AssertComplianceSubTypeList(
				Constants.CountryCodes.KoreaSouth,
				new ExpectedComplianceSubTypeValues()
				{
					Ledger = LedgerTypes.AccountsReceivable,
					TransactionType = TransactionTypes.Invoice,
					ValueList = new List<string> { "101", "102", "301" }
				},
				new ExpectedComplianceSubTypeValues()
				{
					Ledger = LedgerTypes.AccountsReceivable,
					TransactionType = TransactionTypes.CreditNote,
					ValueList = new List<string> { "201", "202", "401" }
				}
			);
		}

		struct ExpectedComplianceSubTypeValues
		{
			public string Ledger;
			public string TransactionType;
			public IEnumerable<string> ValueList;
		}

		void AssertComplianceSubTypeList(string countryCode, params ExpectedComplianceSubTypeValues[] expectedValues)
		{
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			var header = GetNewBusinessObject() as AccTransactionHeader;
			var actualValues = new List<string>();
			foreach (var codeDescriptionBool in header.ComplianceSubTypeList.Cast<ICodeDescription>())
			{
				actualValues.Add(codeDescriptionBool.Code);
			}

			foreach (var expectedValue in expectedValues)
			{
				if ((expectedValue.Ledger == null || header.AH_Ledger == expectedValue.Ledger) && (expectedValue.TransactionType == null || header.AH_TransactionType == expectedValue.TransactionType))
				{
					AssertContainsExactElementsInAnyOrder("expected list and actual list should be same", expectedValue.ValueList, actualValues);
				}
			}
		}

		public void TestCheckRequesterUserFullName()
		{
			AccTransactionHeader header = GetNewBusinessObject() as AccTransactionHeader;
			header.AH_SystemCreateUser = string.Empty;
			AssertEquals("CheckRequesterUserFullName", string.Empty, header.CheckRequesterUserFullName);

			header.AH_SystemCreateUser = "XYZ"; // Random String
			AssertEquals("CheckRequesterUserFullName", string.Empty, header.CheckRequesterUserFullName);

			header.AH_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("CheckRequesterUserFullName", GlbStaff.CurrentUser.GS_FullName, header.CheckRequesterUserFullName);
		}

		public void TestJobNumber()
		{
			AccTransactionHeader header = GetNewBusinessObject() as AccTransactionHeader;

			AssertNull("PreCondition: Expected Job to be null", header.Job);
			AssertEquals("Empty string expected for null Job Number", "", header.JobNumber);

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = "S00001234";
			header.AH_JH = job.PK;
			AssertEquals("Job differs from expected Job", job.PK, header.Job.PK);
			AssertEquals("Job Number differs from expected", job.JH_JobNum, header.JobNumber);
		}

		public void TestAH_GC()
		{
			AccTransactionHeader header = GetNewBusinessObject() as AccTransactionHeader;

			GlbBranch branch = Factory.New<GlbBranch>();
			GlbCompany company = Factory.New<GlbCompany>();
			branch.GB_GC = company.PK;
			header.AH_GB = branch.PK;

			AssertEquals("AH_GB differs from expected", branch.PK, header.AH_GB);
			AssertEquals("AH_GC differs from expected", company.PK, header.AH_GC);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestPostGLFlag()
		{
			AccTransactionHeader header = GetNewBusinessObject() as AccTransactionHeader;

			((INeedRow)header).Row[AccTransactionHeader.Schema.AH_PostToGL] = "M";
			AssertEquals("M", header.AH_PostToGL);

			((INeedRow)header).Row[AccTransactionHeader.Schema.AH_PostToGL] = "Y";
			AssertEquals("Y", header.AH_PostToGL);

			((INeedRow)header).Row[AccTransactionHeader.Schema.AH_PostToGL] = "N";
			AssertEquals("N", header.AH_PostToGL);
		}

		protected virtual void ForceSavingByFactoryIfApplicable(AccTransactionHeader header)
		{
			Assert("IsSavedByFactory has an unexpected value", header.IsSavedByFactory);
		}

		/// <summary>
		/// This method should be used to setup your TransactionHeader object for saving. Always call base.
		/// </summary>
		protected virtual void SetupForSave()
		{
		}
	}
}
