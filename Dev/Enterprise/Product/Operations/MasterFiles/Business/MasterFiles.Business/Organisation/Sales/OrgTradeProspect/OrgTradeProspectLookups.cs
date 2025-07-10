//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgTradeProspectLookups
//
//    This class should be used for overriding collections in AutoOrgTradeProspectLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeProspectLookups : AutoOrgTradeProspectLookups
	{
		public OrgTradeProspectLookups(AutoOrgTradeProspect parent) : base(parent)
		{
		}

		#region Densities

		public CodeDescriptionPairList Densities
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.Densities", () =>
				{
					return new CodeDescriptionPairList(OLookUpEditType.AirDensity);
				});
			}
		}

		#endregion

		#region Incoterms

		public CodeDescriptionPairList Incoterms
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("OrgTradeProspectLookups.Incoterms", () =>
					new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncludingDomesticTerms)
				);
			}
		}

		#endregion

		#region PeriodOfActivityTypes

		public ReadOnlyCodeDescriptionPairList ActivePeriodOfActivityTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.ActivePeriodOfActivityTypes", () =>
				{
					var result = OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetActiveCodeDescriptionPairList();
					result.SortByDescription();
					return result;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList PeriodOfActivityTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.PeriodOfActivityTypes", () =>
				{
					return OrganisationsDataRegistry.Instance.PeriodOfActivityTypes.Value.GetCodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region IndustryVerticalTypes

		public ReadOnlyCodeDescriptionPairList ActiveIndustryVerticalTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.ActiveIndustryVerticalTypes", () =>
				{
					var result = OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetActiveCodeDescriptionPairList();
					result.SortByDescription();
					return result;
				});
			}
		}

		public ReadOnlyCodeDescriptionPairList IndustryVerticalTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.IndustryVerticalTypes", () =>
				{
					return OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value.GetCodeDescriptionPairList();
				});
			}
		}

		#endregion

		#region RecurrenceTypes

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public CodeDescriptionPairList RecurrenceTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.RecurrenceTypes", () =>
				{
					return new OrgTradeProspectRecurrenceTypeList();
				});
			}
		}

		#endregion

		#region ConversionCertaintyLikertInverseItems

		public CodeDescriptionPairList ConversionCertaintyLikertInverseItems
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.ConversionCertaintyLikertInverseItems", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair item in new CertaintyLikertItemList())
					{
						result.AddPair(item.MultilingualDescription, item.Code);
					}

					return result;
				});
			}
		}

		#endregion

		#region ControllingAgents

		public override OrgHeaderCollection ControllingAgents
		{
			get { return controllingAgents ?? (controllingAgents = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrgHeaderCollection controllingAgents;

		#endregion

		#region ServiceProviders

		public override OrgHeaderCollection ServiceProviders
		{
			get { return serviceProviders ?? (serviceProviders = new ShippingProviderCollection(Factory)); }
		}
		OrgHeaderCollection serviceProviders;

		#endregion

		#region Competitors

		public override OrgHeaderCollection Competitors
		{
			get { return competitors ?? (competitors = new CompetitorCollection(Factory)); }
		}
		OrgHeaderCollection competitors;

		#endregion

		#region Expiry Reason

		public CodeDescriptionPairList ExpiryReasons
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.ExpiryReasons", () =>
				{
					var result = new OrgTradeProspectExpiryReasonList();
					result.AddRange(ManualExpiryReasons);
					return result;
				});
			}
		}

		public CodeDescriptionPairList ManualExpiryReasons
		{
			get
			{
				return Factory.GetCachedValue("OrgTradeProspectLookups.ExpiryReasons", () =>
				{
					return OrganisationsDataRegistry.Instance.EstimateExpiryReasons.Value.GetActiveCodeDescriptionPairList();
				});
			}
		}

		#endregion
	}
}
