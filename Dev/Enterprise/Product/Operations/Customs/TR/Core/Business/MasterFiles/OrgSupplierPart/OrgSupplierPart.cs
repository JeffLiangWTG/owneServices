using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using EUMasterFiles = Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs.TR.Business.MasterFiles
{
	public class OrgSupplierPart : EUMasterFiles.OrgSupplierPart, Integration.Customs.TR.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new EUMasterFiles.ClassificationCollection<EUMasterFiles.CusClassification>(this, Core.Constants.CountryCodes.Turkey);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<EUMasterFiles.CusClassPartPivot>(this, Core.Constants.CountryCodes.Turkey);
	}
}
