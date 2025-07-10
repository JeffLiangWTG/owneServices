using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentValidation : CommonShipmentValidation
	{
		public AgencyShipmentValidation(AgencyShipment parent)
			: base(parent) { }

		protected override void CheckJS_INCO()
		{
			ListValidation.ErrorIfInvalidCode(Parent.JS_INCOInfo, Parent.Lookups.JS_INCO_List, ResString.GetMultilingualString("11cc652c-c67f-4266-9241-7794bfc9349e", "Payment Term"));
		}

		protected override void CheckJS_OH_DeliveryAgent()
		{
			TypeValidation.CheckValidGuid(Parent.JS_OH_DeliveryAgentInfo, Res.GetString("4e431ee7-45d2-46be-a4f9-39119b20e7cf", "Principal"));
			if (Parent.JS_ShipmentStatus != ShipmentStatusList.Codes.WebBooking)
			{
				MandatoryValidation.CheckEntered(Parent.JS_OH_DeliveryAgentInfo, Res.GetString("9cdcc224-32ca-4020-94f8-9e10df002956", "Principal"));
			}

			if (Parent.Principal != null && !Parent.Lookups.Principal_List.IsValidPrincipal(Parent.Principal))
			{
				Parent.JS_OH_DeliveryAgentInfo.AddError(Res.GetString("7b10a20e-4ff2-47b3-930c-96db032a7540", "Enter a valid Principal"));
			}

			ListValidation.ErrorIfInvalidPK(Parent.JS_OH_DeliveryAgentInfo);
		}

		protected override void CheckJS_A_BKD()
		{
			base.CheckJS_A_BKD();
			if (!Parent.JS_Calc_CurrentETD.IsEmpty && Parent.JS_A_BKD > Parent.JS_Calc_CurrentETD)
			{
				Parent.JS_A_BKDInfo.AddWarning(Res.GetString("481e4d20-027f-4341-b605-11d96e13d177", "This Sailing has departed."));
			}
		}

		protected override void CheckJS_UnitFreightRate()
		{
			base.CheckJS_UnitFreightRate();
			if (!Parent.JS_RX_NKFrtRateCurrencyInfo.HasErrors())
			{
				MandatoryValidation.CheckUnitEntered(Parent.JS_RX_NKFrtRateCurrencyInfo, Parent.JS_UnitFreightRateInfo);
			}
		}

		protected override void CheckJS_JX()
		{
			base.CheckJS_JX();
			MandatoryValidation.WarnIfNotEntered(Parent.JS_JXInfo);

			if (NoSailingsForEnteredPorts)
			{
				ShouldAddWarningToDichargePortAfterSailingValidation = true;
				ValidateJS_NKDischargePort();
			}
		}

		protected override void CheckJS_RL_NKOrigin()
		{
			base.CheckJS_RL_NKOrigin();
			MandatoryValidation.CheckEntered(Parent.JS_RL_NKOriginInfo);
		}

		protected override void CheckJS_RL_NKDestination()
		{
			base.CheckJS_RL_NKDestination();
			MandatoryValidation.CheckEntered(Parent.JS_RL_NKDestinationInfo);
		}

		protected override void CheckJS_GoodsDescription()
		{
			base.CheckJS_GoodsDescription();
			MandatoryValidation.CheckEntered(Parent.JS_GoodsDescriptionInfo);
		}

		protected override void CheckJS_ShipmentStatus()
		{
			base.CheckJS_ShipmentStatus();

			MandatoryValidation.CheckEntered(Parent.JS_ShipmentStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_ShipmentStatusInfo, Parent.Lookups.JS_ShipmentStatus_List);
		}

		public void ValidateJS_NKDischargePort()
		{
			ValidateCalculatedProperty(Parent.JS_NKDischargePortInfo);
		}

		protected virtual void CheckJS_NKDischargePort()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.JS_NKDischargePortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_NKDischargePortInfo, Parent.Lookups.RefUNLOCO_List);

			if (!Parent.JS_NKDischargePort.IsEmpty)
			{
				if (Parent.JS_NKLoadPort == Parent.JS_NKDischargePort)
				{
					Parent.JS_NKDischargePortInfo.AddError(Res.GetString("a40ee4a3-423c-41bb-ad20-bdd442ab057f", "Discharge and Load Ports cannot be the same."));
				}

				if (Parent.CalcDischargePort != null && !Parent.CalcDischargePort.RL_HasSeaport)
				{
					Parent.JS_NKDischargePortInfo.AddWarning(Res.GetString("51a7bf3e-a9ef-44a5-b213-273a0a8507aa", "{0} does not have a sea port.", Parent.JS_NKDischargePort));
				}
			}

			if (ShouldAddWarningToDichargePortAfterSailingValidation)
			{
				Parent.JS_NKDischargePortInfo.AddWarning(Res.GetString("d11f6116-4fb3-4930-a900-218bfe30b70e", "There are no Sailings for the entered Load and Discharge ports."));
				ShouldAddWarningToDichargePortAfterSailingValidation = false;
			}
		}

		public void ValidateJS_NKLoadPort()
		{
			ValidateCalculatedProperty(Parent.JS_NKLoadPortInfo);
		}

		protected virtual void CheckJS_NKLoadPort()
		{
			MandatoryValidation.WarnIfNotEntered(Parent.JS_NKLoadPortInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_NKLoadPortInfo, Parent.Lookups.RefUNLOCO_List);

			if (!Parent.JS_NKLoadPort.IsEmpty)
			{
				if (Parent.JS_NKLoadPort == Parent.JS_NKDischargePort)
				{
					Parent.JS_NKLoadPortInfo.AddError(Res.GetString("7f3a9521-fbdb-469f-94be-252f3a65b757", "Load and Discharge Ports cannot be the same."));
				}

				if (Parent.CalcLoadPort != null && !Parent.CalcLoadPort.RL_HasSeaport)
				{
					Parent.JS_NKLoadPortInfo.AddWarning(Res.GetString("d4dc5d56-5a67-4db3-a2ef-294ba249cf32", "{0} does not have a sea port.", Parent.JS_NKLoadPort));
				}
			}
		}

		#region Weight / Volume / Packs

		protected override bool ShouldCheckMeasuresAgainstPacklinesTotals
		{
			get { return !Parent.IsTopLevelPacksMode && base.ShouldCheckMeasuresAgainstPacklinesTotals; }
		}

		protected override void CheckJS_ActualWeight()
		{
			base.CheckJS_ActualWeight();

			if (!Parent.JS_ActualWeightInfo.HasNotifications()
				&& Parent.IsTopLevelPacksMode
				&& Parent.JS_ActualWeightReadOnly != Parent.TopLevelPacks.TotalWeightInShipmentWeightUnit)
			{
				Parent.JS_ActualWeightInfo.AddWarning(Res.GetString("f8fd0faf-74f3-4a98-ae82-cb785a043083", "Entered weight does not match total weight of the vehicles/packs."));
			}
		}

		protected override void CheckJS_ActualVolume()
		{
			base.CheckJS_ActualVolume();

			if (!Parent.JS_ActualVolumeInfo.HasNotifications()
				&& Parent.IsTopLevelPacksMode
				&& Parent.JS_ActualVolumeReadOnly != Parent.TopLevelPacks.TotalVolumeInShipmentVolumeUnit)
			{
				Parent.JS_ActualVolumeInfo.AddWarning(Res.GetString("a4ba2028-fb8c-4813-b3a2-434f474deb12", "Entered volume does not match total volume of the vehicles/packs."));
			}
		}

		protected override void CheckJS_OuterPacks()
		{
			base.CheckJS_OuterPacks();

			if (!Parent.JS_OuterPacksInfo.HasNotifications()
				&& Parent.IsTopLevelPacksMode
				&& Parent.JS_OuterPacks != Parent.TopLevelPacks.TotalContainers)
			{
				Parent.JS_OuterPacksInfo.AddWarning(Res.GetString("901a59ed-6999-4370-885b-7a26b7e1585b", "Entered number of packs does not match total number in vehicles/packs."));
			}
		}

		#endregion

		protected override ZQuery GetHouseBillDuplicateCheckAdditionalConditions()
		{
			var shipmentTypeQuery = new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, false);
			shipmentTypeQuery.AddToFilter(JoinCondition.And, JobShipmentSchema.JS_IsCFSRegistered, false);
			return shipmentTypeQuery;
		}

		#region ValidateVoyageVesselForBinding

		public void ValidateVoyageVesselForBinding()
		{
			ValidateCalculatedProperty(Parent.VoyageVesselForBindingInfo);
		}

		protected virtual void CheckVoyageVesselForBinding()
		{
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateJS_NKDischargePort();
			ValidateJS_NKLoadPort();
			ValidateVoyageVesselForBinding();
		}

		#endregion

		#region Implementation

		bool NoSailingsForEnteredPorts
		{
			get { return Parent.JS_JXInfo.HasWarnings() && !Parent.JS_NKDischargePortInfo.HasWarnings() && !Parent.JS_NKDischargePort.IsEmpty; }
		}

		bool ShouldAddWarningToDichargePortAfterSailingValidation { get; set; }

		public new AgencyShipment Parent
		{
			get { return (AgencyShipment)base.Parent; }
		}

		#endregion
	}
}



