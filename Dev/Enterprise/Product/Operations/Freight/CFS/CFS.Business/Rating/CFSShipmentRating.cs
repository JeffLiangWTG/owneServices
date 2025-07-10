using System;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSShipmentRatingAdapter<T> : ShipmentRatingAdapter<T>
		where T : CFSShipment
	{
		protected internal CFSShipmentRatingAdapter(T parent) : base(parent) { }

		public override AdapterType AdapterType => AdapterType.CFSShipment;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new CFSShipmentJobDatesProvider(parent); }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.CFSShipment; }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get { return new ChargeCodeGroupCollection { ChargeCodeGroupList.Codes.CFSShipment }; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.CFS; }
		}

		public override JobServicesCollection JobServices
		{
			get
			{
				var jobServices = new JobServicesCollection();
				jobServices.Add(StorageServiceInfo);

				var services = GetServiceInfosFromJobServices(parent.DocsAndCartage.Services, ChargeCodeGroupList.Codes.CFSShipment);
				jobServices.AddRange(services);

				return jobServices;
			}
		}

		JobServiceInfo StorageServiceInfo
		{
			get
			{
				var measures = (RateableMeasureSet)RateableMeasures;
				var timeInfo = measures.Time;
				var duration = timeInfo != null ? timeInfo.Span : TimeSpan.Zero; //Required for GatePassShipmentRatingAdapter
				var hasStorage = duration.TotalDays > 0;
				var description = Res.GetString("adb0294b-3163-418b-8472-2273e1985a93", "Storage");

				return new JobServiceInfo(hasStorage, ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeSubGroupList.Storage, description, null, duration);
			}
		}

		public override FreightMode FreightMode
		{
			get
			{
				return base.FreightMode & FreightMode.FreightTypeMask;
			}
		}

		protected override SpotRateInfo GetSellSpotRateInfoCore()
		{
			return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.ClientRate);
		}

		protected override SpotRateInfo GetCostSpotRateInfoCore()
		{
			return new SpotRateInfo(Money.Invalid, Core.Constants.FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
		}

		public override PaymentTermInfos PaymentTerm
		{
			get
			{
				var paymentTerm = base.PaymentTerm;
				paymentTerm.Remove(PaymentTermType.Incoterm);

				return paymentTerm;
			}
		}
	}
}
