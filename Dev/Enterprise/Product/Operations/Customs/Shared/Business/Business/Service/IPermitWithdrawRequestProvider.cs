using CargoWise.Types;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.Business
{
	public interface IPermitWithdrawRequestCountryProvider
	{
		IPermitWithdrawRequestProvider FindProviderFor(PermitType permitType);
	}

	public interface IPermitWithdrawRequestProvider
	{
		/// <summary>
		/// Returns a string with a human-readable representation of the given permit criteria.
		/// </summary>
		ZString GetMatchingCriteria(PermitCriteria permitCriteria);

		/// <summary>
		/// Returns an array of active permits that match the given criteria.
		/// <para/>
		/// Permits that are market as closed are not included into result.
		/// </summary>
		BaseCusPermitHeader[] FindMatchingPermit(PermitCriteria permitCriteria);

		ZString GetOutwardEntryNumber(BaseCusPermitHeader permit);

		ZBool IsExemptForMatchingPermit(IPermitTransactionDetail detail);
	}
}
