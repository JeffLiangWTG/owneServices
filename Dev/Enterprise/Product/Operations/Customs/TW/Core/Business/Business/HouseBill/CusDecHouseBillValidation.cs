using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public new Bill Bill => (Bill)base.Bill;

		protected new Bill Parent => (Bill)base.Parent;

		protected override void CheckCU_BillType()
		{
			base.CheckCU_BillType();
			var targetInfo = Parent.CU_BillTypeInfo;
			var billType = Parent.CU_BillType;

			var bills = Parent.Declaration?.Bills?.Cast<Bill>();
			if (bills != null)
			{
				CheckDuplicatedHouseBillType(targetInfo, bills);
				CheckBillTypeAvailable(billType, targetInfo, bills);
				CheckContainerNoteMaximumRows(billType, bills);
			}
		}

		void CheckDuplicatedHouseBillType(ZPropertyInfo targetInfo, IEnumerable<Bill> bills)
		{
			if (Bill.IsHouseBill)
			{
				if (bills.Any(x => x.CU_BillType == Bill.CU_BillType && x.PK != Bill.PK))
				{
					targetInfo.AddMessageError(ValidationConstants.Bill.ErrorOneHouseBillOnly);
				}
			}
		}

		void CheckBillTypeAvailable(ZString billType, ZPropertyInfo targetInfo, IEnumerable<Bill> bills)
		{
			if (billType == BillTypeList.Codes.ContainerNote)
			{
				if (bills.Any(x => x.CU_BillType != billType && x.PK != Parent.PK))
				{
					targetInfo.AddMessageError(ValidationConstants.Bill.ContainerNoteShouldNotExistAtTheSameTime);
				}
			}
			else if (billType == Enterprise.Customs.Business.BillTypeList.Codes.MasterBill || billType == Enterprise.Customs.Business.BillTypeList.Codes.HouseBill)
			{
				if (bills.Any(x => x.CU_BillType == BillTypeList.Codes.ContainerNote && x.PK != Parent.PK))
				{
					targetInfo.AddMessageError(ValidationConstants.Bill.ContainerNoteShouldNotExistAtTheSameTime);
				}
			}
		}

		void CheckContainerNoteMaximumRows(ZString billType, IEnumerable<Bill> bills)
		{
			if (billType == BillTypeList.Codes.ContainerNote)
			{
				var count = bills.Count(x => x.CU_BillType == billType);
				if (count > 99)
				{
					Parent.AddRowMessageError(ValidationConstants.Bill.ContainerNoteMaximumRows);
				}
			}
		}

		protected override bool ShouldAllowMultiMaster => false;

		public override INotificationType NotificationTypeForAirWayBillNumber => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override bool NeedToValidateHouseBillAndPackages => false;
	}
}
