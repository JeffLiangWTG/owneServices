using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFBillRowValidation : AutoISFBillRowValidation
	{
		public ISFBillRowValidation(AutoISFBillRow parent)
			: base(parent) { }

		public new ISFBillRow Parent
		{
			get { return (ISFBillRow)base.Parent; }
		}

		protected override void CheckBillPKIsValidZGuid()
		{
			if (Parent.BillPK.IsEmpty)
			{
				Parent.BillPKInfo.AddError(ValidationConstants.ISFBillRow.ValidBill);
			}
			else
			{
				ListValidation.ErrorIfInvalidPK(Parent.BillPKInfo, Parent.Bills, ValidationConstants.ISFBillRow.ValidBill);
			}
		}

		protected override void CheckBillPK()
		{
			base.CheckBillPK();
			CusISFBill bill = Parent.Bill;
			if (bill != null)
			{
				if (bill.IsHouseBillOfLading)
				{
					CheckIfShipmentExistWithHouseBillNumber(bill, Parent.BillPKInfo);
				}
				CheckNoDuplicateBill();
			}
		}

		void CheckNoDuplicateBill()
		{
			ISFHeaderRow headerRow = Parent.HeaderRow;
			if (headerRow != null)
			{
				foreach (ISFBillRow billRow in headerRow.Bills)
				{
					if (billRow != Parent && billRow.BillPK == Parent.BillPK)
					{
						Parent.BillPKInfo.AddError(ValidationConstants.ISFBillRow.BillAlreadyUsedInExistingRow(Parent.Bill.BillTypeDescriptonAndNumber));
						break;
					}
				}
			}
		}

		void CheckIfShipmentExistWithHouseBillNumber(CusISFBill bill, ZPropertyInfo info)
		{
			ZQuery filter = new ZQuery(JobShipmentSchema.JS_HouseBill, bill.BB_BillNum);
			filter.OrderBy = JobShipmentSchema.Constants.JS_UniqueConsignRef;
			ForwardingShipment[] existingShipments = bill.Factory.Load<ForwardingShipment>(filter);
			if (existingShipments.Length > 0)
			{
				info.AddWarning(GetExistingShipmentMessage(existingShipments, bill.BB_BillNum));
			}
		}

		string GetExistingShipmentMessage(ForwardingShipment[] existingShipments, ZString billNumber)
		{
			ZStringBuilder shipmentReferences = new ZStringBuilder();
			foreach (ForwardingShipment shipment in existingShipments)
			{
				shipmentReferences.Append("'" + shipment.JS_UniqueConsignRef + "'");
			}
			return ValidationConstants.ISFBillRow.BillAlreadyUsedInShipment(billNumber, existingShipments.Length, shipmentReferences.ToStringWithDelimiterBetweenAppends(", "));
		}
	}
}
