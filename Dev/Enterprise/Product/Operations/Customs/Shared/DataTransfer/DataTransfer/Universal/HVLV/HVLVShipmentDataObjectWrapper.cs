using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class HVLVShipmentDataObjectWrapper : ICurrencyConverterDataProvider
	{
		public HVLVShipmentDataObjectWrapper(Shipment hvlvShipment, Shipment consol, UniversalObjectFactory factory)
		{
			this.hvlvShipment = hvlvShipment;
			this.consol = consol;
			this.factory = factory;
		}
		public readonly Shipment hvlvShipment;
		public readonly Shipment consol;
		readonly UniversalObjectFactory factory;

		public ZGuid ShipmentPK
		{
			get
			{
				if (!shipmentPK.HasValue)
				{
					var key = hvlvShipment?.DataContext?.GetMatchingDataSource(UniversalDataBuss.Integration.DataContextType.ForwardingShipment)?.Key ?? ZString.Empty;
					shipmentPK = key.IsEmpty ? ZGuid.Empty : factory.LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, key)?.PK ?? ZGuid.Empty;
				}

				return shipmentPK.Value;
			}
		}
		ZGuid? shipmentPK;

		public ZString ContainerNumber
		{
			get
			{
				if (!fContainerNumber.HasValue && hvlvShipment.PackingLineCollection != null)
				{
					fContainerNumber = hvlvShipment.PackingLineCollection.Select(x => x.ContainerNumber.GetValueOrDefault()).FirstOrDefault(x => !x.IsEmpty);
				}
				return fContainerNumber.GetValueOrDefault();
			}
		}
		ZString? fContainerNumber;

		public ContainerMode ContainerMode
		{
			get { return hvlvShipment.ContainerMode; }
		}

		public GlbCompany Company
		{
			get { return fCompany ?? (fCompany = factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK)); }
		}
		GlbCompany fCompany;

		public ZDateTime DepartureDate
		{
			get
			{
				if (!departureDate.HasValue)
				{
					departureDate = GetUserOverridenDepartureDate();

					if (!departureDate.Value.IsValid && FirstInternationalLeg != null)
					{
						departureDate = FirstInternationalLeg.ActualDeparture.GetValueOrDefault();
					}

					if (!departureDate.Value.IsValid && hvlvShipment.DateCollection != null)
					{
						departureDate = hvlvShipment.DateCollection.Where(x => x.Type.GetValueOrDefault() == DateType.Departure && x.IsEstimate.GetValueOrDefault()).Select(x => x.Value.GetValueOrDefault()).FirstOrDefault();
					}
					if (!departureDate.Value.IsValid && FirstInternationalLeg != null)
					{
						departureDate = FirstInternationalLeg.EstimatedDeparture.GetValueOrDefault();
					}
				}
				return departureDate.Value;
			}
		}
		ZDateTime? departureDate;

		ZDateTime GetUserOverridenDepartureDate()
		{
			ZDateTime result = consol.DateCollection != null ? consol.DateCollection.Where(x => x.Type.GetValueOrDefault() == DateType.LoadingDate && !x.IsEstimate.GetValueOrDefault())
						.Select(x => x.Value.GetValueOrDefault()).FirstOrDefault() : ZDateTime.Empty;

			if (result.IsValid)
			{
				var estimatedDepartureDate = FirstInternationalLeg != null ? FirstInternationalLeg.EstimatedDeparture.GetValueOrDefault() : ZDateTime.Empty;
				if (estimatedDepartureDate.IsValid && estimatedDepartureDate.Date == result.Date)
				{
					result = ZDateTime.Empty;
				}
			}
			return result;
		}

		TransportLeg FirstInternationalLeg
		{
			get
			{
				return fFirstInternationalLeg ?? (fFirstInternationalLeg = consol.TransportLegCollection != null ? consol.TransportLegCollection.Where(x => x.PortOfLoading != null && x.PortOfDischarge != null).OrderBy(x => x.LegOrder.GetValueOrDefault())
					.FirstOrDefault(x => ImportExportHelper.GetJobDirection(x.PortOfLoading.Code.GetValueOrDefault(), x.PortOfDischarge.Code.GetValueOrDefault()) != Directions.Domestic) : null);
			}
		}
		TransportLeg fFirstInternationalLeg;

		public ZDateTime DateOfValuation
		{
			get { return DepartureDate; }
		}

		public ZBool? IsReciprocalOverride
		{
			get { return null; }
		}

		public ZString LocalCurrencyCodeOverride
		{
			get { return ZString.Empty; }
		}

		public int MaximumDaysToFallback
		{
			get { return 7; }
		}

		public ExchangeRateType RateType
		{
			get { return ExchangeRateType.Customs; }
		}
	}
}
