using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.ManifestBase.AsycudaManifestHeader;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(USExportAsycudaManifestHeader))]
	sealed class USExportAsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestHumanReadableShortcutName()
		{
			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_JobReference = "MAN67890";
			AssertEquals("US Sea Exp MAN67890", header.HumanReadableShortcutName);

			header.MasterBOL = "OTT13114";
			AssertEquals("US Sea Exp MAN67890 - OTT13114", header.HumanReadableShortcutName);

			header.MasterBill.ABL_BillIssuer = "OTT1";
			AssertEquals("US Sea Exp MAN67890 - OTT1OTT13114", header.HumanReadableShortcutName);

			header.MasterBOL = ZString.Empty;
			AssertEquals("US Sea Exp MAN67890", header.HumanReadableShortcutName);
		}

		public void TestDefaultAMA_CustomsFirstArrivalPort()
		{
			AssertDefaultScheduleK("AMA_RL_NKPortOfFirstArrival", "AMA_CustomsFirstArrivalPort");
		}

		public void TestDefaultMasterBillCarrierSCAC()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "BBDD", Core.Constants.CountryCodes.UnitedStates);
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TruckCarrierCode, "BBFF", Core.Constants.CountryCodes.UnitedStates);

			var address = carrier.Addresses.AddNewMainAddress();
			address.OA_Address1 = "Address 1";

			var carrier0 = Factory.New<USCarrierCombined>();
			carrier0.UI_Code = "BBFF";
			carrier0.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.AMA_OA_Carrier = address.PK;
			AssertNotNull(manifestHeader.Carrier);
			AssertEquals("BBDD", manifestHeader.MasterBill.ABL_BillIssuer);
			AssertHasMessageError(manifestHeader.MasterBill.ABL_BillIssuerInfo, "Carrier Standard Carrier Alpha Code (SCAC) unable to be found.");

			manifestHeader.MasterBill.ABL_BillIssuer = "BBFF";
			AssertNoMessageError(manifestHeader.MasterBill.ABL_BillIssuerInfo, "Carrier Standard Carrier Alpha Code (SCAC) unable to be found.");
		}

		public void TestPortOfFirstArrivalRefLocoMappings()
		{
			RefUNLOCOTestDataHelper.CreateScheduleKPort(Factory, true);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();

			AssertEquals(false, header.AMA_CustomsFirstArrivalPortIsDropEdit);
			AssertEquals(0, header.PortOfFirstArrivalRefLocoMappings.Count);

			header.AMA_RL_NKPortOfFirstArrival = "TEST1";
			AssertEquals(true, header.AMA_CustomsFirstArrivalPortIsDropEdit);
			AssertEquals(2, header.PortOfFirstArrivalRefLocoMappings.Count);
			Assert(header.PortOfFirstArrivalRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60001"));
			Assert(header.PortOfFirstArrivalRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60002"));

			header.AMA_RL_NKPortOfFirstArrival = "TEST2";
			AssertEquals(false, header.AMA_CustomsFirstArrivalPortIsDropEdit);
			AssertEquals(1, header.PortOfFirstArrivalRefLocoMappings.Count);
			Assert(header.PortOfFirstArrivalRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "60004"));
		}

		public void TestDefaultAMA_CustomsFinalDeparturePort()
		{
			AssertDefaultScheduleD("AMA_RL_NKPortOfFinalDeparture", "AMA_CustomsFinalDeparturePort");
		}

		public void TestPortOfFinalDepartureRefLocoMappings()
		{
			RefUNLOCOTestDataHelper.CreateScheduleDPort(Factory, true);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			AssertEquals(false, header.AMA_CustomsFinalDeparturePortIsDropEdit);
			AssertEquals(0, header.PortOfFinalDepartureRefLocoMappings.Count);

			header.AMA_RL_NKPortOfFinalDeparture = "USTES";
			AssertEquals(true, header.AMA_CustomsFinalDeparturePortIsDropEdit);
			AssertEquals(2, header.PortOfFinalDepartureRefLocoMappings.Count);
			Assert(header.PortOfFinalDepartureRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4001"));
			Assert(header.PortOfFinalDepartureRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4002"));

			header.AMA_TransportMode = "AIR";
			AssertEquals(false, header.AMA_CustomsFinalDeparturePortIsDropEdit);
			AssertEquals(1, header.PortOfFinalDepartureRefLocoMappings.Count);
			Assert(header.PortOfFinalDepartureRefLocoMappings.OfType<ICodeDescription>().Any(x => x.Code == "4004"));
		}

		public void AssertDefaultScheduleD(string uNLOCOPropertyName, string portPropertyName)
		{
			RefUNLOCOTestDataHelper.CreateLocoIfNotExists(Factory, "USLX");
			var locoCode = new ZString("USLX");
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3900", "USLX", USLocoMapSystemUsageList.Codes.SCD, true);
			var userDefiniedLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3899", "USLX", USLocoMapSystemUsageList.Codes.SCD, false);
			var sameLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3901", "USLX", USLocoMapSystemUsageList.Codes.SCD, false);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			header.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals(ZString.Empty, header.GetPropertyValue(portPropertyName));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3899", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3900", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3904", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3903", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			sameLocoMap.Delete();
			header.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			header.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3899", header.GetPropertyValue(portPropertyName));

			userDefiniedLocoMap.Delete();
			header.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			header.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3900", header.GetPropertyValue(portPropertyName));

			sameLocoMap.Delete();
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3902", "USLX", USLocoMapSystemUsageList.Codes.Air, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3903", "USLX", USLocoMapSystemUsageList.Codes.Sea, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3904", "USLX", USLocoMapSystemUsageList.Codes.All, false);
			header.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			header.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3904", header.GetPropertyValue(portPropertyName));

			header.AMA_TransportMode = "AIR";
			header.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			header.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3902", header.GetPropertyValue(portPropertyName));

			header.AMA_TransportMode = "SEA";
			header.SetPropertyValue(uNLOCOPropertyName, ZString.Empty);
			header.SetPropertyValue(uNLOCOPropertyName, locoCode);
			AssertEquals("3903", header.GetPropertyValue(portPropertyName));
		}

		public void AssertDefaultScheduleK(string propertyName, string portPropertyName)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3899", "3899 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "3900", "3900 Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			RefUNLOCOTestDataHelper.CreateLocoIfNotExists(Factory, "USLX");
			var locoCode = new ZString("USLX");
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3900", "USLX", USLocoMapSystemUsageList.Codes.SCK, true);
			var userDefiniedLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3899", "USLX", USLocoMapSystemUsageList.Codes.SCK, false);
			var sameLocoMap = RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3901", "USLX", USLocoMapSystemUsageList.Codes.SCK, false);

			var header = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			AssertEquals(ZString.Empty, header.GetPropertyValue(portPropertyName));

			sameLocoMap.Delete();
			header.SetPropertyValue(propertyName, ZString.Empty);
			header.SetPropertyValue(propertyName, locoCode);
			AssertEquals("3899", header.GetPropertyValue(portPropertyName));

			userDefiniedLocoMap.Delete();
			header.SetPropertyValue(propertyName, ZString.Empty);
			header.SetPropertyValue(propertyName, locoCode);
			AssertEquals("3900", header.GetPropertyValue(portPropertyName));

			sameLocoMap.Delete();
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3902", "USLX", USLocoMapSystemUsageList.Codes.Air, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3903", "USLX", USLocoMapSystemUsageList.Codes.Sea, false);
			RefUNLOCOTestDataHelper.CreateLocoMapIfNotExists(Factory, "3904", "USLX", USLocoMapSystemUsageList.Codes.All, false);
			header.SetPropertyValue(propertyName, ZString.Empty);
			header.SetPropertyValue(propertyName, locoCode);
			AssertEquals("3900", header.GetPropertyValue(portPropertyName));
		}

		public void TestArrivalHeaderLoaded()
		{
			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "C4321";
			var newArrivalHeader = Factory.NewWithValidTestData<AsycudaArrivalHeader>();
			newArrivalHeader.ATH_AMA_ManifestHeader = manifestHeader.PK;
			newArrivalHeader.ATH_ClusterKey = manifestHeader.AMA_ClusterKey;
			AssertEquals(newArrivalHeader.PK, manifestHeader.ArrivalHeader.PK);
		}

		public void TestArrivalHeaderCreated()
		{
			var manifestHeader = Factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "C4321";

			var loadArrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, manifestHeader.PK));
			AssertNull(loadArrivalHeader);

			var arrivalHeader = manifestHeader.ArrivalHeader;
			loadArrivalHeader = Factory.LoadTop1<AsycudaArrivalHeader>(new ZQuery(AsycudaArrivalHeaderSchema.ATH_AMA_ManifestHeader, manifestHeader.PK));
			AssertEquals(loadArrivalHeader.PK, arrivalHeader.PK);
		}

		public void TestBills()
		{
			var header = Factory.New<USExportAsycudaManifestHeader>();
			AssertType<USExportAsycudaBillCollection>(header.Bills);
		}

		public void TestShowVINNumbers()
		{
			var manifestHeader = new BusinessObjectFactory().NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("Should always show VIN Numbers", true, manifestHeader.ShowVINNumbers);

			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("Should always show VIN Numbers", true, manifestHeader.ShowVINNumbers);
		}

		[GuiTest]
		public void TestGetNewBusinessEntityInLocalFactory_DefaultTransportModeTo_MultipleApplicableTransportModes()
		{
			using (USCustomsDataRegistry.Instance.EnableExportManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var controller = new ASYCUDAManifestControllerForTest();
				using (var testForm = controller.ShowNewForm() as ZForm)
				{
					var controllerInternal = (new ASYCUDAManifestControllerForTest()) as ZControllerInternals;
					controller.ProviderForTest = ApplicationBusinessProvider.GetApplicationBusinessProviders(Factory, "US", USExportManifestTypes.Codes.EFM).First(x => x.AsycudaManifestHeaderType.Equals(typeof(USExportAsycudaManifestHeader)));
					controller.CountryCodeForTest = "US";
					var header = testForm.BusinessEntity as AsycudaManifestHeader;
					AssertNotNull(header);
					AssertEquals(string.Empty, header.AMA_TransportMode);
				}
			}
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<USExportAsycudaManifestHeader>();
			manifestHeader.SuspendCheckBusinessObjectType();
			manifestHeader.AMA_JobReference = "C1234";
			return manifestHeader;
		}

		public class ASYCUDAManifestControllerForTest : ASYCUDAManifestController
		{
			public ApplicationBusinessProvider ProviderForTest
			{
				get => Provider;
				set => Provider = value;
			}

			public string CountryCodeForTest
			{
				get => CountryCode;
				set => CountryCode = value;
			}
		}
	}
}
