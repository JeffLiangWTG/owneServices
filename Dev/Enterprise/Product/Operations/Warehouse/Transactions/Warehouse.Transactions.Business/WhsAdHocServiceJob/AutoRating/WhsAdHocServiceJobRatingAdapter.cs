using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdHocServiceJobRatingAdapter : RatingAdapter<WhsAdHocServiceJob>, IAutoRatingWarehouseInfo
	{
		public WhsAdHocServiceJobRatingAdapter(WhsAdHocServiceJob adhocServiceJob)
			: base(adhocServiceJob)
		{
		}

		#region IAutoRating Members

		#region AdapterType

		public override AdapterType AdapterType => AdapterType.WarehouseAdHocService;

		#endregion

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new WhsAdHocServiceJobJobDatesProvider(Parent); }
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
					chargeCodeGroups.AddRange(new[] { ChargeCodeGroupList.Codes.WHSAdHocServiceJob });
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
				result.AddRange(GetServiceInfosFromJobServices(Parent.Services, ChargeCodeGroupList.Codes.WHSAdHocServiceJob));

				return result;
			}
		}

		#endregion

		#region RateTypeToUse

		public override RateType RateTypeToUse
		{
			get { return RateType.Warehouse; }
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
			get { return JobInvoicingConsumerTypes.WarehouseAdHocServiceJob; }
		}

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

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

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = new DebtorOrgCollection();
				var localClient = Parent.InvoicingSupporter?.Job?.LocalCharges;
				if (localClient != null)
				{
					result[Registry.Business.RatingDebtorOrgTypes.LC] = localClient;
				}

				return result;
			}
		}

		#endregion

		#region IAutoRatingFreightInfo Members

		#region IsApplicableToPaymentTermFiltering

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return true;
		}

		#endregion

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);

				// without this the Invoice Detail report will not group under the Ad Hoc Service Job customer reference
				var docketRef = Parent.WSJ_CustomerReference.IsEmpty ? Parent.WSJ_JobNumber : Parent.WSJ_CustomerReference;
				result.SetUnidentifiedQuantityForWarehouseDocket(1, Parent.WSJ_WW_Whs, docketRef);

				var arrivalDate = ((IAutoRating)this).JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate);
				var firstDayOfMonth = new ZDateTime(arrivalDate.Year, arrivalDate.Month, 1);
				result.Time = new TimeInfo(firstDayOfMonth, arrivalDate);
				return result;
			}
		}

		#endregion

		#region ServiceLevel

		public override ServiceLevelRatingInformation ServiceLevel => new ServiceLevelRatingInformation(Array.Empty<ServiceLevelInfo>());

		#endregion

		#region WharfCTOAddress

		public override OrgAddress WharfCTOAddress
		{
			get { return Parent.Warehouse != null ? Parent.Warehouse.WarehouseAddress : null; }
		}

		#endregion

		#endregion

		ZGuid IAutoRatingWarehouseInfo.WarehousePK
		{
			get { return Parent.WSJ_WW_Whs; }
		}

		OrgHeader IAutoRatingWarehouseInfo.WarehouseFallbackConsignorForFilterOnly => null;
	}
}
