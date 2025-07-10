using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public partial class AsycudaManifestHeaderValidation
	{
		public void ValidatePlaceOfEntry()
		{
			ValidateCalculatedProperty(Parent.PlaceOfEntryInfo);
		}

		protected virtual void CheckPlaceOfEntry()
		{
			if (Parent.PlaceOfEntry.IsEmpty)
			{
				if (Parent.IsRoad && Parent.AMA_Nature == ShipmentTypeList.Codes.Transhipment28)
				{
					Parent.PlaceOfEntryInfo.AddMessageError(ValidationConstants.PlaceOfEntryIsCompulsoryForRoadTranshipmentManifest);
				}
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.PlaceOfEntryInfo);
			}
		}

		public void ValidateEstimatedTimeOfLoading()
		{
			ValidateCalculatedProperty(Parent.EstimatedTimeOfLoadingInfo);
		}

		protected virtual void CheckEstimatedTimeOfLoading()
		{
			if (Parent.EstimatedTimeOfLoading.IsEmpty
				&& Parent.AMA_ManifestType == nameof(ManifestDocumentType.ALH))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.EstimatedTimeOfLoadingInfo);
			}
		}

		public void ValidatePlaceOfExit()
		{
			ValidateCalculatedProperty(Parent.PlaceOfExitInfo);
		}

		protected virtual void CheckPlaceOfExit()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.PlaceOfExitInfo);
		}

		public void ValidateTSS_Vessel()
		{
			ValidateCalculatedProperty(Parent.TSS_VesselInfo);
		}

		protected virtual void CheckTSS_Vessel()
		{
			if (Parent.IsTSS && Parent.IsTSSSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSS_VesselInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.TSS_VesselInfo, Parent.Lookups.TSSRefVessels);
			}
		}

		public void ValidateTSS_VoyageFlight()
		{
			ValidateCalculatedProperty(Parent.TSS_VoyageFlightInfo);
		}

		protected virtual void CheckTSS_VoyageFlight()
		{
			if (Parent.IsTSS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSS_VoyageFlightInfo);
			}
		}

		public void ValidateTSS_RadioCallSign()
		{
			ValidateCalculatedProperty(Parent.TSS_RadioCallSignInfo);
		}

		protected virtual void CheckTSS_RadioCallSign()
		{
			if (Parent.IsTSS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSS_RadioCallSignInfo);
			}
		}

		public void ValidateTSS_CargoCarrierPK()
		{
			ValidateCalculatedProperty(Parent.TSS_CargoCarrierPKInfo);
		}

		protected virtual void CheckTSS_CargoCarrierPK()
		{
			if (Parent.IsTSS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSS_CargoCarrierPKInfo);

				ListValidation.ErrorIfInvalidPK(Parent.TSS_CargoCarrierPKInfo, Parent.Lookups.TSSCarrierList, ResString.GetMultilingualString("1EFCACF2-E34B-449D-9038-F19AE49EFC6A", InvalidTranshipmentCarrier));

				var tranportCarrierCode = Parent.TSS_CargoCarrier?.LocalCustomsCarrierCode ?? ZString.Empty;
				if (tranportCarrierCode.IsEmpty)
				{
					Parent.TSS_CargoCarrierPKInfo.AddMessageError(TranshipmentCarrierMissingCCC);
				}
			}
		}

		internal const string InvalidTranshipmentCarrier = "Transhipment carrier captured is not flagged as an Airline/Shipping line, please select a different organization or use F3 to amend";
		internal const string TranshipmentCarrierMissingCCC = "Transhipment carrier captured does not have a ‘CCC’ Registration number where the Country/Region of issue is ZA, please select a different organization or use F3 to amend";

		public void ValidateTSS_DateOfDeparture()
		{
			ValidateCalculatedProperty(Parent.TSS_DateOfDepartureInfo);
		}

		protected virtual void CheckTSS_DateOfDeparture()
		{
			if (Parent.IsTSS)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSS_DateOfDepartureInfo);
			}
		}

		public void ValidateCallPurposeCode()
		{
			ValidateCalculatedProperty(Parent.CallPurposeCodeInfo);
		}

		protected virtual void CheckCallPurposeCode()
		{
			if (Parent.CallPurposeCode.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CallPurposeCodeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CallPurposeCodeInfo);
			}
		}
	}
}
