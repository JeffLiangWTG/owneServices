using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusBondDetailLookups : MasterFiles.Business.CusBondDetailLookups
	{
		public CusBondDetailLookups(CusBondDetail bondData)
			: base(bondData)
		{
		}

		public CodeDescriptionPairList BondTypeList
		{
			get { return Factory.GetCachedValue<ImporterBondTypeList>(); }
		}

		public IBusinessObjectCollection RegionPorts
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public CodeDescriptionPairList ActivityCodeList
		{
			get { return Factory.GetCachedValue<ActivityCodeList>(); }
		}

		public CodeDescriptionPairList FundIndicatorList
		{
			get { return Factory.GetCachedValue<FundIndicatorList>(); }
		}
	}
}
