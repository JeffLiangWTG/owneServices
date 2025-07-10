using System.Windows.Forms;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusRefTariffVersionForm))]
	sealed class CusRefTariffVersionFormTest : ZFormBasherTest
	{
		public void TestControls()
		{
			using (var form = new CusRefTariffVersionForm(Factory.New<CusRefTariffVersion>()))
			{
				form.Show();
				AssertNotNull("CountryCodeFindBox", form.FindSingle<ZCodeFindBox>("CountryCodeFindBox"));
				AssertNotNull("TariffVersionCodeTextBox", form.FindSingle<ZTextBox>("TariffVersionCodeTextBox"));
				AssertNotNull("TariffVersionDescriptionTextBox", form.FindSingle<ZTextBox>("TariffVersionDescriptionTextBox"));
				AssertNotNull("TariffVersionEffectiveDateEdit", form.FindSingle<ZDateEdit>("TariffVersionEffectiveDateEdit"));
			}
		}

		protected override Form GetFormToBashCore() => new CusRefTariffVersionForm(Factory.New<CusRefTariffVersion>());
	}
}
