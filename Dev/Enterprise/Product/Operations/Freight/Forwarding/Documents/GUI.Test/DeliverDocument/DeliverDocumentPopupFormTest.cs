using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	[TestedType(typeof(DeliverDocumentPopupForm))]
	sealed class DeliverDocumentPopupFormTest : ZFormBasherTest
	{
		public void TestFormShown()
		{
			using (var form = new DeliverDocumentPopupFormForTesting())
			{
				form.Show();
				AssertEquals("Your CargoWise system is configured to send and receive electronic messages with this carrier.\r\nPress \"Send Message\" to send electronic message to the carrier.\r\nPress \"Deliver Document\" to proceed with manual delivery of Test", form.Label.Text);
				form.Close();
				form.Dispose();
			}
		}

		public void TestSendMessageButton()
		{
			using (var form = new DeliverDocumentPopupFormForTesting())
			{
				form.Show();
				Application.DoEvents();
				form.SendMessageButtonForTesting.PerformClick();
				AssertEquals(DeliverDocumentPopupAction.SendMessage, form.resultForTesting);
				form.Close();
				form.Dispose();
			}
		}

		public void TestDeliverDocumentButton()
		{
			using (var form = new DeliverDocumentPopupFormForTesting())
			{
				form.Show();
				Application.DoEvents();
				form.DeliverDocumentButtonForTesting.PerformClick();
				AssertEquals(DeliverDocumentPopupAction.DeliverDocument, form.resultForTesting);
				form.Close();
				form.Dispose();
			}
		}

		#region Implement

		protected override Form GetFormToBashCore() => new DeliverDocumentPopupForm("Test");

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		#endregion

		class DeliverDocumentPopupFormForTesting : DeliverDocumentPopupForm
		{
			public DeliverDocumentPopupFormForTesting()
				: base("Test")
			{
			}

			public Button SendMessageButtonForTesting => SendMessageButton;
			public Button DeliverDocumentButtonForTesting => DeliverDocumentButton;

			public Label Label => ShowLabel;
			public DeliverDocumentPopupAction resultForTesting => result;
		}
	}
}
