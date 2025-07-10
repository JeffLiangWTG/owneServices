using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class ShipmentPhaseDependantsProvider : PhaseDependantsProvider
	{
		#region Parent Type

		protected override Type ParentType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(); }
		}

		#endregion

		protected override string ParentWorkflowType
		{
			get { return JobInvoicingConsumerTypes.Shipment.Code; }
		}

		#region ChildDependants

		public override IEnumerable<IPhaseDependant> GetChildDependants()
		{
			yield return new PropertyDependant("InnerPackLines", (NoResString)"InnerPackLine collection");       // Code identifier
			yield return new PropertyDependant("OuterPackLines", (NoResString)"OuterPackLine collection");       // Code identifier
			yield return new PropertyDependant("PickupConfirms", (NoResString)"Pickup Confirmations");           // Code identifier
			yield return new PropertyDependant("DeliveryConfirms", (NoResString)"Delivery Confirmations");       // Code identifier
		}

		public override Dictionary<IPhaseDependant, IEnumerable<IPhaseDependant>> GetChildExpandableDependants()
		{
			PropertyDependant docsAndCartage = new PropertyDependant("DocsAndCartage", "DocsAndCartage");
			var docsAndCartageProperties = GetIZTypePropertiesPrefixedWithName("DocsAndCartage", ObjectFactory.GetType<Integration.Forwarding.IForwardingDocsAndCartage>());

			var result = new Dictionary<IPhaseDependant, IEnumerable<IPhaseDependant>>();
			result.Add(docsAndCartage, docsAndCartageProperties);

			return result;
		}

		#endregion
	}
}
