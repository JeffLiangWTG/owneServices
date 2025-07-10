using System.Collections.Immutable;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobChargeAttrib : AutoJobChargeAttrib
	{
		public JobChargeAttrib(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static bool IsComparableType(ZString name) => !NotComparableTypes.Contains(name);

		static readonly ImmutableHashSet<ZString> NotComparableTypes = ImmutableHashSet.Create<ZString>(
			JobChargeAttribTypeList.Codes.MinimumRateUsed,
			JobChargeAttribTypeList.Codes.ItemsToRate,
			JobChargeAttribTypeList.Codes.CartageZoneDescription,
			JobChargeAttribTypeList.Codes.ItemsToRateUnit,
			JobChargeAttribTypeList.Codes.UnroundedItemsToRate,
			JobChargeAttribTypeList.Codes.RateId,
			JobChargeAttribTypeList.Codes.CalculatorDescription
		);

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (EC_Name.IsEmpty)
			{
				EC_Name = JobChargeAttribTypeList.Codes.UnroundedItemsToRate;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
