using System;
using System.Globalization;

namespace CargoWise.RefDbRepo.Common.Argument
{
	// This class exists to contain throw expressions, in order that other functions
	// that call this can be inlined by the JIT into _their_ calling method.
	static class ThrowHelpers
	{
		public static void ThrowArgumentNullException(string name)
			=> throw new ArgumentNullException(name);

		public static void ThrowArgumentNullException(string name, string message)
			=> throw new ArgumentNullException(name, message);

		public static void ThrowEmptyStringException(string name)
			=> throw new ArgumentException("Value cannot be empty string (\"\").", name);

		public static void ThrowEmptyStringException(string name, string message)
			=> throw new ArgumentException(name, message);

		public static void ThrowArgumentNotGreaterThanOrEqualTo(int argument, int number, string name)
			=> throw new ArgumentOutOfRangeException(
				name,
				string.Format(CultureInfo.InvariantCulture, "Value '{0}' cannot be less than {1}.", argument, number));

		public static void ThrowArgumentNotGreaterThan(int argument, int number, string name)
			=> throw new ArgumentOutOfRangeException(
				name,
				string.Format(CultureInfo.InvariantCulture, "Value '{0}' cannot be less than or equal to {1}.", argument, number));

		public static void ThrowArgumentNotInRange(int argument, int lowerBound, int upperBound, string name)
			=> throw new ArgumentOutOfRangeException(
				name,
				string.Format(CultureInfo.InvariantCulture, "Value '{0}' cannot be less than {1} or greater than {2}.", argument, lowerBound, upperBound));

		public static void ThrowArgumentException(string message, string name)
			=> throw new ArgumentException(message, name);

		public static void ThrowArgumentExactlyEqualTo(int argument, int number, string name)
			=> throw new ArgumentOutOfRangeException(
				name,
				string.Format(CultureInfo.InvariantCulture, "Value '{0}' must be equal to {1}.", argument, number));
	}
}
