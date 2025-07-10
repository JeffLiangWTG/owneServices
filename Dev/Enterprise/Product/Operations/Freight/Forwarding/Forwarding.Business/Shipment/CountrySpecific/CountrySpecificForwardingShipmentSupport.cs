using System;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CountrySpecificForwardingShipmentSupport : CountrySpecificJobSupport<ForwardingShipment>
	{
		protected override ZString CountryCode
		{
			get { return ZString.Empty; }
		}

		#region SupplyChainSecuritySupport

		protected virtual SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return SupportedBO.AviationSecurity.SupplyChainSecurityConfiguration; }
		}

		#endregion

		protected override void RegisterCore()
		{
			base.RegisterCore();
			SupportedBO.OnSavingShipment += SupportedBO_OnSavingShipment;
			SupportedBO.OnShipmentSaved += SupportedBO_OnShipmentSaved;
		}

		bool canadaCargoControlNumberIsSet;

		void SupportedBO_OnSavingShipment(object sender, EventArgs e)
		{
			canadaCargoControlNumberIsSet = false;
			if (SupportedBO.HasChanges && !SupportedBO.IsDeleted)
			{
				canadaCargoControlNumberIsSet = SupportedBO.SetCanadaCargoControlNumberIfNotExist();
			}
		}

		void SupportedBO_OnShipmentSaved(object sender, EventArgs e)
		{
			if (!((CommonShipment.SavedEventArgs)e).HasSaveSucceeded && canadaCargoControlNumberIsSet)
			{
				SupportedBO.RollbackSetCanadaCargoControlNumber();
			}
			canadaCargoControlNumberIsSet = false;
		}

		public ZBool AddCountryCodeToEntryDetailsCaption => AddCountryCodeToEntryDetailsCaptionCore;

		protected virtual ZBool AddCountryCodeToEntryDetailsCaptionCore => true;
	}
}
