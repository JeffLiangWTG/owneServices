using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class OrgSupplierPart : Customs.Business.OrgSupplierPart, Integration.Customs.TW.IOrgSupplierPart
	{
		public OrgSupplierPart(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new ClassificationCollection<CusClassification> ClassificationsForBinding => (ClassificationCollection<CusClassification>)base.ClassificationsForBinding;

		protected override IClassificationCollection<BaseCusClassification> GetNewClassificationCollection() => new ClassificationCollection<CusClassification>(this, Core.Constants.CountryCodes.Taiwan);

		[ChildEditable(true)]
		public new CusClassPartPivotCollection<CusClassPartPivot> PivotsForBinding => (CusClassPartPivotCollection<CusClassPartPivot>)base.PivotsForBinding;

		protected override ICusClassPartPivotCollection<BaseCusClassPartPivot> GetNewParentPivots() => new CusClassPartPivotCollection<CusClassPartPivot>(this, Core.Constants.CountryCodes.Taiwan);

		[ResourceStringData("Enterprise.Customs.TW.Business.OrgSupplierPart|OP_Desc", Caption = "English Desc.")]
		public override ZString OP_Desc { get => base.OP_Desc; set => base.OP_Desc = value; }
	}
}

