using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	[System.Diagnostics.DebuggerDisplay("Type = {D5_Type}, Value = {D5_Value}")]
	[DependentBusinessObject(typeof(SlotAllocation), nameof(SlotAllocation.Aspects))]
	public class SlotAllocationAspect : AutoJobSlotAllocationAspect
	{
		public SlotAllocationAspect(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			args.AddExcludedColumns(new string[]
			{
				JobSlotAllocationAspectSchema.Constants.D5_E0,
			});

			return base.CloneInternal(args);
		}
	}
}
