using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsVASOrderRatingAdapter : RatingAdapter<WhsVASOrder>, IAutoRatingWarehouseInfo
	{
		public WhsVASOrderRatingAdapter(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
		}

		#region IAutoRating Members

		#region AdapterType

		public override AdapterType AdapterType => AdapterType.WarehouseAdHocService;

		#endregion

		#region InvoicingSupporter

		public override IJobInvoicingSupporter InvoicingSupporter => ((IJobInvoicingPlugIn)Parent).InvoicingSupporter;

		#endregion

		#region JobDatesProvider

		public override IJobDatesProvider JobDatesProvider => new WhsVASOrderJobDatesProvider(Parent);

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

		public override RateType RateTypeToUse => RateType.Warehouse;

		#endregion

		#region MergeCharges

		public override MergeChargeOptions MergeCharges => MergeChargeOptions.WithinAdapter;

		#endregion

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.WarehouseVASOrder;

		#endregion

		#endregion

		#region IAutoRatingOrganisations Members

		public override Creditors Creditors
		{
			get
			{
				var result = new List<OrgWithSource>();

				if (Parent.Warehouse?.WarehouseAddress != null)
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
				var localClient = InvoicingSupporter?.Job?.LocalCharges;
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

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell) => true;

		#endregion

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var result = new RateableMeasureSet(AdapterType);
				result.SetUnidentifiedQuantityForWarehouseDocket(1, Parent.WarehousePK, Parent.WVO_CustomerReferenceNo.IsEmpty ? Parent.WVO_JobID : Parent.WVO_CustomerReferenceNo);

				var arrivalDate = ((IAutoRating)this).JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate);
				var firstDayOfMonth = new ZDateTime(arrivalDate.Year, arrivalDate.Month, 1);
				result.Time = new TimeInfo(firstDayOfMonth, arrivalDate);
				return result;
			}
		}

		#endregion

		#region WharfCTOAddress

		public override OrgAddress WharfCTOAddress => Parent.Warehouse?.WarehouseAddress;

		#endregion

		#endregion

		ZGuid IAutoRatingWarehouseInfo.WarehousePK => Parent.WarehousePK;

		OrgHeader IAutoRatingWarehouseInfo.WarehouseFallbackConsignorForFilterOnly => null;
	}
}
