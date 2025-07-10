using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class MessageProcessorHelperTest : TestCaseWithFactory
	{
		public void TestRegistrationUser()
		{
			SetUserInfo();

			var factory = Factory;
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("T1OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");

			var successMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			successMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";
			header.Messages.Add(successMessage);

			var outgoingTROMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingTROMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 13, 30, 38);
			outgoingTROMessage.EM_MessageOwner = "BP";
			header.Messages.Add(outgoingTROMessage);

			var outgoingTROMessage1 = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingTROMessage1.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 14, 02, 51);
			outgoingTROMessage1.EM_MessageOwner = "WZG";
			header.Messages.Add(outgoingTROMessage1);

			var outgoingTROMessage2 = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingTROMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 15, 43, 34);
			outgoingTROMessage2.EM_MessageOwner = "KNZ";
			header.Messages.Add(outgoingTROMessage2);

			var outgoingT1OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingT1OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 17, 04);
			outgoingT1OMessage.EM_MessageOwner = "BP";
			header.Messages.Add(outgoingT1OMessage);

			var incomingT1OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			incomingT1OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 24, 36);
			header.Messages.Add(incomingT1OMessage);

			var outgoingT3OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingT3OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 45, 21);
			outgoingT3OMessage.EM_MessageOwner = "AAA";
			header.Messages.Add(outgoingT3OMessage);

			var incomingT3OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			incomingT3OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 51, 56);
			header.Messages.Add(incomingT3OMessage);

			var messageSignedBy = MessageProcessorHelper.GetMessageSignedBy(header);
			AssertEquals("KNZ", messageSignedBy);
		}

		void SetUserInfo()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "KNZ";
			staff.GS_FullName = "KNZ Testing Signed User";

			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "BP";
			staff1.GS_LoginName = "BP";
			staff1.GS_FullName = "BP Testing Signed User";

			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "WZG";
			staff2.GS_LoginName = "WZG";
			staff2.GS_FullName = "WZG Testing Signed User";

			var staff3 = group.Staff.AddNew();
			staff3.GS_Code = "AAA";
			staff3.GS_LoginName = "AAA";
			staff3.GS_FullName = "AAA Testing Signed User";

			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var user1 = TRGlbStaffWrapper.Get(staff3).TRBPassword;
			user1.GP_UserID = "20201224199";
			user1.CurrentDecryptedPassword = "12345699";
		}
	}
}
