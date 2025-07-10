using System;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CustomsOfficesOfTransitProvider : ICustomsOfficeOfTransit
{
	public CustomsOfficesOfTransitProvider(ZString officeCode, ZDateTime arrivalTime, ZInt sequence)
	{
		SequenceNumeric = sequence;
		ReferenceNumber = officeCode;
		if (arrivalTime.IsValid)
		{
			ArrivalDateAndTimeEstimated = arrivalTime.ToDateTime();
		}
	}

	public int SequenceNumeric { get; }

	public string ReferenceNumber { get; }

	public DateTime? ArrivalDateAndTimeEstimated { get; }
}
