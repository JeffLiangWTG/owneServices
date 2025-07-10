using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportAsycudaBill))]
	public class USExportAsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestHumanReadableShortcutName()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			bill.Header.AMA_JobReference = "MAN0001730";
			AssertEquals("US Sea Exp MAN0001730", bill.HumanReadableShortcutName);

			bill.ABL_BillNumber = "HS08152402";
			AssertEquals("US Sea Exp MAN0001730 - HS08152402", bill.HumanReadableShortcutName);

			bill.ABL_BillIssuer = "FOO";
			AssertEquals("US Sea Exp MAN0001730 - FOOHS08152402", bill.HumanReadableShortcutName);

			bill.ABL_BillNumber = ZString.Empty;
			AssertEquals("US Sea Exp MAN0001730", bill.HumanReadableShortcutName);
		}

		public void TestAESITNNumbers()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			var entryNumber1 = bill.AESITNNumberCollection.AddNew();
			entryNumber1.CE_EntryNum = "001";
			var entryNumber2 = bill.AESITNNumberCollection.AddNew();
			entryNumber2.CE_EntryNum = "002";

			AssertEquals("001,002", bill.AESITNNumbers);

			bill.AESITNNumbers = "111,222";
			AssertEquals("111,222", bill.AESITNNumbers);

			AssertEquals(2, bill.AESITNNumberCollection.Count);
			AssertEquals("111,222", bill.AESITNNumberCollection.GetCodesAsCommaSeparatedString());
		}

		public void TestInBondNumbers()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			var entryNumber1 = bill.InBondNumberCollection.AddNew();
			entryNumber1.CE_EntryNum = "001";
			var entryNumber2 = bill.InBondNumberCollection.AddNew();
			entryNumber2.CE_EntryNum = "002";

			AssertEquals("001,002", bill.InBondNumbers);

			bill.InBondNumbers = "111,222";
			AssertEquals("111,222", bill.InBondNumbers);

			AssertEquals(2, bill.InBondNumberCollection.Count);
			AssertEquals("111,222", bill.InBondNumberCollection.GetCodesAsCommaSeparatedString());
		}

		public void TestGetNewValidationForMasterChild()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			AssertEquals(typeof(USExportAsycudaBillValidationForRegularBill), bill.Validation.GetType());

			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			AssertEquals(typeof(USExportAsycudaBillValdiationForMasterChild), bill.Validation.GetType());
		}

		public void TestDefaultABL_CustomsOriginPort()
		{
			AssertDefaultScheduleD("ABL_RL_NKOrigin", "ABL_CustomsOriginPort");
		}

		public void TestPortOfOriginRefLocoMappings()
		{
			AssertScheduleDRefLocoMappings("ABL_RL_NKOrigin", "ABL_CustomsOriginPortIsDropEdit", "PortOfOriginRefLocoMappings");
		}

		public void TestDefaultABL_CustomsLoadPort()
		{
			AssertDefaultScheduleD("ABL_RL_NKPortOfLoading", "ABL_CustomsLoadPort");
		}

		public void TestPortOfLadingRefLocoMappings()
		{
			AssertScheduleDRefLocoMappings("ABL_RL_NKPortOfLoading", "ABL_CustomsLoadPortIsDropEdit", "PortOfLadingRefLocoMappings");
		}

		public void TestDefaultABL_CustomsDischargePort()
		{
			AssertDefaultScheduleK("ABL_RL_NKPortOfDischarge", "ABL_CustomsDischargePort");
		}

		public void TestPortOfUnladingRefLocoMappings()
		{
			AssertScheduleKRefLocoMappings("ABL_RL_NKPortOfDischarge", "ABL_CustomsDischargePortIsDropEdit", "PortOfUnladingRefLocoMappings");
		}

		public void TestDefaultABL_CustomsFinalDestinationPort()
		{
			AssertDefaultScheduleK("ABL_RL_NKFinalDestination", "ABL_CustomsFinalDestinationPort");
		}

		public void TestPortOfFinalDestinationDefaulter()
		{
			AssertScheduleKRefLocoMappings("ABL_RL_NKFinalDestination", "ABL_CustomsFinalDestinationPortIsDropEdit", "PortOfFinalDestinationRefLocoMappings");
		}

		public void AssertScheduleDRefLocoMappings(string uNLOCOPropertyName, string portIsDropEditPropertyName, string refLocoMappingsPropertyName)
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			var bill = header.Bills.AddNew();
			var locoCode = new ZString("USTES");

			AssertEquals(false, bill.GetPropertyValue(portIsDropEditPropertyName));
			AssertEquals(0, ((BusinessObjectCollection)bill.GetPropertyValue(refLocoMappingsPropertyName)).Count);

			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals(true, bill.GetPropertyValue(portIsDropEditPropertyName));
			var prtOfOriginRefLocoMappings = (BusinessObjectCollection)bill.GetPropertyValue(refLocoMappingsPropertyName);
			AssertEquals(2, prtOfOriginRefLocoMappings.Count);
			Assert(prtOfOriginRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4001"));
			Assert(prtOfOriginRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4002"));

			header.AMA_TransportMode = "AIR";
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			prtOfOriginRefLocoMappings = (BusinessObjectCollection)bill.GetPropertyValue(refLocoMappingsPropertyName);
			AssertEquals(false, bill.GetPropertyValue(portIsDropEditPropertyName));
			AssertEquals(1, prtOfOriginRefLocoMappings.Count);
			Assert(prtOfOriginRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4004"));
		}

		public void AssertDefaultScheduleD(string uNLOCOPropertyName, string portPropertyName)
		{
			RefUNLOCOTestDataHelper.CreateLocoIfNotExists(Factory, "USLX");
			var locoCode = new ZString("USLX");
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3900", "USLX", USLocoMapSystemUsageList.Codes.SCD, true);
			var userDefiniedLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3899", "USLX", USLocoMapSystemUsageList.Codes.SCD, false);
			var sameLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3901", "USLX", USLocoMapSystemUsageList.Codes.SCD, false);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals(ZString.Empty, bill.GetPropertyValue(portPropertyName));

			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3899", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3900", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3904", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3903", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			newFactory.Save();

			sameLocoMap.Delete();
			bill.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3899", bill.GetPropertyValue(portPropertyName));

			userDefiniedLocoMap.Delete();
			bill.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3900", bill.GetPropertyValue(portPropertyName));

			sameLocoMap.Delete();
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3902", "USLX", USLocoMapSystemUsageList.Codes.Air, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3903", "USLX", USLocoMapSystemUsageList.Codes.Sea, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3904", "USLX", USLocoMapSystemUsageList.Codes.All, false);
			bill.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3904", bill.GetPropertyValue(portPropertyName));

			header.AMA_TransportMode = "AIR";
			bill.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3902", bill.GetPropertyValue(portPropertyName));

			header.AMA_TransportMode = "SEA";
			bill.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			bill.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3903", bill.GetPropertyValue(portPropertyName));
		}

		public void AssertScheduleKRefLocoMappings(string uNLOCOPropertyName, string portIsDropEditPropertyName, string refLocoMappingsPropertyName)
		{
			RefUNLOCOTestDataHelper.CreateScheduleKPort(Factory, true);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals(false, bill.GetPropertyValue(portIsDropEditPropertyName));
			AssertEquals(0, ((BusinessObjectCollection)bill.GetPropertyValue(refLocoMappingsPropertyName)).Count);

			bill.SetPropertyValue(uNLOCOPropertyName, new ZString("TEST1"));
			AssertEquals(true, bill.GetPropertyValue(portIsDropEditPropertyName));
			var prtOfOriginRefLocoMappings = (BusinessObjectCollection)bill.GetPropertyValue(refLocoMappingsPropertyName);
			AssertEquals(2, prtOfOriginRefLocoMappings.Count);
			Assert(prtOfOriginRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60001"));
			Assert(prtOfOriginRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60002"));

			bill.SetPropertyValue(uNLOCOPropertyName, new ZString("TEST2"));
			prtOfOriginRefLocoMappings = (BusinessObjectCollection)bill.GetPropertyValue(refLocoMappingsPropertyName);
			AssertEquals(false, bill.GetPropertyValue(portIsDropEditPropertyName));
			AssertEquals(1, prtOfOriginRefLocoMappings.Count);
			Assert(prtOfOriginRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60004"));
		}

		public void AssertDefaultScheduleK(string propertyName, string portPropertyName)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3899", "3899 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3901", "3901 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3900", "3900 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			RefUNLOCOTestDataHelper.CreateLocoIfNotExists(Factory, "USLX");
			var locoCode = new ZString("USLX");
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3900", "USLX", USLocoMapSystemUsageList.Codes.SCK, true);
			var userDefiniedLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3899", "USLX", USLocoMapSystemUsageList.Codes.SCK, false);
			var sameLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3901", "USLX", USLocoMapSystemUsageList.Codes.SCK, false);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.SetPropertyValue(propertyName, locoCode);
			AssertEquals(ZString.Empty, bill.GetPropertyValue(portPropertyName));

			sameLocoMap.Delete();
			bill.SetPropertyValue(propertyName, ZString.Empty);
			bill.SetPropertyValue(propertyName, locoCode);
			AssertEquals("3899", bill.GetPropertyValue(portPropertyName));

			userDefiniedLocoMap.Delete();
			bill.SetPropertyValue(propertyName, ZString.Empty);
			bill.SetPropertyValue(propertyName, locoCode);
			AssertEquals("3900", bill.GetPropertyValue(portPropertyName));

			sameLocoMap.Delete();
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3902", "USLX", USLocoMapSystemUsageList.Codes.Air, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3903", "USLX", USLocoMapSystemUsageList.Codes.Sea, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3904", "USLX", USLocoMapSystemUsageList.Codes.All, false);
			bill.SetPropertyValue(propertyName, ZString.Empty);
			bill.SetPropertyValue(propertyName, locoCode);
			AssertEquals("3900", bill.GetPropertyValue(portPropertyName));
		}

		public void TestDefaultABL_SpecialCargoCode()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			AssertEquals(new ZString(BillOfLadingTypeList.Codes.RegularBillOfLading), bill.ABL_SpecialCargoCode);
		}

		public void TestABL_UCRNumber_ResourceStringDataAttribute()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USExportAsycudaBill), nameof(USExportAsycudaBill.ABL_UCRNumber), false, attribute => attribute.Caption == "AES Exemption Code");
			AssertEquals(3, bill.ABL_UCRNumberInfo.MaxLength);
		}

		public void TestATL_Weight()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			var arrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, bill.Header.PK));
			var arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			CombineAssertions(() =>
			{
				AssertNull(arrivalHeader);
				AssertNull(arrivalLine);
				AssertEquals(ZDecimal.Zero, bill.ATL_Weight);
			});

			bill.ATL_Weight = 1;
			arrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, bill.Header.PK));
			arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			CombineAssertions(() =>
			{
				AssertEquals(bill.Header.PK, arrivalHeader.ATH_AMA_ManifestHeader);
				AssertEquals(new ZDecimal(1), arrivalLine.ATL_Weight);
				AssertEquals(new ZDecimal(1), bill.ATL_Weight);
			});
		}

		public void TestNegativeATL_Weight()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			bill.ATL_Weight = 1;
			AssertEquals(new ZDecimal(1), bill.ATL_Weight);

			bill.ATL_Weight = -1;
			AssertEquals(new ZDecimal(0), bill.ATL_Weight);
		}

		public void TestATL_Quantity()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			var arrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, bill.Header.PK));
			var arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			CombineAssertions(() =>
			{
				AssertNull(arrivalHeader);
				AssertNull(arrivalLine);
				AssertEquals(ZInt.Zero, bill.ATL_Quantity);
			});

			bill.ATL_Quantity = 1;
			arrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, bill.Header.PK));
			arrivalLine = Factory.LoadTop1<AsycudaArrivalLine>(new ZQuery(AsycudaArrivalLineSchema.ATL_ABL_AsycudaBill, bill.PK));
			CombineAssertions(() =>
			{
				AssertEquals(bill.Header.PK, arrivalHeader.ATH_AMA_ManifestHeader);
				AssertEquals(1, arrivalLine.ATL_Quantity);
				AssertEquals(1, bill.ATL_Quantity);
			});
		}

		public void TestNegativeATL_Quantity()
		{
			var bill = GetNewBusinessObject() as USExportAsycudaBill;
			bill.ATL_Quantity = 1;
			AssertEquals(1, bill.ATL_Quantity);

			bill.ATL_Quantity = -1;
			AssertEquals(0, bill.ATL_Quantity);
		}

		public void TestPacks()
		{
			var bill = (USExportAsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection<USExportAsycudaPack, USExportAsycudaBill>>(bill.Packs);
		}

		public void TestCreateNewAsycudaPackCollection()
		{
			var bill = Factory.New<USExportAsycudaBillForTest>();
			AssertType<AsycudaPackCollection<USExportAsycudaPack, USExportAsycudaBill>>(bill.CreateNewAsycudaPackCollection());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<USExportAsycudaBillForTest>();
			AssertEquals(typeof(USExportAsycudaPack), bill.GetPackTypeCore());
		}

		public void TestGetCusCodeDataType()
		{
			var bill = Factory.New<USExportAsycudaBillForTest>();
			AssertEquals(typeof(USExportVisitedPort), ((ICusCodeDataTypeSupporter)bill).GetCusCodeDataTypes()[USExportCusCodeType.Codes.UVP]);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<USExportAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		class USExportAsycudaBillForTest : USExportAsycudaBill
		{
			public USExportAsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();

			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaBill.Schema.ABL_SpecialCargoCode };
		}

		#endregion
	}
}
