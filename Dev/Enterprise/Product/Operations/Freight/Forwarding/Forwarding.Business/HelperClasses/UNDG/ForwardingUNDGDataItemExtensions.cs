using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ForwardingUNDGDataItemExtensions
	{
		static HashSet<string> SpecificSupportForAirFreightCodes => specificSupportForAirFreightCodes ?? (specificSupportForAirFreightCodes = new HashSet<string>
			{
			"0190",
			"0020",
			"0248",
			"0021",
			"3167",
			"3169",
			"3168",
			"3274",
			"3343",
			"3357",
			"2760A",
			"2760B",
			"2782A",
			"2782B",
			"2758A",
			"2758B",
			"2776A",
			"2776B",
			"3024A",
			"3024B",
			"2778A",
			"2778B",
			"2762A",
			"2762B",
			"2784A",
			"2784B",
			"2787A",
			"2787B",
			"3021A",
			"3021B",
			"3346A",
			"3346B",
			"3350A",
			"3350B",
			"2780A",
			"2780B",
			"2772A",
			"2772B",
			"2764A",
			"2764B",
			"3344A",
			"3344C",
			"3344B",
			"1378",
			"1373C",
			"1373D",
			"1373E",
			"1373A",
			"1373B",
			"1373F",
			"3381",
			"3382",
			"3389",
			"3390",
			"3383",
			"3384",
			"3488",
			"3489",
			"3387",
			"3388",
			"3385",
			"3386",
			"3490",
			"3491",
			"3145A",
			"3145B",
			"3145C",
			"2430A",
			"2430B",
			"2430C",
			"3257",
			"3258"
			});

		[ThreadStatic]
		static HashSet<string> specificSupportForAirFreightCodes;

		static bool IsSpecificSupportForAirFreight(UNDGSubstance substance)
		{
			return substance != null && SpecificSupportForAirFreightCodes.Contains(substance.DG_Code.ToUpper());
		}

		public static ZString GetPSNWithAdditionalTextIfSupportForNOS(this ForwardingUNDGDataItem dataItem)
		{
			if (dataItem == null || dataItem.Substance == null)
			{
				return ZString.Empty;
			}

			var result = new ZStringBuilder(dataItem.Substance.DG_PSN);

			if (dataItem.Substance.DG_Standard == UNDGSubstanceStandardTypes.IATA)
			{
				if (dataItem.DI_IsNotOtherwiseSpecified)
				{
					result.Append("n.o.s.");
					result.AppendIfNotEmpty(!IsSpecificSupportForAirFreight(dataItem.Substance) ? ZString.Empty : dataItem.Substance.QualifyingDescriptiveTexts.FirstOrDefault(q => q.DA_Language == "EN")?.DA_Descriptor ?? ZString.Empty);
				}
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		#region IsRadioactiveInExceptedQuantities

		static HashSet<string> GetRadioactiveDGCodes() => new HashSet<string> { "2908", "2909A", "2909B", "2909C", "2910", "2911A", "2911B" };

		public static bool IsRadioactiveInExceptedQuantities(this UNDGSubstance substance) => substance != null && substance.DG_Standard == UNDGSubstanceStandardTypes.IATA && substance.DG_Class == "7" && GetRadioactiveDGCodes().Contains(substance.DG_Code.ToUpper());

		#endregion
	}
}
