using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Business;
using Enterprise.Customs.NO.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NO.Module
{
	public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);
	}
}
