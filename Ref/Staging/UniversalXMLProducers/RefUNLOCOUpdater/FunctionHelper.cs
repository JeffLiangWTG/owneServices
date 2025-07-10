using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public static class FunctionHelper
	{
		public static bool HasAirport(this string function)
		{
			Argument.NotNull(function, nameof(function));
			return function.Contains(Function.Airport);
		}

		public static bool HasRail(this string function)
		{
			Argument.NotNull(function, nameof(function));
			return function.Contains(Function.Rail);
		}

		public static bool HasRoad(this string function)
		{
			Argument.NotNull(function, nameof(function));
			return function.Contains(Function.Road);
		}

		public static bool HasSeaport(this string function)
		{
			Argument.NotNull(function, nameof(function));
			return function.Contains(Function.Seaport);
		}

		public static bool HasPost(this string function)
		{
			Argument.NotNull(function, nameof(function));
			return function.Contains(Function.Post);
		}

		public static bool HasBorderCrossing(this string function)
		{
			Argument.NotNull(function, nameof(function));
			return function.Contains(Function.BorderCrossing);
		}
	}

	public static class Function
	{
		public const string Seaport = "1";
		public const string Rail = "2";
		public const string Road = "3";
		public const string Airport = "4";
		public const string Post = "5";
		public const string BorderCrossing = "B";
	}
}
