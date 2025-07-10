using CargoWise.ComponentModel;

namespace Enterprise.Freight.Business
{
	public class TransportStorageValidation : TransportValidation
	{
		public TransportStorageValidation(Transport transport)
			: base(transport)
		{
		}

		protected override void CheckJW_TransportType()
		{
			// dont call base
		}

		protected override void CheckJW_Status()
		{
			// dont call base
		}

		protected override void CheckETDvsETA()
		{
			if (!Parent.JW_ETDInfo.HasErrors() && Parent.JW_ETD.IsValid && Parent.JW_ETA.IsValid)
			{
				if (Parent.JW_ETA > Parent.JW_ETD)
				{
					Parent.JW_ETDInfo.AddNotification(notificationType, Res.GetString("44054585-4d87-4ab3-9a1e-a2d1e9529b86", "ETA cannot be after ETD."));
				}
			}
		}

		protected override void CheckETAvsETD()
		{
			if (!Parent.JW_ETAInfo.HasErrors() && Parent.JW_ETA.IsValid && Parent.JW_ETD.IsValid)
			{
				if (Parent.JW_ETD < Parent.JW_ETA)
				{
					Parent.JW_ETAInfo.AddNotification(notificationType, Res.GetString("6f7b3059-80ac-4d1d-9d1e-f2f1400092a4", "ETD cannot be before ETA."));
				}
			}
		}

		protected override void CheckATDvsATA()
		{
			if (!Parent.JW_ATDInfo.HasErrors() && Parent.JW_ATA.IsValid)
			{
				if (Parent.JW_ATA > Parent.JW_ATD)
				{
					Parent.JW_ATDInfo.AddNotification(notificationType, Res.GetString("ec29712e-c11e-4219-8c01-0cb01bbd2d5e", "ATA cannot be after ATD."));
				}
			}
		}

		protected override void CheckATAvsATD()
		{
			if (!Parent.JW_ATAInfo.HasErrors() && Parent.JW_ATD.IsValid)
			{
				if (Parent.JW_ATD < Parent.JW_ATA)
				{
					Parent.JW_ATAInfo.AddNotification(notificationType, Res.GetString("35ca0888-220a-4cc8-929d-b695687ac490", "ATD cannot be before ATA."));
				}
			}
		}
	}
}
