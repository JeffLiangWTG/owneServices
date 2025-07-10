using System.Threading;

namespace System.Windows.Forms
{
	public static class ClientUtils
	{
		public static bool IsCriticalException(Exception ex)
			=> ex is NullReferenceException
				|| ex is StackOverflowException
				|| ex is OutOfMemoryException
				|| ex is ThreadAbortException
				|| ex is IndexOutOfRangeException
				|| ex is AccessViolationException;

		public static bool IsSecurityOrCriticalException(Exception ex) => false;

		public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)
		{
			return value >= minValue && value <= maxValue;
		}

		public static bool IsEnumValid(Enum enumValue, int value, int minValue, int maxValue, int maxNumberOfBitsOn)
		{
			return value >= minValue && value <= maxValue && GetBitCount((uint)value) <= maxNumberOfBitsOn;
		}

		public static int GetBitCount(uint x)
		{
			int count = 0;
			while (x > 0)
			{
				x &= x - 1;
				count++;
			}
			return count;
		}
	}
}
