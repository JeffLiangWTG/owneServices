using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CustomsOfficeOfTransitProvider : NumberedCustomsOfficeProvider, ICustomsOfficeOfTransit
{
	public CustomsOfficeOfTransitProvider(int sequenceNumber, NctsPLOfficeCode customsOffice)
		: base(sequenceNumber, customsOffice)
	{
	}

	public DateTime? ArrivalDateAndTimeEstimated => CachedValueHelper.GetValue(ref arrivalDateAndTimeEstimated, () => GetArrivalDateAndTimeEstimated());
	CachedValue<DateTime?> arrivalDateAndTimeEstimated;

	DateTime? GetArrivalDateAndTimeEstimated()
	{
		var dateTime = customsOffice.CY_Date;
		return !dateTime.IsEmpty && dateTime.IsValid
			? dateTime.ToDateTime()
			: null;
	}
}
