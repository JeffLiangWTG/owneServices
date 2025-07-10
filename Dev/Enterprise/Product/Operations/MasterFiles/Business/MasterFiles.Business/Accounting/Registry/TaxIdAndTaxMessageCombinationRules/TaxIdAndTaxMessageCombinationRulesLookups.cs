using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TaxIdAndTaxMessageCombinationRulesLookups
	{
		public TaxIdAndTaxMessageCombinationRulesLookups(ZString countryCode)
		{
			CountryCode = countryCode;
		}

		public CodeDescriptionPairList LineTypes
		{
			get
			{
				if (lineTypes == null)
				{
					lineTypes = new CodeDescriptionPairList();
					lineTypes.AddPair(TransactionLineTypes.Revenue, Res.GetString("52a8a112-7ebb-4993-bdee-80b492a70970", "Revenue"));
					lineTypes.AddPair(TransactionLineTypes.Cost, Res.GetString("01afd92d-abb4-46f1-a7cc-0ea2badee89d", "Cost"));
					lineTypes.AddPair(TransactionTypes.DirectReceipt, Res.GetString("696ac178-32e9-4672-aa5c-2a2903cd1e6c", "Direct Receipt"));
					lineTypes.AddPair(TransactionTypes.DirectPayment, Res.GetString("3917df93-9ea0-41ff-8449-7f3ab023c45a", "Direct Payment"));
				}

				return lineTypes;
			}
		}
		CodeDescriptionPairList lineTypes;

		public AccInvMsgCollection TaxMessages => new AccInvMsgCollection(new BusinessObjectFactory(), CountryCode);

		public virtual AccTaxRateCollection TaxRates => new AccTaxRateCollection(new BusinessObjectFactory(), CountryCode);

		public ZString CountryCode { get; }
	}
}
