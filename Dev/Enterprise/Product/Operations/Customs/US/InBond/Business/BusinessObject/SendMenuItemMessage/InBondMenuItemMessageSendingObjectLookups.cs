using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class InBondMenuItemMessageSendingObjectLookups : ZLookups
	{
		public InBondMenuItemMessageSendingObjectLookups(InBondMenuItemMessageSendingObject parent) : base(parent)
		{
		}

		protected new InBondMenuItemMessageSendingObject Parent => (InBondMenuItemMessageSendingObject)base.Parent;

		public ConsigneeCollection ImporterList
		{
			get { return new ConsigneeCollection(Factory); }
		}

		public ShippingProviderCollection ShippingProviders
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		public USCarrierCombinedCollection CarrierCollection
		{
			get { return new USCarrierCombinedCollection(Factory); }
		}

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public ZZRefCusCodeListCombinedCollection ForeignPorts
		{
			get
			{
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.UnitedStates,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port },
					ZDateTime.Today,
					new[]
					{
					new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, new ZString[] { ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
					});
			}
		}

		public virtual ICodeDescriptionPairList MessageStatusList
		{
			get { return Factory.GetCachedValue<ImportMessageStatusList>(); }
		}

		public CodeDescriptionPairList EntryTypeList
		{
			get
			{
				return Factory.GetCachedValue("", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(InbondCommonTypeList.Codes._2TransportandExport, InbondCommonTypeList.Descriptions._2TransportandExport);
					result.AddPair(InbondCommonTypeList.Codes._3ImmediateExport, InbondCommonTypeList.Descriptions._3ImmediateExport);
					return result;
				});
			}
		}
	}
}
