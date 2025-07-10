
namespace Enterprise.eManifest.Module.Testing
{
	using System;
	using CargoWise.Types;
	using Enterprise.eManifest.Business;
	using Enterprise.Freight.Business;
	using Enterprise.Freight.Business.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using NUnit.Framework;

	[TestedType(typeof(ELoadListForConsolFilterProvider))]
	internal class ELoadListForConsolFilterProviderTest : DefaultFilterProviderTest<ELoadListForConsolFilterProvider>
	{
		public void TestConstructor_ConsolIsNull_ThrowArgumentNullException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ELoadListForConsolFilterProvider(null));
		}

		public void TestSetDefaultFilters_ConsolIsSpecified_FilterCollectionByValuesFromConsol()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_MasterBillNum = "MCLAREN";
			consol.MostInterestingTransportForBinding[0].JW_VoyageFlight = "SV111";
			consol.MostInterestingTransportForBinding[0].JW_Vessel = "TITANIC";

			var destinationDepot = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_UnpackDepotAddress = destinationDepot.MainAddress.PK;

			var collection = new LodgedELoadListCollection(Factory, consol);
			var filterProviderToTest = new ELoadListForConsolFilterProvider(consol);
			filterProviderToTest.SetDefaultFilters(collection);

			AssertHasDefault(collection, ELoadListFilterBusinessObject.Descriptions.MasterBill, "Property", (ZString)"MCLAREN");
			AssertHasDefault(collection, ELoadListFilterBusinessObject.Descriptions.DestinationDepot, "Property", destinationDepot.PK);
			AssertHasDefault(collection, ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel, "Property", (ZString)"SV111");
			AssertHasDefault(collection, ELoadListFilterBusinessObject.Descriptions.FlightVoyageNumAndVessel, "NkProperty", (ZString)"TITANIC");
		}

		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ELoadList; }
		}

		protected override ELoadListForConsolFilterProvider CreateProviderInstance()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var provider = new ELoadListForConsolFilterProvider(consol);

			return provider;
		}
	}
}
