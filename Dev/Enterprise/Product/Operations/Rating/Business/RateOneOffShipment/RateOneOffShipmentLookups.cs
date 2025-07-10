using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateOneOffShipmentLookups : AutoRateOneOffShipmentLookups
	{
		public RateOneOffShipmentLookups(AutoRateOneOffShipment parent)
			: base(parent)
		{
		}

		public new RateOneOffShipment Parent
		{
			get { return (RateOneOffShipment)base.Parent; }
		}

		public CodeDescriptionPairList CompanyTariffLevelList
		{
			get
			{
				if (companyTariffLevelList == null)
				{
					companyTariffLevelList = new CompanyTariffLevelList(Factory);
				}
				return companyTariffLevelList.CompanyTariffLevelOverrideList;
			}
		}

		CompanyTariffLevelList companyTariffLevelList;

		#region Modes

		public CodeDescriptionPairList Modes => Factory.GetCachedValue("RateModes", RatingFreightModeLists.RateModeList);

		public CodeDescriptionPairList WebTrackerModes => Factory.GetCachedValue("WebTrackerModes", delegate
		{
			var result = Modes;
			result.RemoveCode(Core.Constants.RateMode.BCN);
			result.RemoveCode(Core.Constants.RateMode.SCN);
			result.RemoveCode(Core.Constants.RateMode.BLK);
			result.RemoveCode(Core.Constants.RateMode.BBK);
			result.RemoveCode(Core.Constants.ContainerModes.Liquid);
			result.RemoveCode(Core.Constants.RateMode.ROR);
			result.RemoveCode(Core.Constants.RateMode.OBC);
			result.RemoveCode(Core.Constants.RateMode.UNA);

			return result;
		});

		#endregion

		#region TransportModes

		public CodeDescriptionPairList TransportModes => Factory.GetCachedValue("TransportModes", RatingFreightModeLists.TransportModeList);

		#endregion

		#region ContainerModes

		public CodeDescriptionPairList ContainerModes =>
			Factory.GetCachedValue($"ContainerModes_{Parent.TT_TransportMode}",
				() => RatingFreightModeLists.ContainerModeList(Parent.TT_TransportMode));

		#endregion

		#region HBL Delivery Modes

		public CodeDescriptionPairList HBLDeliveryModesList
		{
			get
			{
				return Factory.GetCachedValue(
					"RateOneOffShipment.Lookups.TT_HBLDeliveryMode_" + Parent.TT_ContainerMode,
					delegate
					{
						var list = new CodeDescriptionPairList();
						var hBLDeliveryModes = FreightUtilities.ShipmentHBLDeliveryMode(Parent.TT_ContainerMode);
						foreach (HBLDeliveryMode item in hBLDeliveryModes.Modes)
						{
							if (item.ShowInList)
							{
								list.AddPair(item.Code, item.Description);
							}
						}

						return list;
					});
			}
		}

		#endregion

		#region UnitOfVolumeList

		public CodeDescriptionPairList UnitOfVolumeList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region UnitOfWeightList

		public CodeDescriptionPairList UnitOfWeightList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region Organisations

		public OrgHeaderCollection Organisations
		{
			get { return Factory.GetCachedValue("OrganisationsGeneric", delegate { return new OrgHeaderCollection(Factory); }); }
		}

		public OrgHeaderCollection PickUps
		{
			get { return Factory.GetCachedValue("PickUps", delegate { return new OrganisationsFindBoxCollection(Factory); }); }
		}

		public OrgHeaderCollection Deliveries
		{
			get { return Factory.GetCachedValue((NoResString)"Deliveries", delegate { return new OrganisationsFindBoxCollection(Factory); }); } // Factory Cache Key
		}

		#endregion

		#region Carriers

		public new ShippingProviderCollection Carriers
		{
			get { return Factory.GetCachedValue("ShippingProviderCollection", delegate { return new ShippingProviderCollection(Factory); }); }
		}

		#endregion

		#region IncoTerms

		public CodeDescriptionPairList IncoTerms
		{
			get { return Parent.IsDomesticFreight ? DomesticPaymentTermsList : InternationalPaymentTermsList; }
		}

		CodeDescriptionPairList DomesticPaymentTermsList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms); }
		}

		CodeDescriptionPairList InternationalPaymentTermsList
		{
			get
			{
				return Factory.GetCachedValue("RateOneOffShipmentLookups.InternationalPaymentTermsList", () => new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms));
			}
		}

		#endregion

		#region Cartage Equipment

		public CodeDescriptionPairList Equipments
		{
			get
			{
				if (Parent.TT_ContainerMode == Core.Constants.ContainerModes.FCL || Parent.TT_ContainerMode == Core.Constants.RateMode.SEA)
				{
					return Factory.GetCachedValue<CodeDescriptionPairList>("Equipment_FCL", delegate
					{ return new FCLEquipmentNeededList(false); });
				}
				else
				{
					return Factory.GetCachedValue<CodeDescriptionPairList>("Equipment_LCL", delegate
					{ return new LCLAIREquipmentNeededList(false); });
				}
			}
		}

		#endregion

		#region Transit Times

		public CodeDescriptionPairList AirTransitTimes
		{
			get { return RateEntryLookups.AirTransitTimes; }
		}

		public CodeDescriptionPairList SeaTransitTimes
		{
			get { return RateEntryLookups.SeaTransitTimes; }
		}

		#endregion

		#region Frequency Units

		public CodeDescriptionPairList FrequencyUnits
		{
			get { return RateEntryLookups.FrequencyUnits; }
		}

		#endregion

		#region Implementation

		RateEntryLookups RateEntryLookups
		{
			get { return new RateEntryLookups(Factory.GetNull<RateEntry>()); }
		}

		#region One Off Quote Statistics Registry Configuration Lists

		public OneOffQuoteKPIList OneOffQuoteKPIList
		{
			get { return Factory.GetCachedValue<OneOffQuoteKPIList>(); }
		}

		public OneOffQuoteSourceList OneOffQuoteSourceList
		{
			get { return Factory.GetCachedValue<OneOffQuoteSourceList>(); }
		}

		public OneOffQuoteRevisionReasonList OneOffQuoteRevisionReasonList
		{
			get { return Factory.GetCachedValue<OneOffQuoteRevisionReasonList>(); }
		}

		#endregion

		#endregion

	}
}

