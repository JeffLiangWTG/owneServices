using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingUNDGDataItemLookups : UNDGDataItemLookups
	{
		public ForwardingUNDGDataItemLookups(ForwardingUNDGDataItem parent)
			: base(parent)
		{
			parentShipment = parent?.ParentPackLineShipment;
		}

		readonly ForwardingShipment parentShipment;

		#region UNDGSubstances

		public override UNDGSubstanceCollection UNDGSubstances
		{
			get
			{
				return TryAddTransportRelatedFilter(new ForwardingUNDGSubstanceCollection(Factory, parentShipment));
			}
		}

		T TryAddTransportRelatedFilter<T>(T collection) where T : IActiveBusinessObjectCollection
		{
			var standard = DGStandardCalculator
				.GetCorrespondingStandardForShipment(parentShipment);

			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Standard", "Property", standard, true));
			return collection;
		}

		#endregion

		#region PackingInstructionSectionList

		public override CodeDescriptionPairList PackingInstructionSectionList
		{
			get
			{
				var bo = Parent as UNDGDataItem;

				if (bo != null && bo.Subs != null && bo.Subs.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA)
				{
					if (bo.Subs.DG_UNNO == LithiumBatteryConstants.UNNOCodes.LithiumMetalBatteries || bo.Subs.DG_UNNO == LithiumBatteryConstants.UNNOCodes.LithiumIonBatteries)
					{
						return Factory.GetCachedValue("UNDGDataItemLookups|PackingInstructionSectionList|Lithium|IAIB", () =>
						{
							var list = new CodeDescriptionPairList();
							list.AddPair(PackingInstructionSectionTypeList.Codes.SectionIA, PackingInstructionSectionTypeList.Descriptions.SectionIA);
							list.AddPair(PackingInstructionSectionTypeList.Codes.SectionIB, PackingInstructionSectionTypeList.Descriptions.SectionIB);
							return list;
						});
					}
					else if (bo.Subs.DG_UNNO == LithiumBatteryConstants.UNNOCodes.PackedLithiumMetalBatteries || bo.Subs.DG_UNNO == LithiumBatteryConstants.UNNOCodes.PackedLithiumIonBatteries)
					{
						var list = new CodeDescriptionPairList();
						return Factory.GetCachedValue("UNDGDataItemLookups|PackingInstructionSectionList|PackedLithium", () =>
						{
							list.AddPair(PackingInstructionSectionTypeList.Codes.SectionI, PackingInstructionSectionTypeList.Descriptions.SectionI);
							list.AddPair(PackingInstructionSectionTypeList.Codes.SectionII, PackingInstructionSectionTypeList.Descriptions.SectionII);
							return list;
						});
					}
				}

				return base.PackingInstructionSectionList;
			}
		}

		#endregion

		#region Radioactive Label Categories

		public override CodeDescriptionPairList RadioactiveLabelCategoryList
		{
			get
			{
				return Factory.GetCachedValue("ForwardingUNDGDataItemLookups|RadioactiveLabelCategoryList", () =>
				{
					return new RadioactiveLabelCategoryList();
				});
			}
		}

		#endregion

		#region Radionuclide Elements
		#region SuppressResourceStringsCheckRegion

		public override CodeDescriptionPairList RadionuclideElementList
		{
			get
			{
				return Factory.GetCachedValue("ForwardingUNDGDataItemLookups|RadionuclideElementList", () =>
				{
					return new RadionuclideElementList();
				});
			}
		}
		public override CodeDescriptionPairList RadionuclideElementSuffixList
		{
			get
			{
				var parent = Parent as ForwardingUNDGDataItem;
				return Factory.GetCachedValue($"ForwardingUNDGDataItemLookups|RadionuclideElementSuffixList_{parent.DI_RadionuclideElement}", () =>
				{
					return new RadionuclideElementSuffixList(parent.DI_RadionuclideElement);
				});
			}
		}

		public override CodeDescriptionPairList RadioactiveMaximumActivityUnitList
		{
			get
			{
				return Factory.GetCachedValue("ForwardingUNDGDataItemLookups|RadioactiveMaximumActivityUnitList", () =>
				{
					return new RadioactiveMaximumActivityUnitList();
				});
			}
		}

		#endregion
		#endregion

	}
}
