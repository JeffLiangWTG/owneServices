using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class JobSundryChargesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestD4_OH_BillToParty()
		{
			Org.Factory.Save();
			Sundry.D4_OH_BillToParty = ZGuid.Invalid;
			AssertHasError(Sundry.D4_OH_BillToPartyInfo, "Enter a valid " + Sundry.D4_OH_BillToPartyInfo.Description + ".");
			Sundry.D4_OH_BillToParty = Org.PK;
			AssertNoNotifications(sundry.D4_OH_BillToPartyInfo);
			Sundry.D4_OH_BillToParty = ZGuid.Empty;
			AssertHasError(Sundry.D4_OH_BillToPartyInfo, "Please enter a " + Sundry.D4_OH_BillToPartyInfo.Description + ".");
		}

		public void TestD4_SundriesJobType()
		{
			Sundry.D4_SundriesJobType = "XXX";
			AssertHasError(Sundry.D4_SundriesJobTypeInfo, "Enter a valid " + Sundry.D4_SundriesJobTypeInfo.Description + ".");
			Sundry.D4_SundriesJobType = Sundry.Lookups.Types.DefaultCode;
			AssertNoNotifications(sundry.D4_SundriesJobTypeInfo);
			Sundry.D4_SundriesJobType = ZString.Empty;
			AssertHasError(Sundry.D4_SundriesJobTypeInfo, "Please enter a " + Sundry.D4_SundriesJobTypeInfo.Description + ".");
		}

		public void TestD4_SundryJobMode()
		{
			Sundry.D4_SundryJobMode = "XXX";
			AssertHasError(Sundry.D4_SundryJobModeInfo, "Enter a valid " + Sundry.D4_SundryJobModeInfo.Description + ".");
			Sundry.D4_SundryJobMode = Sundry.Lookups.Modes.DefaultCode;
			AssertNoNotifications(sundry.D4_SundryJobModeInfo);
			Sundry.D4_SundryJobMode = ZString.Empty;
			AssertHasError(Sundry.D4_SundryJobModeInfo, "Please enter a " + Sundry.D4_SundryJobModeInfo.Description + ".");
		}

		public void TestD4_SundryJobActivity()
		{
			Sundry.D4_SundryJobActivity = "XXX";
			AssertHasError(Sundry.D4_SundryJobActivityInfo, "Enter a valid " + Sundry.D4_SundryJobActivityInfo.Description + ".");
			Sundry.D4_SundryJobActivity = Sundry.Lookups.Activities.DefaultCode;
			AssertNoNotifications(sundry.D4_SundryJobActivityInfo);
			Sundry.D4_SundryJobActivity = ZString.Empty;
			AssertHasError(Sundry.D4_SundryJobActivityInfo, "Please enter an " + Sundry.D4_SundryJobActivityInfo.Description + ".");
		}

		public void TestD4_FromDate()
		{
			ZDateTime today = ZDateTime.Today;
			Sundry.D4_ToDate = today;
			Sundry.D4_FromDate = today.AddDays(1);
			AssertHasError(Sundry.D4_FromDateInfo, "From Date cannot be after To Date.");
			Sundry.D4_FromDate = today;
			AssertNoNotifications(Sundry.D4_FromDateInfo);
			Sundry.D4_FromDate = ZDateTime.Empty;
			AssertHasError(Sundry.D4_FromDateInfo, "Please enter a " + Sundry.D4_FromDateInfo.Description + ".");
		}

		[TestDate(2009, 12, 1)]
		public void TestD4_FromDate_Overlap()
		{
			ZDateTime q1s = new ZDateTime(2009, 01, 01);
			ZDateTime q1e = new ZDateTime(2009, 03, 31);
			SundryCharges other = Factory.New<SundryCharges>();
			other.D4_JobNumber = "OTHER";
			other.D4_OH_BillToParty = Org.PK;
			other.D4_FromDate = q1s;
			other.D4_ToDate = q1e;
			Factory.Save();
			const string error = "This job overlaps OTHER (01-Jan-09 to 31-Mar-09) for the same bill to party.";
			Sundry.D4_OH_BillToParty = Org.PK;
			Sundry.D4_FromDate = q1s.AddDays(-2);
			Sundry.D4_ToDate = q1s.AddDays(-1);
			Sundry.Validation.ValidateD4_FromDate();
			AssertNoNotifications(Sundry.D4_FromDateInfo);
			Sundry.D4_ToDate = q1e.AddDays(2);
			Sundry.Validation.ValidateD4_FromDate();
			AssertHasError(Sundry.D4_FromDateInfo, error);
			Sundry.D4_FromDate = q1e.AddDays(1);
			AssertNoNotifications(Sundry.D4_FromDateInfo);
		}

		public void TestD4_ToDate()
		{
			ZDateTime today = ZDateTime.Today;
			Sundry.D4_FromDate = today;
			Sundry.D4_ToDate = today.AddDays(-1);
			AssertHasError(Sundry.D4_ToDateInfo, "To Date cannot be before From Date.");
			Sundry.D4_ToDate = today;
			AssertNoNotifications(Sundry.D4_ToDateInfo);
			Sundry.D4_ToDate = ZDateTime.Empty;
			AssertHasError(Sundry.D4_ToDateInfo, "Please enter a " + Sundry.D4_ToDateInfo.Description + ".");
		}

		[TestDate(2009, 12, 30)]
		public void TestD4_ToDate_Overlap()
		{
			ZDateTime q1s = new ZDateTime(2009, 01, 01);
			ZDateTime q1e = new ZDateTime(2009, 03, 31);
			SundryCharges other = Factory.New<SundryCharges>();
			other.D4_JobNumber = "OTHER";
			other.D4_OH_BillToParty = Org.PK;
			other.D4_FromDate = q1s;
			other.D4_ToDate = q1e;
			Factory.Save();
			const string error = "This job overlaps OTHER (01-Jan-09 to 31-Mar-09) for the same bill to party.";
			Sundry.D4_OH_BillToParty = Org.PK;
			Sundry.D4_FromDate = q1s.AddDays(-2);
			Sundry.D4_ToDate = q1s.AddDays(-1);
			AssertNoNotifications(Sundry.D4_ToDateInfo);
			Sundry.D4_ToDate = q1e.AddDays(2);
			AssertHasError(Sundry.D4_FromDateInfo, error);
			Sundry.D4_FromDate = q1e.AddDays(1);
			Sundry.Validation.ValidateD4_ToDate();
			AssertNoNotifications(Sundry.D4_ToDateInfo);
		}

		#region Implementation
		SundryCharges Sundry
		{
			get
			{
				if (sundry == null)
				{
					sundry = Factory.New<SundryCharges>();
				}

				return sundry;
			}
		}

		SundryCharges sundry;
		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_IsDebtor = true;
					org.CompanyData.OB_IsDebtor = true;
				}

				return org;
			}
		}

		OrgHeader org;
		#endregion
	}
}
