using System;
using CargoWise.RefDbRepo.NZReferenceData.Services;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public class DateProvider : IDateProvider
	{
		public DateTime Today => DateTime.Today;

		public DateTime ActiveDate => Today.AddMonths(-ApplicationConfig.ActiveDateMonthOffset);
	}
}
