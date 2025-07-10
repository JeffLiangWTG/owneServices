using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class DurationAsDateTimeHelper
	{
		public static int DateTimeToDuration(ZDateTime time)
		{
			if (time.IsValid)
			{
				return (int)(time - new ZDateTime(time.Year, 1, 1)).TotalSeconds;
			}
			else
			{
				return 0;
			}
		}
	}
}
