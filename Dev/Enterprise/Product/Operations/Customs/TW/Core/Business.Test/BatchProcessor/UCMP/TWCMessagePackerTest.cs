using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing;

[TestedType(typeof(TWCMessagePacker))]
sealed class TWCMessagePackerTest : UniversalCustomsEDIMessagePackerTest<TWCMessagePacker>
{
	protected override string ApplicationCode => EDIMessage.ApplicationCodes.TaiwanCustoms;

	[TestDate(2019, 7, 19)]
	public void TestMessagesPopulateNewInterchange()
	{
		using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
		{
			var logger = new LoggingInformation();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "AIR";
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = org.PK;
			var branch = GlbCompany.CurrentCompany.Branches.First();
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "111";
			var extPassword = Factory.New<GlbExternalPassword>();
			extPassword.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			extPassword.GP_MailBoxID = "123-3";
			extPassword.GP_UserID = "001";
			extPassword.GP_GS = staff.PK;
			extPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "111";
			declaration.JE_CustomsProfile = "123-3";
			declaration.JE_MessageType = "EXP";
			declaration.JE_CustomsOffice = "AA";
			var messageText = "<?xml version=\"1.0\" encoding=\"utf - 8\"?><Value>您好</Value>";
			TWMessageForTest message;
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.CEI_CustomsOffice = "BB";
				entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 15);
				entryInstruction.CEI_Style = "G1";
				entryInstruction.CEI_BoxNumber = "123";
				message = Factory.New<TWMessageForTest>();
				entryHeader.Messages.Add(message);
				message.EM_ApplicationReference = "333";
				message.EM_MessageType = MessageTypeList.Codes.ICD;
				message.EM_IsTestMessage = true;
				message.EM_MessageOwner = "ICD89319";
				message.ForceDeprecatedNTextUsageForTesting = true;
				message.EM_MessageNText = messageText;
				Factory.Save();

				IUniversalCustomsEDIMessagePacker packer = new TWCMessagePacker();
				packer.Pack(message, Factory.New<EDIInterchange>(), logger);

				Factory.Save();

				message.Reload();

				CombineAssertions(() =>
				{
					var zquery = new ZDBOnlyQuery(typeof(EDIInterchange));
					var ediMessageQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
					ediMessageQuery.AddToFilter(EDIMessageSchema.PK, message.PK);
					zquery.AddSubQuery(ediMessageQuery, JoinCondition.And);
					var interchange = Factory.LoadTop1<EDIInterchange>(zquery);

					AssertEquals("EI_ApplicationCode", message.EM_ApplicationCode, interchange.EI_ApplicationCode);
					AssertEquals("EI_InterchangeType", message.IsLicensingMessageDeliveryNotification ? MessageTypeList.Codes.NXM : message.EM_MessageType, interchange.EI_InterchangeType);
					AssertEquals("EI_ReceiveTransmit", EDIMessage.Direction.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("EI_HeaderText", $"<TWMessageInfo><MailBox>{(message.IsLicensingMessageDeliveryNotification ? ZString.Empty : message.EM_ApplicationReference)}</MailBox><MessageType>{message.EM_MessageType}</MessageType><EntryNumber>{((ITWMessageInfoProvider)message.CusEntryHeader).EntryNumber}</EntryNumber><EntryNumberType>{((ITWMessageInfoProvider)message.CusEntryHeader).EntryNumberType}</EntryNumberType><StaffCode>{declaration.JE_GS_NKCusAgent}</StaffCode><CompanyID>{GlbCompany.CurrentCompany.GC_Code}</CompanyID><PasswordType>{PasswordTypesList.Codes.TVA}</PasswordType></TWMessageInfo>", interchange.EI_HeaderText);
					AssertEquals("EI_From", GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
					AssertEquals("EI_To", "TWCustomsTest", interchange.EI_To);
					AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
					AssertEquals("EI_BodyText", message.EM_MessageText, interchange.EI_BodyText);
					AssertEquals("EI_FooterText", "", interchange.EI_FooterText);
					AssertEquals("EI_IsActive", true, interchange.EI_IsActive);
					AssertEquals("EI_GP", message.EM_GP, interchange.EI_GP);
					AssertEquals("EM_GB", message.EM_GB, interchange.EI_GB);
				});
			}
		}
	}
}
