using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NO.Module;

public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(OrgSupplierPart);

	protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new GUI.OrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
}
