using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business
{
	public class LandedLineCostItemCollection : DependentBusinessObjectCollection<LandedLineCostItem, LandedCostHistory>
	{
		public LandedLineCostItemCollection(LandedCostHistory header)
			: base(header)
		{
			this.History = header;
		}

		public LandedLineCostItem this[string costTypeCode]
		{
			get
			{
				// Do not create if not exists
				return GetElementWithThisCode(costTypeCode);
			}
		}

		public LandedLineCostItem GetElementWithThisCode(ZString costType)
		{
			foreach (LandedLineCostItem cost in Elements)
			{
				if (cost.LZ_CostType == costType)
				{
					return cost;
				}
			}
			return null;
		}

		public LandedLineCostItem AddNew(ZString costTypeCode)
		{
			var newItem = AddNew();
			newItem.LZ_CostType = costTypeCode;
			return newItem;
		}

		public LandedLineCostItem AddNew(ZString costTypeCode, ZDecimal costAmount)
		{
			var newItem = AddNew(costTypeCode);
			newItem.LZ_CostAmount = costAmount;
			return newItem;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(LandedLineCostItemSchema.LZ_LH, History.PK);
			return result;
		}

		protected readonly LandedCostHistory History;
	}
}
