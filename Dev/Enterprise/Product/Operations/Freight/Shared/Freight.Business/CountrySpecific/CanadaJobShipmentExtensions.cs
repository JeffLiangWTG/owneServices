using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public static class CanadaJobShipmentExtensions
	{
		internal static bool ReferenceNumbersContainsCCN(CommonShipment shipment)
		{
			if (shipment.IsInDatabase)
			{
				shipment.Numbers.Reload(false);
			}
			return shipment.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN && n.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
		}

		public static ZString GetCarrierCode(this CommonShipment shipment)
		{
			var carrier = ZString.Empty;

			if (shipment == null)
			{
				ErrorReporter.ReportOnce("CanadaJobShipmentExtensions_GetCarrierCode", $"shipment is null. CurrentCompay = {GlbCompany.CurrentCompany?.GC_RN_NKCountryCode}");
				return carrier;
			}

			if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.Canada && shipment.IsDestinationToCanada())
			{
				carrier = GetCarrierCodeCore(shipment.ArrivalConsol?.ReceivingForwarder);
			}

			if (carrier.IsEmpty)
			{
				var currentBranch = GlbBranch.GetCurrentBranch(shipment.Factory);
				carrier = GetCarrierCodeCore(currentBranch?.OrgProxy);

				if (carrier.IsEmpty)
				{
					var currentCompany = GlbCompany.GetCurrentCompany(shipment.Factory);
					carrier = GetCarrierCodeCore(currentCompany?.OrgProxy);
				}
			}

			return carrier;
		}

		static ZString GetCarrierCodeCore(OrgHeader orgHeader)
		{
			var result = ZString.Empty;
			if (orgHeader != null)
			{
				OrgCusCode orgCustomCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.CarrierCode, Core.Constants.CountryCodes.Canada);
				if (orgCustomCode != null)
				{
					result = orgCustomCode.OK_CustomsRegNo;
				}
			}
			return result;
		}

		public static bool SetCanadaCargoControlNumberIfNotExist(this CommonShipment shipment)
		{
			var cargoControlNumberIsSet = false;
			ZString carrierCode = shipment.GetCarrierCode();
			if (!ReferenceNumbersContainsCCN(shipment) && !carrierCode.IsEmpty && !shipment.IsDeleted && (
				(shipment.IsDestinationToCanada() && !(shipment.MostInterestingDepartureConsol?.MostInterestingTransportForBinding?.FirstOrDefault() is Transport transportFromConsol && transportFromConsol.IsDomestic))
				|| shipment.Consols.OfType<CommonConsol>().FirstOrDefault(x => x.IsGoingViaIgnoringDomesticRoute(Constants.CountryCodes.Canada)) != null))
			{
				var numberTypeList = (shipment as IAdditionalReferenceNumberTypeProvider)?.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					?? new ZArchitecture.Core.CodeDescriptionPairList();
				if (numberTypeList.ContainsCode(CanadaAdditionalReferenceNumberTypes.Codes.CCN))
				{
					string useHousebill = FreightDataRegistry.Instance.CanadaCargoControlNumberCustomization.Value;

					ZString rightPart = "";
					if (useHousebill == Constants.ShipmentCCNCustomizationTypes.Code.HouseBill)
					{
						var numberOfLastDigitsInHousebill = FreightDataRegistry.Instance.CanadaCargoControlNumberHBLDigits.Value;
						rightPart = numberOfLastDigitsInHousebill == 0 ? shipment.JS_HouseBill : shipment.JS_HouseBill.Right(numberOfLastDigitsInHousebill);
					}
					else if (useHousebill == Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain)
					{
						int branchPrefix = FreightDataRegistry.Instance.CanadaCargoControlNumberBranchPrefix.Value;
						int branchPrefixDefault = FreightDataRegistry.Instance.CanadaCargoControlNumberBranchPrefix.DefaultValue;
						ZString fountainNumber =
							Env.NumberFountains.CanadaCargoControlNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(shipment.Factory);

						if (branchPrefix != branchPrefixDefault)
						{
							int digits_in_branch_code = 2;
							rightPart = ZString.Format("{0}{1}", branchPrefix.ToString(CultureInfo.CurrentCulture),
								fountainNumber.SubstringSafe(digits_in_branch_code));
						}
						else
						{
							rightPart = fountainNumber;
						}
					}
					else if (useHousebill == Constants.ShipmentCCNCustomizationTypes.Code.ShipmentNumber)
					{
						var numberOfLastDigitsInHousebill = FreightDataRegistry.Instance.CanadaCargoControlNumberHBLDigits.Value;
						rightPart = numberOfLastDigitsInHousebill == 0 ? shipment.JS_UniqueConsignRef : shipment.JS_UniqueConsignRef.Right(numberOfLastDigitsInHousebill);
					}

					if (!rightPart.IsEmpty)
					{
						CusEntryNumber customReferenceNumber = shipment.Numbers.AddNew();
						customReferenceNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
						customReferenceNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
						customReferenceNumber.CE_EntryNum = ZString.Format("{0}{1}", carrierCode.PadRight(4), rightPart);
						cargoControlNumberIsSet = true;
					}
				}
			}

			return cargoControlNumberIsSet;
		}

		public static void RollbackSetCanadaCargoControlNumber(this CommonShipment shipment)
		{
			foreach (var cusEntryNumber in shipment.Numbers.Find(
				n => n.CE_EntryType == CanadaAdditionalReferenceNumberTypes.Codes.CCN && n.CE_RN_NKCountryCode == Core.Constants.CountryCodes.Canada && !n.IsInDatabase).ToArray())
			{
				cusEntryNumber.Delete();
			}
		}

		public static ZBool IsDestinationToCanada(this CommonShipment shipment)
		{
			return (shipment != null && shipment.JS_RL_NKDestination.StartsWith(Core.Constants.CountryCodes.Canada, StringComparison.Ordinal) && !shipment.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.Canada, StringComparison.Ordinal));
		}
	}
}
