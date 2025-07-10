using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class JobVoyAccountValidationTest : BusinessObjectValidationTestCase
	{
		public void ValidateIsDistinct()
		{
			const string error = "This combination of schedule + principal already exists in the database.";
			OrgHeader principal1 = Factory.New<OrgHeader>();
			OrgHeader principal2 = Factory.New<OrgHeader>();
			JobVoyage voyage1 = Factory.New<JobVoyage>();
			JobVoyage voyage2 = Factory.New<JobVoyage>();
			Account1.NA_JV = voyage1.PK;
			Account1.NA_OH = principal1.PK;
			Account2.NA_JV = voyage2.PK;
			Account2.NA_OH = principal1.PK;
			AssertHasError(Account2.NA_OHInfo, error);
			AssertHasError(Account2.NA_Calc_VesselInfo, error);
			AssertHasError(Account2.NA_Calc_VoyageInfo, error);
			AssertNoNotifications(Account2.NA_JVInfo);
			Account2.NA_OH = principal2.PK;
			AssertNoNotifications(Account2.NA_OHInfo);
			AssertNoNotifications(Account2.NA_Calc_VesselInfo);
			AssertNoNotifications(Account2.NA_Calc_VoyageInfo);
			AssertNoNotifications(Account2.NA_JVInfo);
			Account2.NA_JV = voyage1.PK;
			AssertHasError(Account2.NA_OHInfo, error);
			AssertHasError(Account2.NA_Calc_VesselInfo, error);
			AssertHasError(Account2.NA_Calc_VoyageInfo, error);
			AssertNoNotifications(Account2.NA_JVInfo);
		}

		public void ValidateNA_JV()
		{
			const string error = "Select a valid sailing schedule.";
			JobVoyage voyage = Factory.New<JobVoyage>();
			Account1.NA_JV = ZGuid.Invalid;
			AssertNoNotifications(Account1.NA_JVInfo);
			AssertHasError(Account1.NA_Calc_VesselInfo, error);
			AssertHasError(Account1.NA_Calc_VoyageInfo, error);
			Account1.NA_JV = voyage.PK;
			AssertNoNotifications(Account1.NA_JVInfo);
			AssertNoNotifications(Account1.NA_Calc_VesselInfo);
			AssertNoNotifications(Account1.NA_Calc_VoyageInfo);
			Account1.NA_JV = ZGuid.Empty;
			AssertNoNotifications(Account1.NA_JVInfo);
			AssertHasError(Account1.NA_Calc_VesselInfo, error);
			AssertHasError(Account1.NA_Calc_VoyageInfo, error);
		}

		#region Implementation
		VoyageAccount Account1
		{
			get
			{
				return account1 ?? (account1 = Factory.New<VoyageAccount>());
			}
		}

		VoyageAccount account1;
		VoyageAccount Account2
		{
			get
			{
				return account2 ?? (account2 = Factory.New<VoyageAccount>());
			}
		}

		VoyageAccount account2;
		#endregion
	}
}
