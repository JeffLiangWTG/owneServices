using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.MessageSending.Arrival.Testing;

[TestedType(typeof(CC007CTypeAdditionalDataProvider))]
sealed class CC007CTypeAdditionalDataProviderTest : TestCaseWithFactory
{
	public void TestGetExtensions() => CombineAssertions(() =>
	{
		var messageSendingObject = new NctsHeaderMessageSendingObject(CreateNctsHeader());
		var extensions = DataProvider.GetExtensions(messageSendingObject);
		AssertNotNull("[Pre-Condition] Extensions", extensions);
		AssertEquals("Extensions Array Count", 0, extensions.Count);
	});

	public void TestGetSimplifiedProcedure() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>(() => DataProvider.GetSimplifiedProcedure(null));

		var nctsHeader = CreateNctsHeader();
		AssertEquals("When no CusAuthorizationUsages Records", "0", DataProvider.GetSimplifiedProcedure(nctsHeader));

		var cusAuthorization = nctsHeader.CusAuthorizationUsages.AddNew();
		cusAuthorization.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
		cusAuthorization.AGC_Number = "PQR123";

		AssertEquals("When AGC_Code = ACE", "1", DataProvider.GetSimplifiedProcedure(nctsHeader));

		cusAuthorization.AGC_Code = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
		AssertEquals("When AGC_Code = ACT", "1", DataProvider.GetSimplifiedProcedure(nctsHeader));

		cusAuthorization.AGC_Code = ZString.Empty;
		AssertEquals("When AGC_Code = EMPTY", "0", DataProvider.GetSimplifiedProcedure(nctsHeader));
	});

	public void TestGetIdentificationNumberFromJobDocAddress() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>(() => DataProvider.GetIdentificationNumber(null));

		var nctsHeader = CreateNctsHeader();
		var orgHeader = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited",
			"123 Test Street", "A12B3C4", "City", "IEXX", Constants.CountryCodes.Norway, "0123456789000", "TIR123");
		var contact = orgHeader.Contacts.AddNew();
		contact.OC_ContactName = "Joe Bloggs";
		contact.OC_Phone = "5551234";
		contact.OC_Email = "test@example.com";
		contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;

		AssertEquals("Identification Number when Address with EORI",
			"NO0123456789000",
			DataProvider.GetIdentificationNumber(nctsHeader.DestinationTrader));
	});

	public void TestGetEconomicOperator_WhenE2GovRegNum()
	{
		AssertExceptionThrown<ArgumentNullException>(() => DataProvider.GetEconomicOperator(null));

		var goodsLocation = CreateNctsHeader().ArrivalMovementHeader.GoodsLocation;
		goodsLocation.Address.E2_AddressOverride = true;
		goodsLocation.CGL_Qualifier = "X";
		goodsLocation.Address.E2_GovRegNum = "TEST999";

		var economicOperatorDataProvider = DataProvider.GetEconomicOperator(goodsLocation);

		AssertNotNull("EconomicOperator Data Provider", economicOperatorDataProvider);
		CombineAssertions("Economic Operator Data", () =>
		{
			AssertNotNullOrEmpty("Identification Number", economicOperatorDataProvider.IdentificationNumber);
			AssertEquals("Identification Number Value", "TEST999",economicOperatorDataProvider.IdentificationNumber);
		});
	}

	public void TestGetEconomicOperator_WhenEoriNumber()
	{
		var goodsLocation = CreateNctsHeader().ArrivalMovementHeader.GoodsLocation;
		goodsLocation.CGL_Qualifier = "X";
		var orgHeader = goodsLocation.Address.Organisation;
		orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "TEST123");

		var economicOperatorDataProvider = DataProvider.GetEconomicOperator(goodsLocation);

		AssertNotNull("EconomicOperator Data Provider", economicOperatorDataProvider);
		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("Identification Number", economicOperatorDataProvider.IdentificationNumber);
			AssertEquals("Identification Number Value", "NOTEST123", economicOperatorDataProvider.IdentificationNumber);
		});
	}

	public void TestGetSeals() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>(() => DataProvider.GetSeal(null));

		var nctsHeader = CreateNctsHeader();
		var incident = nctsHeader.EnRouteIncidents.AddNew();
		var container = incident.IncidentContainers.AddNew();
		container.BC_ContainerNum = "CNT12222";

		AssertEquals("When No Seals are present", 0, DataProvider.GetSeal(container).Count);

		container.BC_Seal1 = "123";
		AssertContainsExactElementsInExactOrder("When only BC_Seal1 is present",
			new[] { "123" },
			DataProvider.GetSeal(container).Select(s => s.Identifier));

		container.Seals.AddNew().BK_SealNumber = "S2345";
		container.BC_Seal2 = "345";

		AssertContainsExactElementsInExactOrder("When only BC_Seal1, BC_Seal2 and Seals Array with 1 record is present",
			new[] { "123", "345", "S2345" },
			DataProvider.GetSeal(container).Select(s => s.Identifier));
	});

	public void TestGetNumberOfSeals() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>(() => DataProvider.GetNumberOfSeals(null));

		var nctsHeader = CreateNctsHeader();
		var incident = nctsHeader.EnRouteIncidents.AddNew();
		var container = incident.IncidentContainers.AddNew();
		container.BC_ContainerNum = "CNT12";

		AssertEquals("When No Seals are present", "0", DataProvider.GetNumberOfSeals(container));

		container.BC_Seal2 = "S12";
		AssertEquals("When BC_Seal2 is specified", "1", DataProvider.GetNumberOfSeals(container));

		container.Seals.AddNew().BK_SealNumber = "78837";
		container.BC_Seal1 = "345";

		AssertEquals("When only BC_Seal1, BC_Seal2 and Seals Array with 1 record is present", "3", DataProvider.GetNumberOfSeals(container));
	});

	public void TestGetContainerIndicator() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>(() => DataProvider.GetContainerIndicator(null));

		var nctsHeader = CreateNctsHeader();
		var incident = nctsHeader.EnRouteIncidents.AddNew();

		AssertEquals("When No Containers are added", "0", DataProvider.GetContainerIndicator(incident));

		var container1 = incident.IncidentContainers.AddNew();
		AssertEquals("When One Container is added but with empty BC_Mode", "0", DataProvider.GetContainerIndicator(incident));

		container1.BC_Mode = Core.Constants.ContainerModes.Containerised;
		AssertEquals("When One Container is added but with BC_Mode = CNT", "1", DataProvider.GetContainerIndicator(incident));

		incident.IncidentContainers.DeleteAll();

		incident.IncidentContainers.AddNew().BC_Mode = Constants.ContainerModes.NonContainerised;
		incident.IncidentContainers.AddNew().BC_Mode = Constants.ContainerModes.Containerised;
		AssertEquals("When Two Container is added having one with BC_Mode = CNT", "1", DataProvider.GetContainerIndicator(incident));
	});

	NctsHeader CreateNctsHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
		return nctsHeader;
	}

	ICC007CTypeAdditionalDataProvider DataProvider => dataProvider ??= new CC007CTypeAdditionalDataProvider();
	ICC007CTypeAdditionalDataProvider dataProvider;
}
