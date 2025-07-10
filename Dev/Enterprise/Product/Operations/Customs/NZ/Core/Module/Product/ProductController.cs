using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.GUI.MasterFiles;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module
{
	public class ProductController : Customs.Module.OrgSupplierPartController
	{
		public ProductController()
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
