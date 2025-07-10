using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeaderLookups))]
sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportMeansCodeList_Cached()
	{
		var collection = (ZZRefCusCodeListCombinedCollection)Lookups.TransportMeansCodeList;
		AssertSame("Cached", collection, Lookups.TransportMeansCodeList);
	}

	public void TestTransportMeansCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NOTransportationMeans;
		_ = helper.CreateNewOrGetExistingCusCodeType(codeType, "CL751 Description", Core.Constants.CountryCodes.Norway);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Norway, codeType, "111", "111 Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		Factory.Save();

		var collection = (ZZRefCusCodeListCombinedCollection)Lookups.TransportMeansCodeList;
		collection.Load();

		var lookupRow = collection[0];
		AssertEquals(lookupRow.ZZD_Code, "111");
		AssertEquals(lookupRow.ZZD_Description, "111 Description");
	}

	public void TestTransportMeansCodeListDefaultFilters()
	{
		CombineAssertions(() =>
		{
			AssertTransportMeansCodeListDefaultFilters(Core.Constants.TransportModes.Sea, ZString.Empty, "1");
			AssertTransportMeansCodeListDefaultFilters(Core.Constants.TransportModes.Rail, ZString.Empty, "2");
			AssertTransportMeansCodeListDefaultFilters(Core.Constants.TransportModes.Road, ZString.Empty, "3");
			AssertTransportMeansCodeListDefaultFilters(Core.Constants.TransportModes.Air, ZString.Empty, "4");
			AssertTransportMeansCodeListDefaultFilters(Core.Constants.TransportModes.Sea, "10", "10");
		});
	}

	void AssertTransportMeansCodeListDefaultFilters(ZString transportMode, ZString transportMeans, string expectedFilterValue)
	{
		ManifestHeader.AMA_TransportMeans = transportMeans;
		ManifestHeader.AMA_TransportMode = transportMode;

		var transportMeansCodeList = (ZZRefCusCodeListCombinedCollection)Lookups.TransportMeansCodeList;
		var codeFilter = transportMeansCodeList.FilterBusinessObjectDefaults
					   .Cast<FilterBusinessObjectDefault>()
					   .Single(x => x.FilterName == Constants.ZZRefCusCodeListFilters.Code);
		AssertEquals($"When AMA_TransportMeans: {ManifestHeader.AMA_TransportMeans} and AMA_TransportMode: {ManifestHeader.AMA_TransportMode}", expectedFilterValue, codeFilter.Value);
	}

	public void TestTransportModesCodeList()
	{
		AssertCodeDescriptionPairList(
			Lookups.TransportModeList,
			("ROA", "Road Freight"),
			("AIR", "Air Freight"),
			("SEA", "Sea Freight"),
			("RAI", "Rail Freight")
		);
	}

	public void TestTransportModesCodeList_Cached()
	{
		var transportModes = Lookups.TransportModeList;
		AssertSame("Cached", transportModes, Lookups.TransportModeList);
	}

	AsycudaManifestHeaderLookups Lookups => lookups ??= ManifestHeader.Lookups;
	AsycudaManifestHeaderLookups lookups;

	AsycudaManifestHeader ManifestHeader => manifestHeader ??= CreateBusinessObject(Factory);
	AsycudaManifestHeader manifestHeader;

	AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();
}
