using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using EUMasterFiles = Enterprise.Customs.EU.Business.MasterFiles;

namespace Enterprise.Customs._EUCustomsTemplate_.Business.MasterFiles
{
	public class OrgSupplierPart : EUMasterFiles.OrgSupplierPart, Integration.Customs._EUCustomsTemplate_.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new EUMasterFiles.ClassificationCollection<EUMasterFiles.CusClassification>(this, Core.Constants.CountryCodes._EUTemplateCountryName_);

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<EUMasterFiles.CusClassPartPivot>(this, Core.Constants.CountryCodes._EUTemplateCountryName_);
	}
}
