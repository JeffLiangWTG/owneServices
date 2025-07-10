using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.GUI.Protest;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Module
{
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(JobDeclaration);

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity) => new BrokeragePlugIn((ForwardingShipment)businessEntity);

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity)
		{
			var declaration = (JobDeclaration)businessEntity;
			if (declaration.JE_MessageType == JobMessageTypeList.MoreCodes.Protest)
			{
				return new ProtestForm(new Business.Protest.Protest(declaration));
			}
			else if (declaration.JE_MessageType == JobMessageTypeList.Codes.Recon)
			{
				return new ReconDeclarationForm(declaration.ReconDeclaration ?? new ReconDeclaration(declaration));
			}
			return new JobDeclarationForm(declaration);
		}
	}
}
