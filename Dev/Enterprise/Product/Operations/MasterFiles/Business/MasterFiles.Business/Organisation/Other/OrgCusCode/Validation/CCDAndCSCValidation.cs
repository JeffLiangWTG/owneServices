using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for CCDAndCSCValidation.
	/// </summary>
	public class CCDAndCSCValidation
	{
		public CCDAndCSCValidation(ZString codeRaw)
		{
			fCodeRaw = codeRaw.ToUpper();
			fIsOwnerCode = false;
		}
		public CCDAndCSCValidation(ZString codeRaw, ZBool isOwnerCode)
		{
			fCodeRaw = codeRaw.ToUpper();
			fIsOwnerCode = isOwnerCode;
		}

		#region Constants
		public static string InvalidCCDLength
		{
			get { return Res.GetString("55c50815-bb55-451b-94ea-bbab6d2d8f4e", "The Customs Client Code (CCD) needs to be 8 or less in length including the check digit.") + " " + ModifyProcedureCCD; }
		}
		public static string InvalidCSCLength
		{
			get { return Res.GetString("a9d78ba3-0c55-4c71-9e7b-ecd6de284fc8", "The Customs Supplier Code (CSC) needs to be 8 or less in length including the check digit") + " " + ModifyProcedureCSC; }
		}
		public static string InvalidCheckDigitCCD(string actual, string expected)
		{
			return Res.GetString("1a8eba18-d91c-4ae1-bfa3-75443bb0de20", "The check digit in the Customs Client Code (CCD) is incorrect. Current value is : {0}, expected {1}.", actual, expected);
		}
		public static string InvalidCheckDigitCSC(string actual, string expected)
		{
			return Res.GetString("2f826d76-0897-43be-9f66-b9b06bf755b2", "The check digit in the Customs Supplier Code (CSC) is incorrect. Current value is : {0}, expected {1}.", actual, expected);
		}
		public static string ModifyProcedureCCD
		{
			get { return Res.GetString("a2783825-73c4-4ccf-a8ca-8a59b7172743", "Press F3 to edit master files and modify the CCD on the config tab."); }
		}
		public static string ModifyProcedureCSC
		{
			get { return Res.GetString("7194e56e-0639-4264-8226-2d4a61ce99eb", "Press F3 to edit master files and modify the CSC on the config tab."); }
		}
		public static string RequiredFormatCCD
		{
			get { return Res.GetString("eaec8cd1-ae6a-420a-92e5-0b0187845436", "The Customs Client Code needs to have a format of 7 or less numerics and a check digit."); }
		}
		public static string RequiredFormatCSC
		{
			get { return Res.GetString("363e95ea-1524-4798-befa-15b053b0c1e6", "The Customs Supplier Code needs to have a format of 7 or less numerics and a check digit."); }
		}
		#endregion

		protected ZString fCodeRaw;
		protected ZBool fIsOwnerCode;

		public ZString CheckValid()
		{
			ZString codeTOCheck = fCodeRaw;
			ZString result = ZString.Empty;

			if (codeTOCheck.Length > 8)
			{
				if (fIsOwnerCode)
				{
					result = InvalidCCDLength;
				}
				else
				{
					result = InvalidCSCLength;
				}
			}
			else
			{
				ZString codeToWorkWith = codeTOCheck.PadLeft(11, '0');
				try
				{
					int checkTotal = (((int.Parse(codeToWorkWith[0].ToString())) * 10 +
						int.Parse(codeToWorkWith[1].ToString()) * 9 +
						int.Parse(codeToWorkWith[2].ToString()) * 8 +
						int.Parse(codeToWorkWith[3].ToString()) * 7 +
						int.Parse(codeToWorkWith[4].ToString()) * 6 +
						int.Parse(codeToWorkWith[5].ToString()) * 5 +
						int.Parse(codeToWorkWith[6].ToString()) * 4 +
						int.Parse(codeToWorkWith[7].ToString()) * 3 +
						int.Parse(codeToWorkWith[8].ToString()) * 2 +
						int.Parse(codeToWorkWith[9].ToString()) * 1) % 11) + 1;
					if (fIsOwnerCode)
					{
						if (checkTotal > 8)
						{
							checkTotal += 1;
						}
						char checkDigit = (char)(64 + checkTotal);
						if (checkDigit.ToString() != codeTOCheck.Right(1))
						{
							result = InvalidCheckDigitCCD(codeTOCheck.Right(1), checkDigit.ToString()) + " " + ModifyProcedureCCD;
						}
					}
					else
					{
						ZString checkDigit = "MNPQRTUVWXY".Substring(checkTotal - 1, 1);
						if (checkDigit != codeTOCheck.Right(1))
						{
							result = InvalidCheckDigitCSC(codeTOCheck.Right(1), checkDigit) + " " + ModifyProcedureCSC;
						}
					}
				}
				catch (FormatException)
				{
					if (fIsOwnerCode)
					{
						result = RequiredFormatCCD + " " + ModifyProcedureCCD;
					}
					else
					{
						result = RequiredFormatCSC + " " + ModifyProcedureCSC;
					}
				}
			}
			return result;
		}
	}
}
