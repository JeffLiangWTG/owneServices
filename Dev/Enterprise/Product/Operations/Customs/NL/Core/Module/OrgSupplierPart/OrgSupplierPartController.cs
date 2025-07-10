using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.NL.Business.MasterFiles;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NL.Module;

public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new EUOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
}
