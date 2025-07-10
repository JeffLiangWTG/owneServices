using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class SlotAllocationAspectCollection : ActiveBusinessObjectCollection<SlotAllocationAspect>
	{
		public SlotAllocationAspectCollection(SlotAllocation allocation)
			: base(allocation) { }

		public SlotAllocationAspect this[string code]
		{
			get
			{
				foreach (SlotAllocationAspect aspect in this)
				{
					if (aspect.D5_Type == code)
					{
						return aspect;
					}
				}

				return null;
			}
		}
	}
}
