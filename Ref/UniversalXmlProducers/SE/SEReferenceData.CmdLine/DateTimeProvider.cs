using System;
using CargoWise.RefDbRepo.SEReferenceData.Business;

namespace CargoWise.RefDbRepo.SEReferenceData.CmdLine
{
	internal class DateTimeProvider : IDateTimeProvider
	{
		public DateTime CurrentLocalDate => DateTime.Today;
	}
}
