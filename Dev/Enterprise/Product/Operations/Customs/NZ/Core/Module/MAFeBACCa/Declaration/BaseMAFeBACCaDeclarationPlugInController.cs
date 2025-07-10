using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module.MAFeBACCa
{
	public abstract class BaseMAFeBACCaDeclarationPlugInController : BaseMAFeBACCaPlugInController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected sealed override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var declaration = businessEntity as JobDeclaration;
			if (declaration == null)
			{
				var shipment = businessEntity as ForwardingShipment;
				if (shipment != null)
				{
					declaration = shipment.DeclarationForDocuments as JobDeclaration;
				}
			}
			return declaration == null ? null : GetPlugIn(declaration);
		}

		protected abstract ZPlugIn GetPlugIn(JobDeclaration declaration);

		#region Security
		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NZCustomsSendeBACCa; }
		}
		#endregion
	}
}
