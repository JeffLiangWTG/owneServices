using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(EDIMessage))]
sealed class EDIMessageTest : BaseEdiMessageTest<EDIMessage>
{
	protected override string ApplicationCode => Messaging.Business.EDIInterchange.ApplicationCodes.PLCustomsNCTS;

	[TestDate(2021, 03, 15)]
	public void TestSendersReferencePlaceHolderReplacement()
	{
		const string countryCode = "PL";
		const string year = "21";
		const string eoriNumber = "1122334455";
		const string sequence = "00000001";

		var eoriOrganization = Factory.New<OrgHeader>();
		eoriOrganization.FillWithValidTestData();
		eoriOrganization.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, countryCode);

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var movementHeader = nctsHeader.MovementHeader;
		movementHeader.Representative.OrganisationPK = eoriOrganization.PK;

		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EUJobMessageTypeList.Codes.NctsDeparture;
		message.EM_MessageText = $"<LRN>{Business.EDIMessage.PL_NCTS_LRN_PlaceHolder}</LRN>";
		message.EM_MessageSubType = MessageSubTypeForTest;
		movementHeader.Messages.Add(message);

		var glbStaffCertificate = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		glbStaffCertificate.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		glbStaffCertificate.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		Factory.Save();

		CombineAssertions(() =>
		{
			var expectedLRN = $"{countryCode}{eoriNumber}{year}{sequence}";
			AssertStartsWith("Should replace message 1 placeholder with generated LRN", $"<LRN>{expectedLRN}", message.EM_MessageText);
		});
	}

	[TestDate(2023, 11, 01)]
	public override void TestMessageIdentificationPlaceholder()
	{
		const string testMessageSubType = "015";

		var factory = new BusinessObjectFactory();
		var message = factory.New<EDIMessage>();
		message.EM_MessageSubType = testMessageSubType;
		message.EM_MessageText = $"<Msg>{BaseEDIMessage.PLMessageNumberPlaceHolder}</Msg>";

		var staffPassword = GlbStaff.CurrentUser.GetPLWrapper().PLBPassword;
		staffPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		staffPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
		factory.Save();

		var yearPrefix = ZDate.Today.ToString("yy");
		var messageTypeCode = "IE015";
		var sequence = "0000001";
		var expectedMessageNumber = yearPrefix + messageTypeCode + sequence;
		AssertStartsWith("Message Identification placeholder should be replaced", $"<Msg>{expectedMessageNumber}", message.EM_MessageText);
	}

	protected override string ExpectedMessageNumPrefix => base.ExpectedMessageNumPrefix + "IE" + MessageSubTypeForTest;

	const string MessageSubTypeForTest = "001";

	protected override void SetUp()
	{
		base.SetUp();
		Message.EM_MessageSubType = MessageSubTypeForTest;
	}
}
