using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class AgencyShipmentDetentionConsumerType : AgencyShipmentConsumerType
	{
		public AgencyShipmentDetentionConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.AgencyContainerDetention; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Agency.IContainerDetention>(); }
		}

		public override bool ShouldCreateWIPs(IJobInvoicingPlugIn host, string invoiceType)
		{
			return LinerAgencyDataRegistry.Instance.CreateWIPAccrualsForDetentionCharges.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		public override bool ShouldCreateAccruals(IJobInvoicingPlugIn host, string invoiceType)
		{
			return LinerAgencyDataRegistry.Instance.CreateWIPAccrualsForDetentionCharges.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceShipping; }
		}
	}
}
