using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public sealed class SlotAllocationDependentCollection : DependentBusinessObjectCollection<SlotAllocation, BusinessObject>
	{
		public SlotAllocationDependentCollection(ISlotAllocationParent master)
			: base((BusinessObject)master)
		{
		}

		public SlotAllocation GetAllocation(ZGuid principalPK)
		{
			SlotAllocation result = null;

			foreach (SlotAllocation allocation in this)
			{
				if (allocation.E0_OH_Principal == principalPK)
				{
					result = allocation;
					break;
				}
			}

			if (result == null)
			{
				SlotAllocation allocation = AddNew();
				allocation.E0_OH_Principal = principalPK;
				allocation.HasChanges = false;
				result = allocation;
			}

			return result;
		}

		public void RemoveAllocation(ZGuid principalPK)
		{
			for (int index = Count - 1; index >= 0; index--)
			{
				if (this[index].E0_OH_Principal == principalPK)
				{
					this[index].Delete();
				}
			}
		}

		#region Implementation

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(SlotAllocation);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return JobSlotAllocationSchema.E0_ParentID; }
		}

		#endregion
	}
}
