using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderRatingAdapter<T> : RatingAdapter<T>
		where T : BusinessObject, IJobHeaderParent, IJobNumber, IYardWorkOrderForRating, ICYDJobInvoicingSupporter
	{
		public MNRWorkOrderHeaderRatingAdapter(T parent, MNRWorkOrderLine jobLine) : base(parent)
		{
			JobLine = jobLine;
		}

		public override AdapterType AdapterType => AdapterType.ContainerYard;
		public MNRWorkOrderLine JobLine { get; set; }

		public override void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
		{
			base.OnAutoRated(charges);
			var newCharges = new List<AutoRateInfo>();

			foreach (var charge in charges.OfType<AutoRateInfo>())
			{
				var orgHeader = DetermineDebtorOrg(charge, newCharges);
				if (orgHeader != null)
				{
					charge.DebtorOverridePK = orgHeader.PK;
				}
			}

			if (charges is AutoRateInfoCollection autoRatedInfoCollection)
			{
				autoRatedInfoCollection.CheckAndAddRange(newCharges);
			}
		}

		OrgHeader DetermineDebtorOrg(AutoRateInfo charge, List<AutoRateInfo> newCharges)
		{
			var responsibleParty = (string)JobLine.MWL_ResponsibleParty;

			return responsibleParty switch
			{
				ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Owner =>
					JobLine.WorkOrderHeader.Client,

				ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Lessee =>
					JobLine.WorkOrderHeader.Lessee,

				ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.Insurer =>
					HandleInsurerCharge(charge, newCharges),

				ContainerYardConstants.MaintenanceAndRepair.ResponsibleParty.ThirdParty =>
					JobLine.WorkOrderHeader.ThirdParty,

				_ => null,
			};
		}

		OrgHeader HandleInsurerCharge(AutoRateInfo charge, List<AutoRateInfo> newCharges)
		{
			var responsibleOrgHeader = JobLine.WorkOrderHeader.Insurer;
			if (JobLine.WorkOrderHeader.HasDPP)
			{
				if (JobLine.WorkOrderHeader.DPPAmount == 0)
				{
					responsibleOrgHeader = JobLine.WorkOrderHeader.Lessee;
				}
				else
				{
					var dppAmount = JobLine.WorkOrderHeader.DPPAmount;
					var chargeAmount = charge.Amount;

					if (chargeAmount <= dppAmount)
					{
						JobLine.WorkOrderHeader.DPPAmount -= chargeAmount;
					}
					else
					{
						var insurerExcess = chargeAmount - dppAmount;
						var additionalCharge = CreateExcessCharge(charge, insurerExcess);
						newCharges.Add(additionalCharge);
						AdjustOriginalInsurerCharge(charge);
						JobLine.WorkOrderHeader.DPPAmount = 0;
					}
				}
			}

			return responsibleOrgHeader;
		}

		AutoRateInfo CreateExcessCharge(AutoRateInfo originalCharge, decimal excessAmount)
		{
			var ratingCriteria = new RatingCriteria(this, originalCharge.Factory);
			var calcParams = new AutoRatingCalculatorParametersWithoutFilter(
				ratingCriteria,
				new FreightAutoRater(new RatingContext())
			);

			var paymentsList = new List<PaymentBasis>
			{
				ratingCriteria.CreatePaymentBasis(
				RateInfo.CreateFLT(excessAmount, originalCharge.Line.TL_RX_NKCurrency),
				default
				)
			};

			var calcResult = new CalculationResult(originalCharge.Line, new CalculatorOutput(paymentsList));
			var newCharge = new AutoRateInfo(calcResult, calcParams, originalCharge.Factory)
			{
				DebtorOverridePK = JobLine.WorkOrderHeader.Lessee.PK
			};

			return newCharge;
		}

		void AdjustOriginalInsurerCharge(AutoRateInfo charge)
		{
			var ratingCriteria = new RatingCriteria(this, charge.Factory);
			var originalBasis = charge.Bases[0];
			var adjustedBasis = ratingCriteria.CreatePaymentBasis(
				RateInfo.CreateFLT(JobLine.WorkOrderHeader.DPPAmount, charge.Line.TL_RX_NKCurrency),
				default
			);

			charge.Bases.Add(adjustedBasis);
			charge.Bases.Remove(originalBasis);
		}

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return new CYDJobInvoicingSupporter<T>(Parent); }
		}

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new MNRWorkOrderHeaderDatesProvider<T>(Parent); }
		}

		#endregion

		#region ChargeCodeGroups

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				if (chargeCodeGroups == null)
				{
					chargeCodeGroups = new ChargeCodeGroupCollection();
					chargeCodeGroups.AddRange(new[] { ChargeCodeGroupList.Codes.MNRWorkOrderHeader, ChargeCodeGroupList.Codes.LabourHourRate });
				}
				return chargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection chargeCodeGroups;

		#endregion

		#region RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.ContainerYard; }
		}

		#endregion

		#region MergeCharges

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.CrossAdapter; }
		}

		#endregion

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.MNRWorkOrderHeader; }
		}

		#endregion

		#region IAutoRatingOrganisations Members

		#region Creditors

		public override Creditors Creditors
		{
			get
			{
				var result = new List<OrgWithSource>();

				if (Parent.Yard != null && Parent.Yard.WarehouseAddress != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgAddress>(Parent.Yard.WW_OA_WarehouseAddressInfo));
				}

				return Creditors.New(result);
			}
		}

		#endregion

		#region Measures

		public override IRateableMeasureSet RateableMeasures => GetRateableMeasures();

		public IRateableMeasureSet GetRateableMeasures()
		{
			var rateableMeasures = new RateableMeasureSet(AdapterType);
			rateableMeasures.SetUnidentifiedQuantityForWarehouse(0, Parent.Yard.PK);

			Action<RateableMeasureSet> lazyWorkOrders = measures =>
			{
				measures.AddYardWorkOrder(
					JobLine.PK,
					JobLine.Area,
					JobLine.ChargingByLength ? JobLine.LinearLength : JobLine.Perimeter,
					JobLine.MWL_MaterialQuantity.ToZInt(),
					JobLine.HasLaborHours ? new TimeSpan(JobLine.MWL_LaborHours.ToZInt(), 0, 0) : null,
					Parent.Yard.PK,
					JobLine.ComponentCode.PK,
					JobLine.UnitSection.GroupCode,
					JobLine.RepairCode.PK,
					JobLine.Material.PK
				);
			};

			rateableMeasures.CreateYardWorkOrders(lazyWorkOrders);

			return rateableMeasures;
		}

		#endregion

		#region ServiceLevel

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				return new ServiceLevelRatingInformation(Array.Empty<ServiceLevelInfo>());
			}
		}

		#endregion

		#region DebtorOrgs

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection
				{
					[RatingDebtorOrgTypes.CNR] = Parent.Client,
					[RatingDebtorOrgTypes.CNE] = Parent.Client,
					[RatingDebtorOrgTypes.CCUS] = Parent.Client,
					[RatingDebtorOrgTypes.LC] = Parent.Client,
				};

				return result;
			}
		}

		#endregion

		#endregion
	}
}
