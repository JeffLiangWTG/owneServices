using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.TW.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgSupplierPartForm((OrgSupplierPart)businessEntity);
		}
	}
}
