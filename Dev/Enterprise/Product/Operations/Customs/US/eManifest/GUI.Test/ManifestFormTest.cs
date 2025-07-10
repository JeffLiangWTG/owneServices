using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class ManifestFormTest : TestCaseWithFactory
	{
		public void TestManifestMenu()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var trip1 = Factory.New<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_VoyageNumber = "VOYG1";
			var shipment1 = trip1.Shipments.AddNew();
			shipment1.B0_ReferenceID = "Shipment1";
			var shipment2 = trip1.Shipments.AddNew();
			shipment2.B0_ReferenceID = "Shipment2";
			trip1.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.eManifest;
			using (var form = new ManifestForm(trip1))
			{
				var actionMenu = form.Menu.MenuItems.FindByText("Actions");
				AssertNotNull(actionMenu.MenuItems.FindByText("Create A New Declaration"));
			}
		}

		public void TestValidationViaFormSave_ValidateChangedShipmentsOnly()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = GlbBranch.CurrentBranch.PK;
			var shipmentHasChanges = trip.Shipments.AddNew();
			var shipmentHasNoChanges = trip.Shipments.AddNew();

			shipmentHasChanges.B0_Volume = -5;
			shipmentHasNoChanges.B0_Volume = -5;

			CombineAssertions("Precondition: there should be invalid data on the shipment", () =>
			{
				AssertHasMessageError(shipmentHasChanges.B0_VolumeInfo, "Volume cannot be negative.");
				AssertHasMessageError(shipmentHasNoChanges.B0_VolumeInfo, "Volume cannot be negative.");
			});

			using (shipmentHasChanges.SuspendValidationTesting())
			using (shipmentHasNoChanges.SuspendValidationTesting())
			{
				shipmentHasChanges.ClearAllNotifications();
				shipmentHasNoChanges.ClearAllNotifications();
				shipmentHasNoChanges.ClearHasChanges();

				using (var form = new ManifestForm(trip))
				{
					form.Show();
					form.FireSaveButton();
					CombineAssertions(() =>
					{
						Assert("Existing behaviour: Notifications should be back because it has changes", shipmentHasChanges.HasNotifications());
						AssertHasMessageError("Existing behaviour: Shipment should be validated and message error should be revealed", shipmentHasChanges.B0_VolumeInfo, "Volume cannot be negative.");

						Assert("Should be no notifications because it does not have any changes, so validation is skipped", !shipmentHasNoChanges.HasNotifications());
					});
				}
			}
		}

		public void TestSkipFetchHint_WhenSavingManifestForm()
		{
			var trip1 = Factory.New<Trip>();
			trip1.BH_VoyageNumber = "VOYG1";
			trip1.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.eManifest;

			Factory.AddFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Number, 123));
			using (var form = new ManifestForm(trip1))
			{
				var expectedDBHits = new Dictionary<string, int>()
				{
					{ DummyBizoSchema.Constants.TableName, 0 }
				};
				Factory.ResetDatabaseLoadCount();
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, Factory, true))
				{
					form.FireSaveButton();
				}
			}
		}
	}
}
