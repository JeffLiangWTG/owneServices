using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Registry
{
	public class ConsolPhaseDependantsProvider : PhaseDependantsProvider
	{
		#region Parent Type

		protected override Type ParentType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingConsol>(); }
		}

		#endregion

		protected override string ParentWorkflowType
		{
			get { return JobInvoicingConsumerTypes.Consol.Code; }
		}

		#region ChildDependants

		public override IEnumerable<IPhaseDependant> GetChildDependants()
		{
			yield return new PropertyDependant((NoResString)"Containers", (NoResString)"Container collection");       // Code identifier
			yield return new PropertyDependant((NoResString)"Shipments", (NoResString)"Shipment collection");         // Code identifier
			yield return new PropertyDependant((NoResString)"Routing", (NoResString)"Routing collection");            // Code identifier
		}

		#endregion
	}
}
