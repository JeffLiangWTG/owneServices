using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(ExportJobDeclarationLookups))]
sealed class ExportJobDeclarationLookupsTest : JobDeclarationLookupsAbstractTest<ExportJobDeclarationLookups>
{
	protected override string MessageType => JobMessageTypeList.Codes.Export;

	public void TestShipmentType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes from list", "EU, EX", lookups.MessageSubTypeList.CodesAsString);
			AssertSame("Cached", lookups.MessageSubTypeList, lookups.MessageSubTypeList);
		});
	}

	public void TestLocationOfGoods()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Codes from list", "A, B, C, D", lookups.LocationOfGoodsCollection.CodesAsString);
			AssertSame("Cached", lookups.LocationOfGoodsCollection, lookups.LocationOfGoodsCollection);
		});
	}

	void AssertTestCustomsOfficeList(CodeDescriptionPairList codelist, ZString[] expected)
	{
		CombineAssertions(() =>
		{
			AssertSame("Cached", codelist, codelist);
			AssertType<CodeDescriptionPairList>("Type", codelist);
			AssertContainsExactElementsInAnyOrder("Codes", expected, codelist.GetAllCodes());
		});
	}

	public void TestCustomsOfficeList_TransportModeSEA()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3210, NOCustomsOfficesList.Codes._3220, NOCustomsOfficesList.Codes._3225, NOCustomsOfficesList.Codes._3240,
			NOCustomsOfficesList.Codes._3310, NOCustomsOfficesList.Codes._3315, NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3325, NOCustomsOfficesList.Codes._3330, NOCustomsOfficesList.Codes._3335, NOCustomsOfficesList.Codes._3380, NOCustomsOfficesList.Codes._3390,
			NOCustomsOfficesList.Codes._3410, NOCustomsOfficesList.Codes._3420, NOCustomsOfficesList.Codes._3440, NOCustomsOfficesList.Codes._3450, NOCustomsOfficesList.Codes._3460, NOCustomsOfficesList.Codes._3470, NOCustomsOfficesList.Codes._3480, NOCustomsOfficesList.Codes._3490, NOCustomsOfficesList.Codes._3495,
			NOCustomsOfficesList.Codes._3610, NOCustomsOfficesList.Codes._3620, NOCustomsOfficesList.Codes._3640, NOCustomsOfficesList.Codes._3650, NOCustomsOfficesList.Codes._3680, NOCustomsOfficesList.Codes._3690, NOCustomsOfficesList.Codes._3695,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3725, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3735, NOCustomsOfficesList.Codes._3740, NOCustomsOfficesList.Codes._3755, NOCustomsOfficesList.Codes._3775
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeRAI()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3310, NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3340, NOCustomsOfficesList.Codes._3350, NOCustomsOfficesList.Codes._3360, NOCustomsOfficesList.Codes._3370, NOCustomsOfficesList.Codes._3380, NOCustomsOfficesList.Codes._3390,
			NOCustomsOfficesList.Codes._3610, NOCustomsOfficesList.Codes._3620, NOCustomsOfficesList.Codes._3640, NOCustomsOfficesList.Codes._3680,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3740, NOCustomsOfficesList.Codes._3750, NOCustomsOfficesList.Codes._3775
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeROA()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3225, NOCustomsOfficesList.Codes._3230, NOCustomsOfficesList.Codes._3240, NOCustomsOfficesList.Codes._3250, NOCustomsOfficesList.Codes._3260, NOCustomsOfficesList.Codes._3390, NOCustomsOfficesList.Codes._3270, NOCustomsOfficesList.Codes._3280, NOCustomsOfficesList.Codes._3290,
			NOCustomsOfficesList.Codes._3310, NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3340, NOCustomsOfficesList.Codes._3350, NOCustomsOfficesList.Codes._3360, NOCustomsOfficesList.Codes._3370, NOCustomsOfficesList.Codes._3380,
			NOCustomsOfficesList.Codes._3510,
			NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3725, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3735, NOCustomsOfficesList.Codes._3740, NOCustomsOfficesList.Codes._3750, NOCustomsOfficesList.Codes._3755, NOCustomsOfficesList.Codes._3760, NOCustomsOfficesList.Codes._3770, NOCustomsOfficesList.Codes._3775, NOCustomsOfficesList.Codes._3780
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeAIR()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3220, NOCustomsOfficesList.Codes._3225, NOCustomsOfficesList.Codes._3240, NOCustomsOfficesList.Codes._3250,
			NOCustomsOfficesList.Codes._3310, NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3330, NOCustomsOfficesList.Codes._3380, NOCustomsOfficesList.Codes._3390,
			NOCustomsOfficesList.Codes._3410, NOCustomsOfficesList.Codes._3420, NOCustomsOfficesList.Codes._3430, NOCustomsOfficesList.Codes._3440, NOCustomsOfficesList.Codes._3450, NOCustomsOfficesList.Codes._3460, NOCustomsOfficesList.Codes._3490, NOCustomsOfficesList.Codes._3495,
			NOCustomsOfficesList.Codes._3510,
			NOCustomsOfficesList.Codes._3610, NOCustomsOfficesList.Codes._3630, NOCustomsOfficesList.Codes._3640,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3740
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeMAI()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3210, NOCustomsOfficesList.Codes._3220, NOCustomsOfficesList.Codes._3225, NOCustomsOfficesList.Codes._3240, NOCustomsOfficesList.Codes._3250,
			NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3330, NOCustomsOfficesList.Codes._3380, NOCustomsOfficesList.Codes._3390,
			NOCustomsOfficesList.Codes._3410, NOCustomsOfficesList.Codes._3420, NOCustomsOfficesList.Codes._3430, NOCustomsOfficesList.Codes._3440, NOCustomsOfficesList.Codes._3450,
			NOCustomsOfficesList.Codes._3510,
			NOCustomsOfficesList.Codes._3610, NOCustomsOfficesList.Codes._3620, NOCustomsOfficesList.Codes._3630, NOCustomsOfficesList.Codes._3640, NOCustomsOfficesList.Codes._3650, NOCustomsOfficesList.Codes._3680,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3725, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3735, NOCustomsOfficesList.Codes._3740, NOCustomsOfficesList.Codes._3750, NOCustomsOfficesList.Codes._3755, NOCustomsOfficesList.Codes._3770, NOCustomsOfficesList.Codes._3775, NOCustomsOfficesList.Codes._3780
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeFIX()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3210, NOCustomsOfficesList.Codes._3220, NOCustomsOfficesList.Codes._3240,
			NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3380,
			NOCustomsOfficesList.Codes._3410, NOCustomsOfficesList.Codes._3420, NOCustomsOfficesList.Codes._3440, NOCustomsOfficesList.Codes._3450,
			NOCustomsOfficesList.Codes._3610, NOCustomsOfficesList.Codes._3620, NOCustomsOfficesList.Codes._3640, NOCustomsOfficesList.Codes._3650, NOCustomsOfficesList.Codes._3680,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3740, NOCustomsOfficesList.Codes._3750, NOCustomsOfficesList.Codes._3760, NOCustomsOfficesList.Codes._3770, NOCustomsOfficesList.Codes._3780
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeIWT()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3210, NOCustomsOfficesList.Codes._3320,
			NOCustomsOfficesList.Codes._3380,
			NOCustomsOfficesList.Codes._3650,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3740
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_TransportModeOWN()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3220, NOCustomsOfficesList.Codes._3230, NOCustomsOfficesList.Codes._3240, NOCustomsOfficesList.Codes._3250, NOCustomsOfficesList.Codes._3260, NOCustomsOfficesList.Codes._3270, NOCustomsOfficesList.Codes._3280, NOCustomsOfficesList.Codes._3290,
			NOCustomsOfficesList.Codes._3310, NOCustomsOfficesList.Codes._3320, NOCustomsOfficesList.Codes._3330, NOCustomsOfficesList.Codes._3340, NOCustomsOfficesList.Codes._3350, NOCustomsOfficesList.Codes._3360, NOCustomsOfficesList.Codes._3370, NOCustomsOfficesList.Codes._3380,
			NOCustomsOfficesList.Codes._3410, NOCustomsOfficesList.Codes._3420, NOCustomsOfficesList.Codes._3440, NOCustomsOfficesList.Codes._3450,
			NOCustomsOfficesList.Codes._3610, NOCustomsOfficesList.Codes._3620, NOCustomsOfficesList.Codes._3640, NOCustomsOfficesList.Codes._3650, NOCustomsOfficesList.Codes._3680,
			NOCustomsOfficesList.Codes._3710, NOCustomsOfficesList.Codes._3720, NOCustomsOfficesList.Codes._3730, NOCustomsOfficesList.Codes._3740, NOCustomsOfficesList.Codes._3750, NOCustomsOfficesList.Codes._3760, NOCustomsOfficesList.Codes._3770, NOCustomsOfficesList.Codes._3780
		};
		AssertTestCustomsOfficeList(codelist, expected);
	}

	public void TestCustomsOfficeList_Sorted_TransportModeIWT()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
		var codelist = lookups.CustomsOfficeList as CodeDescriptionPairList;
		var expected = new ZString[]
		{
			NOCustomsOfficesList.Codes._3320,
			NOCustomsOfficesList.Codes._3730,
			NOCustomsOfficesList.Codes._3380,
			NOCustomsOfficesList.Codes._3650,
			NOCustomsOfficesList.Codes._3710,
			NOCustomsOfficesList.Codes._3210,
			NOCustomsOfficesList.Codes._3720,
			NOCustomsOfficesList.Codes._3740
		};

		CombineAssertions(() =>
		{
			AssertSame("Cached", lookups.CustomsOfficeList, lookups.CustomsOfficeList);
			AssertType<CodeDescriptionPairList>("Type", lookups.CustomsOfficeList);
			AssertContainsExactElementsInExactOrder("Codes", expected, codelist.GetAllCodes());
		});
	}
}
