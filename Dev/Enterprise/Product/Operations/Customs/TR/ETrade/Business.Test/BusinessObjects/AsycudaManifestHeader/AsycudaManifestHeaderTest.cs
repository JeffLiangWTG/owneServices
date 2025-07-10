using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	public class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestMessageModeDefaultValueIsTRE()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(TRMessageTypes.Codes.TRE, header.MessageMode);
		}

		public void TestDoNotCreateBOInGetter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			_ = header.GuaranteeType;
			AssertEquals(0, header.Guarantees.Count);
			header.GuaranteeType = "TEST";
			AssertEquals(1, header.Guarantees.Count);

			header.Guarantees.RemoveAndDeleteAll();
			_ = header.GuaranteeRefNo;
			AssertEquals(0, header.Guarantees.Count);
			header.GuaranteeRefNo = "1";
			AssertEquals(1, header.Guarantees.Count);

			header.Guarantees.RemoveAndDeleteAll();
			_ = header.GuaranteeAmount;
			AssertEquals(0, header.Guarantees.Count);
			header.GuaranteeAmount = 10m;
			AssertEquals(1, header.Guarantees.Count);

			_ = header.PreviousContainerNo;
			AssertEquals(0, header.Containers.Count);
			header.PreviousContainerNo = "P1";
			Assert(header.Containers.Cast<AsycudaContainer>().Any(x => x.ContainerLevel == AsycudaManifestHeader.PreviousContainerCode));

			header.Containers.RemoveAndDeleteAll();
			_ = header.NewContainerNo;
			AssertEquals(0, header.Containers.Count);
			header.NewContainerNo = "N1";
			Assert(header.Containers.Cast<AsycudaContainer>().Any(x => x.ContainerLevel == AsycudaManifestHeader.NewContainerCode));

			_ = header.TempRegNo;
			AssertEquals(0, header.ETradeDatas.Count);
			header.TempRegNo = "1";
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.TRGNO));

			header.ETradeDatas.RemoveAndDeleteAll();
			_ = header.TempRegNoDate;
			AssertEquals(0, header.ETradeDatas.Count);
			header.TempRegNoDate = new ZDateTime(2021, 2, 23, 9, 30, 15);
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.TRGNO));

			header.ETradeDatas.RemoveAndDeleteAll();
			_ = header.DischargeRecordNo;
			AssertEquals(0, header.ETradeDatas.Count);
			header.DischargeRecordNo = "1";
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.DRNO));

			header.ETradeDatas.RemoveAndDeleteAll();
			_ = header.DischargeRecordNoDate;
			AssertEquals(0, header.ETradeDatas.Count);
			header.DischargeRecordNoDate = new ZDateTime(2021, 2, 23, 9, 30, 15);
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.DRNO));

			header.ETradeDatas.RemoveAndDeleteAll();
			_ = header.ClosureNo;
			AssertEquals(0, header.ETradeDatas.Count);
			header.ClosureNo = "1";
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.CLNO));

			header.ETradeDatas.RemoveAndDeleteAll();
			_ = header.ClosureNoDate;
			AssertEquals(0, header.ETradeDatas.Count);
			header.ClosureNoDate = new ZDateTime(2021, 2, 23, 9, 30, 15);
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.CLNO));

			header.ETradeDatas.RemoveAndDeleteAll();
			_ = header.InspectionClerk;
			AssertEquals(0, header.ETradeDatas.Count);
			header.InspectionClerk = "TOM";
			Assert(header.ETradeDatas.Cast<ETradeData>().Any(x => x.CY_Code == CusCodeDataTypeList.Codes.INSCLK));
		}

		public void TestDecimalPlaces()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertHasCustomAttribute<DecimalPlacesAttribute>(header.GetType(), "StampTaxValue", false, attr => attr.DecimalPlaces == 2);
		}

		#region ContainerNumbersTests

		public void TestPreviousContainerNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.OfType<AsycudaContainer>().FirstOrDefault(x => !x.ContainerLevel.IsEmpty && x.ContainerLevel == "P");

			AssertNull("PrevCont should not exist", container);

			header.PreviousContainerNo = "ABC123";

			container = header.Containers.OfType<AsycudaContainer>().FirstOrDefault(x => !x.ContainerLevel.IsEmpty && x.ContainerLevel == "P");
			var contPK = container.PK;

			AssertNotNull("PrevCont should exist", container);
			AssertEquals("Container No", "ABC123", container.ACN_ContainerNumber);

			Factory.Save();

			AssertEquals("Should be in db", true, container.IsInDatabase);
			var loadedContainer = Factory.Load<AsycudaContainer>(contPK);
			AssertNotNull("LoadedCont should exist", loadedContainer);
		}

		public void TestGetMessageSendingNotification()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var expectedmsg = "Your customs credentials is marked as invalid, please update your customs credentials.";
			var password = TRGlbStaffWrapper.Get(GlbStaff.CurrentUser).TRBPassword;
			GlbStaff.CurrentUser.GS_EmailAddress = string.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("Missing Email Address", ASYCUDA.Business.ValidationConstants.MissingEmailAddress + "\r\n", header.GetMessageSendingNotificationHelper().GetNotifications());

				GlbStaff.CurrentUser.GS_EmailAddress = "whatisup@gmail.com";
				AssertEquals("Not Missing Email Address", ZString.Empty, header.GetMessageSendingNotificationHelper().GetNotifications());

				password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				AssertEquals("If credentials are Invalid", expectedmsg + "\r\n", header.GetMessageSendingNotificationHelper().GetNotifications());

				password.GP_UserID = "1234";
				password.CurrentDecryptedPassword = "xxx";
				password.GP_CertificateAuthority = "TÜBİTAK";
				password.TR_Chipset = "EKART";
				password.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";

				AssertEquals("If credentials are valid", ZString.Empty, header.GetMessageSendingNotificationHelper().GetNotifications());
			});
		}

		public void TestPreviousContainerDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.PreviousContainerNo = "ABC123";

			var container = header.Containers.OfType<AsycudaContainer>().FirstOrDefault(x => !x.ContainerLevel.IsEmpty && x.ContainerLevel == "P");
			var contPK = container.PK;

			AssertNotNull("PrevCont should exist", container);
			Factory.Save();

			var loadedContainer = Factory.Load<AsycudaContainer>(contPK);
			AssertNotNull("LoadedCont should exist", loadedContainer);

			header.PreviousContainerNo = ZString.Empty;
			Factory.Save();

			loadedContainer = Factory.Load<AsycudaContainer>(contPK);
			AssertNull("LoadedCont should no longer exist", loadedContainer);
		}

		public void TestNewContainerNew()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.OfType<AsycudaContainer>().FirstOrDefault(x => !x.ContainerLevel.IsEmpty && x.ContainerLevel == "N");

			AssertNull("NewCont should not exist", container);

			header.NewContainerNo = "ABC123";

			container = header.Containers.OfType<AsycudaContainer>().FirstOrDefault(x => !x.ContainerLevel.IsEmpty && x.ContainerLevel == "N");
			var contPK = container.PK;

			AssertNotNull("NewCont should exist", container);
			AssertEquals("Container No", "ABC123", container.ACN_ContainerNumber);

			Factory.Save();

			AssertEquals("Should be in db", true, container.IsInDatabase);
			var loadedContainer = Factory.Load<AsycudaContainer>(contPK);
			AssertNotNull("LoadedCont should exist", loadedContainer);
		}

		public void TestNewContainerDelete()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.NewContainerNo = "ABC123";

			var container = header.Containers.OfType<AsycudaContainer>().FirstOrDefault(x => !x.ContainerLevel.IsEmpty && x.ContainerLevel == "N");
			var contPK = container.PK;

			AssertNotNull("NewCont should exist", container);
			Factory.Save();

			var loadedContainer = Factory.Load<AsycudaContainer>(contPK);
			AssertNotNull("LoadedCont should exist", loadedContainer);

			header.NewContainerNo = ZString.Empty;
			Factory.Save();

			loadedContainer = Factory.Load<AsycudaContainer>(contPK);
			AssertNull("LoadedCont should no longer exist", loadedContainer);
		}

		public void TestExistContaierLoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.NewContainerNo = "ABC123";
			header.PreviousContainerNo = "CDE123";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var headerReloaded = newFactory.Load<AsycudaManifestHeader>(header.PK);
			AssertEquals("ABC123", headerReloaded.NewContainerNo);
			AssertEquals("CDE123", headerReloaded.PreviousContainerNo);
		}

		#endregion

		public void TestHumanReadableNameCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU1987";
			AssertEquals("E-Trade ULU1987", header.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals(Core.Constants.CountryCodes.Turkey, header.AMA_RN_NKCountry);
			AssertEquals(ApplicationCodeTypeList.Codes.TRETrade, header.AMA_ApplicationCode);
			AssertEquals(TRETradeManifestTypes.Codes.TRETrade, header.AMA_ManifestType);
			AssertEquals(ShipmentTypeList.Codes.Import23, header.AMA_Nature);
			AssertEquals(TransportTypeList.Codes.Air, header.AMA_TransportMode);
		}

		[TestDate(2020, 12, 24)]
		public void TestSetDefaultValuesWhenAMA_NatureChanged()
		{
			var testDate = new ZDateTime(2020, 12, 24);

			var header = Factory.New<AsycudaManifestHeader>();
			Assert("Header should be import by default", header.IsImport);
			AssertEquals("should be Today(test date)", testDate, header.AMA_DateAtCustomsOffice);
			AssertEquals("should be 4000", AsycudaManifestHeader.Constants.DefaultImportProcedureCode, header.ProcedureCode);
			AssertEquals("should be TR", Core.Constants.CountryCodes.Turkey, header.AMA_RN_NKConveyanceNationality);
			AssertEquals("should be TRIST", AsycudaManifestHeader.Constants.DefaultPortOfFirstArrival, header.AMA_RL_NKPortOfFirstArrival);

			header.AMA_DateAtCustomsOffice = ZDateTime.Empty;
			header.ProcedureCode = ZString.Empty;
			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			header.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertEquals("should be Today(test date)", testDate, header.AMA_DateAtCustomsOffice);
			AssertEquals("should be 1000", AsycudaManifestHeader.Constants.DefaultExportProcedureCode, header.ProcedureCode);
			AssertEquals("should not default for export", ZString.Empty, header.AMA_RN_NKConveyanceNationality);
			AssertEquals("should not default for export", ZString.Empty, header.AMA_RL_NKPortOfFirstArrival);
			AssertEquals("should be TR", Core.Constants.CountryCodes.Turkey, header.DepartureCountryCode);
		}

		public void TestDefaultFieldValue()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			Assert("Header should be import by default", header.IsImport);

			var flightNumber1 = "TK 123";
			header.DepartureFlight = flightNumber1;
			AssertEquals("should default from DepartureFlight", flightNumber1, header.AMA_Voyage);

			var officeCode1 = "TR340300";
			header.AMA_CustomsOffice = ZString.Empty;
			header.PresentationCustomsOffice = officeCode1;
			AssertEquals("should default from PresentationCustomsOffice", officeCode1, header.ImportExportCustomsOffice);
			AssertEquals("should default from PresentationCustomsOffice", officeCode1, header.DischargeLoadingCustomsOffice);
			AssertEquals("should default from PresentationCustomsOffice", officeCode1, header.AMA_CustomsOffice);

			var flightNumber2 = "TK 456";
			header.DepartureFlight = flightNumber2;
			AssertEquals("should not default if not empty", flightNumber1, header.AMA_Voyage);

			var officeCode2 = "TR350000";
			header.PresentationCustomsOffice = officeCode2;
			AssertEquals("should not default if not empty", officeCode1, header.ImportExportCustomsOffice);
			AssertEquals("should not default if not empty", officeCode1, header.DischargeLoadingCustomsOffice);
			AssertEquals("should not default if not empty", officeCode1, header.AMA_CustomsOffice);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_Voyage = ZString.Empty;
			header.ImportExportCustomsOffice = ZString.Empty;
			header.DischargeLoadingCustomsOffice = ZString.Empty;
			header.AMA_CustomsOffice = ZString.Empty;

			header.DepartureFlight = flightNumber1;
			AssertEquals("should default from DepartureFlight", flightNumber1, header.AMA_Voyage);

			header.PresentationCustomsOffice = officeCode1;
			AssertEquals("should default from PresentationCustomsOffice", officeCode1, header.ImportExportCustomsOffice);
			AssertEquals("should default from PresentationCustomsOffice", officeCode1, header.DischargeLoadingCustomsOffice);
			AssertEquals("should not default for export", ZString.Empty, header.AMA_CustomsOffice);
		}

		public void TestSavingSetsJobReference()
		{
			TestConnection.BeginTransaction(); // Updating next number fountain value for the test
			try
			{
				Env.NumberFountains.TRETradeJobReference.SetNext(Factory, 5555);
				var header = Factory.New<AsycudaManifestHeader>();
				header.OnSaving();
				AssertEquals("ETG0005555", header.AMA_JobReference);
			}
			finally
			{
				TestConnection.RollbackTransaction(); // Updating next number fountain value for the test
			}
		}

		public void TestSetNatureIfRequired()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_TransportMode = TransportTypeList.Codes.Road;
			Factory.Save();

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertEquals(TransportTypeList.Codes.Air, header.AMA_TransportMode);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Factory.Save();

			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals(ShipmentTypeList.Codes.Import23, header.AMA_Nature);
		}

		public void TestBusinessObjectWithRelatedEventsOnETrade()
		{
			var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header1.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header1.AMA_TransportMode = TransportTypeList.Codes.Road;

			var bill1 = header1.Bills.AddNew();
			var tax1 = bill1.AsycudaTaxes.AddNew();

			var bill2 = header1.Bills.AddNew();
			var tax2 = bill2.AsycudaTaxes.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newHeader = newFactory.Load<AsycudaManifestHeader>(header1.PK);

			Assert("Bill1 should be in the list of related objects", newHeader.BusinessObjectsWithRelatedEvents.Any(x => x.PK == bill1.PK));
			Assert("Tax1 should be in the list of related objects", newHeader.BusinessObjectsWithRelatedEvents.Any(x => x.PK == tax1.PK));
			Assert("Bill2 should be in the list of related objects", newHeader.BusinessObjectsWithRelatedEvents.Any(x => x.PK == bill2.PK));
			Assert("Tax2 should be in the list of related objects", newHeader.BusinessObjectsWithRelatedEvents.Any(x => x.PK == tax2.PK));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return CreateBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = CreateBusinessObject(factory);
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "AA1234";
			manifestHeader.AMA_OA_Carrier = CarrierOrgAddress.PK;
			return manifestHeader;
		}

		#region TestETradeData

		public void TestICusCodeDataTypeSupporter()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Integration.Customs.ICusCodeDataTypeSupporter supporter = header;
			supporter.AssertType(typeof(ETradeData), ApplicationCodeTypeList.Codes.TRETrade);
			var etradeDataInfo = header.ETradeDatas.AddNew();
			etradeDataInfo.CY_Data = "testCYData";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(etradeDataInfo.PK);
			AssertEquals(typeof(ETradeData), codeData.GetType());
		}

		public void TestETradeDataCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var etradeDataInfo = header.ETradeDatas.AddNew();
			etradeDataInfo.CY_Data = "testCYData";

			Factory.Save();

			AssertEquals("ETradeDatas should contain 1 records", 1, header.ETradeDatas.Count);
			AssertNotNull("etradeDataInfo should exist", new BusinessObjectFactory().Load<ETradeData>(etradeDataInfo.PK));

			header.ETradeDatas.RemoveAndDeleteAll();
			Factory.Save();

			AssertNull("ETradeDatas should be deleted", new BusinessObjectFactory().Load<ETradeData>(etradeDataInfo.PK));
		}

		public void TestTotalBoxQtyAndNumberOfBillsAndReCalc()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_ManifestQty = 25;
			var bill2 = header.Bills.AddNew();
			bill2.ABL_ManifestQty = 15;
			var bill3 = header.Bills.AddNew();
			bill3.ABL_ManifestQty = 10;

			AssertEquals(header.NumberOfBills, header.Bills.AsEnumerable().Count());
			AssertEquals(50, header.TotalBoxQty);

			header.Bills.RemoveAndDelete(bill3);
			AssertEquals(header.NumberOfBills, header.Bills.AsEnumerable().Count());
			AssertEquals(40, header.TotalBoxQty);
		}

		public void TestExchangeRateAndTotalBillValuesForHeaderAndReCalc()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;

				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 7.7448m, ZDateTime.Today);
				helper.SetExchangeRate(helper.USDCurrency, 6.8638m, ZDateTime.Today);
				helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

				var refTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
				refTestDataHelper.CreateOrGetExistingRefSysConfigType("TREPREFRCO", "TR E-Trade Precedent Freight Cost Value", "There is a Precedent Freight Cost Value for TR Customs E-Trade. The limit currency code is EUR");
				refTestDataHelper.CreateOrUpdateExistingRefSysConfig("TREPREFRCO", 3, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));

				header.Branch.Company.GC_IsReciprocal = true;
				Factory.Save();
				var bill1 = header.Bills.AddNew();
				bill1.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill1.ABL_GoodsValue = 200;
				bill1.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill1.ABL_TransportValue = 200;
				bill1.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				bill1.ABL_InsuranceValue = 200;
				bill1.ABL_OtherValue = 100;

				var bill2 = header.Bills.AddNew();
				bill2.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill2.ABL_GoodsValue = 100;
				bill2.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill2.ABL_TransportValue = 100;
				bill2.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				bill2.ABL_InsuranceValue = 100;
				bill2.ABL_OtherValue = 100;

				var bill3 = header.Bills.AddNew();
				bill3.ABL_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.Turkey;
				bill3.ABL_GoodsValue = 774.48;
				bill3.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.Turkey;
				bill3.ABL_TransportValue = 774.48;
				bill3.ABL_RX_NKInsuranceValueCurrency = Core.Constants.CurrencyCodes.Turkey;
				bill3.ABL_InsuranceValue = 774.48;
				bill3.ABL_OtherValue = 100;

				AssertEquals((ZDecimal)7.7448, helper.EURCurrency.CurrentCustomsRate);
				AssertEquals((ZDecimal)6.8638, helper.USDCurrency.CurrentCustomsRate);

				AssertEquals((ZDecimal)7.7448, header.ExchangeRate);

				AssertEquals((ZDecimal)377.25, header.CustomsValue);
				AssertEquals((ZDecimal)377.25, header.FreightValue);
				AssertEquals((ZDecimal)377.25, header.InsuranceValue);
				AssertEquals((ZDecimal)300, header.OtherValue);

				header.Bills.RemoveAndDelete(bill3);

				AssertEquals((ZDecimal)277.25, header.CustomsValue);
				AssertEquals((ZDecimal)277.25, header.FreightValue);
				AssertEquals((ZDecimal)277.25, header.InsuranceValue);
				AssertEquals((ZDecimal)200, header.OtherValue);

				var bill4 = header.Bills.AddNew();
				bill4.ABL_Incoterm = "CIF";
				bill4.ABL_TransportValue = ZDecimal.Zero;
				bill4.ABL_RX_NKTransportValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

				AssertEquals(3m, bill4.PrecedentFreightToDisplay);
				AssertEquals("Should add Precedent value to Total freight", (ZDecimal)280.25, header.FreightValue);

				bill4.ABL_Incoterm = "NNN";
				AssertEquals(0m, bill4.PrecedentFreightToDisplay);
				AssertEquals("Incoterm change should trigger header freight calculation", (ZDecimal)277.25, header.FreightValue);
			}
		}

		public void TestConvertUsingCustomsRate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				Factory.Save();

				CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
				helper.SetExchangeRate(helper.EURCurrency, 7.7448m, ZDateTime.Today);
				helper.SetExchangeRate(helper.USDCurrency, 6.8638m, ZDateTime.Today);
				helper.SetExchangeRate(helper.TRYCurrency, 1m, ZDateTime.Today);

				var originalAmount = new Money((ZDecimal)200, helper.USDCurrency);
				var cC = new RefCurrencyCurrencyConverter(GlbCompany.CurrentCompany, Factory, ZDateTime.Today, ZArchitecture.Core.ExchangeRateType.Customs, 0);
				var result = cC.ConvertRounded(originalAmount, helper.EURCurrency).Amount;

				AssertEquals((ZDecimal)7.7448, helper.EURCurrency.CurrentCustomsRate);
				AssertEquals((ZDecimal)6.8638, helper.USDCurrency.CurrentCustomsRate);
				AssertEquals((ZDecimal)7.7448, header.ExchangeRate);
				AssertEquals((ZDecimal)177.25, result);
			}
		}

		public void TestRegistrationStatusDescription()
		{
			var header = CreateBusinessObject(Factory);
			header.RegistrationStatus = "TRS";
			AssertEquals(header.RegistrationStatusDescription, "Temporary Registration Message was successful accepted");
		}
		public void TestAMA_MessageStatus()
		{
			var header = CreateBusinessObject(Factory);
			header.AMA_MessageStatus = "ERR";
			AssertEquals(header.MessageStatus, "ERR");
		}
		#endregion

		public void TestGoodLocationCode()
		{
			var header = CreateBusinessObject(Factory);
			AssertEquals(header.GoodsLocationCode, "A0001");
		}
		public void TestLocationInformation()
		{
			var header = CreateBusinessObject(Factory);
			AssertEquals(header.LocationInformation, "Test line 1");
		}

		public void TestCarrier()
		{
			var header = CreateBusinessObject(Factory);
			AssertEquals(header.AMA_OA_Carrier, CarrierOrgAddress.PK);
		}

		public void TestCheckNumberOfBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals(header.NumberOfBills, 1);
			var bill2 = header.Bills.AddNew();
			AssertEquals(header.NumberOfBills, 2);
		}

		public void TestProcedureCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.MasterBill.ABL_Procedure = "1010";
			AssertEquals("1010", header.ProcedureCode);
		}

		public void TestPackedItemRelationship()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("pack.IsOnePackedItemRelationship", true, pack.IsOnePackedItemRelationship);
		}

		public void TestTax()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_DateAtCustomsOffice = ZDateTime.Today;
				header.Branch.Company.GC_IsReciprocal = true;
				var bill1 = header.Bills.AddNew();
				bill1.ExportCountry = Core.Constants.CountryCodes.Germany;
				var pack11 = bill1.Packs.AddNew();
				bill1.ExemptionCode1 = "HK18";
				bill1.ABL_CustomsValue = 400;
				bill1.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
				var pack1 = bill1.Packs.AddNew();
				pack1.PackedItem.API_GoodsValue = 600;
				pack1.PackedItem.API_RX_NKGoodsValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
				pack1.PackedItem.API_Tariff = "2000";
				header.CalculateDuties();
				AssertEquals((ZDecimal)119, header.MasterBill.AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(z => z.AET_ChargeType == TaxCodeList.Codes.StampTax).AET_ChargeAmount);
			}
		}

		public void TestReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			CombineAssertions("Read-Only", () =>
			{
				AssertEquals("TempRegNo", false, header.TempRegNoInfo.ReadOnly);
				AssertEquals("TempRegNoDate", false, header.TempRegNoDateInfo.ReadOnly);
				AssertEquals("ClosureNo", false, header.ClosureNoInfo.ReadOnly);
				AssertEquals("ClosureNoDate", false, header.ClosureNoDateInfo.ReadOnly);
				AssertEquals("DischargeRecordNo", false, header.DischargeRecordNoInfo.ReadOnly);
				AssertEquals("DischargeRecordNoDate", false, header.DischargeRecordNoDateInfo.ReadOnly);
				AssertEquals("InspectionClerk", false, header.InspectionClerkInfo.ReadOnly);
			});
		}

		public void TestGetUpdatedMessageLog()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.TempRegNo = "999";
			header.TempRegNoDate = ZDateTime.Now;
			header.ClosureNo = "777";
			header.ClosureNoDate = ZDateTime.Now;
			header.DischargeRecordNo = "666";
			header.DischargeRecordNoDate = ZDateTime.Now;
			header.InspectionClerk = "inc clk";

			AssertEquals(7, header.Logs.Find(l => l.SL_SE_NKEvent == Events.EditedARecord.Code).Count());
		}

		public void TestLabelText()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			CombineAssertions("Voyage label for sea transport", () =>
			{
				AssertEquals("Border Voyage", header.VoyageFlightNoLabel.Caption);
				AssertEquals("Departure Voyage", header.DepartureFlightLabel.Caption);
			});

			header.AMA_TransportMode = "ROA";
			CombineAssertions("Truck Ref label for road transport", () =>
			{
				AssertEquals("Border Truck Ref", header.VoyageFlightNoLabel.Caption);
				AssertEquals("Departure Truck Ref", header.DepartureFlightLabel.Caption);
			});

			header.AMA_TransportMode = "AIR";
			CombineAssertions("Flight label for air transport", () =>
			{
				AssertEquals("Border Flight", header.VoyageFlightNoLabel.Caption);
				AssertEquals("Departure Flight", header.DepartureFlightLabel.Caption);
			});

			header.AMA_TransportMode = "";
			CombineAssertions("Flight label for other transport", () =>
			{
				AssertEquals("Border Flight", header.VoyageFlightNoLabel.Caption);
				AssertEquals("Departure Flight", header.DepartureFlightLabel.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			CurrencyTestHelper helper = new CurrencyTestHelper(Factory);
			helper.SetRefValues();
			CarrierOrgAddress.OA_Code = "CPW1";
			CarrierOrgAddress.OA_Address1 = "Test line 1";
			CarrierOrgAddress.OA_OH = OrganizationHeader.PK;
			CarrierOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Turkey;
			OrgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Turkey;
			OrgCusCode.OK_CustomsRegNo = "A0001";
			OrgCusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			OrgCusCode.OK_OA_PremisesAddress = CarrierOrgAddress.PK;
			OrgCusCode.OK_OH = OrganizationHeader.PK;
			OrganizationHeader.CustomsCodes.Add(OrgCusCode);
			Factory.Save();
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		OrgHeader OrganizationHeader { get { if (orgHeader == null) { orgHeader = Factory.NewWithValidTestData<OrgHeader>(); } return orgHeader; } }
		OrgAddress CarrierOrgAddress
		{
			get
			{
				if (carrierOrgAddress == null)
				{
					carrierOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
				}
				return carrierOrgAddress;
			}
		}
		OrgCusCode OrgCusCode
		{
			get
			{
				if (orgCusCode == null)
				{
					orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
				}
				return orgCusCode;
			}
		}
		OrgHeader orgHeader;
		OrgAddress carrierOrgAddress;
		OrgCusCode orgCusCode;
	}
}
