using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.GUI.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZArchitecture.GUI.IZForm GetFormCore(IBusiness businessEntity)
		{
			return new DeclarationForm((JobDeclaration)businessEntity);
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugIn((ForwardingShipment)businessEntity);
		}

#if DEBUG

		public ZArchitecture.PlugIn.ZPlugIn GetPlugIn_ForTest(IBusiness businessEntity)
		{
			return GetPlugIn(businessEntity);
		}

#endif
	}

	public class JobDeclarationShipmentController : Customs.Module.JobDeclarationShipmentController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugIn((ForwardingShipment)businessEntity);
		}

#if DEBUG

		public ZArchitecture.PlugIn.ZPlugIn GetPlugIn_ForTest(IBusiness businessEntity)
		{
			return GetPlugIn(businessEntity);
		}

#endif
	}
}
