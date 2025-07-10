//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBHeaderLookups
//
//    This class should be used for overriding collections in AutoExportAWBHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class ExportAWBHeaderLookups : AutoExportAWBHeaderLookups
	{
		public ExportAWBHeaderLookups(AutoExportAWBHeader parent)
			: base(parent)
		{
		}

		public RefAirlineCollection Airlines
		{
			get
			{
				return Parent.Factory.GetCachedValue("ExportAWBHeaderLookups.Airlines",
					() => new RefAirlineCollection(Parent.Factory));
			}
		}

		public AWBMessagingStatusList MessagingStatusList
		{
			get
			{
				return Parent.Factory.GetCachedValue("ExportAWBHeaderLookups.MessagingStatusList",
					() => new AWBMessagingStatusList());
			}
		}

		public AWBTypeList AWBTypeList
		{
			get
			{
				return Parent.Factory.GetCachedValue("ExportAWBHeaderLookups.AWBTypeList",
					() => new AWBTypeList());
			}
		}

		public RefCountryCollection IssuingCountryList
		{
			get
			{
				return Parent.Factory.GetCachedValue("ExportAWBHeaderLookups.IssuingCountryList",
					() => new RefCountryCollection(Factory));
			}
		}

		public CodeDescriptionPairList AgentApprovalCategoryList
		{
			get
			{
				return Parent.Factory.GetCachedValue("ExportAWBHeaderLookups.AgentApprovalCategoryList",
					() => new AviationSecuritySchemeMembership());
			}
		}

		public CodeDescriptionPairList AdditionalSecurityStatementList
		{
			get
			{
				return Parent.Factory.GetCachedValue("ExportAWBHeaderLookups.AdditionalSecurityStatementList",
					() => new AWBAdditionalSecurityStatementList());
			}
		}
	}
}
