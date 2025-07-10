using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageApplicationLocalManufacturer : LicensingMessageManufacturer
	{
		public LicensingMessageApplicationLocalManufacturer(TWJobDocAddress localManufacturer)
			: base(localManufacturer)
		{
		}

		protected override IEnumerable<ICommunication> CommunicationsCore
		{
			get
			{
				var phone = ManufacturerAddress.E2_Phone;
				if (!phone.IsEmpty)
				{
					yield return new CommunicationWrapper(phone, ZString.Empty);
				}
			}
		}
	}
}
