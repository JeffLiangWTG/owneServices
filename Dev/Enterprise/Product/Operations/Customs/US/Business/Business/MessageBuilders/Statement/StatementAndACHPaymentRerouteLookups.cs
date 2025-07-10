//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStatementAndACHPaymentRerouteLookups
//
//    This class should be used for overriding collections in AutoStatementAndACHPaymentRerouteLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class StatementAndACHPaymentRerouteLookups : AutoStatementAndACHPaymentRerouteLookups
	{
		public StatementAndACHPaymentRerouteLookups(AutoStatementAndACHPaymentReroute parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList Z9_RerouteTypeList
		{
			get { return Factory.GetCachedValue<StatementTypeList>(); }
		}

		public CodeDescriptionPairList Z9_MessageTypeList
		{
			get { return Factory.GetCachedValue<JobApplicationCodeList>(); }
		}

		public BusinessObjectCollection Z9_ProcessingPortList
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}
	}
}
