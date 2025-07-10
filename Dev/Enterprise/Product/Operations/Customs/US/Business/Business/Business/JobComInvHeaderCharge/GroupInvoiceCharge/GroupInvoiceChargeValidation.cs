
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}

		protected override bool ShouldCheckExRates
		{
			get { return base.ShouldCheckExRates && !ValuationDatesChanged; }
		}

		bool ValuationDatesChanged
		{
			get { return Parent.GroupInvoice != null && Parent.GroupInvoice.JobDeclaration != null && Parent.GroupInvoice.JobDeclaration.ValuationDatesChanged; }
		}

		protected override void CheckJ7_FullOrPartialApportionment()
		{
			base.CheckJ7_FullOrPartialApportionment();
			ListValidation.MessageErrorIfInvalidCode(Parent.J7_FullOrPartialApportionmentInfo, Parent.Lookups.ApportionmentTypeList, (NoResString)FullOrPartialApportionmentShouldBeInList);
		}
		internal const string FullOrPartialApportionmentShouldBeInList = "Please enter a valid Apportion Type code. The code you have selected is not in the Apportion Type codes List.";

		protected override bool IsCIFComponentUsed
		{
			get { return true; }
		}
	}
}
