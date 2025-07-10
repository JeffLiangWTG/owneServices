using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ShipmentCMRConsignmentNote : ICMRConsignmentNote
	{
		public ShipmentCMRConsignmentNote(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			addressFormatter = ObjectFactory.Get<ICMRAddressFormatter>();
			InitializeLazy();
		}
		readonly ForwardingShipment shipment;
		readonly ICMRAddressFormatter addressFormatter;

		ZString ICMRConsignmentNote.SupplierAddress => supplierAddress ?? (supplierAddress = GetSupplierAddress());
		string supplierAddress;

		ZString ICMRConsignmentNote.ImporterAddress => importAddress ?? (importAddress = GetImportAddress());
		string importAddress;

		ZString ICMRConsignmentNote.InternationalConsignmentNote => internationalConsignmentNote ?? (internationalConsignmentNote = ZString.Empty);
		string internationalConsignmentNote;

		ZString ICMRConsignmentNote.PlaceOfDelivery => placeOfDelivery ?? (placeOfDelivery = GetPlaceOfDelivery());
		string placeOfDelivery;

		ZString ICMRConsignmentNote.CityCountryDateOfGoodsTakingOver => cityCountryDateOfGoodsTakingOver ?? (cityCountryDateOfGoodsTakingOver = GetCityCountryDateOfGoodsTakingOver());
		string cityCountryDateOfGoodsTakingOver;

		ZString ICMRConsignmentNote.CarrierAddress => carrierAddress ?? (carrierAddress = GetCarrierAddress());
		string carrierAddress;

		ZString ICMRConsignmentNote.GoodsAttachedDocuments => goodsAttachedDocuments ?? (goodsAttachedDocuments = GetGoodsAttachedDocuments());
		string goodsAttachedDocuments;

		ZString ICMRConsignmentNote.LineDetailsBox6_7_8_9 => lineDetailsBox6_7_8_9 ?? (lineDetailsBox6_7_8_9 = GetLineDetailsBox6_7_8_9());
		string lineDetailsBox6_7_8_9;

		ZString ICMRConsignmentNote.LineDetailsTariffCodeBox10 => lineDetailsTariffCodeBox10 ?? (lineDetailsTariffCodeBox10 = ZString.Empty);
		string lineDetailsTariffCodeBox10;

		ZString ICMRConsignmentNote.LineDetailsGrossWeightInKGBox11 => lineDetailsGrossWeightInKGBox11 ?? (lineDetailsGrossWeightInKGBox11 = GetLineDetailsGrossWeightInKGBox11());
		string lineDetailsGrossWeightInKGBox11;

		ZString ICMRConsignmentNote.LineDetailsVolumeInM3Box12 => lineDetailsVolumeInM3Box12 ?? (lineDetailsVolumeInM3Box12 = GetLineDetailsVolumeInM3Box12());
		string lineDetailsVolumeInM3Box12;

		ZString ICMRConsignmentNote.IncotermAndTextBox14 => incotermAndTextBox14 ?? (incotermAndTextBox14 = GetFormattedIncoterm(shipment.JS_INCO));
		string incotermAndTextBox14;

		ZString ICMRConsignmentNote.TransportIDBox23 => transportIDBox23 ?? (transportIDBox23 = ZString.Empty);
		string transportIDBox23;

		ZString ICMRConsignmentNote.JobNumber => jobNumber ?? (jobNumber = shipment.JobNumber);
		string jobNumber;

		ZString ICMRConsignmentNote.SendersInstructions => sendersInstructions ?? (sendersInstructions = GetSendersInstructions());
		string sendersInstructions;

		ZString ICMRConsignmentNote.SpecialAgreements => specialAgreements ?? (specialAgreements = GetSpecialAgreements());
		string specialAgreements;

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
		UNDGDataItem DangerousGoodsData => dangerousGoodsData ??= GetPackLineWithDangerousGoods();

		ITextLimitCalculator ICMRConsignmentNote.TextLimitCalculator => textLimitCalculator ?? (textLimitCalculator = new FreightTextLimitCalculator());
		ITextLimitCalculator textLimitCalculator;

		#region unused properties for numbering

		// Customs used following properties for numbering, we don't need them for CMR in Freight Forwarding
		ZString ICMRConsignmentNote.Box14PaymentCarriage => ZString.Empty;
		ZString ICMRConsignmentNote.Box19SpecialAgreements => ZString.Empty;
		ZString ICMRConsignmentNote.Box20ToBePaidBy => ZString.Empty;
		ZString ICMRConsignmentNote.Box15CashOnDelivery => ZString.Empty;
		ZString ICMRConsignmentNote.Box23TransportAndTrailerID => ZString.Empty;

		#endregion

		#region Implementation

		ZString GetSupplierAddress()
		{
			return addressFormatter.GetFullAddress(shipment.ConsignorDocumentaryAddress);
		}

		ZString GetImportAddress()
		{
			return addressFormatter.GetFullAddress(shipment.ConsigneeDocumentaryAddress);
		}

		ZString GetFormattedIncoterm(ZString incoTerm) => incoTerm.IsEmpty ? ZString.Empty : (ZString)$"{shipment.Lookups.JS_INCO_List.GetWithDescription(incoTerm)}";

		ZString GetLineDetailsBox6_7_8_9()
		{
			var builder = new ZStringBuilder();

			foreach (var packLine in PackLines)
			{
				var lineBuilder = new ZStringBuilder();
				lineBuilder.AppendIfNotEmpty(packLine.JL_MarksAndNumbers);
				lineBuilder.Append($"{packLine.JL_PackageCount} {packLine.JL_F3_NKPackType}");
				lineBuilder.Append(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(packLine.JL_Description));

				var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);

				builder.Append(cutLineDescription);
			}

			// If no pack lines, use shipment details as fallback
			if (builder.IsEmpty)
			{
				var lineBuilder = new ZStringBuilder();
				lineBuilder.AppendIfNotEmpty(shipment.JS_MarksAndNumbersShort);
				lineBuilder.Append($"{shipment.JS_OuterPacks} {shipment.JS_F3_NKPackType}" );
				lineBuilder.Append(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(shipment.JS_GoodsDescription));

				var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);
				builder.Append(cutLineDescription);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetLineDetailsGrossWeightInKGBox11()
		{
			var builder = new ZStringBuilder();

			foreach (var packLine in PackLines)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.WeightInKilograms(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ)));
			}

			if (builder.IsEmpty)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.WeightInKilograms(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight)));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetLineDetailsVolumeInM3Box12()
		{
			var builder = new ZStringBuilder();

			foreach (var packLine in PackLines)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.VolumeInCubicMeters(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ)));
			}

			if (builder.IsEmpty)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.VolumeInCubicMeters(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume)));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetSendersInstructions()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(GetNoteTextByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)));
			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(GetNoteTextByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)));

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetSpecialAgreements()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(GetNoteTextByDescription(PredefinedNoteTypes.Instance.PickupInstructionsNote.Description)));

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetGoodsAttachedDocuments()
		{
			return string.Empty;
		}

		ZString GetPlaceOfDelivery()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(shipment.ConsigneeDeliveryAddress?.Postcode ?? ZString.Empty);
			builder.AppendIfNotEmpty(shipment.ConsigneeDeliveryAddress?.City.ToUpperInvariant() ?? ZString.Empty);
			builder.AppendIfNotEmpty(shipment.ConsigneeDeliveryAddress?.Country?.Code ?? ZString.Empty);

			if (builder.IsEmpty)
			{
				builder.AppendIfNotEmpty(shipment.ConsigneeDocumentaryAddress?.City.ToUpperInvariant() ?? ZString.Empty);
				builder.AppendIfNotEmpty(shipment.ConsigneeDocumentaryAddress?.Country?.Code ?? ZString.Empty);
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCityCountryDateOfGoodsTakingOver()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(shipment.ConsignorPickupAddress?.City.ToUpperInvariant() ?? ZString.Empty);
			builder.AppendIfNotEmpty(shipment.ConsignorPickupAddress?.Country?.Code ?? ZString.Empty);

			if (!builder.IsEmpty && shipment.DocsAndCartage?.JP_EstimatedPickup != null)
			{
				builder.AppendIfNotEmpty(new ZDate(shipment.DocsAndCartage.JP_EstimatedPickup).ToString(CMRConsignmentNoteConstants.Formats.CMRDateFormat));
			}

			if (builder.IsEmpty)
			{
				builder.AppendIfNotEmpty(shipment.ConsignorDocumentaryAddress?.City.ToUpperInvariant() ?? ZString.Empty);
				builder.AppendIfNotEmpty(shipment.ConsignorDocumentaryAddress?.Country?.Code ?? ZString.Empty);
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCarrierAddress()
		{
			var carrier = shipment.Factory.Load<OrgHeader>(shipment.JS_JK_Carrier);
			var mainAddress = carrier?.Addresses?.MainAddress;

			return addressFormatter.GetFullAddress(mainAddress);
		}

		ZString GetNoteTextByDescription(string description)
		{
			return shipment
				.Notes
				.FindByDescription(description)?
				.FirstOrDefault()?
				.ST_NoteText ?? ZString.Empty;
		}

		UNDGDataItem GetPackLineWithDangerousGoods()
		{
			return PackLines.OrderBy(x => x.JL_ContainerPackingOrder).FirstOrDefault(x => x.UNDGs.Any())?.UNDGs.FirstOrDefault();
		}

		IEnumerable<PackLine> PackLines => lazyPackLines.Value;

		Lazy<IEnumerable<PackLine>> lazyPackLines;

		void InitializeLazy()
		{
			lazyPackLines = new Lazy<IEnumerable<PackLine>>(() => shipment.OuterPackLines.Cast<PackLine>().Select(x => x).ToArray());
		}

		#endregion
	}
}
