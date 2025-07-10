using System;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
	}

	public class RealDateTimeProvider : IDateTimeProvider
	{
		public DateTime Now => DateTime.UtcNow;
	}

	public static class DateTimeUtil
	{
		public static IDateTimeProvider DateTimeProvider { get; set; } = new RealDateTimeProvider();

		public static DateTime GetNow => DateTimeProvider.Now;
	}
}
