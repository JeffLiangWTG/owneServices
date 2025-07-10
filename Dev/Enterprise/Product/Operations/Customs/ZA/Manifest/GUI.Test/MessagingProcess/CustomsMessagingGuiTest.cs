using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.MessagingProcess;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.Customs.ZA.Manifest.Business.MessagingProcess;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI.MessagingProcess.Testing
{
	sealed class CustomsMessagingGuiTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var header = CreateHeader();

			using (var mainForm = new ZForm())
			{
				var zaGUI = new CustomsMessagingGui(header, "ORG", null, mainForm);

				CombineAssertions(() =>
				{
					AssertType<CustomsMessagingProvider>("Provider", zaGUI.MessagingSupporter.Provider);
					AssertSame("Form", mainForm, zaGUI.topLevelBusinessObjectForm);
				});
			}
		}

		public void TestICustomsMessagingGui()
		{
			using (var mainForm = new ZForm())
			{
				var header = CreateHeader();
				var gui = new CustomsMessagingGui(header, "ORG", null, mainForm) as ICustomsMessagingGui;

				CombineAssertions(() =>
				{
					AssertNotNull("Supporter", gui.MessagingSupporter);
					AssertSame("Form", mainForm, gui.TopLevelBusinessObjectForm);
				});
			}
		}

		public void TestSendMessages()
		{
			using (var mainForm = new ZForm())
			{
				var header = CreateHeader();

				AssertEquals("No messages", 0, header.Messages.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Save?
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Validation errors
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				CustomsMessagingGui.SendMessages(header, "ORG", null, mainForm);

				AssertEquals("Msg created", 1, header.Messages.Count);
			}
		}

		AsycudaManifestHeader CreateHeader()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ZA";
			header.AMA_ManifestType = "ALH";
			header.AMA_ManifestNumber = "MAN12345";
			header.AMA_RL_NKPortOfLoading = "GBLON";
			header.AMA_RL_NKPortOfDischarge = "ZADUR";

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "B0001";
			bill1.ABL_BillIssuer = "Billy";

			return header;
		}
	}
}
