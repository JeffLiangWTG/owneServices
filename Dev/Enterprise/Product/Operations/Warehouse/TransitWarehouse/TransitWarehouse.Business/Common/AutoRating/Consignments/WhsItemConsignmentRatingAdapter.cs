using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transit.Business.Common;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemConsignmentRatingAdapter<T> : RatingAdapter<T>, IAutoRatingFreightConditionsSupportable
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitConsignmentForRating,
		IHaveServices,
		ITransitJobInvoicingPlugIn,
		ITransitJobForConsolCosting
	{
		public WhsItemConsignmentRatingAdapter(T parent, IEnumerable<TransitPackageForRatingInfo> transitPackagesForRating = null)
			: base(parent)
		{
			TransitPackagesForRating = transitPackagesForRating ?? parent.TransitPackagesForRating;
		}

		IEnumerable<TransitPackageForRatingInfo> TransitPackagesForRating { get; }

		#region IAutoRating Members

		#region AdapterType

		public override AdapterType AdapterType => AdapterType.TransitWarehouse;

		#endregion

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return new WhsItemConsignmentInvoicingSupporter<T>(Parent); }
		}

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new TransitJobDatesProvider<T>(Parent); }
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
					chargeCodeGroups.AddRange(new[] { Parent.ChargeCodeGroup });
					chargeCodeGroups.AddRange(new[] { ChargeCodeGroupList.Codes.WHSStorage });
				}
				return chargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection chargeCodeGroups;

		#endregion

		#region JobServices

		public override JobServicesCollection JobServices
		{
			get
			{
				var result = new JobServicesCollection();
				result.AddRange(GetServiceInfosFromJobServices(Parent.Services, Parent.ChargeCodeGroup));
				return result;
			}
		}

		#endregion

		#region RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.TransitWarehouse; }
		}

		#endregion

		#region MergeCharges

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		#endregion

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return Parent.ConsumerType; }
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		#region Creditors

		public override Creditors Creditors
		{
			get
			{
				var result = new List<OrgWithSource>();

				if (Parent.Warehouse != null && Parent.Warehouse.WarehouseAddress != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgAddress>(Parent.Warehouse.WW_OA_WarehouseAddressInfo));
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
			rateableMeasures.SetUnidentifiedQuantityForWarehouse(0, Parent.Warehouse.PK);

			Action<RateableMeasureSet> lazyPackageStorage = measures =>
			{
				foreach (var transitPackage in TransitPackagesForRating)
				{
					var weightInKG = TransitWarehouseHelper.GetWeightInKG(transitPackage);
					var volumeInM3 = TransitWarehouseHelper.GetVolumeInM3(transitPackage);
					var packageQty = (ZDecimal)transitPackage.PackageQty;
					var packtype = transitPackage.PackageType;
					var commodityCode = transitPackage.CommodityCode;
					measures.AddWarehousePackage(weightInKG, volumeInM3, packageQty, Parent.Warehouse.PK, packtype, commodityCode);
				}
			};

			rateableMeasures.CreateWarehousePackageList(lazyPackageStorage);

			return rateableMeasures;
		}

		#endregion

		#region ServiceLevel

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				return new ServiceLevelRatingInformation(
					new ServiceLevelInfo(Parent.ServiceLevel, ServiceLevelType.Client),
					new ServiceLevelInfo(Parent.ServiceLevel, ServiceLevelType.Carrier));
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
					[RatingDebtorOrgTypes.CNR] = Parent.ConsignorDocAddress.Organisation,
					[RatingDebtorOrgTypes.CNE] = Parent.ConsigneeDocAddress.Organisation,
					[RatingDebtorOrgTypes.CCUS] = Parent.BookingPartyDocAddress.Organisation
				};

				var localClient = Parent.JobHeader?.LocalCharges;
				if (localClient != null)
				{
					result[RatingDebtorOrgTypes.LC] = localClient;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region IAutoRatingFreightConditionsSupportable Members

		public RateLineConditionsSupporter ConditionsSupporter => conditionsSupporter ?? (conditionsSupporter = new TransitConditionsSupporter<T>(Parent));

		RateLineConditionsSupporter conditionsSupporter;

		#endregion
	}
}
