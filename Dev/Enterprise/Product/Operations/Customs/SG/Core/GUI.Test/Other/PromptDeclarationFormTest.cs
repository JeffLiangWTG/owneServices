using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(PromptDeclarationForm))]
	sealed class PromptDeclarationFormTest : ZFormBasherTest
	{
		public void TestSupportingDocumentsTabPageBound()
		{
			var additionalInformation = new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory);
			using (var form = new PromptDeclarationForm(additionalInformation))
			{
				var doc = additionalInformation.SupportingDocuments.AddNew();
				doc.AddRowError("Documentation Error");
				form.Show();
				AssertEquals("Tab page should be validated with an error icon", Icons.GetImageIndex(IconTypes.Error), form.SupportingDocumentsTabPage.ImageIndex);
			}
		}

		public void TestGridId()
		{
			using (var form = new PromptDeclarationForm())
			{
				var grid = (ZGrid)form.Controls.Find("SupportingDocumentsGrid", true).Single();
				AssertEquals("b522c5b0-62cf-4d63-bafd-fe451fca4952", grid.GridId);
			}
		}

		protected override Form GetFormToBashCore() => new PromptDeclarationForm(new AdditionalMessageInformation(AdditionalMessageInformation.BoundFormTypes.Declaration, Factory));
	}
}
