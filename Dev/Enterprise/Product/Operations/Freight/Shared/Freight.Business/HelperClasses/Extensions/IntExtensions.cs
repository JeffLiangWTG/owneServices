
namespace Enterprise.Freight.Business.Extensions
{
	using CargoWise.Types;

	public static class IntExtensions
	{
		/// <summary>
		///		Returns the <see cref="ZDateTime"/> instances with date equal current date minus <paramref name="daysNumber"/>.
		/// </summary>
		/// <param name="daysNumber">
		///		The number of days to substract from current date.
		/// </param>
		public static ZDateTime DaysAgo(this int daysNumber)
		{
			var now = ZDateTime.Now;

			return new ZDateTime(now.Year, now.Month, now.Day).AddDays(-daysNumber);
		}
	}
}
