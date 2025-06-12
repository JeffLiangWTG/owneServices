using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace eHubDeploymentTools
{
	public class TaskTypes
	{
		public enum TaskType
		{
			[Description("Get Latest Version")]
			GetLatest,
			[Description("Clean eHub Biztalk")]
			CleanBiztalkApplications,
			[Description("Create eHubTransactionDB")]
			CreateEHubTransactionDB,
			[Description("Deploy Biztalk Solution Dev")]
			DeployBtsSolutionDev,
			[Description("Build CSS")]
			BuildContainerTrackingSystem,
			[Description("Publish CSS DB")]
			PublishCSSDatabase
		}

		public enum PropertyType
		{
			FileSelector,
			DirectorySelector,
			TextSelector,
			ValidateDistributive
		}

		public static string GetDescription(object enumValue)
		{
			Type enumType = enumValue.GetType();

			if( !(enumType.BaseType == typeof(Enum))) return null;
			if (!Enum.IsDefined(enumType, enumValue)) return null;

			foreach (string value in Enum.GetNames(enumType))
			{
				if (value == enumValue.ToString())
				{
					FieldInfo fi = enumType.GetField(value);
					object[] descriptionAttrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
					DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
					return description.Description;
				}
			}

			return null;
		}

		public static string GetDescription(Type enumType, string name)
		{
			if (!(enumType.BaseType == typeof(Enum))) return null;
			if (!Enum.IsDefined(enumType, name)) return null;

			foreach (string value in Enum.GetNames(enumType))
			{
				if (value == name.ToString())
				{
					FieldInfo fi = enumType.GetField(value);
					object[] descriptionAttrs = fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
					DescriptionAttribute description = (DescriptionAttribute)descriptionAttrs[0];
					return description.Description;
				}
			}

			return null;
		}
	}
}
