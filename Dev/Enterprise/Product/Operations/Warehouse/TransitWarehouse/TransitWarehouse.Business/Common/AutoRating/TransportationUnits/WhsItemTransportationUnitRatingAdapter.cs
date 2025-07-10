using System;
using System.Collections.Generic;
using System.Linq;
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
	public class WhsItemTransportationUnitRatingAdapter<T> : RatingAdapter<T>, IAutoRatingFreightConditionsSupportable
		where T : BusinessObject,
		IJobHeaderParent,
		IJobNumber,
		ITransitTransportationUnitForRating,
		ITransitJobInvoicingPlugIn
	{
		public WhsItemTransportationUnitRatingAdapter(T parent)
			: base(parent)
		{
		}

		#region IAutoRating Members

		#region AdapterType

		public override AdapterType AdapterType => AdapterType.TransitWarehouse;

		#endregion

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return new TransitJobInvoicingSupporter<T>(Parent); }
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
				}
				return chargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection chargeCodeGroups;

		#endregion

		#region RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.TransitWarehouseTransportationUnit; }
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

		#region RatingAdapter Members

		public override FreightMode FreightMode => Parent.FreightMode ?? base.FreightMode;

		public override ZString ContainerMode => Parent.ContainerForRating?.Key != null ? Core.Constants.ContainerModes.FCL : Core.Constants.ContainerModes.FTL;

		public override OrgHeader Carrier => TransitWarehouseHelper.GetTransportCompanyOrganisation(Parent.TransportCompanyDocAddress);

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var rateableMeasures = new RateableMeasureSet(AdapterType);
				rateableMeasures.SetUnidentifiedQuantityForWarehouse(0, Parent.Warehouse.PK);

				var transitPackagesForRating = Parent.TransitPackagesForRating;
				Action<RateableMeasureSet> lazyPackageStorage = measures =>
				{
					foreach (var transitPackage in transitPackagesForRating)
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

				var containerForRating = Parent.ContainerForRating;
				var containerPackage = containerForRating?.Key;
				if (containerPackage != null)
				{
					var isOnPallets = containerForRating?.Value ?? false;
					var containerInfo = new MeasureInfo.ContainerInfo(packages: transitPackagesForRating.Count(), containerPK: containerPackage.ContainerType.PK);
					rateableMeasures.AddContainerGroup(containerPackage.ContainerType.PK, containerPackage.Package.KP_RH_NKCommodityCode, isOnPallets, "", new[] { containerInfo });
				}

				return rateableMeasures;
			}
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

				if (TransitWarehouseHelper.GetTransportCompanyOrganisation(Parent.TransportCompanyDocAddress) != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgHeader>(Parent.TransportCompanyDocAddress.OrganisationPKInfo));
				}

				if (Parent.Warehouse?.WarehouseAddress != null)
				{
					result.Add(OrgWithSource.NewFrom<OrgAddress>(Parent.Warehouse.WW_OA_WarehouseAddressInfo));
				}

				return Creditors.New(result);
			}
		}

		#endregion

		#region DebtorOrgs

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();

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

		public RateLineConditionsSupporter ConditionsSupporter => new TransitConditionsSupporter<T>(Parent);

		#endregion
	}
}
