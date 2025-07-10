//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefContainerCodeMapValidation
//
//    This class should be used for overriding validation in AutoRefContainerCodeMapValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefContainerCodeMapValidation : AutoRefContainerCodeMapValidation
	{
		public RefContainerCodeMapValidation(AutoRefContainerCodeMap parent) : base(parent)
		{
		}

		public new RefContainerCodeMap Parent => (RefContainerCodeMap)base.Parent;

		protected override void CheckRCM_Code()
		{
			base.CheckRCM_Code();
			var targetInfo = Parent.RCM_CodeInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			if (Parent.RCM_RN_NKCountry == Core.Constants.CountryCodes.Japan)
			{
				ListValidation.ErrorIfInvalidCode(targetInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
			}
		}

		protected override void CheckRCM_Usage()
		{
			base.CheckRCM_Usage();
			var targetInfo = Parent.RCM_UsageInfo;
			if (Parent.ContainerMapProvider != null && Parent.ContainerMapProvider.IsUsageNeeded != UsageRequirement.NotRequire)
			{
				if (Parent.ContainerMapProvider.IsUsageNeeded == UsageRequirement.Require)
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(targetInfo);
				}
				else if (Parent.ContainerMapProvider.IsUsageNeeded == UsageRequirement.MayRequire)
				{
					ListValidation.MessageErrorIfInvalidCode(targetInfo);
				}
			}
			else
			{
				MandatoryValidation.MessageErrorIfIsEntered(targetInfo);
			}

			if (Parent.RefContainer?.CodeMapCollection.Cast<RefContainerCodeMap>().Any(codeMap => codeMap.PK != Parent.PK && codeMap.RCM_RN_NKCountry == Parent.RCM_RN_NKCountry && codeMap.RCM_Usage == Parent.RCM_Usage) ?? false)
			{
				targetInfo.AddError(Res.GetString("f785ad49-3440-4d75-8e9a-f217d0b1d3f3", "There is already a code map with same Country/Region and Usage."));
			}
		}

		protected override void CheckRCM_RN_NKCountry()
		{
			base.CheckRCM_RN_NKCountry();
			var targetInfo = Parent.RCM_RN_NKCountryInfo;
			MandatoryValidation.CheckEntered(targetInfo);
			ListValidation.ErrorIfInvalidCode(targetInfo);
		}
	}
}
