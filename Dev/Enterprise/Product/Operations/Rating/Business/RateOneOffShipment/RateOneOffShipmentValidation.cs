namespace Enterprise.Rating.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;

	public class RateOneOffShipmentValidation : AutoRateOneOffShipmentValidation
	{
		public RateOneOffShipmentValidation(AutoRateOneOffShipment parent)
			: base(parent) { }

		public new RateOneOffShipment Parent
		{
			get { return (RateOneOffShipment)base.Parent; }
		}

		#region TT_ActualVolume

		protected override void CheckTT_ActualVolume()
		{
			base.CheckTT_ActualVolume();
			if (Parent.TT_ActualVolume < 0)
			{
				Parent.TT_ActualVolumeInfo.AddError(Res.GetString("8cadac85-4d87-4086-a5e1-f932aca0db55", "You must specify a volume greater than 0."));
			}

			if (Parent.TT_ContainerMode == Core.Constants.RateMode.LSE ||
				Parent.TT_ContainerMode == Core.Constants.RateMode.LCL)
			{
				if (Parent.TT_ActualVolume.IsEmpty && Parent.TT_ActualWeight.IsEmpty)
				{
					Parent.TT_ActualVolumeInfo.AddError(Res.GetString("33474789-1EDE-4CBA-A2F0-A9EC54AB86E5", "You must specify either a weight or volume if your shipment is LSE or LCL."));
				}
			}

			if (!Parent.TotalLooseVolume.IsEmpty && Parent.TotalLooseVolume != Parent.TT_ActualVolume)
			{
				Parent.TT_ActualVolumeInfo.AddError(Res.GetString("089c4f63-3e6e-449e-bca2-7d4abcc716b3", "The volume you have specified does not match the details specified for Loose Cargo. Please check your calculations."));
			}
		}

		#endregion

		#region TT_ActualWeight

		protected override void CheckTT_ActualWeight()
		{
			base.CheckTT_ActualWeight();
			if (Parent.TT_ActualWeight < 0)
			{
				Parent.TT_ActualWeightInfo.AddError(Res.GetString("a0842f91-894c-45de-b9ce-97bf67ba805d", "You must specify a weight greater than 0."));
			}

			if (Parent.TT_ContainerMode == Core.Constants.RateMode.LSE ||
				Parent.TT_ContainerMode == Core.Constants.RateMode.LCL)
			{
				if (Parent.TT_ActualVolume.IsEmpty && Parent.TT_ActualWeight.IsEmpty)
				{
					Parent.TT_ActualWeightInfo.AddError(Res.GetString("33474789-1EDE-4CBA-A2F0-A9EC54AB86E5", "You must specify either a weight or volume if your shipment is LSE or LCL."));
				}
			}

			if (!Parent.TotalLooseWeight.IsEmpty && Parent.TotalLooseWeight != Parent.TT_ActualWeight)
			{
				Parent.TT_ActualWeightInfo.AddError(Res.GetString("46e892e3-15ff-42d2-888e-e06cc00696e5", "The Weight you have specified does not match the details specified for Loose Cargo. Please check your calculations."));
			}
		}

		#endregion

		#region ValidateConsignorPKAndConsigneePK

		public void ValidateConsignorPK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsValid)
			{
				var organization = validation.Parent.Factory.Load<OrgHeader>(validation.Parent.OrganisationPK);

				if (organization != null && !organization.OH_IsConsignor)
				{
					validation.Parent.OrganisationPKInfo.AddError(IsNotValidConsignorErrorMessage);
				}
			}
		}

		public void ValidateConsigneePK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsValid)
			{
				var organization = validation.Parent.Factory.Load<OrgHeader>(validation.Parent.OrganisationPK);

				if (organization != null && !organization.OH_IsConsignee)
				{
					validation.Parent.OrganisationPKInfo.AddError(IsNotValidConsigneeErrorMessage);
				}
			}
		}

		string IsNotValidConsignorErrorMessage
		{
			get { return Res.GetString("4605d1fb-2f49-497d-8ee8-87273bad0c81", "This organization is not a valid Consignor"); }
		}

		string IsNotValidConsigneeErrorMessage
		{
			get { return Res.GetString("ca646f74-7f9b-4d06-84b0-ee787033d39e", "This organization is not a valid Consignee"); }
		}

		#endregion

		#region TT_UnitOfVolume

		protected override void CheckTT_UnitOfVolume()
		{
			base.CheckTT_UnitOfVolume();

			if (Parent.TT_UnitOfVolume.IsEmpty)
			{
				if (!Parent.TT_ActualVolume.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.TT_UnitOfVolumeInfo);
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.TT_UnitOfVolumeInfo, Parent.Lookups.UnitOfVolumeList);
			}
		}

		#endregion

		#region TT_UnitOfWeight

		protected override void CheckTT_UnitOfWeight()
		{
			base.CheckTT_UnitOfWeight();

			if (Parent.TT_UnitOfWeight.IsEmpty)
			{
				if (!Parent.TT_ActualWeight.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.TT_UnitOfWeightInfo);
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.TT_UnitOfWeightInfo, Parent.Lookups.UnitOfWeightList);
			}
		}

		#endregion

		#region TT_TransportMode

		protected override void CheckTT_TransportMode()
		{
			base.CheckTT_TransportMode();
			if (!(Parent.ParentQuote != null && Parent.ParentQuote.IsInComparisonMode))
			{
				MandatoryValidation.CheckEntered(Parent.TT_TransportModeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.TT_TransportModeInfo, Parent.Lookups.TransportModes);
		}

		#endregion

		#region TT_ContainerMode

		protected override void CheckTT_ContainerMode()
		{
			base.CheckTT_ContainerMode();
			if (!(Parent.ParentQuote != null && Parent.ParentQuote.IsInComparisonMode))
			{
				MandatoryValidation.CheckEntered(Parent.TT_ContainerModeInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.TT_ContainerModeInfo, Parent.Lookups.ContainerModes);
		}

		#endregion

		#region TT_HBLDeliveryMode

		protected override void CheckTT_HBLDeliveryMode()
		{
			base.CheckTT_HBLDeliveryMode();
			ListValidation.ErrorIfInvalidCode(Parent.TT_HBLDeliveryModeInfo, Parent.Lookups.HBLDeliveryModesList);
		}

		#endregion

		#region TT_RL_NKDeliveryLocation

		protected override void CheckTT_RL_NKDeliveryLocation()
		{
			base.CheckTT_RL_NKDeliveryLocation();
			MandatoryValidation.CheckEntered(Parent.TT_RL_NKDeliveryLocationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TT_RL_NKDeliveryLocationInfo, Parent.Lookups.DeliveryLocations);
		}

		#endregion

		#region TT_RL_NKReceivalLocation

		protected override void CheckTT_RL_NKReceivalLocation()
		{
			base.CheckTT_RL_NKReceivalLocation();
			MandatoryValidation.CheckEntered(Parent.TT_RL_NKReceivalLocationInfo);
			ListValidation.ErrorIfInvalidCode(Parent.TT_RL_NKReceivalLocationInfo, Parent.Lookups.ReceivalLocations);
		}

		#endregion

		#region TT_RL_NKViaLocation

		protected override void CheckTT_RL_NKViaLocation()
		{
			base.CheckTT_RL_NKViaLocation();
			ListValidation.ErrorIfInvalidCode(Parent.TT_RL_NKViaLocationInfo, Parent.Lookups.ViaLocations);
		}

		#endregion

		#region TT_RH_NKCommodity

		protected override void CheckTT_RH_NKCommodity()
		{
			base.CheckTT_RH_NKCommodity();
			ListValidation.ErrorIfInvalidCode(Parent.TT_RH_NKCommodityInfo, Parent.Lookups.Commodities);
		}

		#endregion

		#region TT_RS_NKServiceLevel

		protected override void CheckTT_RS_NKServiceLevel()
		{
			base.CheckTT_RS_NKServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.TT_RS_NKServiceLevelInfo, Parent.Lookups.ServiceLevels);
		}

		#endregion

		#region TT_RX_NKGoodsCurrency

		protected override void CheckTT_RX_NKGoodsCurrency()
		{
			base.CheckTT_RX_NKGoodsCurrency();
			if (Parent.TT_ValueOfGoods > 0)
			{
				MandatoryValidation.CheckEntered(Parent.TT_RX_NKGoodsCurrencyInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.TT_RX_NKGoodsCurrencyInfo, Parent.Lookups.GoodsCurrencies);
		}

		#endregion

		#region TT_OH_Carrier

		protected override void CheckTT_OH_Carrier()
		{
			base.CheckTT_OH_Carrier();
			ListValidation.ErrorIfInvalidPK(Parent.TT_OH_CarrierInfo, Parent.Lookups.Carriers);
		}

		#endregion

		#region TT_IncoTerm

		protected override void CheckTT_IncoTerm()
		{
			base.CheckTT_IncoTerm();
			ListValidation.ErrorIfInvalidCode(Parent.TT_IncoTermInfo, Parent.Lookups.IncoTerms);
		}

		#endregion

		#region TT_PickupEquipment

		protected override void CheckTT_PickupEquipment()
		{
			base.CheckTT_PickupEquipment();
			ListValidation.ErrorIfInvalidCode(Parent.TT_PickupEquipmentInfo, Parent.Lookups.Equipments);
		}

		#endregion

		#region TT_DeliveryEquipment

		protected override void CheckTT_DeliveryEquipment()
		{
			base.CheckTT_DeliveryEquipment();
			ListValidation.ErrorIfInvalidCode(Parent.TT_DeliveryEquipmentInfo, Parent.Lookups.Equipments);
		}

		#endregion

		#region TT_NumberOfEntryLines

		protected override void CheckTT_NumberOfEntryLines()
		{
			base.CheckTT_NumberOfEntryLines();
			MandatoryValidation.CheckNotNegative(Parent.TT_NumberOfEntryLinesInfo);
		}

		#endregion

		#region TT_NumberOfEntries

		protected override void CheckTT_NumberOfEntries()
		{
			base.CheckTT_NumberOfEntries();
			MandatoryValidation.CheckNotNegative(Parent.TT_NumberOfEntriesInfo);
		}

		#endregion

		#region TT_QuoteKPI

		protected override void CheckTT_QuoteKPI()
		{
			base.CheckTT_QuoteKPI();
			ListValidation.ErrorIfInvalidCode(Parent.TT_QuoteKPIInfo, new OneOffQuoteKPIList());
		}

		#endregion

		#region TT_QuoteSource

		protected override void CheckTT_QuoteSource()
		{
			base.CheckTT_QuoteSource();
			ListValidation.ErrorIfInvalidCode(Parent.TT_QuoteSourceInfo, new OneOffQuoteSourceList());
		}

		#endregion

		#region TT_RevisionReason

		protected override void CheckTT_RevisionReason()
		{
			base.CheckTT_RevisionReason();
			ListValidation.ErrorIfInvalidCode(Parent.TT_RevisionReasonInfo, new OneOffQuoteRevisionReasonList());
		}

		#endregion

		protected override void CheckTT_CompanyTariffLevelOverride()
		{
			base.CheckTT_CompanyTariffLevelOverride();
			ListValidation.ErrorIfInvalidCode(Parent.TT_CompanyTariffLevelOverrideInfo, Parent.Lookups.CompanyTariffLevelList);
		}

		#region Calculated

		#region IsDomestic

		public void ValidateIsDomesticFreight()
		{
			ValidateCalculatedProperty(Parent.IsDomesticFreightInfo);
		}

		protected void CheckIsDomesticFreight()
		{
			if (Parent.IsDomesticFreight && !Parent.IsDomestic() && !Parent.IsDomesticFreightInfo.HasErrors())
			{
				Parent.IsDomesticFreightInfo.AddError(Res.GetString("dbbf74ea-370b-48b6-81db-2957e3e1bff9", "You have marked this quote as Domestic Freight, but the origin and destination are not in the same country/region."));
			}
		}

		#endregion

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateIsDomesticFreight();
		}

		#endregion
	}
}

