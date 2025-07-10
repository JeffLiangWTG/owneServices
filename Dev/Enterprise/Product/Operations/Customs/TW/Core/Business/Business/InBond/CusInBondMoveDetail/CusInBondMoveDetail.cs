using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusInBondMoveDetail : Customs.Business.CusInBondMoveDetail
	{
		public CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ContainerTypeCore => typeof(CusInBondContainer);

		protected override Type PackTypeCore => typeof(CusInvPack);

		protected override Type MoveLineItemTypeCore => typeof(CusInBondMoveLineItem);

		#region Override Properties
		public override ZGuid B9_BM
		{
			get => base.B9_BM;
			set
			{
				base.B9_BM = value;
				CusInBondMoveLineItemCollection.MarkAsNeedingValidation();
			}
		}
		#endregion
		#region Implementation
		public new CusInBondBill Bill => (CusInBondBill)base.Bill;

		public new CusInBondMoveHeader MoveHeader => Factory.Load<CusInBondMoveHeader>(B9_BM);

		[ChildEditable]
		public new CusInBondContainerCollection Containers => (CusInBondContainerCollection)base.Containers;

		protected override ICusInBondContainerCollection GetContainersCollection() => new CusInBondContainerCollection(this);
		#endregion

		[ChildEditable]
		public CusInBondMoveLineItemCollection CusInBondMoveLineItemCollection
		{
			get
			{
				if (fCusInBondMoveLineItemCollection == null)
				{
					fCusInBondMoveLineItemCollection = new CusInBondMoveLineItemCollection(this);
					RegisterEditableChildObject(fCusInBondMoveLineItemCollection);
				}
				return fCusInBondMoveLineItemCollection;
			}
		}
		CusInBondMoveLineItemCollection fCusInBondMoveLineItemCollection;

		public CusInBondMoveLineItem InBondMoveLineItem
		{
			get
			{
				if (fCusInBondMoveLineItem?.IsDeleted ?? true)
				{
					fCusInBondMoveLineItem = CusInBondMoveLineItemCollection.FirstOrDefault() ?? CusInBondMoveLineItemCollection.AddNew();
				}
				return fCusInBondMoveLineItem;
			}
		}

		CusInBondMoveLineItem fCusInBondMoveLineItem;
	}
}
