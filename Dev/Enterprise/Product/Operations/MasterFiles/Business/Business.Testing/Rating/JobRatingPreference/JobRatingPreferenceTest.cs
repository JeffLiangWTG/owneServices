using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(JobRatingPreference))]
	public class JobRatingPreferenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateAndLoad()
		{
			var testDate = ZDateTime.Today;
			var testCompany = Env.CurrentCompanyPK;

			var bizO1 = Factory.New<BusinessObjectForTest>();
			var preference1 = JobRatingPreference.Create(bizO1);

			var bizO2 = Factory.New<BusinessObjectForTest>();
			var preference2 = JobRatingPreference.Create(bizO2, testCompany, testDate);

			CombineAssertions("Preference 1 should be created without company", () =>
			{
				AssertEquals("Company", ZGuid.Empty, preference1.JRP_GC_Company);
				AssertEquals("ParentID", bizO1.PK, preference1.JRP_ParentID);
				AssertEquals("ParentTableCode", "TST", preference1.JRP_ParentTableCode);
				AssertEquals("AutoratingDate", ZDateTime.Empty, preference1.JRP_AutoratingDate);
			});

			CombineAssertions("Preference 2 should be created with current company and date", () =>
			{
				AssertEquals("Company", testCompany, preference2.JRP_GC_Company);
				AssertEquals("ParentID", bizO2.PK, preference2.JRP_ParentID);
				AssertEquals("ParentTableCode", "TST", preference2.JRP_ParentTableCode);
				AssertEquals("AutoratingDate", ZDateTime.Empty, preference1.JRP_AutoratingDate);
			});

			var preference3 = JobRatingPreference.Load(bizO1);
			CombineAssertions("Preference without country should be loaded", () =>
			{
				AssertEquals("Company", ZGuid.Empty, preference3.JRP_GC_Company);
				AssertEquals("ParentID", bizO1.PK, preference3.JRP_ParentID);
				AssertEquals("ParentTableCode", "TST", preference3.JRP_ParentTableCode);
				AssertEquals("AutoratingDate", ZDateTime.Empty, preference3.JRP_AutoratingDate);
			});

			AssertNull("Preference should not be loaded", JobRatingPreference.Load(bizO1, testCompany));
			AssertNull("Preference should not be loaded", JobRatingPreference.Load(bizO2));

			var preference4 = JobRatingPreference.Load(bizO2, testCompany);
			CombineAssertions("Preference with country should be loaded", () =>
			{
				AssertEquals("Company", testCompany, preference4.JRP_GC_Company);
				AssertEquals("ParentID", bizO2.PK, preference4.JRP_ParentID);
				AssertEquals("ParentTableCode", "TST", preference4.JRP_ParentTableCode);
				AssertEquals("AutoratingDate", testDate, preference4.JRP_AutoratingDate);
			});
		}

		class BusinessObjectForTest(BusinessObjectFactory factory, DataRow row) : DummyBusinessObject(factory, row)
		{
			public override string TablePrefix => "TST";
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var forwardingConsol = Factory.New<IForwardingConsol>();

			var jobRatingPreference = Factory.New<JobRatingPreference>();

			jobRatingPreference.JRP_GC_Company = GlbCompany.CurrentCompany.PK;

			jobRatingPreference.JRP_ParentTableCode = JobConsolSchema.Constants.Prefix;
			jobRatingPreference.JRP_ParentID = forwardingConsol.PK;

			jobRatingPreference.JRP_AutoratingDate = ZDateTime.Today;
			jobRatingPreference.JRP_IsDateOverridden = false;
			jobRatingPreference.JRP_HasDateWarning = false;

			return jobRatingPreference;
		}

		#endregion
	}
}
