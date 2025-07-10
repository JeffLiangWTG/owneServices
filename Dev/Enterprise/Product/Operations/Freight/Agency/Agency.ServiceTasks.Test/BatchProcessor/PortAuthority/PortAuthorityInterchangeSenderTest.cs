using System;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class PortAuthorityInterchangeSenderTest : ShippingManagerInterchangeSenderTest
	{
		[TestDate]
		public void TestSendPortAuthorityEmail()
		{
			const string homePort = "AUBNE";
			const string otherHomePort = "AUPKL";
			const string overseasPort = "SGSIN";
			SetPortAuthoritySettings(homePort, otherHomePort);
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Blaticus";
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "666";
			JobSailing sailing1 = FindOrCreateSailing(voyage, homePort, overseasPort);
			JobSailing sailing2 = FindOrCreateSailing(voyage, otherHomePort, overseasPort);
			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			AddMessage(messages, sailing1.Origin);
			AddMessage(messages, sailing2.Origin);
			Factory.Save();
			PortAuthorityInterchangeSender sender = new PortAuthorityInterchangeSender();
			sender.ExecuteBatch();
			const string attachmentTemplate = "UNB+UNOA:4+SENDERID{0}+RECIPIENTID{0}+{1:yyMMdd:HHmm}+1'BEGIN+{0}+END'UNZ+1+1'";
			AssertEmailSent("message1", "bob1@freadnet.org", "Manifest from: SenderID1 for: Blaticus 666", "00000001.edi", string.Format(attachmentTemplate, 1, ZDateTime.Now));
			AssertEmailSent("message2", "bob2@freadnet.org", "Manifest from: SenderID2 for: Blaticus 666", "00000001.edi", string.Format(attachmentTemplate, 2, ZDateTime.Now));
		}

		#region Implementation
		static EDIMessage AddMessage(NonDependentEDIMessageCollection messages, ISailingEndPoint endPoint)
		{
			EDIMessage result = endPoint.Messages.AddNew(typeof(PortAuthorityMessage));
			result.FillWithValidTestData();
			result.EM_MessageText = "BEGIN+" + EDIMessage.MessageNumberPlaceHolder + "+END'";
			result.EM_MessageOwner = String.Empty;
			result.EM_ApplicationCode = EDIInterchange.ApplicationCodes.PortAuthority;
			result.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			messages.Add(result);
			return result;
		}
		#endregion
	}
}
