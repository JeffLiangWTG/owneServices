using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class ESignatureHelperTest : TestCaseWithFactory
	{
		public void TestForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				new ESignatureHelper().CanSignMessage(AcknowledgeAndSign);
				AssertNotNull("A form should be shown for signing", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(ESignatureForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		protected override void SetUp()
		{
			currentUser = TRGlbStaffWrapper.Get(MasterFiles.Business.GlbStaff.CurrentUser).TRBPassword;
			currentUser.GP_UserID = "1234";
			currentUser.CurrentDecryptedPassword = "xxx";
			currentUser.GP_CertificateAuthority = "TÜBİTAK";
			currentUser.TR_Chipset = "EKART";
			currentUser.GP_CertificateSerialNumber = "02b9572b9cad7250a906b3";
			ediMessage = Factory.NewWithValidTestData<EDIMessage>();
			ediMessage.EM_MessageText = message;
			base.SetUp();
		}
		MessageSendAcknowledgeAndSign AcknowledgeAndSign
		{
			get
			{
				if (acknowledgeAndSignget == null)
				{
					acknowledgeAndSignget = new MessageSendAcknowledgeAndSign(message, currentUser);
				}
				return acknowledgeAndSignget;
			}
		}
		MessageSendAcknowledgeAndSign acknowledgeAndSignget;

		GlbExternalPassword_TR currentUser;
		readonly ZString message = "<Body><Test>MessageToSign</Test></Body>";
		EDIMessage ediMessage;
	}
}
