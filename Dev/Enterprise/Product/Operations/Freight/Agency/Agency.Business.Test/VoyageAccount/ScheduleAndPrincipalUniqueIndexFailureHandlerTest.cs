using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ScheduleAndPrincipalUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			var principal = Factory.New<OrgHeader>();
			principal.OH_Code = "PRINCIPAL";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYAGE";
			Factory.Save();
			ReleaseFactory();
			Factory.RefreshEnabled = false;
			var voyageAccount1 = Factory.New<VoyageAccount>();
			voyageAccount1.NA_JV = voyage.PK;
			voyageAccount1.NA_OH = principal.PK;
			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var voyageAccount2 = secondFactory.New<VoyageAccount>();
			voyageAccount2.NA_JV = voyage.PK;
			voyageAccount2.NA_OH = principal.PK;
			secondFactory.Save();
			try
			{
				Factory.Save();
				Fail("should throw save exception as it violates chedule + principal unique index");
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}

			const string errorMessage = "This combination of schedule + principal already exists in the database.";
			AssertMultilineASCIIEquals("user notification", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertHasError(voyageAccount1.NA_Calc_VoyageInfo, errorMessage);
			AssertHasError(voyageAccount1.NA_Calc_VesselInfo, errorMessage);
			AssertHasError(voyageAccount1.NA_OHInfo, errorMessage);
		}
	}
}
