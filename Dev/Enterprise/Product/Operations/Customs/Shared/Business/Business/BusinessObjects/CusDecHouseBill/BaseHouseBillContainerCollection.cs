using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BaseBillContainerCollection : ManyToManyBusinessObjectCollection
	{
		public BaseBillContainerCollection(Bill associatedObject) : base(associatedObject)
		{
		}

		public BaseCusContainer this[int index]
		{
			get { return (BaseCusContainer)Elements[index]; }
		}

		public new BaseCusContainer AddNew()
		{
			return (BaseCusContainer)base.AddNew();
		}

		#region Implementation

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			var container = (BaseCusContainer)child;
			var bill = (Bill)fAssociatedObject;
			if (container.CO_JE != bill.CU_JE)
			{
				container.CO_JE = bill.CU_JE;
			}
		}

		protected override Type TypeOfRelationshipBusinessObject
		{
			get { return typeof(BasePackingGroup); }
		}

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject
		{
			get { return CusDecHouseContainerPivotSchema.CR_CU_HouseBill; }
		}

		protected override SchemaGuidColumn PivotTableFKToCollectionBusinessObjects
		{
			get { return CusDecHouseContainerPivotSchema.CR_CO_Container; }
		}
		#endregion
	}
}
