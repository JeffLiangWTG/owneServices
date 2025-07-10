using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.Module
{
	public class OrgSupplierPartController : MasterFiles.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPluginGlobal(businessEntity as OrgSupplierPart);
		}
	}
}
