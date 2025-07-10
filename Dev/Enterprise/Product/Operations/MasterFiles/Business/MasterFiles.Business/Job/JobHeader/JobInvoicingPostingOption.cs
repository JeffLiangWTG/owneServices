
namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Invoice Posting Options
	/// </summary>
	public enum JobInvoicingPostingOption
	{
		All = 0,
		LocalClient = 1,
		Gateway = 2,
		Agent = 3,
		Revenue = 4,
		Costs = 5,
		Disbursement = 6,
		CustomsDSBChargeAPOnly = 7,
		CustomsDSBChargeAROnly = 8,
		ConsolCosts = 9,
		AllSisterCompanyCharges = 10,
		LocalSisterCompanyChargesOnly = 11,
	}
}
