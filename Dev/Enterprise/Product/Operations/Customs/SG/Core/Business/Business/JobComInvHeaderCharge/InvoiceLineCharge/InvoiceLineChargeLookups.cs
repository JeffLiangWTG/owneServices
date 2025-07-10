using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class InvoiceLineChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
		}

		#region Charge Type List

		public override CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				if (chargeTypeList == null)
				{
					chargeTypeList = new CodeDescriptionPairList();
					chargeTypeList.AddPair(InvoiceLineCharge.ChargeTypes.OptionalItemCharges, "Optional Item Charges");
				}

				return chargeTypeList;
			}
		}
		CodeDescriptionPairList chargeTypeList;

		#endregion
	}
}
