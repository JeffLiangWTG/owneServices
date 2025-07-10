using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override CodeDescriptionPairList Natures => Factory.GetCachedValue<ZaShipmentTypeList>();

		public override CodeDescriptionPairList ContainerModes
		{
			get
			{
				var list = base.ContainerModes;
				if (list.ContainsCode(Core.Constants.ContainerModes.BreakBulk))
				{
					list.AddOverwriteIfExists(new CodeDescriptionPair(Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModeDescriptions.BreakBulk + "(BB)"));
				}
				if (list.ContainsCode(Core.Constants.ContainerModes.Bulk))
				{
					list.AddOverwriteIfExists(new CodeDescriptionPair(Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModeDescriptions.Bulk + "(DB)"));
				}
				if (list.ContainsCode(Core.Constants.ContainerModes.Containerised))
				{
					list.AddOverwriteIfExists(new CodeDescriptionPair(Core.Constants.ContainerModes.Containerised, Core.Constants.ContainerModeDescriptions.Containerised + "(CN)"));
				}
				if (list.ContainsCode(Core.Constants.ContainerModes.Liquid))
				{
					list.AddOverwriteIfExists(new CodeDescriptionPair(Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModeDescriptions.Liquid + "(LB)"));
				}
				if (list.ContainsCode(Core.Constants.ContainerModes.Other))
				{
					list.AddOverwriteIfExists(new CodeDescriptionPair(Core.Constants.ContainerModes.Other, Core.Constants.ContainerModeDescriptions.Other + "(MX)"));
				}

				return list;
			}
		}

		public override CodeDescriptionPairList AgentTypeList
		{
			get
			{
				var key = "ZAAsycudaManifestHeader.Lookups.AgentTypeList";
				var isAir = Parent.IsAir;
				if (isAir)
				{
					key += "_AIR";
				}

				var isNVC = Parent.IsConsolidator;
				if (isNVC)
				{
					key += "_NVC";
				}
				return Factory.GetCachedValue(key, () =>
				{
					var list = new CodeDescriptionPairList(OLookUpEditType.AgentType);
					if (isAir)
					{
						list.AddPair(Core.Constants.AgentType.AWBCoload, Core.Constants.AgentTypeDescriptions.AWBCoload);
						list.AddPair(Core.Constants.AgentType.AWBMaster, Core.Constants.AgentTypeDescriptions.AWBMaster);

						if (isNVC)
						{
							list.AddPair("FWB", ResString.GetMultilingualString("4C01C2D8-5F78-4198-ACA8-C02651438EFB", "Air Cargo Reported On A Master Air Waybill"));
						}
					}
					return list;
				});
			}
		}

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<ZAMessageStatusList>();

		public ShippingProviderCollection TSSCarrierList
		{
			get
			{
				if (Parent.IsTSSAir)
				{
					return AirShippingLineList;
				}
				else if (Parent.IsTSSSea)
				{
					return SeaShippingLineList;
				}
				else
				{
					return AirOrSeaShippingLineList;
				}
			}
		}

		public RefVesselCollection TSSRefVessels
		{
			get { return new RefVesselCollection(Factory); }
		}

		public RadioCallSignCodeFindBoxCollection VesselRadioCallSigns
		{
			get => new RadioCallSignCodeFindBoxCollection(Factory);
		}

		public CodeDescriptionPairList CallPurposeCodeList => Factory.GetCachedValue<CallPurposeCodeList>();
	}
}
