using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondMoveDetailLookups : US.Business.CusInBondMoveDetailLookups
	{
		public CusInBondMoveDetailLookups(CusInBondMoveDetail parent)
			: base(parent)
		{
		}

		public override ICodeDescriptionPairList CustomsStatusList
		{
			get
			{
				if (Parent != null && (Parent.IsPTTMovement || Parent.IsInBondMovement))
				{
					return Factory.GetCachedValue<AMSBillMessageStatusList>();
				}
				else
				{
					return Factory.GetCachedValue<AMSBillCustomsStatusList>();
				}
			}
		}

		public override ICodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<AMSBillMessageStatusList>(); }
		}

		public ICodeDescriptionPairList InBondStatusList
		{
			get { return AMSBillMessageStatusList.GetInBondCodeList(Factory); }
		}

		public IBusinessObjectCollection RelatedBills
		{
			get
			{
				var moveHeader = Parent.MoveHeader;
				var header = moveHeader != null ? moveHeader.Header : null;
				if (header != null)
				{
					return header.Bills;
				}
				else
				{
					return new ActiveBusinessObjectCollection<CusInBondBill>(Factory, ZQuery.NoResultQuery);
				}
			}
		}

		protected new CusInBondMoveDetail Parent
		{
			get { return (CusInBondMoveDetail)base.Parent; }
		}
	}
}
