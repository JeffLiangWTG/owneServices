using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(PromtCancellationForm))]
	sealed class PromtCancellationFormTest : ZFormBasherTest
	{
		public void TestSupportingDocumentsTabPageBound()
		{
			var additionalInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory);
			using (PromtCancellationForm form = new PromtCancellationForm(additionalInformation))
			{
				form.SupportingDocumentsTabPage.Select();
				SupportingDocument doc = additionalInformation.SupportingDocuments.AddNew();
				doc.AddRowError("Documentation Error");
				form.Show();
				AssertEquals("Tab page should show for Cancellation entries", true, form.SupportingDocumentsTabPage.TabVisible);
			}
		}

		public void TestGridId()
		{
			using (var form = new PromtCancellationForm())
			{
				var grid = (ZGrid)form.Controls.Find("SupportingDocumentsGrid", true).Single();
				AssertEquals("b522c5b0-62cf-4d63-bafd-fe451fca4952", grid.GridId);
			}
		}

		protected override Form GetFormToBashCore() => new PromtCancellationForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Cancellation, Factory));
	}
}
