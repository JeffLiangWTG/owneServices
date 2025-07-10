using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IChildBillCollection<out TBill, out TDeclaration> : IBusinessObjectCollection
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		new TBill this[int index] { get; }
		new TBill AddNew();

		void Add(BusinessObject obj);

		bool HasAGUIPresentationChild { get; }
		TBill GUIPresentationBill { get; }

		void Rebuild();
		new void Remove(BusinessObject obj);
		void RemoveReferenceFromChildren();
	}

	public class ChildBillCollection<TBill, TDeclaration> : BusinessObjectCollectionView<TBill>, IChildBillCollection<TBill, TDeclaration>
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		public ChildBillCollection(TBill parentBill, TDeclaration declaration)
			: base((BusinessObjectCollection)declaration.Bills)
		{
			this.parentBill = parentBill;
			this.declaration = declaration;
			Rebuild();
		}

		public new TBill[] ToArray() => this.Cast<TBill>().ToArray();

		protected readonly TBill parentBill;
		protected readonly TDeclaration declaration;

		public void RemoveReferenceFromChildren()
		{
			foreach (TBill bill in this.ToArray())
			{
				bill.CU_CU_ParentBill = ZGuid.Empty;
			}
		}

		public TBill GUIPresentationBill
		{
			get
			{
				TBill[] result = (TBill[])Find(new ZQuery(CusDecHouseBillSchema.CU_GUIPresentationRecord, ZBool.True));
				return result.Length > 0 ? result[0] : null;
			}
		}

		public bool HasAGUIPresentationChild => Find(new ZQuery(CusDecHouseBillSchema.CU_GUIPresentationRecord, ZBool.True)).Length > 0;

		protected override void RebuildOnConstruction()
		{
			// do not want to do this at this point : ParentBill is null
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			TBill bill = (TBill)element;

			return parentBill.PK == bill.CU_CU_ParentBill;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child1)
		{
			base.SetDefaultsForNewChild(child1);
			TBill child = (TBill)child1;
			child.CU_CU_ParentBill = parentBill.PK;
			ZString billType = ZString.Empty;
			switch (parentBill.CU_BillType)
			{
				case BillTypeList.Codes.MasterBill:
					billType = BillTypeList.Codes.HouseBill;
					break;
				case BillTypeList.Codes.HouseBill:
					billType = BillTypeList.Codes.SubHouseBill;
					break;
			}
			child.CU_BillType = billType;
		}
	}
}
