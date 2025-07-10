using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Transport : DocDataObject, ITransport
	{
		Transport(object identifier = default) : base(identifier)
		{
		}

		#region LegOrder

		public ZInt LegOrder
		{
			get => legOrder;
			set
			{
				if (SetNonPersistentPropertyValue(LegOrderInfo, ref legOrder, value))
				{
					Validate(LegOrderInfo);
				}
			}
		}

		ZInt legOrder;

		public ZPropertyInfo LegOrderInfo => GetZPropertyInfo(nameof(LegOrder));

		#endregion

		#region Mode

		public ICodeDescription Mode
		{
			get => mode;
			set => mode = SetChild(mode, value);
		}

		ICodeDescription mode;

		#endregion

		#region Type

		public ICodeDescription Type
		{
			get => type;
			set => type = SetChild(type, value);
		}

		ICodeDescription type;

		#endregion

		#region AdditionalTransportMode

		public ICodeDescription AdditionalTransportMode
		{
			get => additionalTransportMode;
			set => additionalTransportMode = SetChild(additionalTransportMode, value);
		}

		ICodeDescription additionalTransportMode;

		#endregion

		#region Status

		public ICodeDescription Status
		{
			get => status;
			set => status = SetChild(status, value);
		}

		ICodeDescription status;

		#endregion

		#region VoyageFlightNumber

		public ZString VoyageFlightNumber
		{
			get => voyageFlightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(VoyageFlightNumberInfo, ref voyageFlightNumber, value))
				{
					Validate(VoyageFlightNumberInfo);
				}
			}
		}

		ZString voyageFlightNumber;

		public ZPropertyInfo VoyageFlightNumberInfo => GetZPropertyInfo(nameof(VoyageFlightNumber));

		#endregion

		#region ETD

		public ZDateTime ETD
		{
			get => etd;
			set
			{
				if (SetNonPersistentPropertyValue(ETDInfo, ref etd, value))
				{
					Validate(ETDInfo);
				}
			}
		}

		ZDateTime etd;

		public ZPropertyInfo ETDInfo => GetZPropertyInfo(nameof(ETD));

		#endregion

		#region ETA

		public ZDateTime ETA
		{
			get => eta;
			set
			{
				if (SetNonPersistentPropertyValue(ETAInfo, ref eta, value))
				{
					Validate(ETAInfo);
				}
			}
		}

		ZDateTime eta;

		public ZPropertyInfo ETAInfo => GetZPropertyInfo(nameof(ETA));

		#endregion

		#region ATD

		public ZDateTime ATD
		{
			get => atd;
			set
			{
				if (SetNonPersistentPropertyValue(ATDInfo, ref atd, value))
				{
					Validate(ATDInfo);
				}
			}
		}

		ZDateTime atd;

		public ZPropertyInfo ATDInfo => GetZPropertyInfo(nameof(ATD));

		#endregion

		#region ATA

		public ZDateTime ATA
		{
			get => ata;
			set
			{
				if (SetNonPersistentPropertyValue(ATAInfo, ref ata, value))
				{
					Validate(ATAInfo);
				}
			}
		}

		ZDateTime ata;

		public ZPropertyInfo ATAInfo => GetZPropertyInfo(nameof(ATA));

		#endregion

		#region LCLCutOff

		public ZDateTime LCLCutOff
		{
			get => lclCutOff;
			set
			{
				if (SetNonPersistentPropertyValue(LCLCutOffInfo, ref lclCutOff, value))
				{
					Validate(LCLCutOffInfo);
				}
			}
		}

		ZDateTime lclCutOff;

		public ZPropertyInfo LCLCutOffInfo => GetZPropertyInfo(nameof(LCLCutOff));

		#endregion

		#region LCLReceivalCommences

		public ZDateTime LCLReceivalCommences
		{
			get => lclReceivalCommences;
			set
			{
				if (SetNonPersistentPropertyValue(LCLReceivalCommencesInfo, ref lclReceivalCommences, value))
				{
					Validate(LCLReceivalCommencesInfo);
				}
			}
		}

		ZDateTime lclReceivalCommences;

		public ZPropertyInfo LCLReceivalCommencesInfo => GetZPropertyInfo(nameof(LCLReceivalCommences));

		#endregion

		#region Vessel

		public Vessel Vessel
		{
			get => vessel;
			set => vessel = SetChild(vessel, value);
		}

		Vessel vessel;

		IVessel ITransport.Vessel => Vessel;

		#endregion

		#region PortOfLoading

		public Unloco PortOfLoading
		{
			get => portOfLoading;
			set => portOfLoading = SetChild(portOfLoading, value);
		}

		Unloco portOfLoading;

		IUnloco ITransport.PortOfLoading => PortOfLoading;

		#endregion

		#region PortOfDischarge

		public Unloco PortOfDischarge
		{
			get => portOfDischarge;
			set => portOfDischarge = SetChild(portOfDischarge, value);
		}

		Unloco portOfDischarge;

		IUnloco ITransport.PortOfDischarge => PortOfDischarge;

		#endregion

		#region Carrier

		public Address Carrier
		{
			get => carrier;
			set => carrier = SetChild(carrier, value);
		}

		Address carrier;

		IAddress ITransport.Carrier => Carrier;

		#endregion
	}
}
