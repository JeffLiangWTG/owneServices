using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusInBondMoveHeaderLookups : Customs.Business.CusInBondMoveHeaderLookups
	{
		public CusInBondMoveHeaderLookups(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		protected new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
		}

		public CodeDescriptionPairList YesNoList
		{
			get { return YesNoDefaultList.GetCachedYesNoList(Factory); }
		}

		public virtual ICodeDescriptionPairList MessageStatusList
		{
			get { return new CodeDescriptionPairList(); }
		}

		public USStatesList USStatesList
		{
			get { return Factory.GetCachedValue<USStatesList>(); }
		}

		public virtual InBondTransportModeCodes TransportModeCodes
		{
			get { return Factory.GetCachedValue<InBondTransportModeCodes>(); }
		}

		public RefVesselCollection ConveyanceList
		{
			get { return new RefVesselCollection(Factory); }
		}

		public ShippingProviderCollection ShippingProviders
		{
			get { return new ShippingProviderCollection(Factory); }
		}

		public InbondCommonTypeList EntryTypeList
		{
			get { return Factory.GetCachedValue<InbondCommonTypeList>(); }
		}

		public USCarrierCombinedCollection CarrierCollection
		{
			get { return new USCarrierCombinedCollection(Factory); }
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
						new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal,
						new ZString[] {  ForeignPortTypeList.Codes.Common, ForeignPortTypeList.Codes.InBond })
					});
			}
		}

		public ZZRefCusCodeListCombinedCollection RegionDistrictPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public BondedWarehouseCollection BondedWarehouseCollection
		{
			get { return new BondedWarehouseCollection(Factory); }
		}
	}
}
