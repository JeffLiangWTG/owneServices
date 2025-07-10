using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Business
{
	public class ConsolLeadShipmentRatingAdapter : ShipmentRatingAdapter<CommonShipment>
	{
		public ConsolLeadShipmentRatingAdapter(CommonShipment masterShipment, CommonShipment thisShipment)
			: base(masterShipment)
		{
			this.masterShipment = Argument.NotNull(masterShipment, "masterShipment");
			this.ThisShipment = thisShipment;

			if (!masterShipment.IsBuyersConsolLead && !masterShipment.IsShippersConsolLead)
			{
				throw new NotSupportedException("The ConsolLead class should not be instantiated for non Buyers or Shippers Consol shipments.");
			}
		}

		#region RatingAdapter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get
			{
				return ThisShipment != null
					? ThisShipment.InvoicingSupporter
					: masterShipment.InvoicingSupporter;
			}
		}

		public override AutoRatingStatusInfo StatusInformation
		{
			get
			{
				var autoRatingInfo = AutoRatingAdapters.FirstOrDefault(shipmentAdapterStatus => !shipmentAdapterStatus.StatusInformation.CanExecute);

				return autoRatingInfo != null
						? autoRatingInfo.StatusInformation
						: new AutoRatingStatusInfo(true);
			}
		}

		#region JobServices

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = GetJobServiceInfos(masterShipment, isMasterShipment: true);
				result.AddRange(GetJobServiceInfos(ThisShipment, isMasterShipment: false));
				return result;
			}
		}

		JobServicesCollection GetJobServiceInfos(CommonShipment shipment, bool isMasterShipment)
		{
			if (shipment != null)
			{
				var result = shipment.GetJobServices();

				var containers = Shipments.SelectMany(s => s.Containers)
					.Where(c => c.JC_ContainerMode != Constants.ContainerModes.LCL)
					.Distinct()
					.ToArray();

				var docsAndCartageServices = shipment.DocsAndCartage.Services;
				var serviceInfos = GetServiceInfosFromJobServices(docsAndCartageServices);

				foreach (var service in serviceInfos)
				{
					if (service.IsEnabled)
					{
						service.ChargeCodeGroup = shipment.GetServiceChargeGroup(service);
						result.Add(service);
					}
					else
					{
						var containerServices = FreightRatingHelper.GetServiceInfosFromContainers(containers, service.ServiceCode, FreightRatingHelper.GetServiceInfoDefault);
						result.AddRange(containerServices);
					}
				}

				result.ForEach(jobServiceInfo => jobServiceInfo.IsMasterShipment = isMasterShipment);

				return result;
			}

			return new JobServicesCollection();
		}

		#endregion

		public override MoneyType MonetaryValues
		{
			get
			{
				var result = new MoneyType();

				foreach (CommonShipment shipment in Shipments)
				{
					result.Add(MoneyType.ValueType.GoodsValue, new Money(shipment.JS_GoodsValue, shipment.GoodsValueCurr));
					result.Add(MoneyType.ValueType.InsuranceValue, new Money(shipment.JS_InsuranceValue, shipment.InsuranceCurrency));
				}

				return result;
			}
		}

		public override AdapterType AdapterType => AdapterType.Consolidation;

		public List<IAutoRating> AutoRatingAdapters
		{
			get
			{
				return autoRatingAdapters ?? (autoRatingAdapters = Shipments.Select(shipment => shipment.RatingAdapter).ToList());
			}
		}
		List<IAutoRating> autoRatingAdapters;

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				if (overridenPaymentTerm != null)
				{
					return overridenPaymentTerm;
				}

				if (ThisShipment != null && ThisShipment.ApportionConsol())
				{
					var paymentTerm = new PaymentTermInfos();

					if (!string.IsNullOrWhiteSpace(ThisShipment.JS_INCO))
					{
						var type = this.IsDomestic() ? PaymentTermType.DomesticPaymentTerm : PaymentTermType.Incoterm;
						paymentTerm.AddOrReplace(new PaymentTermInfo(type, CostSell.Cost, ThisShipment.JS_INCO));
						paymentTerm.AddOrReplace(new PaymentTermInfo(type, CostSell.Revenue, ThisShipment.JS_INCO));
					}

					return paymentTerm;
				}
				else
				{
					return base.PaymentTerm;
				}
			}
		}
		#endregion

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				if (StatusInformation.CanExecute)
				{
					FreightRatingHelper.SetCombinedMeasures(result, AutoRatingAdapters, MeasureType.Weight, MeasureType.Volume, MeasureType.Chargeable, MeasureType.Package);
				}

				result.Shipments = 1;
				result.LowestBill = ThisShipment == null || masterShipment.PK == ThisShipment.PK
					? masterShipment.CoLoadShipments.Count
					: 1;
				SetContainers(result);

				return result;
			}
		}

		void SetContainers(RateableMeasureSet measures)
		{
			var packLines = Shipments
				.SelectMany(s => s.OuterPackLines)
				.Cast<PackLine>()
				.ToList();

			var consol = Shipments.Select(x => x.GetFirstOrCorrectConsol()).WhereNotNull().FirstOrDefault();

			FreightRatingHelper.SetContainersFromPackLines(measures, consol, masterShipment, packLines);
		}

		public override ILocation RateOrigin
			=> parent.FreightRateOrigin;

		public override ILocation RateDestination
			=> parent.FreightRateDestination;

		#region IJobDataUpdater

		public override void UpdateChargeable(ZDecimal newChargeable)
		{
			// We don't want to update chargeable on BCN or SCN shipments. Even though it consolidates calculations from all sub shipments,
			// chargeable should stay on shipments and either BCN or SCN lead shipment should reflect only its portion of chargeable.
		}

		public override DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers)
		{
			var jobHeader = InvoicingSupporter.Job;
			if (jobHeader != null)
			{
				var nonBlankContractNumbers = newNumbers.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
				if (nonBlankContractNumbers.Length == 1)
				{
					var newContractNumber = nonBlankContractNumbers.First();
					if (!string.Equals(jobHeader.JH_ClientContractNumber, newContractNumber, StringComparison.OrdinalIgnoreCase))
					{
						jobHeader.JH_ClientContractNumber = newContractNumber;
						return DataUpdateResult.Updated;
					}
				}
			}

			return DataUpdateResult.NoAction;
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();

				if (masterShipment.IsShippersConsolLead)
				{
					result.AddRange(Env.Registry.Rating.ShippersConsolApportionedCodes);
				}
				else if (masterShipment.IsBuyersConsolLead)
				{
					result.AddRange(Env.Registry.Rating.BuyersConsolApportionedCodes);
				}

				return result;
			}
		}

		#endregion

		public override Collection<IBusiness> AutoRatedFor => new Collection<IBusiness> { ThisShipment ?? masterShipment };

		public override void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
		{
			if (!charges.Any())
			{
				return;
			}

			Apportion(charges);
		}

		#region Apportion

		void Apportion(IEnumerable<IAutoRatedCharge> charges)
		{
			if (ThisShipment == null)
			{
				return;
			}

			if (charges.First().CostSell != CostSell.Revenue)
			{
				// We apportion only revenue charges
				return;
			}

			var measures = (RateableMeasureSet)RateableMeasures;
			var parts = measures.GetPartList(MeasureType.Chargeable);
			var consolChargeable = parts != null
				? new Quantity(parts.Sum(p => p.ChargeableMeasure.ForClient), parts.ChargeableUnit)
				: default;

			if (consolChargeable.IsEmpty || consolChargeable.Amount == 0)
			{
				var msg = Res.GetString(
					"c3133e57-6ffa-4e75-a67b-e1faf1169eda",
					"The chargeable amount for all shipment in this Buyers Consol could not be calculated.\r\nPlease check the chargeable entered on each shipment has been specified correctly.");

				throw new AutoRaterException(msg);
			}

			var shipmentChargeable = new Quantity(ThisShipment.JS_DocumentedChargeable, ThisShipment.JS_ChargeableUnit);
			var ratio = shipmentChargeable.AmountFor(consolChargeable.Unit).Amount / consolChargeable.Amount;

			foreach (var charge in charges)
			{
				var typeOfConsolLead = ContainerMode == Constants.ContainerModes.BuyersConsol
					? Res.GetString("8f19feca-6e6c-4809-86db-182090580c85", "Buyers")
					: Res.GetString("42c430e8-503b-4251-b13e-4f3eacfcdae3", "Shippers");

				if (ShouldNotApportion(charge.CalculatorType, charge.ChargeUnit, charge.UnitFactor))
				{
					charge.Multiply(1m);
					charge.Description = AddDescription(charge.Description,
						Res.GetString("2A9CD743-B27E-4B3E-9287-572400235394",
							"This charge was autorated through a {0} Consol and not apportioned by chargeable amount because the Rate Line's Unit Factor is {1} - No Apportionment",
							typeOfConsolLead, charge.UnitFactor));
				}
				else
				{
					charge.Multiply(ratio);
					charge.Description = AddDescription(charge.Description,
						Res.GetString("a24e8762-5873-4be5-9c13-9e01da5e07a6",
							"This charge was autorated through a {0} Consol and apportioned by chargeable amount (This shipment: {1}, {0} Consol Total: {2})",
							typeOfConsolLead, shipmentChargeable.ToString(3),
							consolChargeable.ToString(3)));
				}
			}
		}

		bool ShouldNotApportion(CalculatorType calculatorType, string unit, string unitFactor)
		{
			var noApportionUnitFactor =
				unitFactor == UnitFactorList.Codes.BCN ||
				unitFactor == UnitFactorList.Codes.SCN;

			var noApportionmentUnit = unit.In(
				QuantityUnit.HB,
				QuantityUnit.LW,
				QuantityUnit.KG,
				QuantityUnit.M3,
				QuantityUnit.SV,
				QuantityUnit.HR,
				QuantityUnit.DY,
				QuantityUnit.WK);

			return noApportionUnitFactor &&
				   (calculatorType == CalculatorType.Unit && noApportionmentUnit ||
				   calculatorType == CalculatorType.Flat ||
				   calculatorType == CalculatorType.Combined);
		}

		ZString AddDescription(ZString description, ZString additionalDesc)
		{
			if (!description.IsEmpty)
			{
				return additionalDesc + "\r\n\r\n" + description;
			}

			return description;
		}

		#endregion

		#region Implementation

		readonly CommonShipment masterShipment;
		public CommonShipment ThisShipment { get; }

		List<CommonShipment> Shipments
		{
			get
			{
				if (shipments == null)
				{
					shipments = new List<CommonShipment> { masterShipment };
					shipments.AddRange(masterShipment.CoLoadShipments.Cast<CommonShipment>());
				}

				return shipments;
			}
		}
		List<CommonShipment> shipments;

		#endregion
	}
}
