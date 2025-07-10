using System;
using CargoWise.RefDbRepo.DEReferenceData.Services;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime CurrentLocalDate => DateTime.Today;

		public DateTimeOffset CurrentLocalDateTimeOffset => DateTimeOffset.Now;
	}
}
