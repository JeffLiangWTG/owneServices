using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ElectronicBOLPlugIn : ZPlugIn
	{
		public ElectronicBOLPlugIn(ForwardingShipment hostEntity)
			: base(hostEntity)
		{
			this.shipment = hostEntity;
			Enabled = this.shipment != null &&
					this.shipment.EnabledElectronicBOL;
		}

		readonly ForwardingShipment shipment;

		#region IZPlugIn Numbers

		public override string Name
		{
			get => Res.GetString("197991D0-6D9F-4738-AC6C-D4050CA3F6FC", "Electronic Bill Of Lading");
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get => Env.Licence.Forwarder;
		}

		public override bool CanDelete
		{
			get => true;
		}

		protected override ZBool HasUserControl
		{
			get => true;
		}

		protected override Control GetNewUserControl() => new ElectronicBOLUserControl();

		#endregion

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				return EHBLNotSupportedMessage.IsEmpty ? base.PlugInNotDisplayedMessage : $"{base.PlugInNotDisplayedMessage}\r\n{EHBLNotSupportedMessage}";
			}
		}

		ZString EHBLNotSupportedMessage
		{
			get
			{
				if (shipment != null)
				{
					if (shipment.Origin?.Country?.Rules?.Cast<RefCountryRules>().Any(rules => rules.R7_IsEBLNotSupported && rules.R7_RN_NKOrigin == shipment.Origin.Country.Code && (rules.R7_RN_NKDestination.IsEmpty || rules.R7_RN_NKDestination == shipment.Destination.Country.Code)) ?? false)
					{
						return Res.GetString("87dfb0e2-e918-4f30-8dac-0fd24e748df8", "Electronic Bills Of Lading are not supported in Origin Country {0}.", shipment.Origin.Country.Description);
					}
					else if (shipment.Destination?.Country?.Rules?.Cast<RefCountryRules>().Any(rules => rules.R7_IsEBLNotSupported && rules.R7_RN_NKDestination == shipment.Destination.Country.Code && rules.R7_RN_NKOrigin.IsEmpty) ?? false)
					{
						return Res.GetString("80c4b04d-f2e5-4d0e-9be0-3f6ec5d3370b", "Electronic Bills Of Lading are not supported in Destination Country {0}.", shipment.Destination.Country.Description);
					}
				}

				return ZString.Empty;
			}
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return shipment == null || EHBLNotSupportedMessage.IsEmpty;
		}
	}
}
