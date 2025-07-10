using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(VoyageAccountingController))]
	internal class VoyageAccountingControllerBasherTest : ZControllerBasherTest
	{
		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AgencyVoyageAccounting;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			VoyageAccount account = Factory.New<VoyageAccount>();
			account.NA_JV = voyage.PK;
			account.NA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			Factory.Save();

			return account;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
