using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.TW;

namespace Enterprise.Customs.TW.Business
{
	[DependentBusinessObject(typeof(CusClassPartPivot), "AssignedCusClassPartPivotRefCollection")]
	public class AssignedCusClassPartPivotRef : CusClassPartPivotRef
	{
		public AssignedCusClassPartPivotRef(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.AssignedCusClassPartPivotRef.CIR_ReferenceNumber", Caption = "Assigned Number")]
		public override ZString CIR_ReferenceNumber
		{
			get => base.CIR_ReferenceNumber;
			set
			{
				var oldValue = CIR_ReferenceNumber;
				base.CIR_ReferenceNumber = value;
				var parent = CusClassPartPivot;
				if (parent != null && value != oldValue && !IsCopying && !((ISupportDataImporting)parent).IsImportingData)
				{
					parent.RefreshBinding();
				}
			}
		}
		public override ZString ReferenceType => JobComInvLineRefsType.Codes.AssignedNumber;

		protected override CusClassPartPivotRefValidation GetNewValidation() => new AssignedCusClassPartPivotRefValidation(this);

		protected override ZString HumanReadableNameCore => Res.GetString("0a8354ab-8a55-4668-ac8a-d879c7f5af08", "Assigned Number");

		public new AssignedCusClassPartPivotRefValidation Validation => (AssignedCusClassPartPivotRefValidation)base.Validation;

		public new CusClassPartPivot CusClassPartPivot => (CusClassPartPivot)base.CusClassPartPivot;
	}
}
