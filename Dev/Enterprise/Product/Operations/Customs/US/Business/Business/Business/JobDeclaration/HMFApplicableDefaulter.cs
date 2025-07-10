using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class HMFApplicableDefaulter
	{
		/// <param name="transportType">one of the codes in TransportTypeList ie Sea/Air</param>
		/// <returns>Y/N/[Empty]</returns>
		public ZString GetCalculatedHMFApplicable(ZString transportType, ZString entryType, ZString arrivalPort)
		{
			ZString result = YesNoDefaultList.Codes.No;

			if (transportType == TransportTypeList.Codes.BorderWaterBorne)
			{
				result = YesNoDefaultList.Codes.Yes;
			}
			else if (transportType == TransportTypeList.Codes.Sea)
			{
				result = YesNoDefaultList.Codes.Yes;

				if (arrivalPort == "3302" || arrivalPort == "5271" || arrivalPort == "5272")
				{
					result = YesNoDefaultList.Codes.No;
				}

				if (EntryTypeList.IsHMFNotApplicable(entryType))
				{
					result = YesNoDefaultList.Codes.No;
				}
			}

			return result;
		}
	}
}
