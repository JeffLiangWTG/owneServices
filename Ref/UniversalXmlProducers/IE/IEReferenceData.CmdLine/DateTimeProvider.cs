using System;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CmdLine
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime CurrentLocalDate => DateTime.Today;
	}
}
