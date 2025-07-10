using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.JP.AFR;

namespace Enterprise.MasterFiles.Business
{
	public class UNDGSubstancePivot : AutoUNDGSubstancePivot
	{
		public UNDGSubstancePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IUNDGSubstancePivotParent Parent
		{
			get
			{
				switch (DP_ParentTableCode)
				{
					case UNDGDataItemSchema.Constants.Prefix:
						return Factory.Load<UNDGDataItem>(DP_ParentId);

					case JPAFRBillsSchema.Constants.Prefix:
						return Factory.Load<IJPAFRBills>(DP_ParentId) as IUNDGSubstancePivotParent;
				}

				return null;
			}
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			DP_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			DP_ParentTableCode = UNDGDataItemSchema.Constants.Prefix;
		}

#endif

	}
}
