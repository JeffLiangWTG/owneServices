using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TaxFrameworkAccTaxRateForm))]
	sealed class TaxFrameworkAccTaxRateFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new TaxFrameworkAccTaxRateForm(new TaxFrameworkAccTaxRateLoader());
		}

		[RequiresSTA]
		public void TestAllowNew()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				Assert("Not allowed New display mode", !((IPostingButtonsProvider)form).AllowNew);
			}
		}
	}
}
