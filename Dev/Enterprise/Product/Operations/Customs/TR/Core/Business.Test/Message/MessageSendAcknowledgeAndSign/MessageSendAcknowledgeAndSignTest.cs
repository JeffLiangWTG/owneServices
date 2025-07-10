using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(MessageSendAcknowledgeAndSign))]
	public class MessageSendAcknowledgeAndSignTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageSendAcknowledgeAndSign(message, CreateExternalPassword(staff, "EKART", "02b9572b9cad7250a906b3"));
		}

		GlbExternalPassword_TR CreateExternalPassword(Enterprise.MasterFiles.Business.GlbStaff staff, string chipset, string certificateSerialNumber)
		{
			var result = TRGlbStaffWrapper.Get(staff).TRBPassword;
			result.GP_UserID = "1234";
			result.CurrentDecryptedPassword = "xxx";
			result.GP_CertificateAuthority = "TÜBİTAK";
			result.TR_Chipset = chipset;
			result.GP_CertificateSerialNumber = certificateSerialNumber;
			return result;
		}

		Enterprise.MasterFiles.Business.GlbStaff staff;
		protected override void SetUp()
		{
			staff = Factory.New<Enterprise.MasterFiles.Business.GlbStaff>();
			staff.GS_Code = "TSN";
			staff.GS_FullName = "TR Testing User";
			Factory.Save();

			base.SetUp();
		}

		readonly ZString message = "<Body><Test>MessageToSign</Test></Body>";
	}
}

