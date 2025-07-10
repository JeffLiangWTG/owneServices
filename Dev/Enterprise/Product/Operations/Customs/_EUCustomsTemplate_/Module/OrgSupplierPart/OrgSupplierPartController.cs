using System;
using CargoWise.EntityFramework;
using Enterprise.Customs._EUCustomsTemplate_.Business.MasterFiles;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs._EUCustomsTemplate_.Module
{
	public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new EUOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
	}
}
