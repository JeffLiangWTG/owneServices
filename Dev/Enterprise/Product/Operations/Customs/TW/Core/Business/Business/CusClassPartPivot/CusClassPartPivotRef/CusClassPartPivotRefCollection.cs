using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	public abstract class CusClassPartPivotRefCollection<T> : DependentBusinessObjectCollection<T, BusinessObject> where T : CusClassPartPivotRef
	{
		protected CusClassPartPivotRefCollection(BusinessObject parent, ZString referenceType)
			: base(parent)
		{
			CIR_ReferenceType = Argument.NotNullOrEmpty(referenceType, nameof(referenceType));
		}

		public readonly ZString CIR_ReferenceType;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceType, CIR_ReferenceType);
			return result;
		}
	}
}
