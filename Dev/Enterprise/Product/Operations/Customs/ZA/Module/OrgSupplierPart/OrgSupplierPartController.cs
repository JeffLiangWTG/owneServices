using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.ZA.Module
{
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public OrgSupplierPartController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartFormCustomsPluginGlobal((OrgSupplierPart)businessEntity);
		}
	}
}
