using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class AddInfoJobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestBorderTransportMeansList_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		CombineAssertions(() =>
		{
			AssertBorderTransportMeansList("Empty Codes", ZString.Empty, Array.Empty<ZString>(), declaration);
			AssertBorderTransportMeansList("Sea Codes", TransportTypeList.Codes.Sea, new ZString[] { "10", "11" }, declaration);
			AssertBorderTransportMeansList("Rail Codes", TransportTypeList.Codes.Rail, new ZString[] { "21" }, declaration);
			AssertBorderTransportMeansList("Road Codes", TransportTypeList.Codes.Road, new ZString[] { "30" }, declaration);
			AssertBorderTransportMeansList("Air Codes", TransportTypeList.Codes.Air, new ZString[] { "40", "41" }, declaration);
			AssertBorderTransportMeansList("Mail Codes", TransportTypeList.Codes.Mail, new ZString[] { "10", "11", "21", "30", "40", "41", "80", "81" }, declaration);
			AssertBorderTransportMeansList("FixedTransportInstallations Codes", TransportTypeList.Codes.FixedTransportInstallations, new ZString[] { "10", "11", "21", "30", "40", "41", "80", "81" }, declaration);
			AssertBorderTransportMeansList("InlandWaterwayTransport Codes", TransportTypeList.Codes.InlandWaterwayTransport, new ZString[] { "80", "81" }, declaration);
			AssertBorderTransportMeansList("OwnPropulsion Codes", TransportTypeList.Codes.OwnPropulsion, new ZString[] { "10", "11", "21", "30", "40", "41", "80", "81" }, declaration);
		});
	}

	void AssertBorderTransportMeansList(string assertionMsg, string transportMode, ZString[] expectedListValues, JobDeclaration declaration)
	{
		declaration.JE_TransportMode = transportMode;

		var addInfoLookups = declaration.AddInfoLookups;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertContainsExactElementsInAnyOrder($"Export {assertionMsg}", expectedListValues, addInfoLookups.BorderTransportMeansList.GetAllCodes());

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertContainsExactElementsInAnyOrder($"Import {assertionMsg}", expectedListValues, addInfoLookups.BorderTransportMeansList.GetAllCodes());
	}

	public void TestMethodOfPaymentList()
	{
		JobDeclaration declaration = Factory.New<JobDeclaration>();
		var list = declaration.AddInfoLookups.MethodOfPaymentList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "A, B, C, D, E, G, H, J, K, L, O, P, R, S, T, U, V, Z", list.CodesAsString);
			AssertEquals("Cached", list, declaration.AddInfoLookups.MethodOfPaymentList);
		});
	}

	public void TestAgreedPlaceCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Poland, parent: euDataGrouping);

		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "2", "222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		var list = declaration.AddInfoLookups.AgreedPlaceCodeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "1, 2", ((CodeDescriptionPairList)list).CodesAsString);
			AssertSame("Should be cached", list, declaration.AddInfoLookups.AgreedPlaceCodeList);
		});
	}
}
