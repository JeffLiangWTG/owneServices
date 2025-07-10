using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobConfigurationSelectorHelper
	{
		public JobConfigurationSelectorHelper()
		{
		}

		#region Offset Type
		CodeDescriptionPairList fOffsetTypeList;
		public CodeDescriptionPairList OffsetTypeList
		{
			get
			{
				if (fOffsetTypeList == null)
				{
					fOffsetTypeList = new CodeDescriptionPairList();
					fOffsetTypeList.AddPair(OffsetTypeCodes.Days, OffsetTypeDescriptions.Days);
					fOffsetTypeList.AddPair(OffsetTypeCodes.Periods, OffsetTypeDescriptions.Periods);
				}

				return fOffsetTypeList;
			}
		}
		#endregion

		#region Offset Type

		public CodeDescriptionPairList OffsetTypeListForAutoJobClosure
		{
			get
			{
				if (offsetTypeListForAutoJobClosure == null)
				{
					offsetTypeListForAutoJobClosure = new CodeDescriptionPairList();
					offsetTypeListForAutoJobClosure.AddPair(OffsetTypeCodes.Days, OffsetTypeDescriptions.Days);
					offsetTypeListForAutoJobClosure.AddPair(OffsetTypeCodes.Month, OffsetTypeDescriptions.Month);
				}

				return offsetTypeListForAutoJobClosure;
			}
		}
		CodeDescriptionPairList offsetTypeListForAutoJobClosure;

		#endregion

		#region Validation Helper
		public static string ValidateOffset(ZString offsetType, ZInt offset)
		{
			if (offsetType == JobConfigurationSelectorHelper.OffsetTypeCodes.Days)
			{
				if (offset < 0 || offset > 100)
				{
					return Res.GetString("38456fce-c815-4557-b613-5f8ec26d07bb", "Days Offset must be between 0 and 100.");
				}
			}
			else if (offsetType == JobConfigurationSelectorHelper.OffsetTypeCodes.Periods)
			{
				if (offset < 0 || offset > 3)
				{
					return Res.GetString("a05e60a1-53ad-493f-8ee3-02a4262358ec", "Periods Offset must be between 0 and 3.");
				}
			}
			return string.Empty;
		}

		public static string OffSetTypeText
		{
			get { return Res.GetString("87d62d5b-103c-4558-982e-82d6de12c660", "Offset Type"); }
		}
		#endregion

		public static class OffsetTypeCodes
		{
			public const string Days = "DAY";
			public const string Periods = "PER";
			public const string Month = "MONTH";
		}

		public static class OffsetTypeDescriptions
		{
			public static string Days
			{
				get { return Res.GetString("9aed8963-2b7b-4992-9272-7540354c699c", "Offset by days"); }
			}
			public static string Periods
			{
				get { return Res.GetString("987dac99-3201-4f97-b875-970c5dd43402", "Offset by accounting periods"); }
			}

			public static string Month
			{
				get { return Res.GetString("99c4e1ba-ef90-428b-897f-90612d05ddf5", "Offset by month"); }
			}
		}

		#region Configuration Type List

		CodeDescriptionPairList configurationTypeList;
		public CodeDescriptionPairList ConfigurationTypeList
		{
			get
			{
				if (configurationTypeList == null)
				{
					configurationTypeList = new CodeDescriptionPairList();
					configurationTypeList.AddPair(ConfigurationTypeCodes.Close, ConfigurationTypeDescriptions.Close);
					configurationTypeList.AddPair(ConfigurationTypeCodes.Update, ConfigurationTypeDescriptions.Update);
				}

				return configurationTypeList;
			}
		}

		public static class ConfigurationTypeCodes
		{
			public const string Close = "CLS";
			public const string Update = "UPD";
		}

		public static class ConfigurationTypeDescriptions
		{
			public static string Close
			{
				get { return Res.GetString("5f631713-8cef-4a3b-92ac-ee5c49f526a2", "Close"); }
			}
			public static string Update
			{
				get { return Res.GetString("148ff542-4472-461b-8da2-d08929d922c3", "Update"); }
			}
		}

		#endregion
	}
}
