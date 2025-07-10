using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingWorkflowIntegrationTest : TestCaseWithFactory
	{
		public void TestTriggerOnTransportBookingToCreateCartageAdviceAttachesToTransportBooking()
		{
			var documentQuery = new ZDBOnlyQuery(typeof(StmMenuItem));
			documentQuery.AddToFilter(StmMenuItemSchema.SU_MenuType, Core.Constants.StmMenuItemTypes.WebReports);
			documentQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.DtbBooking);
			documentQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, "Cartage Advice");
			var document = Factory.LoadTop1<StmMenuItem>(documentQuery);

			var shipment = Helper.CreateForwardingShipment("S123", "", "SEA", "FCL");
			var consolidation = Helper.CreateConsolidation((IDtbBookingParent)shipment);
			var booking = Helper.CreateBooking(consolidation);

			var trigger = booking.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.ArrivalCode;
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			triggerAction.PQ_SU_Document = document.PK;

			var log = booking.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = AutoEvents.ArrivalCode;
			}

			Factory.Save();

			var stmPrintJobQuery = new ZDBOnlyQuery(typeof(StmPrintJob));

			var stmPrintJobs = Factory.Load<StmPrintJob>(stmPrintJobQuery);
			AssertEquals("Precondition: No StmPrintJob", 0, stmPrintJobs.Length);

			MasterFilesTestHelper.RunLogWalker();

			stmPrintJobs = Factory.Load<StmPrintJob>(stmPrintJobQuery);
			AssertEquals("Produces only one StmPrintJob", 1, stmPrintJobs.Length);

			var stmPrintJob = stmPrintJobs[0];
			CombineAssertions(() =>
			{
				AssertEquals("Attaches to a transport booking", DtbBookingSchema.Constants.TableName, stmPrintJob.SP_ParentTableName);
				AssertEquals("Attaches to the transport booking that had the trigger", booking.PK, stmPrintJob.SP_ParentGuid);
				AssertNotEquals("Does not attach to the shipment", shipment.PK, stmPrintJob.SP_ParentGuid);
				AssertNotEquals("Does not attach to a shipment", JobShipmentSchema.Constants.TableName, stmPrintJob.SP_ParentTableName);
				Assert("Print job document name contains document menu name", stmPrintJob.SP_DocumentName.StartsWith("Cartage Advice"));
			});
		}

		protected TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
