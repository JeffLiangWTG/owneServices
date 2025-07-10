using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public OrgSupplierPartController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
	}
}
