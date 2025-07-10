using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime CurrentLocalDate => DateTime.Today;
	}
}
