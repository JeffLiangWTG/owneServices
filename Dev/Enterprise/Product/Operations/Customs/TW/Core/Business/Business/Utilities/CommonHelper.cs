using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.TW;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public static class CommonHelper
	{
		public static ZBool CheckIsCarRelatedTariff(ZString tariff) => tariff.StartsWith("86", StringComparison.OrdinalIgnoreCase) || tariff.StartsWith("87", StringComparison.OrdinalIgnoreCase);

		public static void UpdateIfNotEmpty(ZPropertyInfo info, IZType value)
		{
			if (!value.IsEmpty)
			{
				info.Value = value;
			}
		}

		public static ZString ZeroConvertToEmptyString(ZShort value) => value.IsEmpty ? string.Empty : value.ToString();

		public static IeDoc GetFromUniqueKey(this IStorageDocsBaseCollection[] allEDocs, Guid uniqueKey)
		{
			IeDoc ieDoc = null;
			if (allEDocs != null)
			{
				foreach (var eDocs in allEDocs)
				{
					ieDoc = eDocs?.GetFromUniqueKey(uniqueKey);
					if (ieDoc != null)
					{
						break;
					}
				}
			}
			return ieDoc;
		}

		public static IeDoc FirstIeDoc(this IStorageDocsBaseCollection[] allEDocs)
		{
			IeDoc ieDoc = null;
			if (allEDocs != null)
			{
				foreach (var eDocs in allEDocs)
				{
					if (eDocs != null && eDocs.Count > 0)
					{
						ieDoc = eDocs[0];
						break;
					}
				}
			}
			return ieDoc;
		}

		public static ZString RemoveEntryNumberPlaceHolder(ZString id) => id == MessageConstants.EntryNumberPlaceHolder ? ZString.Empty : id;

		public static bool IsMatchEntryNumber(IEntryNumberGeneratorProvider provider, ZString entryNumber)
		{
			var result = true;
			if (!entryNumber.IsEmpty)
			{
				var entryNumberGenerator = EntryNumberGenerator.New(provider);
				result = IsMatchEntryNumber(provider, entryNumber, entryNumberGenerator);
			}
			return result;
		}

		internal static bool IsMatchEntryNumber(CusInBondHeader header, ZString entryNumber)
		{
			var result = true;
			if (!entryNumber.IsEmpty)
			{
				result = IsMatchEntryNumber(header, entryNumber, header.EntryNumberGenerator);
			}
			return result;
		}

		static bool IsMatchEntryNumber(IEntryNumberGeneratorProvider provider, ZString entryNumber, BaseEntryNumberGenerator entryNumberGenerator)
		{
			var result = true;
			if (entryNumberGenerator != null)
			{
				var prefixFromGenerator = entryNumberGenerator.GetEntryNumberPrefix();
				if (prefixFromGenerator.IsEmpty || !entryNumber.StartsWith(prefixFromGenerator, StringComparison.OrdinalIgnoreCase) || entryNumberGenerator.DoesEntryNumberFallIntoThisCategory(entryNumber) || !entryNumberGenerator.IsSequenceNumberMatchEntryNumber(entryNumber, provider.SequenceNumber))
				{
					result = false;
				}
			}
			return result;
		}

		internal static bool IsWaitingForResponseOrHasBeenLodgedAtCustoms(CusEntryHeader cusEntry, bool reLoad = false)
		{
			if (cusEntry != null && cusEntry.IsInDatabase && reLoad)
			{
				cusEntry = new BusinessObjectFactory() { RefreshEnabled = false }.Load<CusEntryHeader>(cusEntry.PK);
			}
			return cusEntry?.IsWaitingForResponseOrHasBeenLodgedAtCustoms ?? false;
		}

		public static ZString GetFormattedStringForRateFormulaDerivedFrom(ZString formula)
		{
			var result = formula;
			if (Regex.IsMatch(formula, "([0-9.]+$)"))
			{
				var rateFormulaValue = new ZDecimal(ZDecimal.ParseSafe(formula, 0m) * 100m).Normalize();
				result = $"{rateFormulaValue}%";
			}
			return result;
		}

		public static ZDecimal GetDecimalForRateFormulaDerivedFrom(ZString formula)
		{
			var result = ZDecimal.Zero;
			var matchs = Regex.Matches(formula, "([0-9.]+)");
			if (matchs.Count == 1)
			{
				result = ZDecimal.Parse(matchs[0].Value);
			}
			return result;
		}

		public static bool ShouldDefaultIsIncludedInIvoiceLineToTrue(ZString incoTerm, ZString chargeType)
		{
			switch (incoTerm)
			{
				case Core.Constants.IncoTerms.FreeAlongsideShip:
				case Core.Constants.IncoTerms.FreeOnBoard:
					return chargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight ||
						chargeType == CustomsChargeTypeList.Codes.PackingCost;
				case Core.Constants.IncoTerms.CostInsuranceAndFreight:
					return chargeType == CustomsChargeTypeList.Codes.OverseasInsurance ||
						chargeType == CustomsChargeTypeList.Codes.OverseasFreight ||
						chargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight ||
						chargeType == CustomsChargeTypeList.Codes.PackingCost;
				case Core.Constants.IncoTerms.CostAndFreight:
					return chargeType == CustomsChargeTypeList.Codes.OverseasFreight ||
						chargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight ||
						chargeType == CustomsChargeTypeList.Codes.PackingCost;
				case Core.Constants.IncoTerms.CostAndInsurance:
					return chargeType == CustomsChargeTypeList.Codes.OverseasInsurance ||
						chargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight ||
						chargeType == CustomsChargeTypeList.Codes.PackingCost;
				case Core.Constants.IncoTerms.CarriageAndInsurancePaidTo:
				case Core.Constants.IncoTerms.DeliveredAtTerminal:
				case Core.Constants.IncoTerms.DeliveredAtPlace:
				case Core.Constants.IncoTerms.DeliveredDutyPaid:
				case Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded:
					return chargeType == CustomsChargeTypeList.Codes.OverseasInsurance ||
						chargeType == CustomsChargeTypeList.Codes.OverseasFreight;
				case Core.Constants.IncoTerms.CarriagePaidTo:
					return chargeType == CustomsChargeTypeList.Codes.OverseasFreight;
				default:
					return false;
			}
		}

		public static bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ZString chargeType)
		{
			switch (incoTerm)
			{
				case Core.Constants.IncoTerms.CostInsuranceAndFreight:
				case Core.Constants.IncoTerms.CostAndFreight:
				case Core.Constants.IncoTerms.CostAndInsurance:
				case Core.Constants.IncoTerms.FreeOnBoard:
				case Core.Constants.IncoTerms.FreeAlongsideShip:
					return chargeType == CustomsChargeTypeList.Codes.AdditionCharge;
				case Core.Constants.IncoTerms.ExWorks:
					return chargeType == CustomsChargeTypeList.Codes.AdditionCharge ||
						chargeType == CustomsChargeTypeList.Codes.ForeignInlandFreight ||
						chargeType == CustomsChargeTypeList.Codes.PackingCost ||
						chargeType == CustomsChargeTypeList.Codes.ExWorks;
				default:
					return chargeType == CustomsChargeTypeList.Codes.AdditionCharge ||
						chargeType == CustomsChargeTypeList.Codes.DeductionCharge;
			}
		}

		public static ZString FormattedEntryNumber(ZString entryNumber)
		{
			var result = new ZStringBuilder();
			if (!entryNumber.IsEmpty)
			{
				result.Append(entryNumber.SubstringSafe(0, 2));
				result.Append(entryNumber.SubstringSafe(2, 2));
				result.Append(entryNumber.SubstringSafe(4, 2));
				result.Append(entryNumber.SubstringSafe(6, 3));
				result.Append(GetEntryNumberPart5(entryNumber));
			}
			return result.ToStringWithDelimiterBetweenAppends("/");
		}

		public static ZString GetEntryNumberPart5(ZString entryNumber) => entryNumber.SubstringSafe(9);

		public static ZString GetTWTransportCode(ZString transportMode, ZString containerMode)
		{
			Func<ZString, ZString> getSeaTransportCode = (seaContainerMode) =>
			{
				switch (seaContainerMode)
				{
					case ContainerModeList.Codes.BreakBulk:
						return TransportCodeList.Codes.SeaPackedSundryGoods;
					case ContainerModeList.Codes.Containerized:
						return TransportCodeList.Codes.SeaContainer;
					case ContainerModeList.Codes.Bulk:
						return TransportCodeList.Codes.SeaBulkGoods;
					case ContainerModeList.Codes.OwnPropulsion:
						return TransportCodeList.Codes.SeaSelfPropelledGoods;
					case ContainerModeList.Codes.HandCarry:
						return TransportCodeList.Codes.SeaPassengerOrCREW;
					case ContainerModeList.Codes.Express:
						return TransportCodeList.Codes.SeaExpressDelivery;
					case ContainerModeList.Codes.Mail:
						return TransportCodeList.Codes.SeaMail;
					case ContainerModeList.Codes.FixedTransportInstallation:
						return TransportCodeList.Codes.SeaAndAirFixedTransportInstallation;
					case ContainerModeList.Codes.Pipeline:
						return TransportCodeList.Codes.SeaAndAirPipeline;
					case ContainerModeList.Codes.PowerLine:
						return TransportCodeList.Codes.SeaAndAirPowerLine;
					case ContainerModeList.Codes.Other:
						return TransportCodeList.Codes.Other;
					default:
						return ZString.Empty;
				}
			};

			Func<ZString, ZString> getAirTransportCode = (airContainerMode) =>
			{
				switch (airContainerMode)
				{
					case ContainerModeList.Codes.Loose:
						return TransportCodeList.Codes.AirNotExpressDelivery;
					case ContainerModeList.Codes.Express:
						return TransportCodeList.Codes.AirExpressDelivery;
					case ContainerModeList.Codes.OwnPropulsion:
						return TransportCodeList.Codes.AirSelfPropelledGoods;
					case ContainerModeList.Codes.HandCarry:
						return TransportCodeList.Codes.AirPassengerOrCREW;
					case ContainerModeList.Codes.Mail:
						return TransportCodeList.Codes.AirMail;
					case ContainerModeList.Codes.FixedTransportInstallation:
						return TransportCodeList.Codes.SeaAndAirFixedTransportInstallation;
					case ContainerModeList.Codes.Pipeline:
						return TransportCodeList.Codes.SeaAndAirPipeline;
					case ContainerModeList.Codes.PowerLine:
						return TransportCodeList.Codes.SeaAndAirPowerLine;
					case ContainerModeList.Codes.Other:
						return TransportCodeList.Codes.Other;
					default:
						return ZString.Empty;
				}
			};
			return transportMode == TransportTypeList.Codes.Sea ? getSeaTransportCode(containerMode)
				: transportMode == TransportTypeList.Codes.Air ? getAirTransportCode(containerMode) : ZString.Empty;
		}

		public static JobDocAddressNumber FindOrCreateWithNumberType(this JobDocAddressNumberCollection numberCollection, ZString numberType, ZString countryCode, IEnumerable<string> includeNumberTypes = null)
		{
			int index = 0;
			if (includeNumberTypes == null || !includeNumberTypes.Any())
			{
				includeNumberTypes = new string[] { numberType };
			}
			var numberTypes = includeNumberTypes.Select(x => new ZString(x));
			var inputOrderLookup = numberTypes.ToDictionary(x => x, x => index++);
			var addressNumbers = numberCollection.Cast<JobDocAddressNumber>().Where(x => !x.IsDeleted && numberTypes.Contains(x.E2N_NumberType) && x.E2N_RN_NKCountryCode == countryCode).OrderBy(x => inputOrderLookup[x.E2N_NumberType]);
			var result = addressNumbers.FirstOrDefault();
			addressNumbers.Skip(1).ToArray().ForEach(x => numberCollection.RemoveAndDelete(x));
			if (result == null)
			{
				result = numberCollection.AddNew(numberType, countryCode);
			}
			else if (!numberType.IsEmpty && result.E2N_NumberType != numberType)
			{
				result.E2N_NumberType = numberType;
			}
			return result;
		}

		public static ZString GetVesselDescription(RefVessel vessel)
		{
			var result = Constants.NIL;
			if (vessel != null)
			{
				if (!vessel.RV_LloydsNumber.IsEmpty)
				{
					result = vessel.RV_LloydsNumber;
				}
				else if (!vessel.RV_RadioCallSign.IsEmpty)
				{
					result = vessel.RV_RadioCallSign;
				}
			}
			return result;
		}

		public static ZString ConvertToNilWhenEmpty(ZString str) => str.IsEmpty ? (ZString)Constants.NIL : str;

		public static void CheckVoyageFlightNoFormat(ZPropertyInfo propertyInfo, ZString flightNo)
		{
			if (!Regex.IsMatch(flightNo, "^([A-Z0-9]{2})\\s([0-9]{4})$"))
			{
				propertyInfo.AddWarning(Res.GetString("F8846E5E-8C91-4E7D-8206-A5D687574A20", "System will automatically declare two-letter airline code and the four-digit flight number, separated by a space character. For example, CI 0008, where the two-letter airline code is CI and the four-digit flight number is 0008."));
			}
		}
	}
}
