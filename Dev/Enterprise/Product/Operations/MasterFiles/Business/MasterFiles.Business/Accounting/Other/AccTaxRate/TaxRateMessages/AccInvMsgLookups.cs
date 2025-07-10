//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccInvMsgLookups
//
//    This class should be used for overriding collections in AutoAccInvMsgLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AccInvMsgLookups : AutoAccInvMsgLookups
	{
		public AccInvMsgLookups(AutoAccInvMsg parent) : base(parent)
		{
		}

		AccTaxRateCollection taxRates;
		public AccTaxRateCollection TaxRates
		{
			get
			{
				if (taxRates == null)
				{
					taxRates = new AccTaxRateCollection(Factory);
				}
				return taxRates;
			}
		}

		CodeDescriptionPairList taxGroupCodes;
		public CodeDescriptionPairList TaxGroupCodes
		{
			get
			{
				if (taxGroupCodes == null)
				{
					taxGroupCodes = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
				}
				return taxGroupCodes;
			}
		}
	}
}
