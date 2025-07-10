using System;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public static class LithiumBatteryConstants
	{
		public static class UNNOCodes
		{
			public static List<string> CodesList => codesList ?? (codesList = new List<string>
			{
				LithiumMetalBatteries,
				LithiumIonBatteries,
				PackedLithiumMetalBatteries,
				PackedLithiumIonBatteries
			});

			[ThreadStatic]
			static List<string> codesList;

			public const string LithiumMetalBatteries = "3090";
			public const string LithiumIonBatteries = "3480";
			public const string PackedLithiumMetalBatteries = "3091";
			public const string PackedLithiumIonBatteries = "3481";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Reference Files UNDGSubstance constant")]
		public static class RefPackingInstructions
		{
			public const string PI965 = "965";
			public const string PI966 = "966";
			public const string PI967 = "967";
			public const string PI968 = "968";
			public const string PI969 = "969";
			public const string PI970 = "970";
			public const string Forbidden = "Forbidden";
		}

		public static class SpecialHandlingCodes
		{
			public const string RBI = "RBI";
			public const string EBI = "EBI";
			public const string RLI = "RLI";
			public const string ELI = "ELI";
			public const string RBM = "RBM";
			public const string EBM = "EBM";
			public const string RLM = "RLM";
			public const string ELM = "ELM";
		}

		public static Dictionary<string, string> RefPackingInstructionCodeDictionary => refPackingInstructionToPackingTypeDictionary
			?? (refPackingInstructionToPackingTypeDictionary = new Dictionary<string, string>()
		{
			{ RefPackingInstructions.PI965, Core.Constants.AWB.LithiumBatteryTypes.Codes.PI965 },
			{ RefPackingInstructions.PI966, Core.Constants.AWB.LithiumBatteryTypes.Codes.PI966 },
			{ RefPackingInstructions.PI967, Core.Constants.AWB.LithiumBatteryTypes.Codes.PI967 },
			{ RefPackingInstructions.PI968, Core.Constants.AWB.LithiumBatteryTypes.Codes.PI968 },
			{ RefPackingInstructions.PI969, Core.Constants.AWB.LithiumBatteryTypes.Codes.PI969 },
			{ RefPackingInstructions.PI970, Core.Constants.AWB.LithiumBatteryTypes.Codes.PI970 },
			{ RefPackingInstructions.Forbidden, Core.Constants.AWB.LithiumBatteryTypes.Codes.LMB }
		});

		[ThreadStatic]
		static Dictionary<string, string> refPackingInstructionToPackingTypeDictionary;
	}
}
