using System;
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs._CustomsTemplate_.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
	}
}
