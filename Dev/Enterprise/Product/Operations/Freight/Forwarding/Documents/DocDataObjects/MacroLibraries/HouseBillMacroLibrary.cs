using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.Integration;
using CargoWise.Macros;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using IDocument = Enterprise.DocumentVisualizer.DocDataObjects.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class HouseBillMacroLibrary : MacroLibraryBase, IHouseBillMacroLibrary
	{
		public override IEnumerator<IMacroMetaData> GetEnumerator() => lazyMacrosRegister.Value.GetEnumerator();

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly Lazy<ICollection<IMacroMetaData>> lazyMacrosRegister = new Lazy<ICollection<IMacroMetaData>>(() => Load(MacroHandlers));

		#region SuppressResourceStringsCheckRegion

		static IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<IEnumerable<object>, string, IReadOnlyCollection<IReferenceNumber>>>(
					"AsReferenceNumbersOfType",
					"Converts collection of string to collection of reference numbers.",
					(numbers, numberType) => AsReferenceNumbersOfType(numbers, numberType));

				yield return new Handler<Func<IEnumerable<object>, MacroMap, int, int, string>>(
					"GenerateReferenceNumber",
					"Generates reference number.",
					(numbers, referenceNumberTypes, maxNumberOfLines, maxNumberOfCharactersPerLine) => GenerateReferenceNumber(numbers, referenceNumberTypes, maxNumberOfLines, maxNumberOfCharactersPerLine));

				yield return new Handler<Func<IPackingLine, string>>(
					"DangerousGoodsDescription",
					"Generates dangerous goods description according to House Bill registry setting.",
					packingLine => CreateDangerousGoodsDescription(packingLine));

				yield return new Handler<Func<HouseBill, string>>(
					"StatementOfApprovalForCFR",
					"Generates the statement of approval if there are any 49CFR standard dangerous goods.",
					houseBill => GetStatementOfApprovalForCFR(houseBill));

				yield return new Handler<Func<HouseBill, string>>(
					"AdditionalHandlingInformationForCFR",
					"Generates the DG Additional Handling Information note if there are any 49CFR standard dangerous goods.",
					houseBill => GetAdditionalHandlingInformationForCFR(houseBill));

				yield return new Handler<Func<IEnumerable<object>, string>>(
					"PackTypeCode",
					"Creates pack type code for collection of packinglines.",
					packingLine => CreatePackTypeCode(packingLine));

				yield return new Handler<Func<IEnumerable<object>, string>>(
					"PackTypeDescription",
					"Creates pack type description for collection of packinglines.",
					packingLine => CreatePackTypeDescription(packingLine));

				yield return new Handler<Func<bool>>(
					"PrintSignature",
					"Returns value of the 'Freight -> House Bills -> Print Signature' registry",
					() => PrintSignature());

				yield return new Handler<Func<IEnumerable<ITransport>, string, bool>>(
					"IsLoadingIn",
					"Returns true when any of the transport legs is loaded in country/city code.",
					(transports, location) => DoesAnyTransportLegFulfillCriteria(transports, location, IsLoadingIn));

				yield return new Handler<Func<IEnumerable<ITransport>, string, bool>>(
					"IsDischargingIn",
					"Returns true when any of the transport legs is discharged in country/city code.",
					(transports, location) => DoesAnyTransportLegFulfillCriteria(transports, location, IsDischargingIn));

				yield return new Handler<Func<IMacroScope, bool>>(
					"HasFollowOnPage",
					"Returns true when Bill of Lading document contains follow on page.",
					scope => HasFollowOnPage(scope));

				yield return new Handler<Func<IMacroScope, int>>(
					"FollowOnPageNumber",
					"Returns current follow on page number. If current page is not follow on returns 0.",
					scope => GetFollowOnPageNumber(scope));

				yield return new Handler<Func<IMacroScope, int>>(
					"TotalFollowOnPages",
					"Returns total number of follow on pages.",
					scope => GetTotalFollowOnPages(scope));

				yield return new Handler<Func<Image>>(
					"FIATALogo",
					"Returns official FIATA logo image.",
					() => GetFIATALogo());

				yield return new Handler<Func<HouseBill, Image>>(
					"FIATALogoText",
					"Returns official FIATA logo image.",
					houseBill => GetFIATALogoText(houseBill));

				yield return new Handler<Func<IMoney, string>>(
					"MoneyToWords",
					"Converts monetary amount to words.",
					money => MoneyToWords(money));

				yield return new Handler<Func<IMeasurement, string, IMeasurement>>(
					"ConvertTo",
					"Converts weight/volume to another unit.",
					(measurement, unit) => ConvertMeasurement(measurement, unit));

				yield return new Handler<Func<IMeasurement, bool>>(
					"IsImperial",
					"Returns true if the measurement is imperial otherwise false.",
					measurement => IsImperial(measurement));
			}
		}

		#endregion

		#region AsReferenceNumbersOfType

		static IReadOnlyCollection<IReferenceNumber> AsReferenceNumbersOfType(IEnumerable<object> numbers, string numberType)
		{
			if (numbers == null)
			{
				return Array.Empty<IReferenceNumber>();
			}

			var numberTypes = new CodeDescriptionPairList();
			numberTypes.AddPair(numberType, string.Empty);

			return numbers
				.Select(Convert.ToString)
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.Select(n =>
				{
					return new ReferenceNumber
					{
						Value = n,
						Type = new CodeDescription(numberTypes)
						{
							Code = numberType
						}
					};
				})
				.ToArray();
		}

		#endregion

		#region GenerateReferenceNumber

		static string GenerateReferenceNumber(IEnumerable<object> referenceNumbers, MacroMap referenceNumberTypes, int maxNumberOfLines, int maxNumberOfCharactersPerLine)
		{
			if (referenceNumbers == null
				|| referenceNumberTypes == null)
			{
				return string.Empty;
			}

			var typedReferenceNumbers = referenceNumbers
				.Select(referenceNumber =>
				{
					switch (referenceNumber)
					{
						case IReferenceNumber number:
							return number;

						case IDynamicData dd:
							if (dd.Value is IReferenceNumber value)
							{
								return value;
							}
							return null;

						default:
							return null;
					}
				})
				.Where(referenceNumber => referenceNumber != null)
				.ToArray();

			var codes = referenceNumberTypes
				.Keys
				.ToArray();

			if (!codes.Any())
			{
				return string.Empty;
			}

			return string.Join(System.Environment.NewLine,
				SelectLines(codes, referenceNumberTypes, typedReferenceNumbers, maxNumberOfLines, maxNumberOfCharactersPerLine));
		}

		static IEnumerable<string> SelectLines(string[] codes, MacroMap referenceNumberTypes, IEnumerable<IReferenceNumber> referenceNumbers, int maxNumberOfLines, int maxNumberOfCharactersPerLine)
		{
			var groupedNumbersByCode = referenceNumbers
				.Where(n => n.Type != null && codes.Any(c => c == n.Type.Code))
				.GroupBy(n => n.Type.Code.ToString())
				.ToArray();

			int numberOfLines = 0;

			foreach (var code in codes)
			{
				var nums = groupedNumbersByCode
					.FirstOrDefault(g => g.Key == code);

				if (nums == null)
				{
					continue;
				}

				var label = Convert.ToString(referenceNumberTypes[code]);

				const string separator = ", ";

				var str = string.Join(separator,
					SelectNumberForLine(nums, separator.Length, maxNumberOfCharactersPerLine - label.Length));

				yield return string.Concat(label, str);

				if (++numberOfLines >= maxNumberOfLines)
				{
					break;
				}
			}
		}

		static IEnumerable<string> SelectNumberForLine(IEnumerable<IReferenceNumber> numbers, int separatorLength, int maxLength)
		{
			var runningLength = 0;
			var numbersReturned = 0;

			foreach (var number in numbers)
			{
				var value = number.Value;

				if (runningLength + value.Length + separatorLength * numbersReturned > maxLength)
				{
					break;
				}

				runningLength += value.Length;
				numbersReturned++;

				yield return value;
			}
		}

		#endregion

		#region CreateDangerousGoodsDescription

		static string CreateDangerousGoodsDescription(IPackingLine packingLine)
		{
			return packingLine.DangerousGoods?.Count > 0
				? string.Join(System.Environment.NewLine, packingLine.DangerousGoods.Select(CreateDescription))
				: string.Empty;
		}

		static string CreateDescription(IDangerousGood dangerousGood) => dangerousGood.WriteSummary();

		#endregion

		#region StatementOfApprovalForCFR

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Documents do not require translation")]
		const string StatementOfApproval = "This is to certify that the above-named/herein-named materials are properly classified, described, packaged, marked and labeled, and are in proper condition for transportation according to the applicable regulations of the Department of Transportation.";

		static string GetStatementOfApprovalForCFR(HouseBill houseBill)
		{
			return AnyPackLineHasCFR(houseBill)
				? StatementOfApproval
				: string.Empty;
		}

		static bool AnyPackLineHasCFR(HouseBill houseBill)
		{
			var packingLines = houseBill.Containers?.SelectMany(t => t.PackingLines) ?? Enumerable.Empty<IPackingLine>();
			if (houseBill.LoosePackingLines != null)
			{
				packingLines = packingLines.Concat(houseBill.LoosePackingLines);
			}

			return packingLines?.Any(packingLine =>
			{
				return packingLine?
					.DangerousGoods?
					.Any(dangerousGood => dangerousGood.Standard == UNDGSubstanceStandardTypes.CFR)
					?? false;
			}) ?? false;
		}

		#endregion

		#region AdditionalHandlingInformationForCFR

		static string GetAdditionalHandlingInformationForCFR(HouseBill houseBill)
		{
			return AnyPackLineHasCFR(houseBill)
				? GetNoteTextByDescription(houseBill, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)
				: string.Empty;
		}

		static string GetNoteTextByDescription(HouseBill houseBill, string description)
		{
			return houseBill
				.Notes
				.SingleOrDefault(note => note.Description == description)
				?.Text ?? string.Empty;
		}

		#endregion

		#region CreatePackTypeCode

		static string CreatePackTypeCode(IEnumerable<object> collection)
		{
			var arr = collection?.ToArray() ?? Array.Empty<object>();

			if (arr.Length == 0)
			{
				return string.Empty;
			}

			if (arr[0].As<IPackingLine>() != null)
			{
				return arr
					.Select(p => p.As<IPackingLine>())
					.ToArray()
					.GetPackTypeCode();
			}

			return arr
				.Select(p => p.As<DocumentVisualizer.DocDataObjects.ICodeDescription>())
				.ToArray()
				.GetPackTypeCode();
		}

		#endregion

		#region PrintSignature

		static bool PrintSignature() => FreightDataRegistry.Instance.PrintSignatureForHBLDocuments.Value;

		#endregion

		#region CreatePackTypeDescription

		static string CreatePackTypeDescription(IEnumerable<object> collection)
		{
			var arr = collection?.ToArray() ?? Array.Empty<object>();

			if (arr.Length == 0)
			{
				return string.Empty;
			}

			if (arr[0].As<IPackingLine>() != null)
			{
				return arr
					.Select(p => p.As<IPackingLine>())
					.ToArray()
					.GetPackTypeDescription();
			}

			return arr
				.Select(p => p.As<DocumentVisualizer.DocDataObjects.ICodeDescription>())
				.ToArray()
				.GetPackTypeDescription();
		}

		#endregion

		#region IsLoadingIn/IsDischargingIn

		static bool DoesAnyTransportLegFulfillCriteria(IEnumerable<ITransport> transports, string locationCode, Func<ITransport, string, bool> comparer)
		{
			if (transports == null
				|| string.IsNullOrWhiteSpace(locationCode))
			{
				return false;
			}

			foreach (var obj in transports)
			{
				if (comparer(obj, locationCode))
				{
					return true;
				}
			}

			return false;
		}

		const int countryCodeLength = 2;
		const int unlocoCodeLength = 5;

		static bool IsLoadingIn(ITransport transport, string locationCode)
		{
			if (transport == null
				|| string.IsNullOrWhiteSpace(locationCode))
			{
				return false;
			}

			switch (locationCode.Length)
			{
				case countryCodeLength:
					return string.Compare(transport.PortOfLoading?.Country?.Code, locationCode, StringComparison.OrdinalIgnoreCase) == 0;

				case unlocoCodeLength:
					return string.Compare(transport.PortOfLoading?.Code, locationCode, StringComparison.OrdinalIgnoreCase) == 0;

				default:
					return false;
			}
		}

		static bool IsDischargingIn(ITransport transport, string locationCode)
		{
			if (transport == null
				|| string.IsNullOrWhiteSpace(locationCode))
			{
				return false;
			}

			switch (locationCode.Length)
			{
				case countryCodeLength:
					return string.Compare(transport.PortOfDischarge?.Country?.Code, locationCode, StringComparison.OrdinalIgnoreCase) == 0;

				case unlocoCodeLength:
					return string.Compare(transport.PortOfDischarge?.Code, locationCode, StringComparison.OrdinalIgnoreCase) == 0;

				default:
					return false;
			}
		}

		#endregion

		#region HasFollowOnPage

		static bool HasFollowOnPage(IMacroScope scope)
		{
			if (scope?.GetVariable(VariableNames.Document) is IDocument doc)
			{
				return doc
					.Pages
					.Any(p => p.Name?.StartsWith(HouseBillPageNames.FollowOn, StringComparison.OrdinalIgnoreCase) ?? false);
			}

			return false;
		}

		#endregion

		#region FollowOnPageNumber

		static int GetFollowOnPageNumber(IMacroScope scope)
		{
			var result = 0;

			var currentPage = scope?.GetPage();
			var document = scope?.GetDocument();

			if (currentPage != null
				&& document != null)
			{
				foreach (var page in document.Pages)
				{
					if (!page.Name?.StartsWith(HouseBillPageNames.FollowOn, StringComparison.OrdinalIgnoreCase) ?? false)
					{
						continue;
					}

					result++;

					if (currentPage == page)
					{
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region TotalFollowOnPages

		static int GetTotalFollowOnPages(IMacroScope scope)
		{
			var result = 0;

			var document = scope?.GetDocument();

			if (document != null)
			{
				foreach (var page in document.Pages)
				{
					if (!page.Name?.StartsWith(HouseBillPageNames.FollowOn, StringComparison.OrdinalIgnoreCase) ?? false)
					{
						continue;
					}

					result++;
				}
			}

			return result;
		}

		#endregion

		#region FIATA Logo

		static Image GetFIATALogo()
		{
			if (!GlbBranch.CurrentBranch?.Country?.Code.IsEmpty ?? false)
			{
				var fiataLogoProvider = ObjectFactory.Get<IFIATALogoProvider>();
				return fiataLogoProvider?.GetFIATALogo(GlbBranch.CurrentBranch.Country.Code);
			}

			return null;
		}

		static Image GetFIATALogoText(HouseBill houseBill)
		{
			if (houseBill == null)
			{
				return null;
			}

			var isSeaWaybill = string.Compare(houseBill.ReleaseType?.Code.ToString(), Core.Constants.ShipmentReleaseTypes.SeaWaybill, StringComparison.OrdinalIgnoreCase) == 0;

			var fiataLogoProvider = ObjectFactory.Get<IFIATALogoProvider>();
			return fiataLogoProvider?.GetFIATATextLogo(isSeaWaybill);
		}

		#endregion

		#region MoneyToWords

		static string MoneyToWords(IMoney money)
		{
			if (money == null)
			{
				return string.Empty;
			}

			var converter = ObjectFactory.Get<ICurrencyToWordsConverter>();
			var res = converter
				.Convert((double)money.Amount, money.Currency?.Code)
				.Replace(" only", string.Empty);

			return res;
		}

		#endregion

		#region ConvertMeasurement

		static IMeasurement ConvertMeasurement(IMeasurement measurement, string unit)
		{
			if (measurement?.Unit == null
				|| string.Compare(measurement.Unit.Code, unit, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return measurement;
			}

			if (Core.Constants.Weight.ContainsCode(measurement.Unit.Code)
				&& Core.Constants.Weight.ContainsCode(unit))
			{
				return ConvertWeight(measurement, unit);
			}

			if (Core.Constants.Volume.ContainsCode(measurement.Unit.Code)
				&& Core.Constants.Volume.ContainsCode(unit))
			{
				return ConvertVolume(measurement, unit);
			}

			return measurement;
		}

		static IMeasurement ConvertWeight(IMeasurement weight, string unit)
		{
			return new Measurement
			{
				Value = Core.Constants.Weight.Convert(weight.Value, weight.Unit.Code, unit),
				Unit = new CodeDescription(weight.Unit.Codes as ICodeDescriptionPairList)
				{
					Code = unit
				}
			};
		}

		static IMeasurement ConvertVolume(IMeasurement volume, string unit)
		{
			return new Measurement
			{
				Value = Core.Constants.Volume.Convert(volume.Value, volume.Unit.Code, unit),
				Unit = new CodeDescription(volume.Unit.Codes as ICodeDescriptionPairList)
				{
					Code = unit
				}
			};
		}

		#endregion

		#region IsImperial

		static bool IsImperial(IMeasurement measurement)
		{
			if (measurement?.Unit == null)
			{
				return false;
			}

			if (Core.Constants.Weight.ContainsCode(measurement.Unit.Code)
				&& Core.Constants.Weight.IsImperial(measurement.Unit.Code))
			{
				return true;
			}

			if (Core.Constants.Volume.ContainsCode(measurement.Unit.Code)
				&& Core.Constants.Volume.IsImperial(measurement.Unit.Code))
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}
