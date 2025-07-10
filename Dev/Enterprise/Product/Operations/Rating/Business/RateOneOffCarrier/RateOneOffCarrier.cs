using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RateOneOffCarrier : AutoRateOneOffCarrier
	{
		public RateOneOffCarrier(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(OneOffShipment))]
		public override ZGuid TTC_TT
		{
			get => base.TTC_TT;
			set
			{
				base.TTC_TT = value;
				oneOffShipment = null;
			}
		}

		public RateOneOffShipment OneOffShipment
		{
			get { return oneOffShipment ?? Factory.Load<RateOneOffShipment>(TTC_TT); }
			internal set { oneOffShipment = value; }
		}
		RateOneOffShipment oneOffShipment;

		[List(nameof(CarrierLookups))]
		public override ZGuid TTC_OH_Carrier
		{
			get => base.TTC_OH_Carrier;
			set
			{
				base.TTC_OH_Carrier = value;

				if (TTC_OH_Creditor.IsEmpty && OneOffShipment is not null)
				{
					TTC_OH_Creditor = DefaultCreditorHelper.GetDefaultCreditor(
						OneOffShipment,
						Carrier,
						OneOffShipment.TT_TransportMode,
						OneOffShipment.TT_ContainerMode,
						OneOffShipment.TT_RL_NKReceivalLocation,
						OneOffShipment.TT_RL_NKDeliveryLocation,
						Factory);
				}
			}
		}

		public ShippingProviderCollection CarrierLookups => carrierLookups ??= new (Factory);
		ShippingProviderCollection carrierLookups;

		public ZString CarrierName => Carrier?.OH_FullName ?? ZString.Empty;

		protected override bool SupportsCloneCore() => true;

		[List("Lookups.TransitTimesList")]
		public override ZString TTC_TransitTime
		{
			get { return base.TTC_TransitTime; }
			set { base.TTC_TransitTime = value; }
		}

		[List("Lookups.FrequencyUnits")]
		public override ZString TTC_FrequencyUnit
		{
			get { return base.TTC_FrequencyUnit; }
			set { base.TTC_FrequencyUnit = value; }
		}
	}
}
