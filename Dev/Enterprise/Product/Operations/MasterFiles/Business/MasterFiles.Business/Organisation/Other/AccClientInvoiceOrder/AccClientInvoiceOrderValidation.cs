using CargoWise.ComponentModel;

namespace Enterprise.MasterFiles.Business
{
	public class AccClientInvoiceOrderValidation : AutoAccClientInvoiceOrderValidation
	{
		public AccClientInvoiceOrderValidation(AutoAccClientInvoiceOrder parent) : base(parent)
		{
		}

		#region AI_PrintOrder
		protected override void CheckAI_PrintOrder()
		{
			base.CheckAI_PrintOrder();

			if (!Parent.AI_PrintOrderInfo.HasErrors())
			{
				if (Parent.AI_PrintOrder < 0 || Parent.AI_PrintOrder > 999)
				{
					Parent.AI_PrintOrderInfo.AddError(Res.GetString("200754e2-e888-466a-98e7-5865830ef122", "Print order should be between 0 and 999"));
				}
			}
		}
		#endregion
	}
}
