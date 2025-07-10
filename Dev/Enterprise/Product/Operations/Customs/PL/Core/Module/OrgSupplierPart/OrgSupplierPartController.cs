using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.PL.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.PL.Module;

public class OrgSupplierPartController : EU.Module.OrgSupplierPartController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
	{
		return new EUOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
	}
}
