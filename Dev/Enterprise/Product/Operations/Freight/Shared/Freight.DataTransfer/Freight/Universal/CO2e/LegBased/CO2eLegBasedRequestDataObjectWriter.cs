using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public abstract class CO2eLegBasedRequestDataObjectWriter<TCO2ECalculationSupporter, TLeg> : CO2eRequestDataObjectWriter<TCO2ECalculationSupporter>
		where TCO2ECalculationSupporter : BusinessObject, ICO2eLegBasedSupporter
		where TLeg : BusinessObject
	{
		protected CO2eLegBasedRequestDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected CO2eLegBasedRequestDataObjectWriter(IDataWritingManager writeManager, ICO2eCalculationSupporter hostSupporter) : base(writeManager, hostSupporter)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override void PopulateDataObject(TCO2ECalculationSupporter sourceBO, UniversalShipment shipmentData)
		{
			base.PopulateDataObject(sourceBO, shipmentData);
			PopulateTopLevelData(shipmentData, sourceBO);
			PopulateLegs(shipmentData, sourceBO);
			PopulateSubShipments(shipmentData, sourceBO);
			if (IncludePrePostCarriageLegs(sourceBO))
			{
				PopulatePrePostCarriageLegs(shipmentData.SetPreCarriageShipmentCollection, () => GetCarriageCollection(sourceBO, GetPreCarriageLegs(sourceBO)));
				PopulatePrePostCarriageLegs(shipmentData.SetPostCarriageShipmentCollection, () => GetCarriageCollection(sourceBO, GetPostCarriageLegs(sourceBO)));
			}
		}

		protected virtual bool IncludePrePostCarriageLegs(TCO2ECalculationSupporter sourceBO) => sourceBO.RequiresPrePostCarriageLegs;

		protected override void PopulateWorkflowCustomFields(TCO2ECalculationSupporter sourceBO, ref UniversalShipment dataObject)
		{
		}

		protected override void PopulateCosts(TCO2ECalculationSupporter sourceBO, UniversalShipment dataObject)
		{
		}

		protected override void PopulateWorkflowRelatedProperties(TCO2ECalculationSupporter sourceBO, UniversalShipment dataObject)
		{
		}

		void PopulateTopLevelData(UniversalShipment shipmentData, TCO2ECalculationSupporter sourceBO)
		{
			PopulateWeightAndTEU(shipmentData, sourceBO);
			PopulateContainerMode(shipmentData, sourceBO);

			var transportMode = sourceBO.ConvertTransportModeForCO2eCalculation();
			shipmentData.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(
				transportMode,
				new CodeDescriptionPairList(OLookUpEditType.TransportType));

			shipmentData.PortOfLoading = GetUNLOCOByPortAndTransportMode(sourceBO.LoadPort, sourceBO.ConvertTransportModeForCO2eCalculation());
			shipmentData.PortOfDischarge = GetUNLOCOByPortAndTransportMode(sourceBO.DischargePort, sourceBO.ConvertTransportModeForCO2eCalculation());

			shipmentData.SetDateCollection(() =>
			{
				var dates = CreateDates(sourceBO).ToList();
				return dates.Any() ? dates : null;
			});

			if (sourceBO.RequiresTemperatureControl)
			{
				shipmentData.RequiresTemperatureControl = sourceBO.RequiresTemperatureControl;
			}
		}

		protected virtual void PopulateWeightAndTEU(UniversalShipment shipmentData, TCO2ECalculationSupporter sourceBO)
		{
			shipmentData.TotalWeight = sourceBO.Weight;
			shipmentData.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(sourceBO.UnitOfWeight, BindToLists.GetCachedLists(sourceBO.Factory).WeightUnits);

			if (sourceBO.IncludeTEU)
			{
				shipmentData.TEU = new TEU()
				{
					NumberOfTEU = sourceBO.NumberOfTEU,
					TonnesPerTEU = Utilities.Round(sourceBO.TonnesPerTEU, 6),
					ContainerEmptyWeightPerTEU = Utilities.Round(sourceBO.ContainerEmptyWeightPerTEU, 6),
					ContainerEmptyWeightPerTEUUnit = ListHelper.GetWithDescription<UnitOfWeight>(sourceBO.ContainerEmptyWeightPerTEUUnit, BindToLists.GetCachedLists(sourceBO.Factory).WeightUnits)
				};
			}
		}

		void PopulateContainerMode(UniversalShipment shipmentData, TCO2ECalculationSupporter sourceBO)
		{
			if (!sourceBO.ContainerMode.IsEmpty)
			{
				shipmentData.ContainerMode = ListHelper.GetWithDescription<ContainerMode>(
					sourceBO.ConvertContainerModeForCO2eCalculation(),
					new CodeDescriptionPairList(OLookUpEditType.ContainerMode));
			}
		}

		protected virtual bool LegsOnly => false;
		protected virtual bool UseRoadForFirstAndLastLegs => true;

		protected virtual void PopulateLegs(UniversalShipment shipmentData, TCO2ECalculationSupporter sourceBO)
		{
			if (sourceBO.SupportVirtualLegs)
			{
				var completeLegs = new CO2eCompleteChainOfLegsProvider(sourceBO, UseRoadForFirstAndLastLegs).GetChain();

				shipmentData.SetTransportLegCollection(() =>
				{
					var result = new DataObjectList<TransportLeg> { Content = CollectionContent.Complete };
					var writer = GetLegDataObjectWriter();
					foreach (var leg in completeLegs)
					{
						TransportLeg dataObject = null;
						if (leg.IsVirtual)
						{
							dataObject = GetTransportLegDataObjectForVirtualLeg(leg, sourceBO);
						}
						else if (leg.ActualLeg is TLeg legBO)
						{
							dataObject = writer.GetDataObject(legBO);
						}

						if (dataObject != null)
						{
							var unlocoLoader = new RefUNLOCO.Loader(sourceBO.Factory);
							if (leg.From?.IsPort == true)
							{
								dataObject.PortOfLoading = GetUNLOCOByPortAndTransportMode(unlocoLoader.Load(leg.From.UNLOCO), leg.TransportMode);
							}
							if (leg.To?.IsPort == true)
							{
								dataObject.PortOfDischarge = GetUNLOCOByPortAndTransportMode(unlocoLoader.Load(leg.To.UNLOCO), leg.TransportMode);
							}
							PopulateFlightNoFromCarrierIfEmpty(leg, dataObject);
							result.Add(dataObject);
						}
					}
					return result;
				});
			}
			else
			{
				shipmentData.SetTransportLegCollection(() =>
					ProcessCollection(sourceBO.Legs,
						GetLegDataObjectWriter(), CollectionContent.Complete, true)
					?? new DataObjectList<TransportLeg> { Content = CollectionContent.Complete });
			}
		}

		protected virtual void PopulateSubShipments(UniversalShipment shipmentData, TCO2ECalculationSupporter sourceBO)
		{
		}

		protected void PopulateFlightNoFromCarrierIfEmpty(PrePostCarriageLegWrapper leg, TransportLeg dataObject)
		{
			if (!leg.IsVirtual && leg.TransportMode == Core.Constants.TransportModes.Air && string.IsNullOrEmpty(dataObject.VoyageFlightNo))
			{
				dataObject.VoyageFlightNo = leg.ActualLeg.Carrier?.MiscServ?.AirlineTwoCharacterCode;
			}
		}

		protected virtual TransportLeg GetTransportLegDataObjectForVirtualLeg(PrePostCarriageLegWrapper leg, TCO2ECalculationSupporter sourceBO)
		{
			return sourceBO.SupportVirtualLegs
				? leg.ToTransportLegDO(writeManager.WriterStrategy, sourceBO.Factory)
				: null;
		}

		protected virtual IEnumerable<PrePostCarriageLegWrapper> GetPreCarriageLegs(TCO2ECalculationSupporter sourceBO) => sourceBO.GetPreCarriageLegs();

		protected virtual IEnumerable<PrePostCarriageLegWrapper> GetPostCarriageLegs(TCO2ECalculationSupporter sourceBO) => sourceBO.GetPostCarriageLegs();

		void PopulatePrePostCarriageLegs(Func<Func<List<UniversalShipment>>, bool> setCollection, Func<List<UniversalShipment>> getCollection)
		{
			setCollection(() =>
			{
				var result = getCollection();
				if (result.Count == 0 || result[0].TransportLegCollection == null || result[0].TransportLegCollection.Count == 0)
				{
					return null;
				}
				return result;
			});
		}

		List<UniversalShipment> GetCarriageCollection(TCO2ECalculationSupporter sourceBO, IEnumerable<PrePostCarriageLegWrapper> carriageLegs)
		{
			var carriageShipment = new UniversalShipment(writeManager.WriterStrategy);
			carriageShipment.SetTransportLegCollection(() =>
			{
				var result = new DataObjectList<TransportLeg> { Content = CollectionContent.Complete };
				carriageLegs.ForEach(leg => result.Add(leg.ToTransportLegDO(writeManager.WriterStrategy, sourceBO.Factory)));
				return result;
			});

			return new List<UniversalShipment> { carriageShipment };
		}

		protected abstract DataObjectWriter<TLeg, TransportLeg> GetLegDataObjectWriter();

		protected abstract override DataContextType GetTopLevelDataContextType();

		protected static UNLOCO GetUNLOCOByPortAndTransportMode(IRefUNLOCO unloco, ZString transportMode)
		{
			if (unloco == null)
			{
				return null;
			}

			if (transportMode == Core.Constants.TransportModes.Air && !unloco.RL_IATA.IsEmpty)
			{
				return new UNLOCO { Code = unloco.RL_IATA, Name = unloco.RL_PortName };
			}

			return UNLOCO.New(unloco);
		}

		IEnumerable<Date> CreateDates(TCO2ECalculationSupporter sourceBO)
		{
			if (sourceBO.ETA.IsValid)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					IsEstimate = true,
					Value = sourceBO.ETA
				};
			}
			if (sourceBO.ETD.IsValid)
			{
				yield return new Date
				{
					Type = DateType.Departure,
					IsEstimate = true,
					Value = sourceBO.ETD
				};
			}
		}
	}
}
