using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public static class PreferenceListHelper
	{
		public static void DefaultPreference(ICodeDescriptionPairList preferenceList, ZPropertyInfo primaryPreferenceInfo)
		{
			ICodeDescription foundPreference = null;
			if (preferenceList.Count < 3)
			{
				if (preferenceList.Count == 1)
				{
					foundPreference = (ICodeDescription)preferenceList[0];
				}
				else
				{
					var nonStandardTypes = preferenceList.Cast<ICodeDescription>().Where(x => x.Code != UniversalReferenceConstants.PrimaryPreference.Standard).ToArray();
					if (nonStandardTypes.Length == 1)
					{
						foundPreference = nonStandardTypes[0];
					}
				}
			}
			primaryPreferenceInfo.Value = foundPreference == null ? ZString.Empty : new ZString(foundPreference.Code);
		}
	}
}
