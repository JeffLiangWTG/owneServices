using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccPeriodManagementCollection))]
	public class AccPeriodManagementCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccPeriodManagementCollection(Factory);
		}

		public void TestStaticFilterByCurrentCompany()
		{
			AccPeriodManagement periodForCurrentCompany = Factory.New<AccPeriodManagement>();
			AccPeriodManagement periodForNonCurrentCompany = Factory.New<AccPeriodManagement>();
			GlbCompany nonCurrentCompany = Factory.New<GlbCompany>();
			GlbCompany currentCompany = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);

			SetupCompany(nonCurrentCompany);
			SetupPeriod(periodForCurrentCompany, currentCompany);
			SetupPeriod(periodForNonCurrentCompany, nonCurrentCompany);

			AccPeriodManagementCollection periodCollection = (AccPeriodManagementCollection)GetCollectionToTest();
			periodCollection.Load();
			Assert("Charge Code for Non Current Company should not be in collection", !periodCollection.Contains(periodForNonCurrentCompany.PK));
		}

		#region Implementation

		void SetupCompany(GlbCompany company)
		{
			company.GC_StartDate = Env.Time.CurrentLocalDateTime;
			company.GC_RN_NKCountryCode = Env.CurrentCompany.Country.Code;
			company.GC_RX_NKLocalCurrency = Env.CurrentCompany.LocalCurrency.Code;
		}

		void SetupPeriod(AccPeriodManagement period, GlbCompany company)
		{
			period.AM_GC_Company = company.PK;
		}

		#endregion
	}
}
