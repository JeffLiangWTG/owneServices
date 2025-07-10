using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ManifestTallyFilterControlTest : TestCase
	{
		public void TestHideCanadaSpecificNumbersColumns()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (ZForm form = new ZForm())
			using (ManifestTallyModule module = new ManifestTallyModule())
			{
				var filter = (ManifestTallyFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNull("The CCN columns should not be added", filter.FilteredGrid.GetColumnStyle("CanadaCCNNumber"));
				AssertNull("The PCN columns should not be added", filter.FilteredGrid.GetColumnStyle("CanadaPCNNumber"));
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);

			using (ZForm form = new ZForm())
			using (ManifestTallyModule module = new ManifestTallyModule())
			{
				var filter = (ManifestTallyFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull("The CCN columns should be added", filter.FilteredGrid.GetColumnStyle("CanadaCCNNumber"));
				AssertNotNull("The PCN columns should be added", filter.FilteredGrid.GetColumnStyle("CanadaPCNNumber"));
			}
		}
	}
}
