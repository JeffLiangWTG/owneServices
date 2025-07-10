using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class CanadaCFSShipmentSupport : CountrySpecificJobSupport<CFSShipment>
	{
		protected override ZString CountryCode
		{
			get { return ZString.Empty; } // Please don't remove this line, eManifest will not only work for Canada customs but also for Shipments which destination port is Canada
		}

		protected override void RegisterCore()
		{
			base.RegisterCore();
			SupportedBO.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Shipment_FactorySaving);
			SupportedBO.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Shipment_FactorySaved);
		}

		bool canadaCargoControlNumberIsSet;

		void Shipment_FactorySaving(BusinessObjectFactory factory)
		{
			canadaCargoControlNumberIsSet = false;
			if (SupportedBO.HasChanges && !SupportedBO.IsDeleted)
			{
				canadaCargoControlNumberIsSet = SupportedBO.SetCanadaCargoControlNumberIfNotExist();
			}
		}

		void Shipment_FactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!savedSuccessfully && canadaCargoControlNumberIsSet)
			{
				SupportedBO.RollbackSetCanadaCargoControlNumber();
			}
			canadaCargoControlNumberIsSet = false;
		}
	}
}
