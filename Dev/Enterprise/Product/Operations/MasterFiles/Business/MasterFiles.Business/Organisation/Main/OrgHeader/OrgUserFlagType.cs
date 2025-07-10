using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public struct OrgUserFlagType
	{
		OrgUserFlagType(ZString flagNumber, SchemaBoolColumn orgHeaderColumn, ZString label)
		{
			FlagNumber = flagNumber;
			OrgHeaderColumn = orgHeaderColumn;
			Label = label;
		}

		public readonly ZString FlagNumber;
		public readonly SchemaBoolColumn OrgHeaderColumn;
		public readonly ZString Label;

		#region Equals / HashCode

		public override bool Equals(object obj)
		{
			if (!(obj is OrgUserFlagType))
			{
				return false;
			}

			var objAsUserFlagType = (OrgUserFlagType)obj;
			return
				FlagNumber == objAsUserFlagType.FlagNumber
				&& OrgHeaderColumn == objAsUserFlagType.OrgHeaderColumn
				&& Label == objAsUserFlagType.Label;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 13;
				hash = (hash * 7) + FlagNumber.GetHashCode();
				hash = (hash * 7) + OrgHeaderColumn.GetHashCode();
				hash = (hash * 7) + Label.GetHashCode();

				return hash;
			}
		}

		#endregion

		#region Static

		public static IEnumerable<OrgUserFlagType> All
		{
			get
			{
				yield return new OrgUserFlagType("01", OrgHeaderSchema.OH_IsUserFlag1, Env.Registry.OrgUserFlag1Label);
				yield return new OrgUserFlagType("02", OrgHeaderSchema.OH_IsUserFlag2, Env.Registry.OrgUserFlag2Label);
				yield return new OrgUserFlagType("03", OrgHeaderSchema.OH_IsUserFlag3, Env.Registry.OrgUserFlag3Label);
				yield return new OrgUserFlagType("04", OrgHeaderSchema.OH_IsUserFlag4, Env.Registry.OrgUserFlag4Label);
				yield return new OrgUserFlagType("05", OrgHeaderSchema.OH_IsUserFlag5, Env.Registry.OrgUserFlag5Label);
				yield return new OrgUserFlagType("06", OrgHeaderSchema.OH_IsUserFlag6, Env.Registry.OrgUserFlag6Label);
				yield return new OrgUserFlagType("07", OrgHeaderSchema.OH_IsUserFlag7, Env.Registry.OrgUserFlag7Label);
				yield return new OrgUserFlagType("08", OrgHeaderSchema.OH_IsUserFlag8, Env.Registry.OrgUserFlag8Label);
				yield return new OrgUserFlagType("09", OrgHeaderSchema.OH_IsUserFlag9, Env.Registry.OrgUserFlag9Label);
				yield return new OrgUserFlagType("10", OrgHeaderSchema.OH_IsUserFlag10, Env.Registry.OrgUserFlag10Label);
				yield return new OrgUserFlagType("11", OrgHeaderSchema.OH_IsUserFlag11, Env.Registry.OrgUserFlag11Label);
				yield return new OrgUserFlagType("12", OrgHeaderSchema.OH_IsUserFlag12, Env.Registry.OrgUserFlag12Label);
				yield return new OrgUserFlagType("13", OrgHeaderSchema.OH_IsUserFlag13, Env.Registry.OrgUserFlag13Label);
				yield return new OrgUserFlagType("14", OrgHeaderSchema.OH_IsUserFlag14, Env.Registry.OrgUserFlag14Label);
				yield return new OrgUserFlagType("15", OrgHeaderSchema.OH_IsUserFlag15, Env.Registry.OrgUserFlag15Label);
				yield return new OrgUserFlagType("16", OrgHeaderSchema.OH_IsUserFlag16, Env.Registry.OrgUserFlag16Label);
				yield return new OrgUserFlagType("17", OrgHeaderSchema.OH_IsUserFlag17, Env.Registry.OrgUserFlag17Label);
				yield return new OrgUserFlagType("18", OrgHeaderSchema.OH_IsUserFlag18, Env.Registry.OrgUserFlag18Label);
				yield return new OrgUserFlagType("19", OrgHeaderSchema.OH_IsUserFlag19, Env.Registry.OrgUserFlag19Label);
				yield return new OrgUserFlagType("20", OrgHeaderSchema.OH_IsUserFlag20, Env.Registry.OrgUserFlag20Label);
				yield return new OrgUserFlagType("21", OrgHeaderSchema.OH_IsUserFlag21, Env.Registry.OrgUserFlag21Label);
				yield return new OrgUserFlagType("22", OrgHeaderSchema.OH_IsUserFlag22, Env.Registry.OrgUserFlag22Label);
				yield return new OrgUserFlagType("23", OrgHeaderSchema.OH_IsUserFlag23, Env.Registry.OrgUserFlag23Label);
				yield return new OrgUserFlagType("24", OrgHeaderSchema.OH_IsUserFlag24, Env.Registry.OrgUserFlag24Label);
				yield return new OrgUserFlagType("25", OrgHeaderSchema.OH_IsUserFlag25, Env.Registry.OrgUserFlag25Label);
				yield return new OrgUserFlagType("26", OrgHeaderSchema.OH_IsUserFlag26, Env.Registry.OrgUserFlag26Label);
				yield return new OrgUserFlagType("27", OrgHeaderSchema.OH_IsUserFlag27, Env.Registry.OrgUserFlag27Label);
				yield return new OrgUserFlagType("28", OrgHeaderSchema.OH_IsUserFlag28, Env.Registry.OrgUserFlag28Label);
				yield return new OrgUserFlagType("29", OrgHeaderSchema.OH_IsUserFlag29, Env.Registry.OrgUserFlag29Label);
				yield return new OrgUserFlagType("30", OrgHeaderSchema.OH_IsUserFlag30, Env.Registry.OrgUserFlag30Label);
				yield return new OrgUserFlagType("31", OrgHeaderSchema.OH_IsUserFlag31, Env.Registry.OrgUserFlag31Label);
				yield return new OrgUserFlagType("32", OrgHeaderSchema.OH_IsUserFlag32, Env.Registry.OrgUserFlag32Label);
			}
		}

		#endregion
	}
}
