using System;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public sealed class DateTimeProvider : IDateTimeProvider
	{
		DateTime IDateTimeProvider.Now => DateTime.Now;
	}
}
