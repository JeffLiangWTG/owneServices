using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class LinkedDGSubstanceInfo : NonPersistentBusinessObject
	{
		public LinkedDGSubstanceInfo(UNDGDataItem item, UNDGSubstance substance) : base(substance.Factory)
		{
			this.substance = substance;
			this.item = item;
			this.pivot = TryGetPivot();
			this.isLinked = pivot != null;
		}

		readonly UNDGSubstance substance;
		UNDGSubstancePivot pivot;
		readonly UNDGDataItem item;

		public ZBool IsLinked
		{
			get { return isLinked; }
			set
			{
				if (isLinked != value)
				{
					isLinked = value;
					if (isLinked)
					{
						pivot = item.UNDGSubstancePivotCollection.AddPivotFromSubstance(substance);
					}
					else
					{
						item.UNDGSubstancePivotCollection.Delete(pivot);
					}

					IsLinkedInfo.RefreshBinding();
				}
			}
		}

		ZBool isLinked;

		public bool IsLinked_ReadOnly => pivot != null && pivot.DP_IsDefault;

		public ZPropertyInfo IsLinkedInfo
		{
			get { return GetZPropertyInfo(nameof(IsLinked)); }
		}

		UNDGSubstancePivot TryGetPivot()
		{
			return item.UNDGSubstancePivotCollection.FirstOrDefault(p => p.DP_ParentId == item.PK
															&& p.DP_UNNO == substance.DG_UNNO
															&& p.DP_Variant == substance.DG_Variant
															&& p.DP_Standard == substance.DG_Standard);
		}

		public ZString Mode => substance.DG_Mode;
		public ZString Country => substance.DG_Country;
		public ZString ProperShippingName => substance.DG_PSN;
		public ZString Class => substance.DG_Class;
		public ZString SubLabel => substance.DG_SubLabel1;
		public ZDecimal LimitedQuantity => substance.DG_LQMaxAmt;
		public ZString ExceptedQuantity => substance.DG_ExceptedQuantityCode;
		public ZString AdditionalInformation => substance.SpecialProvisionDescriptor;
	}
}
