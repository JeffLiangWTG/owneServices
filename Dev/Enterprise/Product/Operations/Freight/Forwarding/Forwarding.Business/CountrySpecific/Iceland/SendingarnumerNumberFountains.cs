using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SendingarnumerNumberFountains
	{
		#region Instance
		public static SendingarnumerNumberFountains Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new SendingarnumerNumberFountains();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static SendingarnumerNumberFountains instance;
		#endregion

		#region SendingarnumerImportAir
		public INumberFountainProxy SendingarnumerImportAir
		{
			get
			{
				if (fSendingarnumerImportAir == null)
				{
					var factory = new FormattedNumberFountainFactory("SendingarnumerImportAir", rollOver: true, maxValue: 999, formatDigits: 3);
					fSendingarnumerImportAir = factory.New();
				}

				return fSendingarnumerImportAir;
			}
		}
		INumberFountainProxy fSendingarnumerImportAir;
		#endregion

		#region SendingarnumerImportSea
		public INumberFountainProxy SendingarnumerImportSea
		{
			get
			{
				if (fSendingarnumerImportSea == null)
				{
					var factory = new FormattedNumberFountainFactory("SendingarnumerImportSea", rollOver: true, formatDigits: 3, maxValue: 999);
					fSendingarnumerImportSea = factory.New();
				}

				return fSendingarnumerImportSea;
			}
		}
		INumberFountainProxy fSendingarnumerImportSea;
		#endregion

		#region SendingarnumerExportAir
		public INumberFountainProxy SendingarnumerExportAir
		{
			get
			{
				if (fSendingarnumerExportAir == null)
				{
					var factory = new FormattedNumberFountainFactory("SendingarnumerExportAir", rollOver: true, formatDigits: 3, maxValue: 999);
					fSendingarnumerExportAir = factory.New();
				}

				return fSendingarnumerExportAir;
			}
		}
		INumberFountainProxy fSendingarnumerExportAir;
		#endregion

		#region SendingarnumerExportSea
		public INumberFountainProxy SendingarnumerExportSea
		{
			get
			{
				if (fSendingarnumerExportSea == null)
				{
					var factory = new FormattedNumberFountainFactory("SendingarnumerExportSea", rollOver: true, formatDigits: 3, maxValue: 999);
					fSendingarnumerExportSea = factory.New();
				}

				return fSendingarnumerExportSea;
			}
		}
		INumberFountainProxy fSendingarnumerExportSea;
		#endregion
	}
}
