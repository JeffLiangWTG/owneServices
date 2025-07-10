using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USDrawbackAdditionalImportTariffNumber)]
	public class DrawbackAdditionalImportTariffNumberAddInfo : AutoDrawbackAdditionalImportTariffNumberAddInfo
	{
		public DrawbackAdditionalImportTariffNumberAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return ((DrawbackAdditionalImportTariffNumber)base.Parent).InvoiceLine; }
		}

		#region US_Tariff

		public override ZString US_Tariff
		{
			get { return base.US_Tariff; }
			set
			{
				var oldTariff = US_Tariff;
				base.US_Tariff = value;
				if (oldTariff != US_Tariff && !IsCopying)
				{
					if (US_Tariff.IsEmpty)
					{
						var descriptionFromOldTariff = GetDescriptionFromTariff(oldTariff);
						if (descriptionFromOldTariff == US_Description)
						{
							US_Description = ZString.Empty;
						}
					}
					else if (US_Description.IsEmpty)
					{
						var descriptionFromTariff = GetDescriptionFromTariff(US_Tariff);
						US_Description = descriptionFromTariff.Left(DrawbackAdditionalImportTariffNumberAddInfo.Schema.US_DescriptionMaxLength);
					}
				}
			}
		}

		ZString GetDescriptionFromTariff(ZString tariffCode)
		{
			var result = ZString.Empty;
			var date = InvoiceLine?.EffectiveDateForDutyRate ?? ZDate.Today;
			var tariff = new USCTariff.Loader(Factory).LoadBestMatchOrMostRecent(tariffCode, date);

			if (tariff != null)
			{
				result = tariff.UE_ShortDescription.ToUpper();
			}
			return result;
		}

		#endregion
	}
}
