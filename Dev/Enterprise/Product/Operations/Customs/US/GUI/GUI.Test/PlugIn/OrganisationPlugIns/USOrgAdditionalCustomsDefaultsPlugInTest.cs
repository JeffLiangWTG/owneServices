using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class USOrgAdditionalCustomsDefaultsPlugInTest : ZPlugInGenericTest
	{
		public void TestHasUserControl()
		{
			using (var plugin = GetNewPlugIn())
			{
				AssertNotNull(plugin.UserControl);
			}
		}

		public new void TestBashUserControlOfPlugIn()
		{
			Assert(true);
		}

		protected override ZPlugIn GetPlugInToTest() => GetNewPlugIn();

		USOrgAdditionalCustomsDefaultsPlugIn GetNewPlugIn() => new USOrgAdditionalCustomsDefaultsPlugIn(Organization);

		OrgHeader organization;
		OrgHeader Organization
		{
			get
			{
				if (organization == null)
				{
					organization = Factory.NewWithValidTestData<OrgHeader>();
					var link = organization.SupplierLinks.AddNew();
					link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
				}

				return organization;
			}
		}
	}
}
