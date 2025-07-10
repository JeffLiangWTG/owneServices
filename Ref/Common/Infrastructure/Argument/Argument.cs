using System;

namespace CargoWise.RefDbRepo.Common.Argument
{
	/// <summary>
	/// Assertion utility methods that simplify things such as argument checks.
	/// </summary>
	public static class Argument
	{
		/// <summary>
		/// Checks the value of the supplied <paramref name="argument"/> and throws an
		/// <see cref="ArgumentNullException"/> if it is <see langword="null"/>.
		/// </summary>
		/// <param name="argument">The object to check.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="ArgumentNullException">
		/// The supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		public static T NotNull<T>(T argument, string name)
		{
			if (argument == null)
			{
				ThrowHelpers.ThrowArgumentNullException(name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied <paramref name="argument"/> and throws an
		/// <see cref="ArgumentNullException"/> if it is <see langword="null"/>.
		/// </summary>
		/// <param name="argument">The object to check.</param>
		/// <param name="name">The argument name.</param>
		/// <param name="message">An arbitrary message that will be passed to any thrown <see cref="ArgumentNullException"/>.</param>
		/// <exception cref="ArgumentNullException">
		/// The supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		public static T NotNull<T>(T argument, string name, string message)
		{
			if (argument == null)
			{
				ThrowHelpers.ThrowArgumentNullException(name, message);
			}

			return argument;
		}

		public static bool IsTrue(bool argument, string name)
		{
			if (!argument)
			{
				ThrowHelpers.ThrowArgumentException($"{name} is expected to be true", name);
			}

			return argument;
		}

		public static bool IsFalse(bool argument, string name)
		{
			if (argument)
			{
				ThrowHelpers.ThrowArgumentException($"{name} is expected to be false", name);
			}

			return argument;
		}

		public static Guid GuidIsNotEmpty(Guid argument, string name)
		{
			if (argument == Guid.Empty)
			{
				ThrowHelpers.ThrowArgumentException($"{name} can not be equal to Guid.Empty", name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied string <paramref name="argument"/> and throws an
		/// <see cref="ArgumentException"/> if it is <see langword="null"/> or empty.
		/// </summary>
		/// <param name="argument">The string to check.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="ArgumentNullException">
		/// The supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The supplied <paramref name="argument"/> is empty.
		/// </exception>
		public static string NotNullOrEmpty(string argument, string name)
		{
			if (argument == null)
			{
				ThrowHelpers.ThrowArgumentNullException(name);
			}

			if (argument.Length == 0)
			{
				ThrowHelpers.ThrowEmptyStringException(name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied string <paramref name="argument"/> and throws an
		/// <see cref="ArgumentNullException"/> if it is <see langword="null"/> or empty.
		/// </summary>
		/// <param name="argument">The string to check.</param>
		/// <param name="name">The argument name.</param>
		/// <param name="message">An arbitrary message that will be passed to any thrown exception.</param>
		/// <exception cref="ArgumentNullException">
		/// The supplied <paramref name="argument"/> is <see langword="null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The supplied <paramref name="argument"/> is empty.
		/// </exception>
		public static string NotNullOrEmpty(string argument, string name, string message)
		{
			if (argument == null)
			{
				ThrowHelpers.ThrowArgumentNullException(name, message);
			}

			if (argument.Length == 0)
			{
				ThrowHelpers.ThrowEmptyStringException(name, message);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentOutOfRangeException"/> if it is not greater than or equal
		/// to given <paramref name="number"/> to compare with.
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="number">The number to compare with.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The supplied <paramref name="argument"/> is less than <paramref name="number"/>.
		/// </exception>
		public static int GreaterThanOrEqual(int argument, int number, string name)
		{
			if (argument < number)
			{
				ThrowHelpers.ThrowArgumentNotGreaterThanOrEqualTo(argument, number, name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentOutOfRangeException"/> if it is not greater than or equal
		/// to given <paramref name="number"/> to compare with.
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="number">The number to compare with.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The supplied <paramref name="argument"/> is less than <paramref name="number"/>.
		/// </exception>
		public static int InRangeWithBoundIncluded(int argument, int lowerBound, int upperBound, string name)
		{
			if (argument < lowerBound || argument > upperBound)
			{
				ThrowHelpers.ThrowArgumentNotInRange(argument, lowerBound, upperBound, name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentOutOfRangeException"/> if it is not greater than or equal
		/// to given <paramref name="number"/> to compare with.
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="number">The number to compare with.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The supplied <paramref name="argument"/> is less than <paramref name="number"/>.
		/// </exception>
		public static int InRangeWithBoundExcluded(int argument, int lowerBound, int upperBound, string name)
		{
			if (argument <= lowerBound || argument >= upperBound)
			{
				ThrowHelpers.ThrowArgumentNotInRange(argument, lowerBound, upperBound, name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="System.ArgumentOutOfRangeException"/> if it is not greater than or equal
		/// to given <paramref name="number"/> to compare with.
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="number">The number to compare with.</param>
		/// <param name="name">The argument name.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The supplied <paramref name="argument"/> is less than <paramref name="number"/>.
		/// </exception>
		public static int ExactlyEqualTo(int argument, int number, string name)
		{
			if (argument == number)
			{
				ThrowHelpers.ThrowArgumentExactlyEqualTo(argument, number, name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="ArgumentNullException"/> if it is not greater than
		/// given <paramref name="number"/> to compare with.
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="number">The number to compare with.</param>
		/// <param name="name">The argument name.</param>
		public static int GreaterThan(int argument, int number, string name)
		{
			if (argument <= number)
			{
				ThrowHelpers.ThrowArgumentNotGreaterThan(argument, number, name);
			}

			return argument;
		}

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="ArgumentOutOfRangeException"/> if it is not greater than zero
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="name">The argument name.</param>
		public static int GreaterThanZero(int argument, string name)
			=> GreaterThan(argument, 0, name);

		/// <summary>
		/// Checks the value of the supplied int <paramref name="argument"/> and throws an
		/// <see cref="ArgumentOutOfRangeException"/> if it is not greater than or equal to zero
		/// </summary>
		/// <param name="argument">The argument value.</param>
		/// <param name="name">The argument name.</param>
		public static int GreaterThanOrEqualToZero(int argument, string name)
			=> GreaterThanOrEqual(argument, 0, name);
	}
}
