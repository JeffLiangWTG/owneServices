
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business
{
	public abstract class AddInfo : AutoSGAddInfo
	{
		#region Constructor

		protected AddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			this.Parent = addInfoProperty.BizObj;
			this.AddInfoProperty = addInfoProperty;
			LoadPropertiesFromAddInfoProperty(false);
			isInitialised = true;
		}

		#endregion

		#region Implementation

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				if (!Parent.IsSettingHasChangesSuspended)
				{
					base.HasChanges = value;
					if (base.HasChanges && !Parent.IsMarkingAsNeedingValidationSuspended)
					{
						Parent.MarkAsNeedingValidation();
					}
				}
			}
		}

		protected override string GetDBName(string propertyName)
		{
			return GetDBNameMapped(propertyName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static string GetDBNameMapped(string propertyName)
		{
			switch (propertyName)
			{
				case SGAddInfoSchema.Constants.SG_AdditionalRecipientID1:
					return "SG1";
				case SGAddInfoSchema.Constants.SG_AdditionalRecipientID2:
					return "SG2";
				case SGAddInfoSchema.Constants.SG_AdditionalRecipientID3:
					return "SG3";
				case SGAddInfoSchema.Constants.SG_ApplicationProductType:
					return "SG4";
				case SGAddInfoSchema.Constants.SG_CertAdditionalInformation:
					return "SG6";
				case SGAddInfoSchema.Constants.SG_CertItemValue:
					return "SG8";
				case SGAddInfoSchema.Constants.SG_CertItemQuantity:
					return "SG9";
				case SGAddInfoSchema.Constants.SG_CertItemQuantityUnit:
					return "SG10";
				case SGAddInfoSchema.Constants.SG_CertOriginCriterion1:
					return "SG11";
				case SGAddInfoSchema.Constants.SG_CertOriginCriterion2:
					return "SG12";
				case SGAddInfoSchema.Constants.SG_CertOriginCriterion3:
					return "SG13";

				case SGAddInfoSchema.Constants.SG_Cert1Type:
					return "SG14";
				case SGAddInfoSchema.Constants.SG_Cert2Type:
					return "SG15";
				case SGAddInfoSchema.Constants.SG_Cert1CopiesNo:
					return "SG16";
				case SGAddInfoSchema.Constants.SG_Cert2CopiesNo:
					return "SG17";
				case SGAddInfoSchema.Constants.SG_Cert1PercCommContent:
					return "SG18";
				case SGAddInfoSchema.Constants.SG_CertSendInvDetails:
					return "SG19";
				case SGAddInfoSchema.Constants.SG_RX_NKCertReferenceCurrency:
					return "SG20";
				case SGAddInfoSchema.Constants.SG_Cert1AdditionalDetails:
					return "SG22";
				case SGAddInfoSchema.Constants.SG_Cert1TransportDetails:
					return "SG24";
				case SGAddInfoSchema.Constants.SG_ClaimantCode:
					return "SG26";
				case SGAddInfoSchema.Constants.SG_ClaimantName:
					return "SG27";
				case SGAddInfoSchema.Constants.SG_DutyPercentageRate:
					return "SG28";
				case SGAddInfoSchema.Constants.SG_DutyUnitRate:
					return "SG29";
				case SGAddInfoSchema.Constants.SG_EndDateTempImport:
					return "SG30";
				case SGAddInfoSchema.Constants.SG_EndUseDescription:
					return "SG31";
				case SGAddInfoSchema.Constants.SG_EngineCapacityPower:
					return "SG32";
				case SGAddInfoSchema.Constants.SG_EngineCapacityPowerUnit:
					return "SG33";
				case SGAddInfoSchema.Constants.SG_EntryYear:
					return "SG34";
				case SGAddInfoSchema.Constants.SG_ESNDPIndicator:
					return "SG35";
				case SGAddInfoSchema.Constants.SG_ExcisePercentageRate:
					return "SG36";
				case SGAddInfoSchema.Constants.SG_ExciseUnitRate:
					return "SG37";
				case SGAddInfoSchema.Constants.SG_GoodsImportedUnderMESorBWS:
					return "SG38";
				case SGAddInfoSchema.Constants.SG_GS_Declarant:
					return "SG39";
				case SGAddInfoSchema.Constants.SG_GSTRate:
					return "SG40";
				case SGAddInfoSchema.Constants.SG_InPackQuantity:
					return "SG41";
				case SGAddInfoSchema.Constants.SG_InPackQuantityUnit:
					return "SG42";
				case SGAddInfoSchema.Constants.SG_InmostPackQuantity:
					return "SG43";
				case SGAddInfoSchema.Constants.SG_InmostPackQuantityUnit:
					return "SG44";
				case SGAddInfoSchema.Constants.SG_InnerPackQuantity:
					return "SG45";
				case SGAddInfoSchema.Constants.SG_InnerPackQuantityUnit:
					return "SG46";
				case SGAddInfoSchema.Constants.SG_InwardHAWB:
					return "SG47";
				case SGAddInfoSchema.Constants.SG_InwardMAWB:
					return "SG48";
				case SGAddInfoSchema.Constants.SG_LastSellingPrice:
					return "SG49";
				case SGAddInfoSchema.Constants.SG_LotNo:
					return "SG50";
				case SGAddInfoSchema.Constants.SG_ManufacturingCostStatementDate:
					return "SG51";
				case SGAddInfoSchema.Constants.SG_OuterPackQuantity:
					return "SG60";
				case SGAddInfoSchema.Constants.SG_OuterPackQuantityUnit:
					return "SG61";
				case SGAddInfoSchema.Constants.SG_OutwardHAWB:
					return "SG62";
				case SGAddInfoSchema.Constants.SG_OutwardMAWB:
					return "SG63";
				case SGAddInfoSchema.Constants.SG_OutwardTransportMode:
					return "SG64";
				case SGAddInfoSchema.Constants.SG_OutwardVoyageFlightNo:
					return "SG65";
				case SGAddInfoSchema.Constants.SG_PercAlcohol:
					return "SG67";
				case SGAddInfoSchema.Constants.SG_PercContent:
					return "SG68";
				case SGAddInfoSchema.Constants.SG_PreviousEntryStatus:
					return "SG69";
				case SGAddInfoSchema.Constants.SG_PreviousLotNo:
					return "SG70";
				case SGAddInfoSchema.Constants.SG_PreviousPermitNo:
					return "SG71";
				case SGAddInfoSchema.Constants.SG_FirstRegistrationDate:
					return "SG72";
				case SGAddInfoSchema.Constants.SG_RefundForItemCustomsDutyAmount:
					return "SG73";
				case SGAddInfoSchema.Constants.SG_RefundForItemExciseAmount:
					return "SG74";
				case SGAddInfoSchema.Constants.SG_RefundForItemGSTAmount:
					return "SG75";
				case SGAddInfoSchema.Constants.SG_RemovalStartDate:
					return "SG76";
				case SGAddInfoSchema.Constants.SG_ReplacementPermitNo:
					return "SG77";
				case SGAddInfoSchema.Constants.SG_RL_NKNextPortOfCall:
					return "SG78";
				case SGAddInfoSchema.Constants.SG_RL_NKFinalPortOfCall:
					return "SG79";
				case SGAddInfoSchema.Constants.SG_RN_NKDonorCountry:
					return "SG80";
				case SGAddInfoSchema.Constants.SG_RN_NKFinalDestination:
					return "SG81";
				case SGAddInfoSchema.Constants.SG_OutwardVesselName:
					return "SG82";
				case SGAddInfoSchema.Constants.SG_TowingVesselName:
					return "SG83";
				case SGAddInfoSchema.Constants.SG_SupplyIndicator:
					return "SG84";
				case SGAddInfoSchema.Constants.SG_TextileCatCode:
					return "SG85";
				case SGAddInfoSchema.Constants.SG_TextileQuotaQuantity:
					return "SG86";
				case SGAddInfoSchema.Constants.SG_TextileQuotaQuantityUnit:
					return "SG87";
				case SGAddInfoSchema.Constants.SG_TobaccoMultiplier:
					return "SG88";
				case SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTY:
					return "SG89";
				case SGAddInfoSchema.Constants.SG_TotalDutiableWGTVOLQTYUnit:
					return "SG90";
				case SGAddInfoSchema.Constants.SG_TowingVoyageNumber:
					return "SG91";
				case SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTY:
					return "SG92";
				case SGAddInfoSchema.Constants.SG_UnitDutiableWGTVOLQTYUnit:
					return "SG93";
				case SGAddInfoSchema.Constants.SG_US_NKInwardVesselBerth:
					return "SG94";
				case SGAddInfoSchema.Constants.SG_US_NKOutwardVesselBerth:
					return "SG995";
				case SGAddInfoSchema.Constants.SG_US_NKPlaceOfCargoRelease:
					return "SG96";
				case SGAddInfoSchema.Constants.SG_US_NKPlaceOfReceipt:
					return "SG97";
				case SGAddInfoSchema.Constants.SG_US_NKPlaceOfStorage:
					return "SG98";
				case SGAddInfoSchema.Constants.SG_VehicleRegistrationNumber:
					return "SG99";
				case SGAddInfoSchema.Constants.SG_IsSeaStore:
					return "SG100";
				case SGAddInfoSchema.Constants.SG_TariffCommodityType:
					return "SG102";
				case SGAddInfoSchema.Constants.SG_ImporterNameOverride:
					return "SG103";
				case SGAddInfoSchema.Constants.SG_CategoryCode:
					return "SG104";
				case SGAddInfoSchema.Constants.SG_EndUseCode1:
					return "SG105";
				case SGAddInfoSchema.Constants.SG_EndUseCode2:
					return "SG106";
				case SGAddInfoSchema.Constants.SG_EndUseCode3:
					return "SG107";
				case SGAddInfoSchema.Constants.SG_NoOfCrew:
					return "SG108";
				case SGAddInfoSchema.Constants.SG_VoyageDuration:
					return "SG109";
				case SGAddInfoSchema.Constants.SG_IsInwardHandCarried:
					return "SG110";
				case SGAddInfoSchema.Constants.SG_IsOutwardHandCarried:
					return "SG111";
				case SGAddInfoSchema.Constants.SG_StrategicGoodsCategory:
					return "SG112";
				case SGAddInfoSchema.Constants.SG_OutwardFolio:
					return "SG113";
				case SGAddInfoSchema.Constants.SG_StrategicGoodsProductCodeQuantity:
					return "SG114";
				case SGAddInfoSchema.Constants.SG_StrategicGoodsProductCodeQuantityUnit:
					return "SG115";
			}
			return propertyName;
		}

		#endregion
	}
}
