using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class OrgSupplierPart : TypeSafeOrgSupplierPart, Integration.Customs.ZA.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static OrgSupplierPart New(BusinessObjectFactory factory) => factory.New<OrgSupplierPart>();

		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		public new OrgSupplierPartValidation Validation => (OrgSupplierPartValidation)base.Validation;

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.SouthAfrica);

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.SouthAfrica);

		protected override MasterFiles.Business.OrgSupplierPartValidation GetNewValidation() => new OrgSupplierPartValidation(this);
	}
}
