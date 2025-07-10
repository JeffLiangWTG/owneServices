using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ExporterSchemePluginUK))]
	sealed class ExporterSchemePluginUKTest : ExporterSchemePluginTest
	{
		[RequiresSTA]
		public void TestCaption()
		{
			using (var form = (DummyForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Supply Chain Security (UK)", form.TabControl.TabPages[0].Text);
			}
		}

		#region Implementation

		protected override ExporterSchemePlugin GetNewPluginToTest(OrgHeader hostEntity)
		{
			return new ExporterSchemePluginUK(hostEntity);
		}

		protected override ZString CountryForTest => Core.Constants.CountryCodes.UnitedKingdom;

		#endregion
	}
}
