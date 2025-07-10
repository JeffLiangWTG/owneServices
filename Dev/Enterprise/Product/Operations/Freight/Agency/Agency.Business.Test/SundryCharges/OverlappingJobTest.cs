using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class OverlappingJobTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			GlbCompany company1 = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbCompany company2 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			OrgHeader party1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader party2 = Factory.NewWithValidTestData<OrgHeader>();
			SundryCharges sundry1 = NewSundryCharge(party1, "sundry1", StartOfMonth(1), EndOfMonth(1));
			SundryCharges sundry2 = NewSundryCharge(party2, "sundry2", StartOfMonth(1), EndOfMonth(1));
			SundryCharges sundry3 = NewSundryCharge(party1, "sundry3", StartOfMonth(2), EndOfMonth(2));
			Factory.Save();
			AssertOverlappingJob("Nothing should be overlapping sundry1", sundry1, Array.Empty<string>());
			AssertOverlappingJob("Nothing should be overlapping sundry2", sundry2, Array.Empty<string>());
			AssertOverlappingJob("Nothing should be overlapping sundry3", sundry3, Array.Empty<string>());
			AssertOverlappingJob("no party", NewSundryCharge(null, "", StartOfMonth(1), EndOfMonth(1)), null);
			AssertOverlappingJob("no from date", NewSundryCharge(party1, "", ZDateTime.Empty, EndOfMonth(1)), null);
			AssertOverlappingJob("no to date", NewSundryCharge(party1, "", StartOfMonth(1), ZDateTime.Empty), null);
			const string sundry1Text = "sundry1 : [(2009-01-01) - (2009-01-31)]";
			const string sundry2Text = "sundry2 : [(2009-01-01) - (2009-01-31)]";
			const string sundry3Text = "sundry3 : [(2009-02-01) - (2009-02-28)]";
			AssertOverlappingJob("overlapping sundry1", NewSundryCharge(party1, "", StartOfMonth(1), EndOfMonth(1)), new string[] { sundry1Text });
			AssertOverlappingJob("overlapping sundries 1 & 3", NewSundryCharge(party1, "", StartOfMonth(1), EndOfMonth(2)), new string[] { sundry1Text, sundry3Text });
			AssertOverlappingJob("overlapping sundry3", NewSundryCharge(party1, "", StartOfMonth(2), EndOfMonth(2)), new string[] { sundry3Text });
			AssertOverlappingJob("overlapping sundry2", NewSundryCharge(party2, "", StartOfMonth(1), EndOfMonth(2)), new string[] { sundry2Text });
		}

		#region Implementation
		ZDateTime StartOfMonth(int month)
		{
			return new ZDateTime(2009, month, 1);
		}

		ZDateTime EndOfMonth(int month)
		{
			return new ZDateTime(2009, month, 1).AddMonths(1).AddDays(-1);
		}

		SundryCharges NewSundryCharge(OrgHeader billToParty, ZString jobNumber, ZDateTime fromDate, ZDateTime toDate)
		{
			SundryCharges result = Factory.New<SundryCharges>();
			result.D4_JobNumber = jobNumber;
			result.D4_OH_BillToParty = billToParty == null ? ZGuid.Empty : billToParty.PK;
			result.D4_FromDate = fromDate;
			result.D4_ToDate = toDate;
			return result;
		}

		void AssertOverlappingJob(string message, SundryCharges sundry, string[] expected)
		{
			OverlappingJob[] loadedJobs = OverlappingJob.Load(sundry);
			if (expected == null)
			{
				AssertEquals(null, loadedJobs);
			}
			else
			{
				AssertNotNull(message, loadedJobs);
				AssertContainsExactElementsInAnyOrder(message, expected, Array.ConvertAll(loadedJobs, Format));
			}
		}

		string Format(OverlappingJob overlap)
		{
			return string.Format("{0} : [({1:yyyy-MM-dd}) - ({2:yyyy-MM-dd})]", overlap.JobNumber, overlap.FromDate, overlap.ToDate);
		}
		#endregion
	}
}
