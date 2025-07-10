using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class NCTSResponseMessageHelperTest : TestCaseWithFactory
{
	public void TestNameSpacesIgnored()
	{
		var messageInvalid = Factory.New<EDIMessage>();
		messageInvalid.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		messageInvalid.EM_MessageText = "<ns2:TypeForTest xmlns:ns2=\"http://ecs.dgtaxud.ec\"><stringPropertyForTest>test value</stringPropertyForTest></ns2:TypeForTest>";

		AssertNoExceptionThrown(() => messageInvalid.GetCachedInboundProvider<TypeForTest, TypeProviderForTest>());
	}

	public void TestXSDValidation()
	{
		var messageInvalid = Factory.New<EDIMessage>();
		messageInvalid.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		messageInvalid.EM_MessageText = "<TypeForTest><stringPropertyForTest>value that is too long for XSD validation</stringPropertyForTest></TypeForTest>";

		AssertNoExceptionThrown("Should NOT have Exception even though it has an invalid length for StringPropertyForTest.", () => messageInvalid.GetCachedInboundProvider<TypeForTest, TypeProviderForTest>());
	}

	public void TestGetCachedInboundProvider_MessageIsNull()
	{
		AssertNull(((EDIMessage)null).GetCachedInboundProvider<TypeForTest, TypeProviderForTest>());
	}

	public void TestGetCachedInboundProvider_MessageIsNotReceive()
	{
		AssertNull(Factory.New<EDIMessage>().GetCachedInboundProvider<TypeForTest, TypeProviderForTest>());
	}

	public void TestGetCachedInboundProvider_Valid()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_MessageText = "<TypeForTest><stringPropertyForTest>test value</stringPropertyForTest></TypeForTest>";
		var cachedInboundProvider = message.GetCachedInboundProvider<TypeForTest, TypeProviderForTest>();

		CombineAssertions("The value in the provider should be equal to the value we set in the XML.", () =>
		{
			AssertEquals(typeof(TypeProviderForTest), cachedInboundProvider.GetType());
			AssertEquals("test value", cachedInboundProvider.StringPropertyForTest);
		});
	}

	public void TestGetBranchPkFromJobBO()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var movementHeader = nctsHeader.MovementHeader;
		CombineAssertions(() =>
		{
			AssertEquals(movementHeader.RegistryBranchPK, NCTSResponseMessageHelper.GetBranchPkFromJobBO(movementHeader));
			AssertEquals(nctsHeader.RegistryBranchPK, NCTSResponseMessageHelper.GetBranchPkFromJobBO(nctsHeader));
		});
	}
}

[Serializable]
[XmlRoot("TypeForTest", Namespace = "http://ecs.dgtaxud.ec")]
public class TypeForTest
{
	[MaxLength(10)]
	[XmlElement("stringPropertyForTest", Form = XmlSchemaForm.Unqualified)]
	public string StringPropertyForTest { get; set; }
}

public class TypeProviderForTest : INCTSIncomingDataProvider
{
	public TypeProviderForTest(TypeForTest xmlObject)
	{
		StringPropertyForTest = xmlObject.StringPropertyForTest;
	}
	public string StringPropertyForTest { get; set; }

	public string LRN => null;

	public string MRN => null;

	public string MessageSender => null;

	public string MessageRecipient => null;

	public DateTime PreparationDateTime => default;

	public DateTime DeclarationAcceptanceDate => default;

	public string MessageIdentification => null;

	public string CorrelationIdentifier => null;

	public string CustomsOfficeOfDepartureReferenceNumber => null;

	public INCTSHolderOfTheTransitProcedureProvider HolderOfTheTransitProcedure => null;
}
