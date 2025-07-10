using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class DtbBookingCMRConsignmentNote : ICMRConsignmentNote
	{
		public DtbBookingCMRConsignmentNote(DtbBooking booking, IDocDataObjectParameters parameters)
		{
			this.booking = Argument.NotNull(booking, nameof(booking));
			Argument.NotNull(parameters, nameof(parameters));

			pickup = ((DtbBookingInstruction[])parameters.Data).Single(i => i.IsPickUp);
			delivery = ((DtbBookingInstruction[])parameters.Data).Single(i => i.IsDelivery);
			addressFormatter = ObjectFactory.Get<ICMRAddressFormatter>();

			//if there are more than one delivery instructions, we need to use the main package divots from the delivery instruction
			if (booking.Instructions.Count(i => i.IsDelivery) > 1)
			{
				mainPackageDivots = delivery.PackageDivots;
			}
			else
			{
				mainPackageDivots = pickup.PackageDivots;
			}
		}

		readonly DtbBooking booking;
		readonly DtbBookingInstruction pickup;
		readonly DtbBookingInstruction delivery;
		readonly DtbBookingInstructionPkgDivotCollection mainPackageDivots;
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

		ZString ICMRConsignmentNote.JobNumber => jobNumber ?? (jobNumber = booking.KM_JobID);
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
		UNDGDataItem DangerousGoodsData => dangerousGoodsData ??= GetAssignedPackageWithDangerousGoods();

		ITextLimitCalculator ICMRConsignmentNote.TextLimitCalculator => textLimitCalculator ?? (textLimitCalculator = new FreightTextLimitCalculator());
		ITextLimitCalculator textLimitCalculator;

		ZString ICMRConsignmentNote.InternationalConsignmentNote => ZString.Empty;
		ZString ICMRConsignmentNote.IncotermAndTextBox14 => ZString.Empty;
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
			return addressFormatter.GetFullAddress(pickup.Address);
		}

		ZString GetImportAddress()
		{
			return addressFormatter.GetFullAddress(delivery.Address);
		}

		ZString GetLineDetailsBox6_7_8_9()
		{
			var builder = new ZStringBuilder();

			foreach (var packageDivot in mainPackageDivots)
			{
				var lineBuilder = new ZStringBuilder();
				lineBuilder.AppendIfNotEmpty(packageDivot.Package.KP_MarksAndNumbers);
				lineBuilder.Append($"{packageDivot.Package.KP_PackageQty} {packageDivot.Package.KP_F3_NKPackType}");
				lineBuilder.AppendIfNotEmpty(packageDivot.Package.KP_GoodsDescription);

				var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);

				builder.Append(cutLineDescription);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetLineDetailsGrossWeightInKGBox11()
		{
			var builder = new ZStringBuilder();

			foreach (var packageDivot in mainPackageDivots)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.WeightInKilograms(packageDivot.Package.KP_Weight, packageDivot.Package.KP_WeightUQ)));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetLineDetailsVolumeInM3Box12()
		{
			var builder = new ZStringBuilder();

			foreach (var packageDivot in mainPackageDivots)
			{
				builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.VolumeInCubicMeters(packageDivot.Package.KP_Volume, packageDivot.Package.KP_VolumeUQ)));
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetSendersInstructions()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(pickup.KN_ServiceInstruction));
			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(GetNoteTextByDescription(PredefinedNoteTypes.Instance.HandlingInstructions.Description)));
			builder.AppendIfNotEmpty(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(GetNoteTextByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description)));

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetPlaceOfDelivery()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(delivery.Address.E2_Postcode);
			builder.AppendIfNotEmpty(delivery.Address.E2_City.ToUpperInvariant());
			builder.AppendIfNotEmpty(delivery.Address.E2_RN_NKCountryCode);

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCityCountryDateOfGoodsTakingOver()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(pickup.Address.E2_City.ToUpperInvariant());
			builder.AppendIfNotEmpty(pickup.Address.E2_RN_NKCountryCode);

			builder.AppendIfNotEmpty(new ZDate(pickup.Estimated).ToString(CMRConsignmentNoteConstants.Formats.CMRDateFormat));

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCarrierAddress()
		{
			return addressFormatter.GetFullAddress(booking.Address);
		}

		ZString GetNoteTextByDescription(string description)
		{
			return booking
				.Notes
				.FindByDescription(description)?
				.FirstOrDefault()?
				.ST_NoteText ?? ZString.Empty;
		}

		UNDGDataItem GetAssignedPackageWithDangerousGoods()
		{
			return mainPackageDivots.FirstOrDefault(p => p.Package?.UNDGs.Any() ?? false)?.Package?.UNDGs.FirstOrDefault();
		}
	}
}
