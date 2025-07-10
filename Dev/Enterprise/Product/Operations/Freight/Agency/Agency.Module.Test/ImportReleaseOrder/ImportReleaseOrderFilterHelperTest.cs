using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Module.ImportReleaseOrder;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal class ImportReleaseOrderFilterHelperTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			var containers = new BillOfLadingContainer[10];
			containers[0] = AddContainer("Container 0");
			containers[1] = AddContainer("Container 1", new EventInfo(Events.MessageSent, Constants.EventReferenceMessageTypes.ImportReleaseOrder), new EventInfo(Events.InterchangeRejected, Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal), new EventInfo(Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.ImportReleaseOrder));
			containers[2] = AddContainer("Container 2", new EventInfo(Events.InterchangeRejected, Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal), new EventInfo(Events.MessageSent, Constants.EventReferenceMessageTypes.ImportReleaseOrder), new EventInfo(Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.ImportReleaseOrder));
			containers[3] = AddContainer("Container 3", new EventInfo(Events.MessageWithdrawCancelRequest, Constants.EventReferenceMessageTypes.ImportReleaseOrder), new EventInfo(Events.MessageSent, Constants.EventReferenceMessageTypes.ImportReleaseOrder), new EventInfo(Events.InterchangeRejected, Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal));
			containers[4] = AddContainer("Container 4", new EventInfo(Events.MessageSent, "Unknown type"), new EventInfo(Events.InterchangeRejected, "Unknown type"), new EventInfo(Events.MessageWithdrawCancelRequest, "Unknown type"));
			containers[5] = AddContainer("Container 5", new EventInfo(Events.InterchangeRejected, "Unknown type"), new EventInfo(Events.MessageSent, "Unknown type"), new EventInfo(Events.MessageWithdrawCancelRequest, "Unknown type"));
			containers[6] = AddContainer("Container 6", new EventInfo(Events.MessageWithdrawCancelRequest, "Unknown type"), new EventInfo(Events.MessageSent, "Unknown type"), new EventInfo(Events.InterchangeRejected, "Unknown type"));
			Factory.Save();
			Asserter.AddFieldOfInterest(AgencyShipmentContainer.Schema.JC_ImportReleaseOrderStatus);
			Asserter.AssertMatches("NotSent containers", ImportReleaseOrderFilterHelper.GetFilterQuery(ImportReleaseOrderFilterList.Codes.NotSent), containers[0], containers[3], containers[4], containers[5], containers[6]);
			Asserter.AssertMatches("Sent containers", ImportReleaseOrderFilterHelper.GetFilterQuery(ImportReleaseOrderFilterList.Codes.Sent), containers[1]);
			Asserter.AssertMatches("Rejected containers", ImportReleaseOrderFilterHelper.GetFilterQuery(ImportReleaseOrderFilterList.Codes.InterchangeRejected), containers[2]);
		}

		#region Implementation
		BillOfLadingContainer AddContainer(string containerNumber, params EventInfo[] events)
		{
			var container = Shipment.RealContainers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			container.JC_ContainerNum = containerNumber;
			Asserter.AddToScope(container);
			foreach (var eventInfo in events.Reverse())
			{
				System.Threading.Thread.Sleep(100);
				container.Logs.AddNew(eventInfo.Event, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType.AsKeyFor(eventInfo.MessageType));
				Factory.Save();
			}

			return container;
		}

		BillOfLading Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<BillOfLading>();
					shipment.JS_UniqueConsignRef = "McLaren";
				}

				return shipment;
			}
		}

		FilterStripAsserter<BillOfLadingContainer> Asserter
		{
			get
			{
				if (asserter == null)
				{
					asserter = new FilterStripAsserter<BillOfLadingContainer>(Factory, (c) => c.JC_ContainerNum);
				}

				return asserter;
			}
		}

		BillOfLading shipment;
		FilterStripAsserter<BillOfLadingContainer> asserter;
		#endregion
		#region Types
		struct EventInfo
		{
			public Event Event;
			public string MessageType;
			public EventInfo(Event evnt, string messageType)
			{
				Event = evnt;
				MessageType = messageType;
			}
		}
		#endregion
	}
}
