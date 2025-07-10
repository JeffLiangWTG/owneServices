using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.SG.V4.Module
{
	/// <summary>
	/// Module Controller for SGPlaces.
	/// </summary>
	public class OrgSupplierPartController : Customs.Module.OrgSupplierPartController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgSupplierPart); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new OrgSupplierPartForm((OrgSupplierPart)businessEntity);
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new OrgSupplierPartPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
