using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccSurchargeConfigurationCollectionForm))]
	public class AccSurchargeConfigurationCollectionFormTest : ZFormBasherTest
	{
		public void TestColumnReadOnly()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();

			var accSurchargeConfiguration1 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration1.ASC_GC_Company = company.PK;
			accSurchargeConfiguration1.ASC_Code = "T01";

			var accSurchargeConfiguration2 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration2.ASC_GC_Company = company.PK;
			accSurchargeConfiguration2.ASC_Code = "T02";

			var accSurchargeConfiguration3 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			accSurchargeConfiguration3.ASC_GC_Company = company.PK;
			accSurchargeConfiguration3.ASC_Code = "T03";

			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration1);
			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration2);
			company.AccSurchargeConfigurations.Add(accSurchargeConfiguration3);
			Factory.Save();

			var surchargeCodes = new AccSurchargeConfigurationCollection(Factory, company.PK, true);
			surchargeCodes.Load();
			using (AccSurchargeConfigurationCollectionForm form = new AccSurchargeConfigurationCollectionForm(surchargeCodes))
			{
				var grid = form.GetControl<ZGrid>("AccSurchargeConfigurationGrid");
				AssertNotNull(grid);

				var column1 = grid.GetColumnStyle("ASC_Code");
				AssertNotNull(column1);
				Assert(column1.IsReadOnly);

				var column2 = grid.GetColumnStyle("ASC_Description");
				AssertNotNull(column2);
				Assert(column2.IsReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var surchargeCodes = new AccSurchargeConfigurationCollection(Factory, GlbCompany.CurrentCompany.PK, true);
			surchargeCodes.Load();
			return new AccSurchargeConfigurationCollectionForm(surchargeCodes);
		}
	}
}
