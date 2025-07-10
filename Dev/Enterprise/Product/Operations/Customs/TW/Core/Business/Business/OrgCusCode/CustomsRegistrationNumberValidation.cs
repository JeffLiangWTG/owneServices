using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CustomsRegistrationNumberValidation
	{
		public CustomsRegistrationNumberValidation(BusinessObject master)
		{
			Master = master;
		}

		protected readonly BusinessObject Master;

		protected virtual INotificationType ErrorType => CargoWise.ComponentModel.NotificationType.Error;

		protected virtual INotificationType WarningOrMessageError => CargoWise.ComponentModel.NotificationType.Warning;

		internal virtual void CheckVATCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			TaiwanUnifiedBusinessNumberValidator.Validate(customsRegNoInfo, number, ErrorType);
		}

		internal virtual void CheckPASCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 14)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("F9D31886-C857-485B-AB61-B18B32D20F95", "The length of PAS (Passport Number) shouldn't be more than 14."));
			}
			if (number.StartsWith(Constants.Number))
			{
				customsRegNoInfo.AddMessageError(Res.GetString("C1889B94-8058-4DD8-95F5-7B8A7F37CA76", "PAS number is formatted as NOxxxxxxxxx, but you only need to enter the suffix component of the number here."));
			}
		}

		internal virtual void CheckAEOCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number, ZString registrationNumberCountryCode)
		{
			if (!number.IsEmpty)
			{
				if (registrationNumberCountryCode == Core.Constants.CountryCodes.Taiwan)
				{
					if (number.Length > 14)
					{
						customsRegNoInfo.AddMessageError(Res.GetString("348c69ff-417b-494b-971a-d1a201417543", "The length of AEO (Authorized Economic Operator) shouldn't be more than 14."));
					}
					else if (!number.IsNumbersOnlyOrEmpty)
					{
						customsRegNoInfo.AddMessageError(Res.GetString("72a374b9-9635-445c-b4de-83ec355de495", "AEO Number is formatted as TWAEO-nnnnnnnnn, but you only need to enter the 9 digit suffix component of the number here."));
					}
				}
				if (!number.IsNumbersOnlyOrEmpty)
				{
					customsRegNoInfo.AddWarning(Res.GetString("1c2be5d9-bc00-46a2-9476-b68d1bcebd6e", "Not allow special characters."));
				}
			}
		}

		internal virtual void CheckTPCCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 8)
			{
				customsRegNoInfo.AddError(Res.GetString("165954b4-5e6e-406f-a4be-98f16cc0326c", "The length of TPC shouldn't be more than 8."));
			}
		}

		internal virtual void CheckPIDCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 10)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("27165e9a-3f34-4a5e-ba50-e242aed4cfb8", "The length of PID (Republic of China (Taiwan) National ID Card Number) shouldn't be more than 10."));
			}
		}

		internal virtual void CheckPBRCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 12)
			{
				customsRegNoInfo.AddError(Res.GetString("DAB09EAE-6C0F-4766-846A-57E7602F92DF", "The length of PBR shouldn't be more than 12."));
			}
		}

		internal virtual void CheckGTXCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length != 9 || !number.IsNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddErrorIfEnforced(Res.GetString("090EB3B8-D250-434B-AC6E-3CEA70B2B776F", "Taiwan GTX (Government Tax File Code) should be 9 digits NNNNNNNNN."), OrgCusCode.CodeTypes.TaxFileCode);
			}
		}

		internal virtual void CheckPIGCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			var regex = new Regex(@"^\d{3,7}$");
			if (!regex.IsMatch(number))
			{
				customsRegNoInfo.AddErrorIfEnforced(Res.GetString("BB776D48-811D-4298-8310-9F29590AA3FC", "Taiwan PIG (Public Interest Group) should be between 3 - 7 digits NNN, NNNN, NNNNN, NNNNNN, NNNNNNN."), OrgCusCode.TaiwanCodeTypes.PIG);
			}
		}

		internal virtual void CheckMCICustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			var regex = new Regex(@"^/{1}[A-Z0-9+-.]{7}$");
			if (!regex.IsMatch(number))
			{
				customsRegNoInfo.AddErrorIfEnforced(Res.GetString("FB776D48-811D-4298-8310-9F29590AA3FC", "Taiwan MCI (Mobile Carrier ID) should be 8 characters starting with '/' follow by 7 characters. The accepted characters are '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ+-. E.g. /J8U743A"), OrgCusCode.TaiwanCodeTypes.MCI);
			}
		}

		internal virtual void CheckEPZCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length != 5)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("B37F14A7-4C06-427C-864A-51AB07CAF9F4", "An Export Processing Zone Customs Controlling Premises Code must be 5 characters long."));
			}
			else if (!number.SubstringSafe(3, 2).IsNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("9D4A9FE2-3F40-4F6C-8070-8D23F91CA683", "The 4th and the 5th characters must be numbers."));
			}
			else if (!number.IsLettersAndNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("78EDCEAD-0360-43FA-8A3A-5E98D36028EA", "An EPZ code can only have alphanumeric characters."));
			}
		}

		internal virtual void CheckCBFCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length != 5)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("C3EFB564-BAA0-49DB-A535-97D653F0C20D", "A Bonded Factory Customs Controlling Premises Code must be 5 characters long."));
			}
			else if (!number.IsLettersAndNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("CD3266BB-2B66-4B14-8A2A-CFCC06804B57", "A CBF code can only have alphanumeric characters."));
			}
		}

		internal virtual void CheckFTZCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length != 5)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("04C85354-65D2-4C35-B98C-C3B2D152067B", "A Free Trade Zone Customs Controlling Premises Code must be 5 characters long."));
			}
			else if (!new ZString[] { "W", "X", "Y", "Z", "P", "Q", "R", "S" }.Contains(number.SubstringSafe(0, 1)))
			{
				customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("F55C0BEB-EE22-4C8B-A7F6-270A907579E1", "The first character of FTZ code should be one of \"W\", \"X\", \"Y\", \"Z\", \"P\", \"Q\", \"R\", \"S\"."));
			}
			else if (!number.IsLettersAndNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("9A27F6BD-5281-4C7F-BF7F-90F0AEFF477D", "A FTZ code can only have alphanumeric characters."));
			}
		}

		internal virtual void CheckCPWCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length != 5 && number.Length != 8)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("365F05B9-FC29-4956-8EB7-23430B7431AE", "A Customs Bonded Warehouse Customs Controlling Premises Code must be 5 or 8 characters long."));
			}
			else if (!new ZString[] { "G", "D" }.Contains(number.SubstringSafe(number.Length - 4, 1)))
			{
				if (number.Length == 5)
				{
					customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("FA5A166B-EDBE-4F8E-ACA0-D1FA75DF5470", "The second character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses."));
				}
				else if (number.Length == 8)
				{
					customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("9A0E6388-0E5E-4181-8CA1-B93CA18105E4", "The fifth character a CPW code should be 'G' for Non-Personal Warehouses and 'D' for Personal Warehouses."));
				}
			}
			else if (!new ZString[] { "A", "B", "C", "D" }.Contains(number.SubstringSafe(number.Length - 5, 1)))
			{
				customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("840A3F70-D10C-4EA5-8FBF-AEC15C5441DC", "This Registration Number is invalid."));
			}
			else if (!number.IsLettersAndNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("D2C4EDD3-F113-45E7-90CF-4C97BB26C50B", "A CPW code can only have alphanumeric characters."));
			}
			else if (number.Length == 8 && !number.SubstringSafe(0, 3).IsNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("B3CB8697-1F71-4505-9C08-AABE4D4BA5CF", "The first three characters of a CPW code must be numeric digits."));
			}
		}

		internal virtual void CheckCCPCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length != 5 && number.Length != 8)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("A751CC38-CCAB-48B5-94E6-A1E1BC77D36B", "A Customs Logistic Center Customs Controlling Premises Code must be 5 or 8 characters long."));
			}
			else if (number.SubstringSafe(number.Length - 4, 1) != "L")
			{
				if (number.Length == 5)
				{
					customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("091FAC9E-0ADA-4DD9-8551-40BBB77859BC", "The second character a CCP code should be 'L'."));
				}
				else if (number.Length == 8)
				{
					customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("A1FDBEE6-4027-4403-A351-3C1A0B08C961", "The fifth character a CCP code should be 'L'."));
				}
			}
			else if (!new ZString[] { "A", "B", "C", "D" }.Contains(number.SubstringSafe(number.Length - 5, 1)))
			{
				customsRegNoInfo.AddNotification(WarningOrMessageError, Res.GetString("EC6CE72D-8939-468C-9CCB-4B4FA751C22F", "This Registration Number is invalid."));
			}
			else if (!number.IsLettersAndNumbersOnlyOrEmpty)
			{
				customsRegNoInfo.AddNotification(ErrorType, Res.GetString("8F64C1F4-F4C3-448C-87D0-6050288C1792", "A CCP code can only have alphanumeric characters."));
			}
		}

		internal virtual void CheckFEICustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 15)
			{
				customsRegNoInfo.AddError(ValidationConstants.OrgCusCode.FEIMaximumAllowedLengthExceeded);
			}
		}

		internal void CheckFRICustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			CheckFRICustomsRegistrationNumber(customsRegNoInfo, number, ErrorType);
		}

		internal virtual void CheckFRICustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number, INotificationType notificationType)
		{
			if (number.Length != 8)
			{
				customsRegNoInfo.AddNotification(notificationType, Res.GetString("be1b4e83-36c9-4174-bc76-c340c8127169", "The length of FRI must be 8."));
			}
		}

		internal void CheckATPCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 5)
			{
				customsRegNoInfo.AddError(ValidationConstants.OrgCusCode.ATPMaximumAllowedLengthExceeded);
			}
		}

		internal void CheckCCCCCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 14)
			{
				customsRegNoInfo.AddError(ValidationConstants.OrgCusCode.CCCMaximumAllowedLengthExceeded);
			}
		}

		internal void CheckSPKCustomsRegistrationNumber(ZPropertyInfo customsRegNoInfo, ZString number)
		{
			if (number.Length > 5)
			{
				customsRegNoInfo.AddError(ValidationConstants.OrgCusCode.SPKMaximumAllowedLengthExceeded);
			}
		}
	}
}
