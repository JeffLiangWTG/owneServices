using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(CIMEDIMessage))]
	public class CIMEDIMessageTest : EnterpriseBusinessObjectTestCaseWithListChecking<CIMEDIMessage>
	{
		public void TestMessageTypeDescription()
		{
			var message = (CIMEDIMessage)GetNewBusinessObject();
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FWB;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FWB, message.MessageTypeDescription);
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FHL;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FHL, message.MessageTypeDescription);
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FSR;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FSR, message.MessageTypeDescription);

			message.EM_MessageType = CIMEDIMessage.MessageTypes.Received.FMA;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FMA, message.MessageTypeDescription);
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Received.FNA;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FNA, message.MessageTypeDescription);
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Received.FSA;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FSA, message.MessageTypeDescription);
			message.EM_MessageType = CIMEDIMessage.MessageTypes.Received.FSU;
			AssertEquals(CargoIMPMessageTypeList.Descriptions.FSU, message.MessageTypeDescription);

			message.EM_MessageType = "X_X";
			AssertEquals(CIMEDIMessage.UnknownMessageTypeDescription, message.MessageTypeDescription);
			message.EM_MessageType = "";
			AssertEquals(CIMEDIMessage.UnknownMessageTypeDescription, message.MessageTypeDescription);
		}

		public void TestParentInterchage()
		{
			var message = (CIMEDIMessage)GetNewBusinessObject();
			AssertNull(message.Interchange);

			var interchange = Factory.New<CIMEDIInterchange>();
			message.EM_EI = interchange.PK;
			AssertNotNull(message.Interchange);
			AssertEquals(interchange, message.Interchange);
		}

		public void TestSetDefaultValues()
		{
			CIMEDIMessage message = (CIMEDIMessage)GetNewBusinessObject();
			AssertEquals(ZBool.True, message.EM_IsActive);
			AssertEquals(ExpectedApplicationCode, message.EM_ApplicationCode);
			AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, message.EM_GE);
			AssertEquals(CIMEDIMessage.Status.Queued, message.EM_Status);
		}

		public void TestSave()
		{
			CIMEDIMessage message = (CIMEDIMessage)GetNewBusinessObject();
			Factory.Save();
			Assert(message.EM_MessageNum != "");
		}

		public void TestEM_User()
		{
			CIMEDIMessage message = (CIMEDIMessage)GetNewBusinessObject();
			message.EM_ReceiveTransmit = CIMEDIMessage.Direction.Transmit;
			Factory.Save();
			AssertEquals(GlbStaff.CurrentUser.GS_FullName, message.EM_User);

			message = (CIMEDIMessage)GetNewBusinessObject();
			message.EM_ReceiveTransmit = CIMEDIMessage.Direction.Receive;
			Factory.Save();
			AssertEquals("System", message.EM_User);
		}

		public void TestEM_StatusDateTime()
		{
			CIMEDIMessage message = (CIMEDIMessage)GetNewBusinessObject();
			Assert(message.EM_StatusDateTime.IsEmpty);
			Factory.Save();
			Assert(!message.EM_StatusDateTime.IsEmpty);
		}

		public void TestStaff()
		{
			CIMEDIMessage message = (CIMEDIMessage)GetNewBusinessObject();
			message.EM_ReceiveTransmit = CIMEDIMessage.Direction.Transmit;
			Factory.Save();
			AssertNotNull(message.Staff);
		}

		public void TestClearMessageNumberOnFailureToSave()
		{
			var message = (CIMEDIMessage)GetNewBusinessObject();
			AssertEquals(true, message.ClearMessageNumberOnFailureToSave);
		}

		protected virtual string ExpectedApplicationCode
		{
			get { return CIMEDIMessage.ApplicationCodes.CIM; }
		}
	}
}
