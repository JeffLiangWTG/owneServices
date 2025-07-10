using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.Business
{
	public class ISFHeaderRowValidation : AutoISFHeaderRowValidation
	{
		public ISFHeaderRowValidation(AutoISFHeaderRow parent)
			: base(parent) { }

		public new ISFHeaderRow Parent
		{
			get { return (ISFHeaderRow)base.Parent; }
		}

		protected override void CheckConsolPK()
		{
			base.CheckConsolPK();
			ValidateMasterBillPK();
		}

		protected override void CheckMasterBillPKIsValidZGuid()
		{
			ListValidation.ErrorIfInvalidPK(Parent.MasterBillPKInfo, Parent.MasterBills, ValidationConstants.ISFHeaderRow.ValidMasterBill);
		}

		protected override void CheckMasterBillPK()
		{
			base.CheckMasterBillPK();
			CusISFBill masterBill = Parent.MasterBill;
			if (masterBill != null)
			{
				CheckIfConsolExistWithOceanBillNumber(masterBill, Parent.MasterBillPKInfo);
			}
		}

		void CheckIfConsolExistWithOceanBillNumber(CusISFBill bill, ZPropertyInfo info)
		{
			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, bill.BB_BillNum);
			filter.AddToFilter(JobConsolSchema.PK, SQLComparisonOperator.NotEqual, Parent.ConsolPK);
			filter.OrderBy = JobConsolSchema.Constants.JK_UniqueConsignRef;
			ForwardingConsol[] existingConsols = bill.Factory.Load<ForwardingConsol>(filter);
			if (existingConsols.Length > 0)
			{
				info.AddWarning(GetExistingConsolMessage(existingConsols, bill));
			}
		}

		string GetExistingConsolMessage(ForwardingConsol[] existingConsols, CusISFBill bill)
		{
			ZStringBuilder consolReferences = new ZStringBuilder();
			foreach (ForwardingConsol consol in existingConsols)
			{
				consolReferences.Append("'" + consol.JK_UniqueConsignRef + "'");
			}
			return ValidationConstants.ISFHeaderRow.BillAlreadyUsedInConsol(bill.BillTypeDescriptonAndNumber, existingConsols.Length, consolReferences.ToStringWithDelimiterBetweenAppends(", "));
		}
	}
}
