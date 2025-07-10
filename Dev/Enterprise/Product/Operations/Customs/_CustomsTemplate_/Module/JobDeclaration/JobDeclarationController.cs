using System;
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs._CustomsTemplate_.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs._CustomsTemplate_.Module
{
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness topLevelObject) => new JobDeclarationForm((JobDeclaration)topLevelObject);
	}
}
