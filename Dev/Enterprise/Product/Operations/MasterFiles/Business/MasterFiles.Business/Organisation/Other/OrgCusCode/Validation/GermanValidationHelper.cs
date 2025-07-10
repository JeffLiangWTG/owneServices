using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class GermanValidationHelper
	{
		public static void ValidateUST(ZPropertyInfo codeInfo)
		{
			var regoNo = (ZString)codeInfo.Value;
			if (!Regex.IsMatch(regoNo, @"^(DE|)(\d){9}$"))
			{
				codeInfo.AddWarning(Res.GetString("745f9900-f926-427c-a270-a7be4c25d46f", "VAT Business Registration Number structures for Germany are either 'DE999999999' or '999999999'."));
			}
		}

		public static void ValidateEBS(ZPropertyInfo codeInfo)
		{
			var regNo = (ZString)codeInfo.Value;
			if (!regNo.IsNumbersOnlyOrEmpty || regNo.Length != 4)
			{
				codeInfo.AddMessageError(Res.GetString("C30F7B22-74F3-4D8F-976E-6A6E34AA435A", "EORI Branch Suffix should be 4 digits and different to the EORI code, e.g. 0001"));
			}
		}

		public static void ValidateCWA(ZPropertyInfo codeInfo)
		{
			if (!((ZInt)((ZString)codeInfo.Value).Length).IsInRange(10, 33))
			{
				codeInfo.AddWarning(Res.GetString("6D75CEE8-83B3-4CC6-BABE-9461385168D5", "Customs Warehouse Procedure Authorization code should be 10-33 characters long."));
			}
		}

		internal static void ValidateLCO(ZPropertyInfo codeInfo)
		{
			if (!((ZInt)((ZString)codeInfo.Value).Length).IsInRange(12, 35))
			{
				codeInfo.AddWarning(Res.GetString("64F11211-AB00-4602-89F4-A135FC52BB19", "Local Clearance Outward Processing code should be 12-35 characters long."));
			}
		}

		internal static void ValidateOPR(ZPropertyInfo codeInfo)
		{
			if (!((ZInt)((ZString)codeInfo.Value).Length).IsInRange(12, 35))
			{
				codeInfo.AddWarning(Res.GetString("CC00CB98-AC23-488A-A2D7-49FE31431E69", "Outward Processing code should be 12-35 characters long."));
			}
		}

		internal static void ValidateAEX(ZPropertyInfo codeInfo)
		{
			if (!((ZInt)((ZString)codeInfo.Value).Length).IsInRange(12, 35))
			{
				codeInfo.AddWarning(Res.GetString("5209E0B4-C5C4-4EC1-B18C-868C12D49804", "Accredited Exporter code should be 12-35 characters long."));
			}
		}

		internal static void ValidateTAO(ZPropertyInfo codeInfo)
		{
			var regoNo = (ZString)codeInfo.Value;
			if (!regoNo.IsNumbersOnlyOrEmpty || (ZInt.TryParse(regoNo, out ZInt parseResult) && !parseResult.IsInRange(1000, 9999)))
			{
				codeInfo.AddWarning(Res.GetString("50B9B853-3652-4081-B3B0-3893BF34E48E", "TAO value must be a number between 1000 and 9999."));
			}
		}

		internal static void ValidateDBI(ZPropertyInfo codeInfo)
		{
			var regoNo = (ZString)codeInfo.Value;
			if (regoNo.Length > 10)
			{
				codeInfo.AddError(Res.GetString("748728dc-1f6a-ac86-454b-be001a127a6d", "The maximum length is 10 characters."));
			}
		}

		internal static void ValidateIMA(ZPropertyInfo codeInfo)
		{
			var regNo = (ZString)codeInfo.Value;
			if (regNo.Length != 5)
			{
				codeInfo.AddMessageError(Res.GetString("5381A701-8F8D-4CF8-B158-CB5BAC106E42", "IMA Code must contain 5 characters."));
			}
		}

		internal static string OnlyOneRecordAllowedCountryAndTypeMessage(OrgCusCode orgCusCode)
		{
			return Res.GetString("8195B52B-FC89-44F4-93E3-FCBECD303067",
				"Only one record allowed (Uniqueness: Country of Issue + Type '{0}')", orgCusCode.OK_CodeType);
		}

		internal static void ValidateHandelsRegister(ZPropertyInfo codeInfo)
		{
			var regNo = (ZString)codeInfo.Value;
			if (regNo.Length < 4)
			{
				codeInfo.AddErrorIfEnforced(Res.GetString("de6c3669-00aa-4d7b-8b2d-5049cfbeb4d0", "HRB length must be more than 3 characters."), OrganisationRegistry.RegistrationNumberFormatFields.DEHRB);
			}
			else if (!Regex.IsMatch(regNo, "^(HRB|HRA) [0-9]+( ?[A-Z]+)?$"))
			{
				codeInfo.AddErrorIfEnforced(Res.GetString("5cc2c991-1300-4367-9b62-a2d3dcbee48b", "Format of Handelsregister / Business Registration Number (HRB) is not correct. Accepted formats are: HRB or HRA followed by a space and one or more digits, e.g. 'HRB  123' or 'HRA 12345' OR HRB or HRA followed by a space, then one or more digits, followed by an optional space then alpha characters, e.g. 'HRB 123 HL' or 'HRB 123HL'."), OrganisationRegistry.RegistrationNumberFormatFields.DEHRB);
			}
		}
	}
}
