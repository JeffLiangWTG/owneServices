using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class BaseApportionedCharge : CommonApportionedCharge
	{
		public new static readonly BaseApportionedChargeTypeDecider TypeDecider = new BaseApportionedChargeTypeDecider();
		public BaseApportionedCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new BaseApportionedChargeValidation(this);
		}

		public BaseJobComInvoiceHeader Invoice
		{
			get { return (BaseJobComInvoiceHeader)base.Parent; }
		}

		#region Overrides

		public override bool HasChanges
		{
			set
			{
				base.HasChanges = value;
				if (!IsDeleted && value && Invoice != null && !IsCopying)
				{
					Invoice.RefreshBinding();
				}
			}
		}

		[RelatedBusinessObject("Invoice")]
		public override ZGuid J7_ParentID
		{
			get { return base.J7_ParentID; }
			set { base.J7_ParentID = value; }
		}

		protected override ZBool GetNeedCheckChargeType()
		{
			return !(Invoice?.JobDeclaration?.IsInterface ?? ZBool.False);
		}
		#endregion
	}
}
