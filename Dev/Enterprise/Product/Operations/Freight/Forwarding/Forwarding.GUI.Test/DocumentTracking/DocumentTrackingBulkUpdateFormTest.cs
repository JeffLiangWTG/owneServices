using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI
{
	[TestedType(typeof(DocumentTrackingBulkUpdateForm))]
	public class DocumentTrackingBulkUpdateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DocumentTrackingBulkUpdateBusinessObject bizO = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			return new DocumentTrackingBulkUpdateForm(bizO);
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestAttachButton_Click()
		{
			DocumentTrackingBulkUpdateBusinessObject bO = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			using (TestDocumentTrackingBulkUpdateForm form = new TestDocumentTrackingBulkUpdateForm(bO))
			{
				form.Show();
				Application.DoEvents();
				try
				{
					form.AttachButton_Click(null, null);
					Application.DoEvents();
				}
				finally
				{
					if (form.LastShownAttachPopup != null)
					{
						form.LastShownAttachPopup.Dispose();
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDetachButton_Click()
		{
			DocumentTrackingBulkUpdateBusinessObject bO = Factory.New<DocumentTrackingBulkUpdateBusinessObject>();
			bO.SelectedDocuments.AddNew();
			using (TestDocumentTrackingBulkUpdateForm form = new TestDocumentTrackingBulkUpdateForm(bO))
			{
				form.Show();
				Application.DoEvents();
				form.DetachButton_Click(null, null);
				Application.DoEvents();
			}
		}

		public class TestDocumentTrackingBulkUpdateForm : DocumentTrackingBulkUpdateForm
		{
			public TestDocumentTrackingBulkUpdateForm(DocumentTrackingBulkUpdateBusinessObject bizO) : base(bizO)
			{
			}

			public new void AttachButton_Click(object sender, System.EventArgs e)
			{
				base.AttachButton_Click(sender, e);
			}

			public new void DetachButton_Click(object sender, System.EventArgs e)
			{
				base.DetachButton_Click(sender, e);
			}

			public new EmbeddedModulePopup LastShownAttachPopup
			{
				get { return base.LastShownAttachPopup; }
			}
		}
	}
}
