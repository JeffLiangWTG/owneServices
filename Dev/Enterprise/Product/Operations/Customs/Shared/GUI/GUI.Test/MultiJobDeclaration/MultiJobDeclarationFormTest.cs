using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(MultiJobDeclarationForm))]
	sealed class MultiJobDeclarationFormTest : ZFormBasherTest
	{
		public void TestGridID()
		{
			using (var form = new MultiJobDeclarationForm(new MultiJobDeclarationHeader(Factory)))
			{
				form.Show();
				var grid = (ZGrid)form.Controls.Find("grid", true).First();
				AssertEquals("GridLayout0vOX7B27P3cmSNhVPzLE8w==", grid.GridId);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			formsAtStart = new List<Form>();
			foreach (Form form in Application.OpenForms)
			{
				formsAtStart.Add(form);
			}
		}

		List<Form> formsAtStart;

		protected override void TearDown()
		{
			var formsToCheck = new List<Form>();
			foreach (Form form in Application.OpenForms)
			{
				formsToCheck.Add(form);
			}

			foreach (var form in formsToCheck)
			{
				if (!formsAtStart.Contains(form))
				{
					form.Dispose();
				}
			}
			base.TearDown();
		}

		protected override Form GetFormToBashCore() => new MultiJobDeclarationForm(new MultiJobDeclarationHeader(Factory));
	}
}
