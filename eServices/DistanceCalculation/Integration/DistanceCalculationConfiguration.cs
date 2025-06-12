using System;

namespace Enterprise.Freight.DistanceCalculation.Integration
{
	[Serializable]
	public class DistanceCalculationConfiguration
	{
		public string ProviderCode { get; set; }
		public string ProviderVersion { get; set; }
		public string CalculationMethod { get; set; }
		public string UnitsForCalculation { get; set; }
		public string HazardousType { get; set; }
		public string RoutingType { get; set; }
		public string AvoidTollRoads { get; set; }
	}

	#region Constants

	public static class DistanceCalculationConstants
	{
		public static class Providers
		{
			public const string PCMiler = "PCM";
			public const string Google = "GOO";
			public const string CargoWise = "CWS";
			public const string DefaultFromRegistry = "DEF";
		}

		public static class ProviderVersions
		{
			public static class PCMiler
			{
				public const string Current = "CUR";
				public const string v18 = "18";
				public const string v19 = "19";
				public const string v20 = "20";
				public const string v21 = "21";
				public const string v22 = "22";
                public const string v23 = "23";
                public const string v24 = "24";
			}
		}

		public static class CalculationMethods
		{
			public static class PCMiler
			{
				public const string Practical = "PRA";
				public const string Shortest = "SHO";
			}
		}

		public static class UnitsForCalculation
		{
			public const string Miles = "M";
			public const string Kilometres = "K";
			public const string Default = Miles;
		}
	}

	public static class DistanceCalculationPCMilerConstants
	{
		public static class AvoidTollRoads
		{
			public const string Yes = "Y";
			public const string No = "N";

			public const string Default = No;
		}

		public static class HazardousTypes
		{
			public const string None = "N";
			public const string General = "G";
			public const string Caustic = "C";
			public const string Explosives = "E";
			public const string Flammable = "F";
			public const string Inhalants = "I";
			public const string Radioactive = "R";

			public const string Default = None;
		}

		public static class RoutingTypes
		{
			public const string Practical = "P";
			public const string Shortest = "S";
			public const string LineOfSight = "L";

			public const string Default = Practical;
		}
	}

	#endregion
}