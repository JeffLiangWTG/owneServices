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
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitsRatingAdapter<TParent, TRatingUnits>(TParent parent, TRatingUnits unitsForRating, CYDYardUnitState yardUnit) : RatingAdapter<TParent>(parent), IAutoRatingWarehouseInfo, IRatingAdapterHumanReadableName
		where TParent : BusinessObject, IJobHeaderParent, IJobNumber, IJobInvoicingPlugIn
		where TRatingUnits : ICYDYardUnitsForRating
	{
		public override AdapterType AdapterType => AdapterType.ContainerYard;

		public override FreightMode FreightMode => FreightMode.ROA;

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ??= Parent.InvoicingSupporter; }
		}

		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return jobDatesProvider ?? (jobDatesProvider = new CYDYardRatingDatesProvider<TParent>(Parent, yardUnit)); }
		}

		IJobDatesProvider jobDatesProvider;

		#endregion

		#region ChargeCodeGroups

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				if (chargeCodeGroups == null)
				{
					chargeCodeGroups = new ChargeCodeGroupCollection();
					chargeCodeGroups.AddRange(unitsForRating.ChargeCodeGroupList.ToArray());
				}
				return chargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection chargeCodeGroups;

		#endregion

		#region RateTypeToUse

		public override RateType RateTypeToUse
		{
			get
			{
				return Parent is CYDTransportationUnit
					? RateType.ContainerYardTransportationUnit
					: RateType.ContainerYard;
			}
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
			get { return InvoicingSupporter.ConsumerType; }
		}

		#endregion

		#region IAutoRatingOrganisations Members

		#region Creditors

		public override Creditors Creditors
		{
			get
			{
				var result = new List<OrgWithSource>();
				result.Add(OrgWithSource.NewFrom<OrgAddress>(yardUnit.CurrentYard.WW_OA_WarehouseAddressInfo));

				return Creditors.New(result);
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
					[RatingDebtorOrgTypes.CNR] = unitsForRating.Client,
					[RatingDebtorOrgTypes.CNE] = unitsForRating.Client,
					[RatingDebtorOrgTypes.CCUS] = unitsForRating.Client,
					[RatingDebtorOrgTypes.LC] = unitsForRating.Client,
				};

				return result;
			}
		}

		#endregion

		#endregion

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var rateableMeasures = new RateableMeasureSet(AdapterType);
				var container = new RateableContainer
				{
					UnitCount = 1,
					PackageType = PkgUnit.Unit,
					ContainerNumber = yardUnit.YUS_UnitID,
					ContainerTypePk = yardUnit.Delivery.UnitLineItem.YLI_RC_ContainerType.IsEmpty ? null : yardUnit.Delivery.UnitLineItem.YLI_RC_ContainerType.ToGuid(),
					YardUnitType = yardUnit.Delivery.UnitLineItem.YLI_Type,
					YardUnitLoad = yardUnit.Delivery.UnitLineItem.YLI_IsEmpty ? ContainerYardConstants.YardUnitLoad.Codes.EMP : ContainerYardConstants.YardUnitLoad.Codes.LAD,
					WarehousePk = yardUnit.YUS_WW_CurrentYard.ToGuid()
				};
				var partList = new RateablePartList
				{
					HasPackageType = true,
					HasContainerNumber = true,
					HasContainerType = true,
					HasWarehouse = true,
					HasYardUnitType = true,
					HasYardUnitLoad = true,
				};
				partList.AddPart(container);
				rateableMeasures.AddPartList(MeasureType.Unit, partList);
				rateableMeasures.AddPartList(MeasureType.ContainerCount, partList);

				return rateableMeasures;
			}
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

		#region IAutoRatingWarehouseInfo Members

		public ZGuid WarehousePK => unitsForRating.Yard.PK;

		public OrgHeader WarehouseFallbackConsignorForFilterOnly => null;

		#endregion

		#region IRatingAdapterHumanReadableName Members

		ZString IRatingAdapterHumanReadableName.HumanReadableName => yardUnit.YUS_UnitID;

		#endregion
	}
}
