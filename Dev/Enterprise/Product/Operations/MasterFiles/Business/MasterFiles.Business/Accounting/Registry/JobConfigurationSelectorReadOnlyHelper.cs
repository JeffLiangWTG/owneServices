using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class JobConfigurationSelectorReadOnlyHelper
	{
		public static bool DirectionCode_ReadOnly(string jobType)
			=> !string.IsNullOrEmpty(jobType)
				&& jobType != "SHP"
				&& jobType != "QSH"
				&& jobType != "CSH"
				&& jobType != "CLL"
				&& jobType != "FCN"
				&& jobType != "GCN"
				&& jobType != "AGS";

		public static bool Mode_ReadOnly(string jobType)
			=> !string.IsNullOrEmpty(jobType)
				&& jobType != "SHP"
				&& jobType != "CSH"
				&& jobType != "CLL"
				&& jobType != "FCN"
				&& jobType != "GCN"
				&& jobType != "QSH";

		public static void UpdateFieldIfShouldBeReadOnly(ZPropertyInfo propertyInfo, ref ZString fieldValueToRestore)
		{
			var valueAsString = (ZString)propertyInfo.Value;

			if (propertyInfo.ReadOnly)
			{
				if (!valueAsString.IsEmpty)
				{
					fieldValueToRestore = valueAsString;
					propertyInfo.Value = ZString.Empty;
				}
			}
			else if (!string.IsNullOrEmpty(fieldValueToRestore) && valueAsString.IsEmpty)
			{
				propertyInfo.Value = fieldValueToRestore;
			}
		}
	}
}
