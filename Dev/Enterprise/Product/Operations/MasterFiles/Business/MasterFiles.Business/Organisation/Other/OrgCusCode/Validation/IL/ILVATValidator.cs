using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Organisation.Other.OrgCusCode.Validation.IL
{
	public static class ILVATValidator
	{
		internal static void ValidateVATRegistrationNumber(ZPropertyInfo customsRegNoInfo)
		{
			var ilVAT = customsRegNoInfo.Value.ToString();
			if (ilVAT.Length != 9
				|| !ilVAT.All(char.IsDigit)
				|| !LuhnAlgorithm.IsValidLastDigitChecksum(ilVAT)
				)
			{
				var invalidVATFormat = Res.GetString("28888ec7-49ee-4bdc-bdbc-d60651484372", "Israel VAT Business Registration Number should be 9 digits NNNNNNNNN and compiled with last digit checksum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please check you have input the correct value.");
				customsRegNoInfo.AddErrorIfEnforced(invalidVATFormat, OrganisationRegistry.RegistrationNumberFormatFields.ILVAT);
			}
		}
	}
}
