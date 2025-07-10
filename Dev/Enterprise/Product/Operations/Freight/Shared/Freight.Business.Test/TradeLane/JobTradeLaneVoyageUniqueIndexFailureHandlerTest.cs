using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobTradeLaneVoyageUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			((IBusinessObjectFactoryInternals)newFactory).DisableQueryCacheReset = true;

			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "Principal";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;

			JobVoyage voyage = Factory.New<JobVoyage>();
			Factory.Save();

			JobVoyage voyageFromNewFactory = newFactory.Load<JobVoyage>(voyage.PK);

			JobTradeLaneVoyage jobTradeLaneVoyage = voyage.TradeLanes.AddNew();
			jobTradeLaneVoyage.NB_OH = principal.PK;

			JobTradeLaneVoyage jobTradeLaneVoyageFromNewFactory = voyageFromNewFactory.TradeLanes.AddNew();
			jobTradeLaneVoyageFromNewFactory.NB_OH = principal.PK;

			Factory.Save();

			AssertEquals("precondition: Voyages should be the same", voyage.PK, voyageFromNewFactory.PK);
			Assert("precondtion:Voyages should be in separate factories", voyage.Factory != voyageFromNewFactory.Factory);
			AssertNotEquals("precondition:JobTradeLaneVoyages should be different", jobTradeLaneVoyage.PK, jobTradeLaneVoyageFromNewFactory.PK);

			((IBusinessObjectFactoryInternals)newFactory).DisableQueryCacheReset = false;

			try
			{
				newFactory.Save();
				Fail("First save should not have scceeded");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals("User should have been notified of the problem.", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Another user has added the principal Principal to this schedule, please review your changes and save again.\r\n", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("JobTradeLaneVoyage2 should be deleted", true, jobTradeLaneVoyageFromNewFactory.IsDeleted);

				newFactory.Save();
			}

			AssertEquals("voyageFromNewFactory should contain 1 tradelane", 1, voyageFromNewFactory.TradeLanes.Count);
			jobTradeLaneVoyageFromNewFactory = voyageFromNewFactory.TradeLanes[0];
			AssertEquals("Should use existing JobTradeLaneVoyage now", jobTradeLaneVoyage.PK, jobTradeLaneVoyageFromNewFactory.PK);
		}
	}
}
