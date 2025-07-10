using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	public abstract class TestDeclarationCreator
	{
		public TestDeclarationCreator(JobDeclaration declaration)
		{
			factory = declaration.Factory;
			Declaration = declaration;
			declaration.DisableDefaultPackingInformation = true;
		}

		public readonly JobDeclaration Declaration;

		public static void InitialiseFlightVesselReferenceData(BusinessObjectFactory factory)
		{
			var refHelper = new UniversalReferenceTestDataHelper(factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "QF117", "QF117", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "BUNGA BIDARA", "BUNGA BIDARA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public virtual void SetupTestConsignmentDetails()
		{
			Declaration.JE_OH_ShippingLine = ShippingLine.PK;
			Declaration.JE_OH_Forwarder = Forwarder.PK;

			Declaration.JE_HouseBill = "HOUSETEST123";
			Declaration.JE_GoodsDescription = "DRIED VET BILLS";
			Declaration.JE_TotalWeight = 20;
			Declaration.JE_TotalWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			Declaration.JE_TotalVolume = 0.020m;
			Declaration.JE_TotalVolumeUnit = Enterprise.Core.Constants.Volume.CubicMetres;
			Declaration.JE_TotalNoOfPacks = 2;
			Declaration.JE_TotalNoOfPacksPackType = "PK";

			Declaration.JE_EntrySubmittedDate = new ZDateTime(2004, 1, 1);
		}

		public virtual void SetupTestContainer()
		{
			CusContainer cusContainer = Declaration.CusContainers.AddNew();
			cusContainer.CO_ContainerNumber = "OOCL0000006";
			cusContainer.CO_FCL_LCL_AIR = ContainerModeList.Codes.FCL;
			cusContainer.CO_ContainerSize = ContainerSizeList.Codes.ContainerIc20Ft;
		}

		public void SetEDITransmitDateToToday()
		{
			Declaration.JE_EntrySubmittedDate = ZDateTime.Today;
		}

		public void SetImportDateToToday()
		{
			Declaration.JE_DateOfArrival = ZDateTime.Today;
		}

		public void SetExportDateToToday()
		{
			Declaration.JE_ExportDate = ZDateTime.Today;
		}

		public void SetUniqueishJobNumber()
		{
			ZDateTime now = ZDateTime.Now;
			string numberString = now.Month.ToString() + "-" + now.Day.ToString() + "/" + now.Hour.ToString() + "-" + now.Minute.ToString() + "-" + now.Second.ToString();
			Declaration.JE_DeclarationReference = numberString;
		}

		public void SetupTestForAir()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			Declaration.JE_VoyageFlightNo = "QF117";
			Declaration.JE_MasterBill = "081-11111111";
		}

		public void SetupTestForSea()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_VesselName = "BUNGA BIDARA";
			Declaration.JE_VoyageFlightNo = "109";
			Declaration.JE_MasterBill = "OBL123456";
		}

		public virtual void SetupTestForImportFromAU()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Importer = LocalParty.PK;
			Declaration.JE_OH_Supplier = OSParty.PK;
			Declaration.JE_DateOfArrival = new ZDateTime(2004, 1, 1);
			Declaration.JE_RL_NKOrigin = uNLOCOSydney;
			Declaration.JE_RL_NKPortOfLoading = uNLOCOSydney;
			Declaration.JE_RL_NKPortOfArrival = uNLOCOAuckland;
			Declaration.JE_RL_NKFinalDestination = uNLOCOAuckland;
		}

		public virtual void SetupTestForExportToAU()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_OH_Importer = OSParty.PK;
			Declaration.JE_OH_Supplier = LocalParty.PK;
			Declaration.JE_ExportDate = new ZDateTime(2004, 1, 1);
			Declaration.JE_RL_NKOrigin = uNLOCOAuckland;
			Declaration.JE_RL_NKPortOfLoading = uNLOCOAuckland;
			Declaration.JE_RL_NKPortOfArrival = uNLOCOSydney;
			Declaration.JE_RL_NKFinalDestination = uNLOCOSydney;
		}

		public void SetupTestAddRemarks()
		{
			Declaration.CustomsMessageRemarks = "I LIKE REMARKS";
		}

		#region Implementation
		protected BusinessObjectFactory factory;
		protected string uNLOCOSydney = "AUSYD";
		protected string uNLOCOAuckland = "NZAKL";

		protected OrgHeader fLocalParty;
		public OrgHeader LocalParty
		{
			get
			{
				if (fLocalParty == null)
				{
					fLocalParty = GetTestOrgHeaderLocalParty();
				}
				return fLocalParty;
			}
		}

		protected OrgHeader fOSParty;
		public OrgHeader OSParty
		{
			get
			{
				if (fOSParty == null)
				{
					fOSParty = GetTestOrgHeaderOSParty();
				}
				return fOSParty;
			}
		}

		protected OrgHeader fShippingLine;
		protected OrgHeader ShippingLine
		{
			get
			{
				if (fShippingLine == null)
				{
					fShippingLine = GetTestOrgHeaderShippingLine();
				}
				return fShippingLine;
			}
		}

		protected OrgHeader fForwarder;
		protected OrgHeader Forwarder
		{
			get
			{
				if (fForwarder == null)
				{
					fForwarder = GetTestOrgHeaderForwarder();
				}
				return fForwarder;
			}
		}

		protected RefCurrency fRefCurrencyNZD;
		protected RefCurrency RefCurrencyNZD
		{
			get
			{
				if (fRefCurrencyNZD == null)
				{
					fRefCurrencyNZD = GetRefCurrencyNZD();
				}
				return fRefCurrencyNZD;
			}
		}

		protected OrgHeader GetTestOrgHeaderLocalParty()
		{
			OrgHeader orgHeader = GetTestOrgHeader();
			orgHeader.OH_Code = "ZZIMPE";
			orgHeader.OH_FullName = "ADULT BOOKS LTD";
			orgHeader.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			orgHeader.MainAddress.OA_Address2 = "LOCATED IN NZAKL";
			orgHeader.OH_RL_NKClosestPort = "NZAKL";

			orgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "00782903F");

			return orgHeader;
		}

		protected OrgHeader GetTestOrgHeaderOSParty()
		{
			OrgHeader orgHeader = GetTestOrgHeader();
			orgHeader.OH_Code = "ZZSUPA";
			orgHeader.OH_FullName = "TEST SUPPLIER AU";
			orgHeader.MainAddress.OA_Address1 = "TEST CODE FOR NZ CUSTOMS";
			orgHeader.MainAddress.OA_Address2 = "LOCATED IN AUSYD";
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			orgHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "00710841Y");

			return orgHeader;
		}

		protected OrgHeader GetTestOrgHeader()
		{
			OrgHeader orgHeader = OrgHeader.New(factory);
			orgHeader.OH_IsActive = true;
			orgHeader.OH_IsConsignee = true;
			orgHeader.OH_IsConsignor = true;
			orgHeader.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;

			return orgHeader;
		}

		protected OrgHeader GetTestOrgHeaderShippingLine()
		{
			OrgHeader orgHeader = GetTestOrgHeader();
			orgHeader.OH_Code = "DHL";
			orgHeader.OH_FullName = "DHL INTERNATIONAL LTD";
			orgHeader.MainAddress.OA_Address1 = "CNR LAURENCE STEVENS & HAPE DRIVE";
			orgHeader.MainAddress.OA_Address2 = "AUCKLAND INTERNATIONAL AIRPORT";
			orgHeader.OH_RL_NKClosestPort = "NZAKL";

			return orgHeader;
		}

		protected OrgHeader GetTestOrgHeaderForwarder()
		{
			return GetTestOrgHeaderShippingLine();
		}

		protected RefCurrency GetRefCurrencyNZD()
		{
			return factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "NZD");
		}
		#endregion
	}

	public abstract class TestDeclarationCreatorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSetupConsignmentDetails()
		{
			DecCreator.SetupTestConsignmentDetails();
		}

		[ExpectNoExceptions]
		public void TestSetupTestForAir()
		{
			DecCreator.SetupTestForAir();
		}

		[ExpectNoExceptions]
		public void TestSetupTestForSea()
		{
			DecCreator.SetupTestForSea();
		}

		[ExpectNoExceptions]
		public void TestSetupTestForImportFromAU()
		{
			DecCreator.SetupTestForImportFromAU();
		}

		[ExpectNoExceptions]
		public void TestSetupTestForExportToAU()
		{
			DecCreator.SetupTestForExportToAU();
		}

		[ExpectNoExceptions]
		public void TestSetupTestAddRemarks()
		{
			DecCreator.SetupTestAddRemarks();
		}

		[ExpectNoExceptions]
		public virtual void TestSetupTestContainer()
		{
			DecCreator.SetupTestContainer();
		}

		[ExpectNoExceptions]
		public void TestSetEDITransmitDateToToday()
		{
			DecCreator.SetEDITransmitDateToToday();
		}

		[ExpectNoExceptions]
		public void TestSetUniqueJobNumber()
		{
			DecCreator.SetUniqueishJobNumber();
		}

		[ExpectNoExceptions]
		public void TestSetImportDateToToday()
		{
			DecCreator.SetImportDateToToday();
		}

		[ExpectNoExceptions]
		public void TestSetExportDateToToday()
		{
			DecCreator.SetExportDateToToday();
		}

		#region Implementation
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region DecCreator
		protected abstract TestDeclarationCreator GetNewTestDeclarationCreator();

		protected TestDeclarationCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = GetNewTestDeclarationCreator();
				}
				return fDecCreator;
			}
		}
		TestDeclarationCreator fDecCreator;
		#endregion
		#endregion
	}
}
