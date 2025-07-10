using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class MultimodalDangerousGoodsDeclarationBuilder
	{
		public MultimodalDangerousGoodsDeclarationBuilder(IDocDataObjectParameters parameters)
		{
			container = (ForwardingContainer)Argument.NotNull(parameters.Data, nameof(parameters.Data));
			consol = Argument.NotNull(container.Consol, nameof(container.Consol));
			context = new CommonContext(container.Factory.GetCachedReadOnlyFactory());
			transportMode = consol.Transports.OfType<Freight.Business.Transport>().Any(t => t.JW_TransportMode == Core.Constants.TransportModes.Air)
				? Core.Constants.TransportModes.Air
				: Core.Constants.TransportModes.Sea;
		}

		readonly ForwardingContainer container;
		readonly ForwardingConsol consol;
		readonly IContext context;

		const string UnitOfWeight = Core.Constants.Weight.Kilograms;
		const string UnitOfVolume = Core.Constants.Volume.CubicMetres;

		#region Transports

		IReadOnlyCollection<Freight.Business.Transport> Transports => transports ?? (transports = GetTransports());
		IReadOnlyCollection<Freight.Business.Transport> transports;

		IReadOnlyCollection<Freight.Business.Transport> GetTransports()
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol?.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.ToArray();
		}

		readonly string transportMode;

		#endregion

		#region Destination

		RefUNLOCO Destination
		{
			get
			{
				if (destination == null)
				{
					destination = GetDestinationFromShipments() ?? consol.DischargePort;
				}

				return destination;
			}
		}

		RefUNLOCO destination;

		RefUNLOCO GetDestinationFromShipments()
		{
			var shipments = consol
				.Shipments
				.OfType<ForwardingShipment>()
				.ToArray();

			var destination = shipments
				.FirstOrDefault()?
				.Destination;

			return destination != null && (consol.IsDirect || shipments.Skip(1).All(s => s.Destination != null && s.Destination.PK == destination.PK))
				? destination
				: null;
		}

		#endregion

		public MultimodalDangerousGoodsDeclaration Build()
		{
			var res = new MultimodalDangerousGoodsDeclaration(container.PK);

			var shipper = AddressBuilder.Create(context, consol.SendingForwarderAddress);
			shipper.AddressFormattedInfo.AddErrorIfEmpty(Res.GetString("ef6a41ce-3c1e-4818-9d8e-20f0c043f16a", "Shipper address is required"));
			res.Shipper = shipper;

			var consignee = AddressBuilder.Create(context, consol.ReceivingForwarderAddress);
			consignee.AddressFormattedInfo.AddErrorIfEmpty(Res.GetString("ce10f9a3-9a3f-4b2f-a668-1a44829bbdd5", "Consignee address is required"));
			res.Consignee = consignee;

			res.TransportDocumentNumber = consol.JK_MasterBillNum;
			res.ShipperReference = consol.JK_BookingReference;
			res.FreightForwarderReference = consol.JK_AgentsReference;

			var transport = consol
				.MostInterestingTransportForBinding
				.Cast<Freight.Business.Transport>()
				.FirstOrDefault();

			res.Vessel = new Vessel
			{
				Name = transport?.Vessel?.RV_Name ?? ZString.Empty,
				LloydsIMO = transport?.Vessel?.RV_LloydsNumber ?? ZString.Empty,
				RadioCallSign = transport?.Vessel?.RV_RadioCallSign ?? ZString.Empty,
			};

			res.MostInterestingTransportReference = GetTransportReference(transport);

			var firstPort = Transports.FirstOrDefault(t => t.JW_TransportMode == transportMode);
			var lastPort = Transports.LastOrDefault(t => t.JW_TransportMode == transportMode);

			res.PortOfLoading = Unloco.Create(context, firstPort?.LoadPort)
				.AddAsciiCharactersValidation()
				.AddRequiredValidation((NoResString)"Port Of Loading"); // non-translatable validation message
			res.PortOfDischarge = Unloco.Create(context, lastPort?.DiscPort)
				.AddAsciiCharactersValidation()
				.AddRequiredValidation((NoResString)"Port Of Discharge"); // non-translatable validation message
			res.Destination = Unloco.Create(context, Destination)
				.AddAsciiCharactersValidation()
				.AddRequiredValidation((NoResString)"Destination"); // non-translatable validation message

			res.ETD = firstPort != null ? firstPort.JW_ETD : ZDateTime.Empty;

			var dangerousGoodsAdditionalHandlingInfoNote = consol.Notes.FindByDescription(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Code).FirstOrDefault();
			if (dangerousGoodsAdditionalHandlingInfoNote != null)
			{
				res.AdditionalHandlingInformation = dangerousGoodsAdditionalHandlingInfoNote.ST_NoteText;
			}

			res.ContainerNumber = container.JC_ContainerNum;
			res.SealNumber = container.JC_SealNum;

			res.ContainerType = new ContainerType(context.ContainerTypes)
			{
				Code = container.RefContainer?.RC_Code ?? ZString.Empty
			};

			var tareWeight = Core.Constants.Weight.Convert(container.JC_TareWeight, container.JC_GrossWeightUQ, UnitOfWeight);
			var grossWeight = Core.Constants.Weight.Convert(container.JC_GrossWeight, container.JC_GrossWeightUQ, UnitOfWeight);

			res.TareWeight = new Measurement
			{
				Value = tareWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = UnitOfWeight
				}
			};

			res.GrossWeight = new Measurement
			{
				Value = grossWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = UnitOfWeight
				}
			};

			PopulateGoodsDetails(res);

			res.ValidateAllIncludingChildren();

			return res;
		}

		ZString GetTransportReference(Freight.Business.Transport transportBO)
		{
			if (transportBO == null)
			{
				return ZString.Empty;
			}

			var stringBuilder = new ZStringBuilder();
			if (transportBO.JW_TransportMode != Core.Constants.TransportModes.Air)
			{
				stringBuilder.Append(transportBO.JW_Vessel);
			}

			stringBuilder.Append(transportBO.JW_VoyageFlight);

			if (transportBO.JW_TransportMode == Core.Constants.TransportModes.Sea)
			{
				stringBuilder.Append(transportBO.Vessel?.RV_LloydsNumber ?? ZString.Empty);
			}
			else
			{
				var departureDate = transportBO.JW_ATD.IsEmpty ? transportBO.JW_ETD : transportBO.JW_ATD;
				stringBuilder.Append(departureDate.IsEmpty ? ZString.Empty : (ZString)departureDate.ToString("dd-MMM"));
			}

			return stringBuilder.ToStringWithDelimiterBetweenAppends(" / ");
		}

		#region PopulateGoodsDetails
		#region SuppressResourceStringsCheckRegion

		void PopulateGoodsDetails(MultimodalDangerousGoodsDeclaration declaration)
		{
			var goodsDetails = GenerateGoodsDetails();

			if (goodsDetails.Length == 0)
			{
				declaration.FollowOnPages = Array.Empty<MultimodalDangerousGoodsDeclarationFollowOn>();
				return;
			}

			if (goodsDetails.Length == 1)
			{
				declaration.GoodsDetails = goodsDetails[0];
				declaration.FollowOnPages = Array.Empty<MultimodalDangerousGoodsDeclarationFollowOn>();
				return;
			}

			var followOnPages = new List<MultimodalDangerousGoodsDeclarationFollowOn>();

			foreach (var followOnGoodsDetails in goodsDetails.Skip(1))
			{
				var followOn = new MultimodalDangerousGoodsDeclarationFollowOn
				{
					Shipper = declaration.Shipper,
					TransportDocumentNumber = declaration.TransportDocumentNumber,
					ShipperReference = declaration.ShipperReference,
					FreightForwarderReference = declaration.FreightForwarderReference,
				};

				followOn.GoodsDetails = followOnGoodsDetails;
				followOnPages.Add(followOn);
			}

			declaration.GoodsDetails = goodsDetails[0];
			declaration.FollowOnPages = followOnPages;
		}

		string[] GenerateGoodsDetails()
		{
			var data = GetGoodsDetailsItems();

			const string macro =
@"{
	Name    = ""packing lines"",
	Columns = [
		{ Width = 21, Content = { " + nameof(GoodsDetailsItem.ShippingMarks) + @" } },
		{ Width = 10, LeftPadding = 2, Content = { " + nameof(GoodsDetailsItem.NumberAndTypeOfPackages) + @" } },
		{ Width = 35, LeftPadding = 2, Content = { none } },
		{ Width = 14, LeftPadding = 2, Content = { " + nameof(GoodsDetailsItem.GrossMass) + @" } },
		{ Width = 14, LeftPadding = 2, Content = { " + nameof(GoodsDetailsItem.NetMass) + @" } },
		{ Width = 16, LeftPadding = 2, Content = { " + nameof(GoodsDetailsItem.Cube) + @" } }
	],
	Sections = [Table({
		Name    = ""dangerous goods"",
		Data    = { DangerousGoods },
		Columns = [
			{ Width = 21, Content = { none } },
			{ Width = 10, LeftPadding = 2, Content = { " + nameof(DangerousGoodsDetailsItem.NumberAndTypeOfPackages) + @" } },
			{ Width = 35, LeftPadding = 2, Content = { " + nameof(DangerousGoodsDetailsItem.Description) + @" } },
			{ Width = 14, LeftPadding = 2, Content = { none } },
			{ Width = 14, LeftPadding = 2, Content = { " + nameof(DangerousGoodsDetailsItem.Weight) + @" } },
			{ Width = 16, LeftPadding = 2, Content = { " + nameof(DangerousGoodsDetailsItem.Volume) + @" } }
		]
	})]
}";

			var textGenerator = new TextGenerator();

			int GetPageHeight(int pageNumber)
			{
				return pageNumber == 1
					? 20
					: 40;
			}

			return textGenerator.Generate(data, macro, GetPageHeight);
		}

		GoodsDetailsItem[] GetGoodsDetailsItems()
		{
			var items = new List<GoodsDetailsItem>();

			foreach (var packLine in container.PackLines.Cast<PackLine>())
			{
				var undgs = GetRequiredUNDGItems(packLine);
				if (undgs.Count == 0)
				{
					continue;
				}

				items.Add(GetDetailsItem(packLine, undgs));
			}

			return items.ToArray();
		}

		GoodsDetailsItem GetDetailsItem(PackLine packLine, List<UNDGDataItem> undgs)
		{
			var actualWeight = Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, UnitOfWeight);
			var actualVolume = Core.Constants.Volume.Convert(packLine.JL_ActualVolume, packLine.JL_ActualVolumeUQ, UnitOfVolume);
			var netMass = undgs.Count > 1 ? undgs.Sum(undg => Core.Constants.Weight.Convert(undg.DI_DGWeight, undg.DI_UnitOfWeight, UnitOfWeight)) : 0m;

			var item = new GoodsDetailsItem
			{
				ShippingMarks = packLine.JL_MarksAndNumbers,
				NumberAndTypeOfPackages = FormateNumberAndTypeOfPackages(packLine.JL_PackageCount, packLine.JL_F3_NKPackType),
				GrossMass = packLine.JL_ActualWeight > 0
					? FormattableString.Invariant($"{actualWeight:0.00} {UnitOfWeight}")
					: string.Empty,
				NetMass = netMass > 0
					? FormattableString.Invariant($"{netMass:0.00} {UnitOfWeight}")
					: string.Empty,
				Cube = packLine.JL_ActualVolume > 0
					? FormattableString.Invariant($"{actualVolume:0.00} {UnitOfVolume}")
					: string.Empty,
				DangerousGoods = undgs.Count > 0
					? undgs.Select(GetDangerousGoodsDetailsItem).ToArray()
					: null,
			};

			return item;
		}

		List<UNDGDataItem> GetRequiredUNDGItems(PackLine packLine)
		{
			var supportedStandards = Transports
				.Select(transport => DGStandardCalculator.GetAllValidStandardsForTransport(transport))
				.SelectMany(standard => standard).ToList();

			var validForAllStandards = new List<ZString>
			{
				UNDGSubstanceStandardTypes.JTT,
				UNDGSubstanceStandardTypes.ADN,
				UNDGSubstanceStandardTypes.CFR
			};

			return packLine.UNDGs.OfType<UNDGDataItem>().Where(item =>
			{
				var dgStandard = item.Substance?.DG_Standard ?? ZString.Empty;
				if (dgStandard.IsEmpty)
				{
					return false;
				}
				if (validForAllStandards.Contains(dgStandard))
				{
					item.Validation.ValidateDI_DG();
					return !item.DI_DGInfo.HasErrors();
				}
				return supportedStandards.Contains(dgStandard);
		}).ToList();
		}

		DangerousGoodsDetailsItem GetDangerousGoodsDetailsItem(UNDGDataItem undg)
		{
			var dgWeight = Core.Constants.Weight.Convert(undg.DI_DGWeight, undg.DI_UnitOfWeight, UnitOfWeight);
			var dgVolume = Core.Constants.Volume.Convert(undg.DI_DGVolume, undg.DI_UnitOfVolume, UnitOfVolume);

			return new DangerousGoodsDetailsItem
			{
				NumberAndTypeOfPackages = CreateNumberAndTypeOfPackages(undg),
				Description = CreateDangerousGoodDescription(undg),
				Weight = undg.DI_DGWeight > 0 || !undg.DI_UnitOfWeight.IsEmpty
					? FormattableString.Invariant($"{dgWeight:0.00} {UnitOfWeight}")
					: string.Empty,
				Volume = undg.DI_DGVolume > 0 || !undg.DI_UnitOfVolume.IsEmpty
					? FormattableString.Invariant($"{dgVolume:0.00} {UnitOfVolume}")
					: string.Empty
			};
		}

		string CreateDangerousGoodDescription(UNDGDataItem undg)
		{
			var infos = GetInfosForDescription(undg)
				.Where(i => !string.IsNullOrWhiteSpace(i));

			var commaSeparatedInfos = GetCommaSeparatedInfosForDescription(undg)
				.Where(i => !string.IsNullOrWhiteSpace(i));

			var spacedDescription = string.Join(" ", infos);

			return string.Join(", ", commaSeparatedInfos.Prepend(spacedDescription));
		}

		string CreateNumberAndTypeOfPackages(UNDGDataItem undg)
		{
			return FormateNumberAndTypeOfPackages(undg.DI_PackageCount, undg.DI_F3_NKPackType);
		}

		string FormateNumberAndTypeOfPackages(ZInt packageCount, ZString packType)
		{
			return packageCount == 0 && packType.IsEmpty ? string.Empty : FormattableString.Invariant($"{packageCount} {packType}");
		}

		IEnumerable<string> GetInfosForDescription(UNDGDataItem undg)
		{
			yield return FormattableString.Invariant($"UN{undg.Substance?.DG_UNNO}");
			yield return undg.Substance?.DG_PSN;
			yield return undg.DI_TechnicalName;

			if (!undg.DI_IMOClass.IsEmpty)
			{
				yield return FormattableString.Invariant($"CLASS {undg.DI_IMOClass}");
			}

			if (!undg.Substance?.DG_PG.IsEmpty ?? false)
			{
				yield return FormattableString.Invariant($"PG {undg.Substance.DG_PG}");
			}

			if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
			{
				if (undg.DI_IsCombustible)
				{
					yield return FormattableString.Invariant($"({undg?.DI_DGFlashPoint:0.0}{Core.Constants.Temperature.Centigrade} c.c.)");
				}
			}
			else
			{
				yield return FormattableString.Invariant($"({undg?.DI_DGFlashPoint:0.0}{Core.Constants.Temperature.Centigrade} c.c.)");
			}

			if (undg.DI_IsLimitedQuantity && undg is IDangerousGood dangerousGood)
			{
				yield return dangerousGood.LimitedQuantityDescription();
			}

			if (!undg.Substance?.DG_EMS.IsEmpty ?? false)
			{
				yield return FormattableString.Invariant($"{undg.Substance.DG_EMS}");
			}

			var undgDocObject = new DangerousGoodBuilder().Build(undg, null);
			var radioactiveComponents = undgDocObject.GetRadioactiveSummary();
			yield return string.Join(", ", radioactiveComponents.Where(component => !string.IsNullOrEmpty(component)));
		}

		IEnumerable<string> GetCommaSeparatedInfosForDescription(UNDGDataItem undg)
		{
			if (undg.DI_NECWeight > 0 || !undg.DI_NECWeightUQ.IsEmpty)
			{
				var dgNECWeight = Core.Constants.Weight.Convert(undg.DI_NECWeight, undg.DI_NECWeightUQ, UnitOfWeight);
				yield return FormattableString.Invariant($"NEC: {dgNECWeight:0.00} {UnitOfWeight}");
			}

			yield return string.Empty;
		}

		sealed class GoodsDetailsItem
		{
			public string ShippingMarks { get; set; }
			public string NumberAndTypeOfPackages { get; set; }
			public string GrossMass { get; set; }
			public string NetMass { get; set; }
			public string Cube { get; set; }

			public IReadOnlyCollection<DangerousGoodsDetailsItem> DangerousGoods { get; set; }
		}

		sealed class DangerousGoodsDetailsItem
		{
			public string NumberAndTypeOfPackages { get; set; }
			public string Description { get; set; }
			public string Weight { get; set; }
			public string Volume { get; set; }
		}

		#endregion
		#endregion
	}
}
