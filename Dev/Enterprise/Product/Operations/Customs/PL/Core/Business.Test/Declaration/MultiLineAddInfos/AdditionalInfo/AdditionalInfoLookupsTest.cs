using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeListCollection()
	{
		SetUpCusCodeList();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		CombineAssertions("Export", () =>
		{
			additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
			var infCodeList = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AI1" }, infCodeList.Select(x => x.ZZD_Code));

			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			var refCodeList = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AR1" }, refCodeList.Select(x => x.ZZD_Code));

			additionalInfo.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			var traCodeList = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "TD1" }, traCodeList.Select(x => x.ZZD_Code));
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var importAdditionalInfo = declaration.AdditionalInfos.AddNew();
		CombineAssertions("Import", () =>
		{
			var traCodeList = (CodeDescriptionPairList)importAdditionalInfo.Lookups.CodeList;
			AssertEquals("Values for Import", "AII", traCodeList.CodesAsString);
			AssertSame("Cached", traCodeList, additionalInfo.Lookups.CodeList);
		});
	}

	public void TestCodeListCollection_Export_WrongSubtype()
	{
		SetUpCusCodeList();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		additionalInfo.CSI_SubType = "000";
		var codeList = (ZZRefCusCodeListCombinedCollection)additionalInfo.Lookups.CodeList;
		AssertEquals("No code loaded", 0, codeList.Count);
	}

	void SetUpCusCodeList()
	{
		var config = new RefDataConfig(
			dataGroupings:
			[
				new(code: "EUN", description: "European Union", parent: ""),
				new(code: "PL", description: "Poland", parent: "EUN")
			],
			cusCodeTypes:
			[
				new(typeCode: "ADDIN", description: "Additional Information", attributeTypes: [new(name: "Level", dataGrouping: "PL")],
					cusCodes:
					[
						new(code: "AII", dataGrouping: "PL", attributes: [new(name: "Level", value: "Header")]),
					]
				),
				new(typeCode: "AI44E", description: "ExportAddDocAdditionalInformation", attributeTypes: [new(name: "Level", dataGrouping: "PL")],
					cusCodes:
					[
						new(code: "AI1", dataGrouping: "PL", attributes: [new(name: "Level", value: "Header")]),
					]
				),
				new(typeCode: "AR44E", description: "ExportAddDocAdditionalReference", attributeTypes: [new(name: "Level", dataGrouping: "EUN")],
					cusCodes:
					[
						new(code: "AR1", dataGrouping: "EUN", attributes: [new(name: "Level", value: "Header")]),
					]
				),
				new(typeCode: "TD44E", description: "ExportAddDocTransportContract", attributeTypes: [new(name: "Level", dataGrouping: "PL")],
					cusCodes:
					[
						new(code: "TD1", dataGrouping: "PL", attributes: [new(name: "Level", value: "Header")]),
					]
				)
			]
		);
		EUUniversalTestDataHelper.SetUpTestRefData(Factory, config);
	}

	public void TestSubTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var additionalInfo = declaration.AdditionalInfos.AddNew();
		var list = additionalInfo.Lookups.SubTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "INF, REF, TRA", list.CodesAsString);
			AssertSame("Cached", list, additionalInfo.Lookups.SubTypeList);
		});
	}
}
