using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentCMRConsignmentNote : ICMRConsignmentNote
	{
		public DtbConsignmentCMRConsignmentNote(DtbConsignment consignment)
		{
			this.consignment = Argument.NotNull(consignment, nameof(consignment));
			addressFormatter = ObjectFactory.Get<ICMRAddressFormatter>();
			InitializeLazy();
		}

		readonly DtbConsignment consignment;
		readonly ICMRAddressFormatter addressFormatter;

		ZString ICMRConsignmentNote.SupplierAddress => supplierAddress ?? (supplierAddress = GetSupplierAddress());
		string supplierAddress;

		ZString ICMRConsignmentNote.ImporterAddress => importAddress ?? (importAddress = GetImportAddress());
		string importAddress;

		ZString ICMRConsignmentNote.PlaceOfDelivery => placeOfDelivery ?? (placeOfDelivery = GetPlaceOfDelivery());
		string placeOfDelivery;

		ZString ICMRConsignmentNote.CityCountryDateOfGoodsTakingOver => cityCountryDateOfGoodsTakingOver ?? (cityCountryDateOfGoodsTakingOver = GetCityCountryDateOfGoodsTakingOver());
		string cityCountryDateOfGoodsTakingOver;

		ZString ICMRConsignmentNote.CarrierAddress => carrierAddress ?? (carrierAddress = GetCarrierAddress());
		string carrierAddress;

		ZString ICMRConsignmentNote.LineDetailsBox6_7_8_9 => lineDetailsBox6_7_8_9 ?? (lineDetailsBox6_7_8_9 = GetLineDetailsBox6_7_8_9());
		string lineDetailsBox6_7_8_9;

		ZString ICMRConsignmentNote.LineDetailsGrossWeightInKGBox11 => lineDetailsGrossWeightInKGBox11 ?? (lineDetailsGrossWeightInKGBox11 = GetLineDetailsGrossWeightInKGBox11());
		string lineDetailsGrossWeightInKGBox11;

		ZString ICMRConsignmentNote.LineDetailsVolumeInM3Box12 => lineDetailsVolumeInM3Box12 ?? (lineDetailsVolumeInM3Box12 = GetLineDetailsVolumeInM3Box12());
		string lineDetailsVolumeInM3Box12;

		ZString ICMRConsignmentNote.JobNumber => jobNumber ?? (jobNumber = consignment.LTC_JobID);
		string jobNumber;

		ZString ICMRConsignmentNote.SendersInstructions => sendersInstructions ?? (sendersInstructions = GetSendersInstructions());
		string sendersInstructions;

		ZString ICMRConsignmentNote.EstablishedInDate => establishedInDate ?? (establishedInDate = ZDateTime.Now.ToBestReadableDateString());
		string establishedInDate;

		ZString ICMRConsignmentNote.EstablishedInPlace => establishedInPlace ?? (establishedInPlace = GlbBranch.CurrentBranch?.HomePort?.RL_PortName ?? ZString.Empty);
		string establishedInPlace;

		ZString ICMRConsignmentNote.DangerousGoodsClass => dangerousGoodsClass ?? (dangerousGoodsClass = DangerousGoodsData?.DI_IMOClass ?? ZString.Empty);
		string dangerousGoodsClass;
		ZString ICMRConsignmentNote.DangerousGoodsNumber => dangerousGoodsNumber ?? (dangerousGoodsNumber = DangerousGoodsData?.SubstanceCode.ToUpperInvariant() ?? ZString.Empty);
		string dangerousGoodsNumber;

		ZString ICMRConsignmentNote.DangerousGoodsLetter => ZString.Empty;

		UNDGDataItem dangerousGoodsData;
		UNDGDataItem DangerousGoodsData => dangerousGoodsData ??= GetPackagesWithDangerousGoods();

		ITextLimitCalculator ICMRConsignmentNote.TextLimitCalculator => textLimitCalculator ?? (textLimitCalculator = new FreightTextLimitCalculator());
		ITextLimitCalculator textLimitCalculator;

		ZString ICMRConsignmentNote.InternationalConsignmentNote => ZString.Empty;
		ZString ICMRConsignmentNote.IncotermAndTextBox14 => incotermAndTextBox14 ?? (incotermAndTextBox14 = GetFormattedIncoterm(consignment.LTC_Incoterm));
		string incotermAndTextBox14;

		ZString ICMRConsignmentNote.TransportIDBox23 => ZString.Empty;
		ZString ICMRConsignmentNote.GoodsAttachedDocuments => ZString.Empty;
		ZString ICMRConsignmentNote.LineDetailsTariffCodeBox10 => ZString.Empty;
		ZString ICMRConsignmentNote.SpecialAgreements => ZString.Empty;

		// Customs used following properties for numbering, we don't need them for CMR in Freight Forwarding
		ZString ICMRConsignmentNote.Box14PaymentCarriage => ZString.Empty;
		ZString ICMRConsignmentNote.Box19SpecialAgreements => ZString.Empty;
		ZString ICMRConsignmentNote.Box20ToBePaidBy => ZString.Empty;
		ZString ICMRConsignmentNote.Box15CashOnDelivery => ZString.Empty;
		ZString ICMRConsignmentNote.Box23TransportAndTrailerID => ZString.Empty;

		ZString GetSupplierAddress()
		{
			var supplierAddress = (consignment.DocAddresses.FindByDocAddressType(DocAddressType.OriginatingConsignorAddress)) ??
											(consignment.PickupAddress?.Address);

			return addressFormatter.GetFullAddress(supplierAddress);
		}

		ZString GetImportAddress()
		{
			var importAddress = (consignment.DocAddresses.FindByDocAddressType(DocAddressType.FinalConsigneeAddress)) ??
								(consignment.DeliveryAddress?.Address);

			return addressFormatter.GetFullAddress(importAddress);
		}

		ZString GetFormattedIncoterm(ZString incoTerm)
		{
			if (string.IsNullOrEmpty(incoTerm))
			{
				return ZString.Empty;
			}

			var defaultDescription = RatingDataRegistry.Instance.IncoTermDefinition.Value.FirstOrDefault(item => ((IncoTermChargeCodes)item).IncoTerm == incoTerm) as IncoTermChargeCodes;
			return $"{incoTerm} - {defaultDescription?.IncoTermDescription}";
		}

		ZString GetLineDetailsBox6_7_8_9()
		{
			var builder = new ZStringBuilder();

			foreach (var pkg in Packages)
			{
				var lineBuilder = new ZStringBuilder();
				lineBuilder.AppendIfNotEmpty(!string.IsNullOrEmpty(pkg.KP_MarksAndNumbers) ? pkg.KP_MarksAndNumbers : pkg.KP_PackageID);

				lineBuilder.Append($"{pkg.KP_PackageQty} {pkg.KP_F3_NKPackType}");
				lineBuilder.Append(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(pkg.KP_GoodsDescription));
				var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);
				builder.Append(cutLineDescription);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetLineDetailsGrossWeightInKGBox11()
		{
			var builder = new ZStringBuilder();

			foreach (var pkg in Packages)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.WeightInKilograms(pkg.KP_Weight, pkg.KP_WeightUQ)));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetLineDetailsVolumeInM3Box12()
		{
			var builder = new ZStringBuilder();

			foreach (var pkg in Packages)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.VolumeInCubicMeters(pkg.KP_Volume, pkg.KP_VolumeUQ)));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetSendersInstructions()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(consignment.PickupAddress?.LTS_Notes ?? ZString.Empty));
			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(GetNoteTextByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)));

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetPlaceOfDelivery()
		{
			var builder = new ZStringBuilder();
			var deliveryAddress = consignment.DeliveryAddress?.Address;

			if (deliveryAddress != null)
			{
				builder.AppendIfNotEmpty(deliveryAddress.Postcode);
				builder.AppendIfNotEmpty(deliveryAddress.City.ToUpperInvariant());
				builder.AppendIfNotEmpty(deliveryAddress.Country?.Code ?? ZString.Empty);
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCityCountryDateOfGoodsTakingOver()
		{
			var builder = new ZStringBuilder();

			var city = consignment.PickupAddress?.Address.City.ToUpperInvariant() ?? ZString.Empty;
			var country = consignment.PickupAddress?.Address.Country?.Code ?? ZString.Empty;
			builder.AppendIfNotEmpty(city);
			builder.AppendIfNotEmpty(country);

			// Append date if city or country are present
			if (!builder.IsEmpty)
			{
				var estimatedTime = consignment.Addresses
					.OrderBy(i => i.LTS_Sequence)
					.FirstOrDefault(i => i.IsPickUp)?
					.Actions.OrderBy(x => x.Estimated)
					.FirstOrDefault()?.Estimated;

				ZDate? dateToAppend = null;

				if (estimatedTime != null && estimatedTime.Value != ZDateTime.Empty)
				{
					dateToAppend = new ZDate(estimatedTime.Value);
				}
				else if (consignment.PickupAddress?.ReqFrom != null)
				{
					dateToAppend = new ZDate(consignment.PickupAddress.ReqFrom.ToZDateTime());
				}

				if (dateToAppend != null)
				{
					builder.AppendIfNotEmpty(dateToAppend.Value.ToString(CMRConsignmentNoteConstants.Formats.CMRDateFormat));
				}
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCarrierAddress()
		{
			var currentBranch = GlbBranch.CurrentBranch;

			if (currentBranch == null)
			{
				return ZString.Empty;
			}

			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(currentBranch.CompanyName.ToUpperInvariant());
			builder.AppendIfNotEmpty(currentBranch.Address1.ToUpperInvariant());
			builder.AppendIfNotEmpty(currentBranch.Address2.ToUpperInvariant());

			var cityAndCountryBuilder = new ZStringBuilder();
			cityAndCountryBuilder.AppendIfNotEmpty(currentBranch.City.ToUpperInvariant());
			cityAndCountryBuilder.AppendIfNotEmpty(currentBranch.Postcode.ToUpperInvariant());
			cityAndCountryBuilder.AppendIfNotEmpty(currentBranch.Country?.Description.ToUpperInvariant() ?? ZString.Empty);

			var cityAndCountryString = ((ZString)cityAndCountryBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.DashBetweenSpaces));
			builder.AppendIfNotEmpty(cityAndCountryString);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetNoteTextByDescription(string description)
		{
			return consignment
				.Notes
				.FindByDescription(description)?
				.FirstOrDefault()?
				.ST_NoteText ?? ZString.Empty;
		}

		UNDGDataItem GetPackagesWithDangerousGoods()
		{
			return Packages.FirstOrDefault(x => x.UNDGs.Any())?.UNDGs.FirstOrDefault();
		}

		IEnumerable<PkgPackage> Packages => lazyPackages.Value;

		Lazy<IEnumerable<PkgPackage>> lazyPackages;

		void InitializeLazy()
		{
			lazyPackages = new Lazy<IEnumerable<PkgPackage>>(() => consignment.PackageJob.Packages.Select(x => x).ToArray());
		}
	}
}
