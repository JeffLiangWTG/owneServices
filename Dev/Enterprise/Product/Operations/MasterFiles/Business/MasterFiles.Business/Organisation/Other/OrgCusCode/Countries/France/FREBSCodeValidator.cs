using System.Linq;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	sealed class FREBSCodeValidator
	{
		internal void Validate(OrgCusCode orgCusCode)
		{
			var codeInfo = orgCusCode.OK_CustomsRegNoInfo;

			var ebsValue = orgCusCode.OK_CustomsRegNo;
			if (!ebsValue.IsNumbersOnlyOrEmpty || ebsValue.Length != 5)
			{
				codeInfo.AddMessageError(InvalidEbsLengthMessage);
			}

			var organisation = orgCusCode.Organisation;
			if (organisation != null)
			{
				var existingEori = organisation.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori && x.CodeCountry.Code == CountryCodes.France);

				if (existingEori == null)
				{
					codeInfo.AddError(NoExistingEoriMessage);
				}
				else
				{
					if (existingEori.OK_CustomsRegNo.Length != 9)
					{
						codeInfo.AddWarning(InvalidEoriLengthMessage);
					}
				}
			}
		}

		static string InvalidEbsLengthMessage => Res.GetString("EFD034C7-F372-413A-9A4C-1F3ABDD30E5B", "EORI Branch Suffix should be 5 digits and different to the EORI code, e.g. 00001");

		static string NoExistingEoriMessage => Res.GetString("E07F060F-A983-452D-8C1C-5C9FB19170E2", "There is no FR EORI to link this information to.");

		static string InvalidEoriLengthMessage => Res.GetString("B3A7920A-B549-4D73-840E-7ED25184995C", "The linked FR EORI should be 9 digits long.");
	}
}
