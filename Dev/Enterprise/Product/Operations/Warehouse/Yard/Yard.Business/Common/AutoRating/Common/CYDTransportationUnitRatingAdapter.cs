using System;
using System.Collections.Generic;
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
	public class CYDTransportationUnitRatingAdapter<T, U>(T parent, U transportationUnitForRating) : RatingAdapter<T>(parent), IAutoRatingWarehouseInfo
		where T : BusinessObject, IJobHeaderParent, IJobNumber, ICYDJobInvoicingSupporter
		where U : ICYDYardUnitsForRating
	{
		public override AdapterType AdapterType => AdapterType.ContainerYard;

		public override FreightMode FreightMode => FreightMode.ROA;

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return invoicingSupporter ?? (invoicingSupporter = new CYDJobInvoicingSupporter<T>(Parent)); }
		}

		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return jobDatesProvider ?? (jobDatesProvider = new CYDYardTransportationUnitRatingDatesProvider<T>(Parent)); }
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
					chargeCodeGroups = [.. transportationUnitForRating.ChargeCodeGroupList];
				}
				return chargeCodeGroups;
			}
		}
		ChargeCodeGroupCollection chargeCodeGroups;

		#endregion

		#region RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.ContainerYardTransportationUnit; }
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
			get { return Parent.ConsumerType; }
		}

		#endregion

		#region IAutoRatingOrganisations Members

		#region Creditors

		public override Creditors Creditors
		{
			get
			{
				var result = new List<OrgWithSource>();
				result.Add(OrgWithSource.NewFrom<OrgAddress>(transportationUnitForRating.Yard.WW_OA_WarehouseAddressInfo));

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
					[RatingDebtorOrgTypes.CNR] = transportationUnitForRating.Client,
					[RatingDebtorOrgTypes.CNE] = transportationUnitForRating.Client,
					[RatingDebtorOrgTypes.CCUS] = transportationUnitForRating.Client,
					[RatingDebtorOrgTypes.LC] = transportationUnitForRating.Client,
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
				var partList = new RateablePartList
				{
					HasPackageType = true,
					HasContainerNumber = true,
					HasContainerType = true,
					HasWarehouse = true,
					HasYardUnitType = true,
					HasYardUnitLoad = true,
					HasYardUnitClient = true,
				};

				foreach (var yardUnit in transportationUnitForRating.YardUnits)
				{
					AddRateableYardUnit(partList, yardUnit);
				}

				if (partList.Count > 0)
				{
					rateableMeasures.AddPartList(MeasureType.Unit, partList);
				}

				rateableMeasures.SetUnidentifiedQuantityForWarehouse(0, transportationUnitForRating.Yard.PK);

				return rateableMeasures;
			}
		}

		void AddRateableYardUnit(RateablePartList rateablePartList, CYDYardUnitState yardUnit)
		{
			var ratableContainer = new RateableContainer
			{
				UnitCount = 1,
				PackageType = PkgUnit.Unit,
				ContainerNumber = yardUnit.YUS_UnitID,
				ContainerTypePk = yardUnit.Delivery.UnitLineItem.YLI_RC_ContainerType.IsEmpty ? null : yardUnit.Delivery.UnitLineItem.YLI_RC_ContainerType.ToGuid(),
				YardUnitType = yardUnit.Delivery.UnitLineItem.YLI_Type,
				YardUnitLoad = yardUnit.Delivery.UnitLineItem.YLI_IsEmpty ? ContainerYardConstants.YardUnitLoad.Codes.EMP : ContainerYardConstants.YardUnitLoad.Codes.LAD,
				YardUnitClient = yardUnit.ReceiveAdvice?.Client?.Organisation.PK.ToGuid() ?? Guid.Empty,
				WarehousePk = yardUnit.YUS_WW_CurrentYard.ToGuid()
			};
			rateablePartList.AddPart(ratableContainer);
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

		public ZGuid WarehousePK => transportationUnitForRating.Yard.PK;

		public OrgHeader WarehouseFallbackConsignorForFilterOnly => null;

		#endregion
	}
}
