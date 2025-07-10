using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test.Triggers.LineTriggers.IntegrationTests
{
	class PkgPackageLineTriggerTest : LineTriggerTestCase
	{
		#region TestPackageLineTriggerFire_NTF

		public void TestPackageLineTriggerFire_NTF()
		{
			AssertPackageLineTriggerFire((p) => SetupEmailNotification(p),
				(p) =>
				{
					RunLogWalker();
					RunLogWalker();
					AssertEquals("There should be one email sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				});
		}

		#endregion

		#region TestLinesPkgPackageLineTriggerFire_FLD

		public void TestLinesPkgPackageLineTriggerFire_FLD()
		{
			AssertPackageLineTriggerFire((p) => SetupFieldNotification(p), package =>
			{
				RunLogWalker();
				package.Reload();
				AssertEquals("TES", package.KP_GoodsDescription);
			});
		}

		protected override string FieldNameToUpdate => PkgPackageSchema.Constants.KP_GoodsDescription;

		#endregion

		void AssertPackageLineTriggerFire(Action<ProcessTaskNotification> setupNotifications, Action<PkgPackage> assertResults)
		{
			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1");
			warehouse[WhsWarehouseSchema.Constants.WW_WarehouseType] = "TRW";
			var recieveTransportationUnit = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			recieveTransportationUnit.WRH_WW_Warehouse = warehouse.PK;

			var consignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			consignment.BookingPartyDocAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			consignment.WRC_WW_IntendedWarehouse = warehouse.PK;
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(consignment);
			var package = packageJob.Packages.AddNew("PLT");
			var packageState = consignment.PackageStates.AddNew();
			packageState.WPS_WRH_TransitReceiveHeader = recieveTransportationUnit.PK;
			packageState.WPS_Status = "ARV";
			packageState.WPS_KP_Package = package.PK;
			packageState.WPS_WW_Warehouse = warehouse.PK;
			packageState.WPS_WL_LastLocation = recieveTransportationUnit.WRH_WL_StagingLocation;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			consignment.Logs.AddNew(new EventValue(AutoEvents.EditedARecord, eventTime: ZDateTimeOffset.Now.AddDays(1), deferFiringWorkflow: true));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();
			package.Logs.AddNew(new EventValue(AutoEvents.Arrival, eventTime: ZDateTimeOffset.Now.AddDays(1), deferFiringWorkflow: true));
			Factory.Save();

			var templateTask = CreateTriggerInNewFactory(WorkflowDescriptors.TransitReceiveConsignment, TriggerLineTypes.Codes.PkgPackage, AutoEvents.ArrivalCode);
			setupNotifications(templateTask.ProcessTaskNotifications.AddNew());
			templateTask.Factory.Save();

			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, consignment.PK);
			AssertEquals(0, Factory.Load<ProcessTask>(query).Length);

			RunLogWalker();
			RunLogWalker();
			assertResults(package);
			consignment.Reload();
			AssertEquals(1, consignment.WorkflowItems.Triggers.Count);
		}
	}
}
