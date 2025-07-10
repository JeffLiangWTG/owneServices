using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolForTest : ForwardingConsol
	{
		public ForwardingConsolForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ForwardingConsolForTestDocumentSupporter DocumentSupporterForForwardingConsolTest
		{
			get { return new ForwardingConsolForTestDocumentSupporter(this); }
		}

		public new ZBool AnyGoodsTravelToOrThroughFHLCountry
		{
			get { return base.AnyGoodsTravelToOrThroughFHLCountry; }
		}

		public void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			DocumentSupporterForForwardingConsolTest.SetDocumentEventSource_DocumentPrintRequested(sender, e);
		}

		public List<ForwardingShipment> UnContainerisedHazShipments
		{
			get { return DocumentSupporterForForwardingConsolTest.GetUnContainerisedHazShipments(); }
		}

		public DeliveryAgentToSelectFromForPrintingCollection DeliveryAgentsToSelectFrom
		{
			get { return DocumentSupporterForForwardingConsolTest.GetDeliveryAgentsToSelectFrom(); }
		}
		public ContainerToSelectFromForPrintingCollection ContainersToSelectFrom
		{
			get { return DocumentSupporterForForwardingConsolTest.GetContainersToSelectFrom(); }
		}

		internal bool IsDPSFreightMovementRestrictedCore_Exposed() => IsDPSFreightMovementRestrictedCore();

		#region ForwardingConsolForTestDocumentSupporter

		public class ForwardingConsolForTestDocumentSupporter : ForwardingConsolDocumentSupporter
		{
			public ForwardingConsolForTestDocumentSupporter(ForwardingConsolForTest consol)
				: base(consol)
			{
			}

			public List<ForwardingShipment> GetUnContainerisedHazShipments()
			{
				return Consol.Shipments.Cast<ForwardingShipment>().Where(HasUncontainerisedHazPackLines).ToList();
			}

			public ContainerToSelectFromForPrintingCollection GetContainersToSelectFrom()
			{
				return base.ContainersToSelectFrom;
			}

			public DeliveryAgentToSelectFromForPrintingCollection GetDeliveryAgentsToSelectFrom()
			{
				return base.DeliveryAgentsToSelectFrom;
			}

			public void SetDocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				base.DocumentEventSource_DocumentPrintRequested(sender, e);
			}

			public void SetDocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				base.DocumentEventSource_DocumentPrinted(sender, e);
			}

			public CommonContainer CreateContainerDocumentSupportableForTest(CommonContainer container)
			{
				return base.CreateContainerDocumentSupportable(container);
			}
		}

		#endregion

		public override ZDateTime GetAWBIssueDate()
		{
			GetAWBIssueDateCalled = true;
			return base.GetAWBIssueDate();
		}
		public bool GetAWBIssueDateCalled;

		public override void OnSaving()
		{
			base.OnSaving();

			if (OnSavingAction != null)
			{
				OnSavingAction();
			}
		}

		public Action OnSavingAction { get; set; }
	}
}
