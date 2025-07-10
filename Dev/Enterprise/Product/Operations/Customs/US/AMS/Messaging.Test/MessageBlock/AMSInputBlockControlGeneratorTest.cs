using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSInputBlockControlGeneratorTest : TestCaseWithFactory
	{
		public void TestConstructorWithMessageAttachee()
		{
			var generator = new AMSInputBlockControlGenerator();
			AssertEquals("AMSUserCode should only be populated in EDIInterchange for sending to AMS", "", generator.B.AMSUserCode);
			AssertEquals("Password should only be populated in EDIInterchange for sending to AMS", "", generator.B.Password);
		}

		public void TestBandYBlocks()
		{
			var obj = new AMSInputBlockControlGenerator();
			AssertEquals(typeof(APLACR), obj.B.GetType());
			AssertEquals(typeof(APLZCR), obj.Y.GetType());
		}

		public void TestDeserialise()
		{
			var obj = new AMSInputBlockControlGenerator();
			var aplacr = new APLACR() { ApplicationIdentifier = AMSApplicationIdentifierCodeList.Codes.EquipmentInventory };
			var equc01 = new EQUC01() { ContainerEquipmentDescriptionCode = "C1" };
			var aplzcr = new APLZCR() { ApplicationIdentifier = "Y1" };

			AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not start with a 'ACR' block", () => obj.Deserialise(equc01.Serialise()));
			AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not end with a 'ZCR' block", () => obj.Deserialise(aplacr.Serialise() + equc01.Serialise()));
			AssertNoExceptionThrown(() => obj.Deserialise(aplacr.Serialise() + equc01.Serialise() + aplzcr.Serialise()));

			AssertEquals(1, obj.MessageBlocks.Count);
			AssertEquals(AMSEDIMessage.ApplicationCodes.AMS, obj.ApplicationCode);
			AssertEquals(aplacr, obj.B);
			AssertEquals(equc01, obj.MessageBlocks[0]);
			AssertEquals(aplzcr, obj.Y);
		}

		public void TestCreateMessage()
		{
			var obj = new AMSInputBlockControlGenerator();
			var aplacr = new APLACR() { ApplicationIdentifier = AMSApplicationIdentifierCodeList.Codes.EquipmentInventory };
			var equc01 = new EQUC01() { ContainerEquipmentDescriptionCode = "C1" };
			var aplzcr = new APLZCR() { ApplicationIdentifier = "Y1" };

			obj.Deserialise(aplacr.Serialise() + equc01.Serialise() + aplzcr.Serialise());
			var message = obj.CreateMessage<AMSEDIMessage>(Factory);
			AssertEquals(CBPEDIInterchange.ApplicationCodes.AMS, message.EM_ApplicationCode);
			AssertEquals(AMSApplicationIdentifierCodeList.Codes.EquipmentInventory, message.EM_MessageType);
			AssertEquals(CBPEDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(CBPEDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("ACR          EI                                                                 C01                                            C1                               ZCR          Y1                   00000", message.EM_MessageText);
		}

		public void TestSerialise()
		{
			var obj = new AMSInputBlockControlGenerator();
			var aplacr = new APLACR() { ApplicationIdentifier = AMSApplicationIdentifierCodeList.Codes.EquipmentInventory };
			var equc01 = new EQUC01() { ContainerEquipmentDescriptionCode = "C1" };
			var aplzcr = new APLZCR() { ApplicationIdentifier = "Y1" };

			obj.Deserialise(aplacr.Serialise() + equc01.Serialise() + aplzcr.Serialise());
			AssertEquals("ACR          EI                                                                 C01                                            C1                               ZCR          Y1                   00000                                         ", obj.Serialise());
			AssertMultilineASCIIEquals("", @"-----------------APLACR-----------------
 Application Identifier (14-15) :EI

-----------------EQUC01-----------------
 Container Equipment Description Code (48-49) :C1

-----------------APLZCR-----------------
 Application Identifier (14-15)               :Y1
 Number Of Transaction Detail Records (35-39) :0
", obj.Serialise(true));
			AssertEquals("ACR          EI                                                                 C01                                            C1                               ZCR          Y1                   00000                                         ", obj.Serialise(false));
		}
	}
}
