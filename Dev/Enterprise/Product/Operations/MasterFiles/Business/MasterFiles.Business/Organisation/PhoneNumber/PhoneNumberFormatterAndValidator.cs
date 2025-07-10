using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.MasterFiles.Business
{
	public class PhoneNumberFormatterAndValidator : ValidationProvider
	{
		public static string InvalidPhoneNumberFormat
		{
			get { return Res.GetString("c82ce097-57cd-47d7-8d85-b626b1b84e36", "The phone number as entered has a high probability of being incorrect."); }
		}

		#region Validate

		public ZString GetCountryCode(ZString phoneNumber, RefUNLOCO defaultUnloco)
		{
			var countryCode = phoneNumberFormatter.GetCountryCode(phoneNumber, GetCountryCodeFromUnloco(defaultUnloco));
			return countryCode != null ? (ZString)countryCode.IsoCode : ZString.Empty;
		}

		public ZString GetCountryCode(ZString phoneNumber, ZString defaultCountryCode)
		{
			var countryCode = phoneNumberFormatter.GetCountryCode(phoneNumber, defaultCountryCode);
			return countryCode != null ? (ZString)countryCode.IsoCode : ZString.Empty;
		}

		public void Validate(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty, RefUNLOCO defaultUnloco)
		{
			Validate(phoneNumberProperty, rawPhoneNumberProperty, phoneNumberIsManuallyVerifiedProperty, GetCountryCodeFromUnloco(defaultUnloco));
		}

		public void Validate(ZPropertyInfo phoneNumberProperty, ZPropertyInfo rawPhoneNumberProperty, ZPropertyInfo phoneNumberIsManuallyVerifiedProperty, ZString defaultCountryCode)
		{
			Argument.NotNull(phoneNumberProperty, "phoneNumberProperty");
			// The validation should not work if readonly.
			if (!phoneNumberProperty.ReadOnly && (phoneNumberIsManuallyVerifiedProperty == null || (ZBool)phoneNumberIsManuallyVerifiedProperty.Value == ZBool.False))
			{
				var phoneNumber = (ZString)phoneNumberProperty.Value;
				if (!phoneNumber.IsEmpty)
				{
					var normalizedPhoneNumber = phoneNumberFormatter.FormatE164(phoneNumber, defaultCountryCode);
					if (string.IsNullOrEmpty(normalizedPhoneNumber))
					{
						rawPhoneNumberProperty = rawPhoneNumberProperty ?? phoneNumberProperty;
						if (rawPhoneNumberProperty.MaxLength > -1 && phoneNumber.Length > rawPhoneNumberProperty.MaxLength)
						{
							phoneNumberProperty.AddError(Res.GetString("BA8FFB3A-C10C-4C6F-B04D-A3A3DB5ABE0B", "The length of the phone number should not exceed {0}.", rawPhoneNumberProperty.MaxLength));
						}
						else
						{
							var errorMessageBuilder = new ZStringBuilder(InvalidPhoneNumberFormat);
							errorMessageBuilder.Append(string.Empty);

							var countryCode = phoneNumberFormatter.GetCountryCode(phoneNumber, defaultCountryCode);

							if (countryCode != null)
							{
								var exampleNumberInLocal = phoneNumberFormatter.GetExampleNumberInLocal(countryCode.IsoCode, countryCode.DiallingCode);
								var exampleNumberInInternational = phoneNumberFormatter.GetExampleNumberInInternational(countryCode.IsoCode, countryCode.DiallingCode);
								if (!string.IsNullOrEmpty(exampleNumberInLocal) && !string.IsNullOrEmpty(exampleNumberInInternational))
								{
									errorMessageBuilder.Append(Res.GetString("6fc3ac6a-d1f0-4cba-b33f-7c2637f709c2", "Please check the format and see below examples:"));
									errorMessageBuilder.Append(Res.GetString("2D3EC1EF-7DB2-4D72-B8F9-2C45363A3484", "{0} (local format)", exampleNumberInLocal));
									errorMessageBuilder.Append(Res.GetString("27F56329-5694-4D83-BED0-158765E66797", "{0} (international format)", exampleNumberInInternational));
								}
							}
							else
							{
								errorMessageBuilder.Append(Res.GetString("B6C3B279-02DC-4354-BCC0-DB3FEC1CD271", "No country/region identified to format the number. Please enter the number in an international format. See below example:"));
								if (GlbBranch.CurrentBranch != null && GlbBranch.CurrentBranch.Country != null)
								{
									errorMessageBuilder.Append(Res.GetString("51C91481-068E-4F81-B16F-08A091B3BE1F", "{0}", phoneNumberFormatter.GetExampleNumberInInternational(GlbBranch.CurrentBranch.Country.Code)));
								}
							}

							var downgradeToWarning = Env.Registry.DowngradeInvalidPhoneNumbersToAWarning && Env.IsCargoWiseDomain(IPGlobalProperties.GetIPGlobalProperties().DomainName);
							phoneNumberProperty.AddNotification(downgradeToWarning ? CargoWise.ComponentModel.NotificationType.Warning : CargoWise.ComponentModel.NotificationType.Error, errorMessageBuilder.ToStringWithNewLineBetweenAppends());
						}
					}
				}
			}
		}

		public static string GetInvalidPhoneNumberFormatNotificationMessage(IEnumerable<INotification> notifications)
		{
			var invalidPhoneNumberFormatNotification = notifications.FirstOrDefault(n => n.Message.StartsWith(InvalidPhoneNumberFormat, StringComparison.OrdinalIgnoreCase));
			return invalidPhoneNumberFormatNotification != null ? invalidPhoneNumberFormatNotification.Message : string.Empty;
		}

		#endregion

		#region Format

		public ZString FormatInternational(ZString phoneNumber, RefUNLOCO defaultUnloco)
		{
			return phoneNumberFormatter.FormatInternational(phoneNumber, GetCountryCodeFromUnloco(defaultUnloco));
		}

		public ZString FormatInternational(ZString phoneNumber, ZString defaultCountryCode)
		{
			return phoneNumberFormatter.FormatInternational(phoneNumber, defaultCountryCode);
		}

		public ZString FormatLocal(ZString phoneNumber, RefUNLOCO defaultUnloco)
		{
			return phoneNumberFormatter.FormatLocal(phoneNumber, GetCountryCodeFromUnloco(defaultUnloco));
		}

		public ZString FormatLocal(ZString phoneNumber, ZString defaultCountryCode)
		{
			return phoneNumberFormatter.FormatLocal(phoneNumber, defaultCountryCode);
		}

		public ZString Normalize(ZString phoneNumber, RefUNLOCO defaultUnloco)
		{
			return phoneNumberFormatter.FormatE164(phoneNumber, GetCountryCodeFromUnloco(defaultUnloco));
		}

		public ZString Normalize(ZString phoneNumber, ZString defaultCountryCode)
		{
			return phoneNumberFormatter.FormatE164(phoneNumber, defaultCountryCode);
		}

		#endregion

		#region Implementation

		static ZString GetCountryCodeFromUnloco(RefUNLOCO unloco)
		{
			return unloco != null ? unloco.RL_RN_NKCountryCode : ZString.Empty;
		}

		readonly PhoneNumberFormatter phoneNumberFormatter = new PhoneNumberFormatter();

		#endregion

		static GenCustomAddOnRuleAck GetPhoneManuallyOverrideAck(BusinessObject parentBizo, ZString parentTableColumnName, GenCustomAddOnRuleAckCollection acks)
		{
			Argument.NotNull(parentBizo, "parentBizo");
			Argument.NotNullOrEmpty(parentTableColumnName, "parentTableColumnName");
			Argument.NotNull(acks, "acks");

			return acks.FirstOrDefault(ack => ack.XK_ParentTableColumn == parentTableColumnName
				&& ack.XK_RuleID == Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation
				&& ack.XK_ParentTableCode == parentBizo.TablePrefix
				&& ack.XK_ParentID == parentBizo.PK
			);
		}

		public static bool IsManuallyVerified(BusinessObject parentBizo, ZString parentTableColumnName, GenCustomAddOnRuleAckCollection acks)
		{
			return GetPhoneManuallyOverrideAck(parentBizo, parentTableColumnName, acks) != null;
		}

		public static void SetIsManuallyVerified(ZPropertyInfo isManuallyVerifiedPropertyInfo, ZBool value, GenCustomAddOnRuleAckCollection acks, BusinessObject parentBizo, ZString parentTableColumnName, Action revalidatePhone, ZPropertyInfo bindingProperty)
		{
			Argument.NotNull(isManuallyVerifiedPropertyInfo, "isManuallyVerifiedPropertyInfo");
			Argument.NotNull(acks, "acks");
			Argument.NotNull(parentBizo, "parentBizo");
			Argument.NotNullOrEmpty(parentTableColumnName, "parentTableColumnName");

			var ack = GetPhoneManuallyOverrideAck(parentBizo, parentTableColumnName, acks);
			if ((ack == null) == value)
			{
				if (value)
				{
					ack = acks.AddNew();
					ack.XK_ParentTableColumn = parentTableColumnName;
					ack.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;
				}
				else
				{
					acks.Delete(ack);
				}

				parentBizo.HasChanges = true;
				if (revalidatePhone != null)
				{
					revalidatePhone();
				}

				if (bindingProperty != null)
				{
					bindingProperty.RefreshBinding();
				}

				isManuallyVerifiedPropertyInfo.RefreshBinding();
			}
		}
	}
}