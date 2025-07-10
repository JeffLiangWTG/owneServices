using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ExporterSchemePluginUS))]
	sealed class ExporterSchemePluginUSTest : ExporterSchemePluginTest
	{
		[RequiresSTA]
		public void TestCaption()
		{
			using (DummyForm form = (DummyForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("TSA Known Shipper", form.TabControl.TabPages[0].Text);
			}
		}

		#region Implementation

		protected override ExporterSchemePlugin GetNewPluginToTest(OrgHeader hostEntity)
		{
			return new ExporterSchemePluginUS(hostEntity);
		}

		protected override ZString CountryForTest
		{
			get { return Core.Constants.CountryCodes.UnitedStates; }
		}

		#endregion
	}
}
