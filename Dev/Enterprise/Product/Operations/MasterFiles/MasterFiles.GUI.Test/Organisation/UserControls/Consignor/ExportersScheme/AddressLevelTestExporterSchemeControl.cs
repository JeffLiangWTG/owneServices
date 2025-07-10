using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressLevelTestExporterSchemeControl : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestControlIsShown()
		{
			using (ZForm testForm = new ZForm())
			{
				using (var testControl = new AddressLevelExporterSchemeControl())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();

					var groupBox = testForm.Controls.Find("MajorExporterGroupBox", true).FirstOrDefault() as ZGroupBox;
					AssertNotNull(groupBox);
					AssertEquals("Aviation Security", groupBox.CaptionResourceString.Caption);
				}
			}
		}

		[RequiresSTA]
		public void TestIssuingAuthorityCountryColumn()
		{
			AssertIssuingAuthorityCountryColumnForCountry("GB", true);
			AssertIssuingAuthorityCountryColumnForCountry("AU", false);
		}

		void AssertIssuingAuthorityCountryColumnForCountry(string countryCode, bool expectedUseIssuingAuthorityCountry)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (ZForm testForm = new ZForm())
			{
				var supplyChainSecurityConfiguration = ObjectFactory.New<ISupplyChainSecurityConfigurationHelper>().GetConfiguration();
				AssertEquals(expectedUseIssuingAuthorityCountry, supplyChainSecurityConfiguration.UseIssuingAuthorityCountry);
				using (var testControl = new AddressLevelExporterSchemeControl(supplyChainSecurityConfiguration))
				{
					testForm.Controls.Add(testControl);
					testForm.Show();
					var countryDataGrid = (ZArchitecture.ZGrid)testControl.Controls.Find("CountryDataCollectionForThisCompanyGrid", true).First();
					var issuingAuthorityCountryColumn = countryDataGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == "OV_RN_NKIssuingAuthorityCountry");
					AssertEquals(!expectedUseIssuingAuthorityCountry, issuingAuthorityCountryColumn.IsUnavailable);
				}
			}
		}
	}
}
