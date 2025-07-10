using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.V902.Messages.CUSRES;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSRESHeaderProvider))]
sealed class CUSRESHeaderProviderTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When message is null", () => new CUSRESHeaderProvider(Factory, null));
		AssertExceptionThrown<ArgumentNullException>("When factory is null", () => new CUSRESHeaderProvider(null, new CUSRESMessage()));
	});

	public void TestObjectInitialization() => CombineAssertions(() =>
	{
		AssertNull("When message param is null", CUSRESHeaderProvider.New(null));

		var ediMessage = Factory.New<EDIMessage>();
		AssertNull("When EM_MessageText is null or empty", CUSRESHeaderProvider.New(ediMessage));
		const string BadEDIMessage = "UNH+724180+CUSRES:1:92:UN:NEP+9355964402023012600024901'" +
			"BGM+932++137:20230126:102+32'" +
			"NAD+EX+911705907::NO1+NOBLE HARVEST AS'" +
			"NAD+DT+935596440::NO1'" +
			"DTM+58:20230126:102'" +
			"DTM+184:20230126114307:204'" +
			"TAX+4+161:272'" +
			"RFF+ABT:4410912300030065'" +
			"UNT+9+724180'";

		ediMessage.EM_MessageText = BadEDIMessage;
		AssertNull("When bad EM_MessageText string is passed", CUSRESHeaderProvider.New(ediMessage));

		ediMessage.EM_MessageText = CUSRESMessage;
		AssertNotNull("When correct EM_MessageText string is passed", CUSRESHeaderProvider.New(ediMessage));
	});

	public void TestDeclarationID()
	{
		AssertEquals("Declaration Id", "9355964402023012600011401", headerProvider.DeclarationID);
	}

	public void TestMessageType()
	{
		AssertEquals("MessageType", "932", headerProvider.MessageType);
	}

	public void TestMessageFunction()
	{
		AssertEquals("MessageFunction", "32", headerProvider.MessageFunction);
	}

	public void TestMessageTextCodes()
	{
		AssertContainsExactElementsInAnyOrder("MessageTextCodes", new string[] { "999", "954" }, headerProvider.MessageTextCodes);
	}

	public void TestRequestedControlAction()
	{
		AssertEquals("RequestedControlAction", "5", headerProvider.RequestedControlAction);
	}

	public void TestReleaseNumber()
	{
		const string CUSRESMessageWithRFF = "UNH+724180+CUSRES:1:902:UN:NEP+9355964402023012600024901'" +
			"BGM+932++137:20230126:102+32'" +
			"NAD+EX+911705907::NO1+NOBLE HARVEST AS'" +
			"NAD+DT+935596440::NO1'" +
			"DTM+58:20230126:102'" +
			"DTM+184:20230126114307:204'" +
			"TAX+4+161:272'" +
			"RFF+ABT:4410912300030065'" +
			"UNT+9+724180'";

		var cusresResponse = Factory.New<EDIMessage>();
		cusresResponse.EM_MessageText = CUSRESMessageWithRFF;
		ICUSRESHeader provider = CUSRESHeaderProvider.New(cusresResponse);

		AssertEquals("ReleaseNumber", "4410912300030065", provider.ReleaseNumber);
	}

	public void TestReleaseDate()
	{
		AssertEquals("ReleaseDate", new ZDateTime(2023, 1, 26), headerProvider.ReleaseDate);
	}

	public void TestLimitDate()
	{
		AssertEquals("LimitDate", new ZDateTime(2022, 4, 13), headerProvider.LimitDate);
	}

	public void TestCreationDate()
	{
		AssertEquals("CreationDate", new ZDateTime(2023, 1, 26), headerProvider.CreationDate);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var testMessage = Factory.NewWithValidTestData<EDIMessage>();
		testMessage.EM_MessageText = CUSRESMessage;
		return CUSRESHeaderProvider.New(testMessage);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var responseMessage = Factory.New<EDIMessage>();
		responseMessage.EM_MessageText = CUSRESMessage;
		headerProvider = CUSRESHeaderProvider.New(responseMessage);
	}

	ICUSRESHeader headerProvider;

	const string CUSRESMessage = "UNH+07140311062255+CUSRES:1:902:UN:NEP-I+9355964402023012600011401'" +
		"BGM+932++137:20230126:102+32'" +
		"DTM+255:20220413:102'" +
		"DTM+58:20230126:102'GIS+5::'" +
		"FTX+AAP+++999:Ikke mulig med forhnds deklarering(godsnr 307)'" +
		"FTX+AAP+++954:Melding om dokumentkontroll - dokumentene skal leveres Tolletaten'" +
		"UNT+6+07140311062255'";
}
