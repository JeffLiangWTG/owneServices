using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class ConsolCMRConsignmentNote : ICMRConsignmentNote
	{
		public ConsolCMRConsignmentNote(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
			factory = consol.Factory;
			addressFormatter = ObjectFactory.Get<ICMRAddressFormatter>();

			if (parameters?.Data is ForwardingContainer container)
			{
				selectedContainer = container;
				hasContainer = true;
				InitializeLazy();
			}
		}

		readonly ForwardingConsol consol;
		readonly ForwardingContainer selectedContainer;
		readonly BusinessObjectFactory factory;
		readonly bool hasContainer;
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

		ZString ICMRConsignmentNote.IncotermAndTextBox14 => incotermAndTextBox14 ?? (incotermAndTextBox14 = GetFormattedIncoterm());
		string incotermAndTextBox14;

		ZString ICMRConsignmentNote.TransportIDBox23 => transportIDBox23 ?? (transportIDBox23 = ZString.Empty);
		string transportIDBox23;

		ZString ICMRConsignmentNote.JobNumber => jobNumber ?? (jobNumber = consol.JobNumber);
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
			if (consol.SendingForwarderAddress != null)
			{
				return addressFormatter.GetFullAddress(consol.SendingForwarderAddress);
			}

			if (consol.JK_AgentType == AgentType.Direct && consol.Shipments.Any())
			{
				var shipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
				return addressFormatter.GetFullAddress(shipment?.ConsignorDocumentaryAddress);
			}

			return ZString.Empty;
		}

		ZString GetImportAddress()
		{
			if (consol.ReceivingForwarderAddress != null)
			{
				return addressFormatter.GetFullAddress(consol.ReceivingForwarderAddress);
			}

			if (consol.JK_AgentType == AgentType.Direct && consol.Shipments.Any())
			{
				var shipment = consol.Shipments.FirstOrDefault() as ForwardingShipment;
				return addressFormatter.GetFullAddress(shipment?.ConsigneeDocumentaryAddress);
			}

			return ZString.Empty;
		}

		ZString GetFormattedIncoterm()
		{
			if (hasContainer)
			{
				var packLineWithIncoTerm = PackLines.OrderBy(x => x.JL_ContainerPackingOrder).FirstOrDefault(x => !string.IsNullOrEmpty(x.Shipment?.JS_INCO));

				if (packLineWithIncoTerm != null)
				{
					return (ZString)$"{packLineWithIncoTerm.Shipment.Lookups.JS_INCO_List.GetWithDescription(packLineWithIncoTerm.Shipment.JS_INCO)}";
				}
			}
			else if (consol.Shipments.Any())
			{
				var shipmentWithIncoTerm = consol.Shipments.Cast<ForwardingShipment>().FirstOrDefault(x => !string.IsNullOrEmpty(x.JS_INCO));
				if (shipmentWithIncoTerm != null)
				{
					return (ZString)$"{shipmentWithIncoTerm.Lookups.JS_INCO_List.GetWithDescription(shipmentWithIncoTerm.JS_INCO)}";
				}
			}

			return ZString.Empty;
		}

		ZString GetLineDetailsBox6_7_8_9()
		{
			var builder = new ZStringBuilder();

			if (hasContainer)
			{
				foreach (var packLine in PackLines)
				{
					var lineBuilder = new ZStringBuilder();
					lineBuilder.AppendIfNotEmpty(packLine.JL_MarksAndNumbers);
					lineBuilder.Append($"{packLine.JL_PackageCount} {packLine.JL_F3_NKPackType}");
					lineBuilder.Append(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(packLine.JL_Description));

					var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);

					builder.Append(cutLineDescription);
				}
			}
			else
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					var lineBuilder = new ZStringBuilder();
					lineBuilder.AppendIfNotEmpty(shipment.JS_MarksAndNumbers);
					lineBuilder.Append($"{shipment.JS_OuterPacks} {shipment.JS_F3_NKPackType}");
					lineBuilder.Append(CMRConsignmentNoteFormat.RemoveCarriageReturnsFromDescription(shipment.JS_GoodsDescription));

					var cutLineDescription = new ZString(lineBuilder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SemicolonAndSpace)).Left(CMRConsignmentNoteConstants.Length.MaxLineDetailLength);

					builder.Append(cutLineDescription);
				}
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		ZString GetLineDetailsGrossWeightInKGBox11()
		{
			var builder = new ZStringBuilder();

			if (hasContainer)
			{
				foreach (var packLine in PackLines)
				{
					builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.WeightInKilograms(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ)));
				}
			}
			else
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.WeightInKilograms(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight)));
				}
			}

			return CMRConsignmentNoteFormat.GetFormattedColumn(builder);
		}

		ZString GetLineDetailsVolumeInM3Box12()
		{
			var builder = new ZStringBuilder();

			if (hasContainer)
			{
				foreach (var packLine in PackLines)
				{
					builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.VolumeInCubicMeters(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ)));
				}
			}
			else
			{
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					builder.Append(CMRConsignmentNoteFormat.GetFormattedDecimal(CMRConsignmentNoteUnitConverter.VolumeInCubicMeters(shipment.JS_ActualVolume, shipment.JS_UnitOfVolume)));
				}
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

			builder.AppendIfNotEmpty(consol.ReceivingForwarderAddress?.Postcode ?? ZString.Empty);
			builder.AppendIfNotEmpty(consol.ReceivingForwarderAddress?.City.ToUpperInvariant() ?? ZString.Empty);
			builder.AppendIfNotEmpty(consol.ReceivingForwarderAddress?.Country?.Code ?? ZString.Empty);

			if (builder.IsEmpty)
			{
				builder.AppendIfNotEmpty(consol.DischargePort?.RL_PortName.ToUpperInvariant() ?? ZString.Empty);
				builder.AppendIfNotEmpty(consol.DischargePort?.Country?.Code ?? ZString.Empty);
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCityCountryDateOfGoodsTakingOver()
		{
			var builder = new ZStringBuilder();

			builder.AppendIfNotEmpty(consol.SendingForwarderAddress?.City.ToUpperInvariant() ?? ZString.Empty);
			builder.AppendIfNotEmpty(consol.SendingForwarderAddress?.Country?.Code ?? ZString.Empty);

			if (!builder.IsEmpty)
			{
				builder.AppendIfNotEmpty(new ZDate(consol.Transports?.MostInterestingTransport?.JW_ETD).ToString(CMRConsignmentNoteConstants.Formats.CMRDateFormat));
			}

			if (builder.IsEmpty)
			{
				builder.AppendIfNotEmpty(consol.Transports.MostInterestingTransport?.LoadPort?.RL_PortName.ToUpperInvariant() ?? ZString.Empty);
				builder.AppendIfNotEmpty(consol.Transports.MostInterestingTransport?.LoadPort?.Country?.Code ?? ZString.Empty);
				builder.AppendIfNotEmpty(new ZDate(consol.Transports?.MostInterestingTransport?.JW_ETD).ToString(CMRConsignmentNoteConstants.Formats.CMRDateFormat));
			}

			return builder.ToStringWithDelimiterBetweenAppends(CMRConsignmentNoteConstants.Delimiters.SimpleSpace);
		}

		ZString GetCarrierAddress()
		{
			var carrier = factory.Load<OrgAddress>(consol.JK_OA_ShippingLineAddress);
			return addressFormatter.GetFullAddress(carrier);
		}

		ZString GetNoteTextByDescription(string description)
		{
			return consol
				.Notes
				.FindByDescription(description)?
				.FirstOrDefault()?
				.ST_NoteText ?? ZString.Empty;
		}

		IEnumerable<PackLine> GetPackLinesFromContainer()
		{
			if (selectedContainer != null)
			{
				var containerPackPivotQuery = new ZDBOnlySubQuery(typeof(JobContainerPackPivot), JobContainerPackPivotSchema.J6_JL);
				containerPackPivotQuery.AddToFilter(JobContainerPackPivotSchema.J6_JC, SQLComparisonOperator.Equal, selectedContainer.PK);

				var packLinesQuery = new ZDBOnlyQuery(typeof(PackLine));
				packLinesQuery.AddSubQuery(JobPackLinesSchema.PK, containerPackPivotQuery, JoinCondition.And);

				return factory.Load<PackLine>(packLinesQuery);
			}

			return Enumerable.Empty<PackLine>();
		}

		UNDGDataItem GetPackLineWithDangerousGoods()
		{
			if (hasContainer)
			{
				return PackLines.FirstOrDefault(x => x.UNDGs.Any())?.UNDGs.FirstOrDefault();
			}

			if (consol.Shipments.Any())
			{
				return GetShipmentWithPackLineWithDangerousGoods();
			}

			return null;
		}

		IEnumerable<PackLine> PackLines => lazyPackLines.Value;

		Lazy<IEnumerable<PackLine>> lazyPackLines;

		void InitializeLazy()
		{
			lazyPackLines = new Lazy<IEnumerable<PackLine>>(() => GetPackLinesFromContainer().Select(x => x).OrderBy(x => x.JL_ContainerPackingOrder).ToArray());
		}

		UNDGDataItem GetShipmentWithPackLineWithDangerousGoods()
		{
			foreach (var shipment in consol.Shipments.Cast<ForwardingShipment>())
			{
				var packLineWithUNDG = GetShipmentPackLines(shipment)
				.OrderBy(x => x.JL_ContainerPackingOrder)
				.FirstOrDefault(pl => pl.UNDGs?.Any() == true);

				if (packLineWithUNDG != null)
				{
					return packLineWithUNDG.UNDGs.FirstOrDefault();
				}
			}

			return null;
		}

		List<PackLine> GetShipmentPackLines(ForwardingShipment shipment)
		{
			return shipment.OuterPackLines.Cast<PackLine>().Select(x => x).ToList();
		}

		#endregion
	}
}
