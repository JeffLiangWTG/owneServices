using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class CusClassPartPivotCollection : Customs.Business.CusClassPartPivotCollection<CusClassPartPivot>
	{
		public CusClassPartPivotCollection(OrgSupplierPart part)
			: base(part, Core.Constants.CountryCodes.UnitedStates)
		{
		}

		public CusClassPartPivotCollection(CusClassPartPivot pivot)
			: base(pivot, Core.Constants.CountryCodes.UnitedStates)
		{
			this.pivot = pivot;
		}

		readonly CusClassPartPivot pivot;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (CusClassPartPivot)child;
			using (newElement.GetValidationSuspender())
			using (newElement.SuspendSettingHasChanges())
			{
				var previousPivot = (CusClassPartPivot)GetNonDeletedPivots().LastOrDefault();
				var childType = previousPivot?.CI_ChildType ?? (pivot == null ? ClassificationTypeList.Codes.HTI : ClassificationChildTypeList.Codes.COMPONENT);
				newElement.CI_ChildType = childType;
			}
		}
	}
}
