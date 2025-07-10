using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	internal static class OrgHeaderTypeValidation
	{
		internal static void ValidateOrgTypeMandatoryIfExpected(OrgHeader org, ZPropertyInfo info)
		{
			if (org != null && org.ExpectedOrgTypeInfos.Contains(info) && !(ZBool)info.Value)
			{
				info.AddError(Res.GetString("104a3dad-df32-4658-899c-c06088d5cfc4", "An Organization selected from here must have an Organization Type of {0} selected.", info.Description));
			}
		}

		internal static void ValidateOrgTypeMandatoryIfAtleastOneExpected(OrgHeader org, ZPropertyInfo info)
		{
			if (org != null && org.ExpectedAtleastOneOrgTypeInfos.Contains(info) && !(ZBool)info.Value)
			{
				foreach (ZPropertyInfoBool otherInfo in org.ExpectedAtleastOneOrgTypeInfos)
				{
					if (otherInfo.Value)
					{
						return;
					}
				}

				var propertyNames = Array.ConvertAll(org.ExpectedAtleastOneOrgTypeInfos.ToArray(), x => x.Description);
				var propertyNamesExceptLast = new List<string>(propertyNames);
				propertyNamesExceptLast.RemoveAt(propertyNamesExceptLast.Count - 1);
				var propertyNamesAsString = string.Join(", ", propertyNamesExceptLast.ToArray());
				propertyNamesAsString += " " + Res.GetString("7d7a5be9-0ec3-456c-857b-692ae9f7d039", "or") + " " + propertyNames[propertyNames.Length - 1];

				info.AddError(Res.GetString("bcc84cdc-6e9b-4fbf-ae76-0fd6be18b4ed", "An Organization selected from here must have an Organization type of either {0} selected.", propertyNamesAsString));
			}
		}
	}
}
