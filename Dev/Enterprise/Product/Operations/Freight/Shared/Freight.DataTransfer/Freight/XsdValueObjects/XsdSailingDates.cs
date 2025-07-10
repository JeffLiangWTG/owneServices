using CargoWise.Types;
using Enterprise.Freight.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public static class XsdSailingDates
	{
		public static Xsd.SailingDates ToLCLValueObject(JobSailing sailing, ZDateTime availableDate, ZDateTime cutOffDate, ZDateTime receivalCommencesDate, ZDateTime storageDate)
		{
			Xsd.SailingDates result = new Xsd.SailingDates();
			bool datesPopulated = false;
			if (sailing.JX_DepotAvailabilityDate.IsValid)
			{
				result.AvailableDate = sailing.JX_DepotAvailabilityDate.ToDateTime();
				datesPopulated = true;
			}
			if (sailing.JX_DepotCutOff.IsValid)
			{
				result.CutOffDate = sailing.JX_DepotCutOff.ToDateTime();
				datesPopulated = true;
			}
			if (sailing.JX_DepotReceivalCommences.IsValid)
			{
				result.ReceivalCommencesDate = sailing.JX_DepotReceivalCommences.ToDateTime();
				datesPopulated = true;
			}
			if (sailing.JX_DepotStorageDate.IsValid)
			{
				result.StorageDate = sailing.JX_DepotStorageDate.ToDateTime();
				datesPopulated = true;
			}
			return datesPopulated ? result : null;
		}

		public static Xsd.SailingDates ToFCLValueObject(JobSailing sailing, ZDateTime availableDate, ZDateTime cutOffDate, ZDateTime receivalCommencesDate, ZDateTime storageDate)
		{
			Xsd.SailingDates result = new Xsd.SailingDates();
			bool datesPopulated = false;
			if (sailing.JX_JB_CTOAvailabilityDate.IsValid)
			{
				result.AvailableDate = sailing.JX_JB_CTOAvailabilityDate.ToDateTime();
				datesPopulated = true;
			}
			if (sailing.JX_JA_CTOCutOff.IsValid)
			{
				result.CutOffDate = sailing.JX_JA_CTOCutOff.ToDateTime();
				datesPopulated = true;
			}
			if (sailing.JX_JA_CTOReceivalCommences.IsValid)
			{
				result.ReceivalCommencesDate = sailing.JX_JA_CTOReceivalCommences.ToDateTime();
				datesPopulated = true;
			}
			if (sailing.JX_JB_CTOStorageDate.IsValid)
			{
				result.StorageDate = sailing.JX_JB_CTOStorageDate.ToDateTime();
				datesPopulated = true;
			}
			return datesPopulated ? result : null;
		}

		public static void FromLCLValueObject(JobSailing sailing, Xsd.SailingDates dates)
		{
			if (dates != null)
			{
				if (!sailing.JX_DepotAvailabilityDate.IsValid && dates.AvailableDate.IsValid)
				{
					sailing.JX_DepotAvailabilityDate = dates.AvailableDate;
				}
				if (!sailing.JX_DepotCutOff.IsValid && dates.CutOffDate.IsValid)
				{
					sailing.JX_DepotCutOff = dates.CutOffDate;
				}
				if (!sailing.JX_DepotReceivalCommences.IsValid && dates.ReceivalCommencesDate.IsValid)
				{
					sailing.JX_DepotReceivalCommences = dates.ReceivalCommencesDate;
				}
				if (!sailing.JX_DepotStorageDate.IsValid && dates.StorageDate.IsValid)
				{
					sailing.JX_DepotStorageDate = dates.StorageDate;
				}
			}
		}

		public static void FromFCLValueObject(JobSailing sailing, Xsd.SailingDates dates)
		{
			if (dates != null)
			{
				VoyageDestination destination = sailing.Destination;
				VoyageOrigin origin = sailing.Origin;

				if (!destination.JB_AvailabilityDate.IsValid && dates.AvailableDate.IsValid)
				{
					destination.JB_AvailabilityDate = dates.AvailableDate;
				}
				if (!origin.JA_CutOff.IsValid && dates.CutOffDate.IsValid)
				{
					origin.JA_CutOff = dates.CutOffDate;
				}
				if (!origin.JA_ReceivalCommences.IsValid && dates.ReceivalCommencesDate.IsValid)
				{
					origin.JA_ReceivalCommences = dates.ReceivalCommencesDate;
				}
				if (!destination.JB_StorageDate.IsValid && dates.StorageDate.IsValid)
				{
					destination.JB_StorageDate = dates.StorageDate;
				}
			}
		}
	}
}
