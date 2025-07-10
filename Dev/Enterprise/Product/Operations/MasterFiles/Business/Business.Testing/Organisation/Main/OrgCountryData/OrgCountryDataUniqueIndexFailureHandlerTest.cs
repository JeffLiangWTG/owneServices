using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCountryDataUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		void MakeConflict(Action<OrgHeader> assertErrorReport)
		{
			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var parentInAnotherFactory = anotherFactory.Load<OrgHeader>(parent.PK);

			parent.CountryDataCollectionForThisCompany.RemoveAndDeleteAll();
			var countryData = parent.CountryDataCollectionForThisCompany.AddNew();

			parentInAnotherFactory.CountryDataCollectionForThisCompany.RemoveAndDeleteAll();
			var countryDataInAnotherFactory = parentInAnotherFactory.CountryDataCollectionForThisCompany.AddNew();
			Factory.Save();

			AssertEquals("precondition: Parents should be the same", parent.CountryData.OrgHeader.PK, parentInAnotherFactory.CountryData.OrgHeader.PK);
			Assert("precondition: countryData should be in separate factorys", parent.CountryData.Factory != parentInAnotherFactory.CountryData.Factory);
			Assert("precondition: Should not be the same countryData", parent.CountryData.PK != parentInAnotherFactory.CountryData.PK);

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			try
			{
				anotherFactory.Save();
				Fail("First save should not have succeeded.");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				assertErrorReport(parentInAnotherFactory);

				AssertEquals("countryData should be deleted", true, countryDataInAnotherFactory.IsDeleted);

				anotherFactory.Save();
			}
		}

		public void TestConflictResolution_WithErrorWhenUserIsNotInteractive()
		{
			Globals.IsUserInteractive = true;
			MakeConflict((o) =>
			{
				AssertEquals("User should have been notified of the problem.", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("User should have been notified of the problem.", string.Format("Another user has made changes to {0}, please review your changes and save again.", o.OH_Code), UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestConflictResolution_WithoutErrorWhenUserIsInteractive()
		{
			Globals.IsUserInteractive = false;
			MakeConflict((o) =>
				{
					AssertEquals("User should not see any error report.", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				});
		}

		[NUnit.Framework.ExpectNoExceptions]
		public void TestConflictResolution_WhenNotInDatabase_ShouldNotReload()
		{
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = Factory.New<OrgHeader>().PK;

			Assert(!countryData.OrgHeader.IsInDatabase);

			var handler = new OrgCountryDataUniqueIndexFailureHandler(countryData);
			handler.NotifyUserAndAttemptToResolve(new Mock<INotificationHandler>().Object, handler.HandledUniqueIndexNames.Single());
		}
	}
}
