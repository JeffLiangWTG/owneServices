using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondCargoDescLookups : Customs.Business.CusInBondCargoDescLookups
	{
		public CusInBondCargoDescLookups(CusInBondCargoDesc parent)
			: base(parent)
		{
		}

		protected CusInBondCargoDesc Commodity
		{
			get { return (CusInBondCargoDesc)base.Parent; }
		}

		public ICodeDescriptionPairList ManifestUnitList
		{
			get
			{
				var commodity = Commodity;
				var container = commodity != null ? commodity.Container : null;
				var bill = container != null ? container.Bill : null;
				var header = bill != null ? bill.Header : null;
				var transportMode = header != null ? header.BH_ImportTransportMode : ZString.Empty;
				return Factory.GetCachedValue<CodeDescriptionPairList>("USAMSCommodityManifestUnitList" + transportMode, delegate
				{
					var list = new ManifestUnitList();
					if (transportMode == TransportTypeList.Codes.VesselContainer ||
						transportMode == TransportTypeList.Codes.VesselNonContainer)
					{
						list.RemoveCode(Enterprise.Customs.US.AMS.Business.ManifestUnitList.Codes.PalletNotUsedInSeaAms);
					}
					return list;
				});
			}
		}

		public CodeDescriptionPairList WeightUnitList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public USCTariffCollection Tariffs
		{
			get { return new USCTariffCollection(Factory); }
		}

		protected new CusInBondCargoDesc Parent
		{
			get { return (CusInBondCargoDesc)base.Parent; }
		}
	}
}
