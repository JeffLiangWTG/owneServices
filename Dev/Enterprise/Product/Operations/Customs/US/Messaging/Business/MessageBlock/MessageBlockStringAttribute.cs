using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public enum Justification { Left, Right }

	public enum LengthViolationAction
	{
		SetInvalidValue,
		SetInvalidValueAndThrowException,
		Substring,
		SubstringFromRight,
	}

	public sealed class MessageBlockStringAttribute : MessageBlockAttribute
	{
		public MessageBlockStringAttribute(byte length, byte position, string status)
			: base(length, position, status)
		{
			ApplicationCode = string.Empty;
			Justification = Justification.Left;
			ShouldTrimBegining = true;
			OnLengthViolation = LengthViolationAction.SetInvalidValueAndThrowException;
			IsSpecialReplacingBehaviourOfInvalidCharacterOn = false;
			IsPersonalInformation = false;
			MaskButDoNotCheckFormat = false;
		}

		public string ApplicationCode
		{
			get;
			set;
		}

		public bool IsPersonalInformation
		{
			get;
			set;
		}

		public bool MaskButDoNotCheckFormat
		{
			get;
			set;
		}

		public Justification Justification
		{
			get;
			set;
		}

		public LengthViolationAction OnLengthViolation
		{
			get;
			set;
		}

		public bool ShouldTrimBegining
		{
			get;
			set;
		}

		/// <summary>
		/// If this is on, then system will replace a whole input with blank if there are more than 2 illegal characters or the illegal characters represent more than 1/4 of the total number of characters of input
		/// </summary>
		public bool IsSpecialReplacingBehaviourOfInvalidCharacterOn
		{
			get;
			set;
		}

		protected override string SerialiseCore(MessageBlock block, IZType value, bool humanFriendly)
		{
			var result = value.ToString();
			if (!humanFriendly)
			{
				if (Justification == Justification.Left)
				{
					result = result.PadRight(Length);
				}
				else
				{
					result = result.PadLeft(Length);
				}
			}
			else
			{
				result = MaskSocialSecurityNumberIfRequired(result);
			}

			if (result.Length > Length)
			{
				var invalidValue = new string(InvalidPaddingCharacter, Length);
				switch (OnLengthViolation)
				{
					case LengthViolationAction.SetInvalidValue:
						result = invalidValue;
						break;
					case LengthViolationAction.Substring:
						result = new ZString(result).Left(Length);
						break;
					case LengthViolationAction.SubstringFromRight:
						result = new ZString(result).Right(Length);
						break;
					case LengthViolationAction.SetInvalidValueAndThrowException:
					default:
						throw new MessageBlockSerialisationException(string.Format("Data provided exceeded allowable maximum length:{0}Maximum Length:{1}{0}Actual Length:{2}", System.Environment.NewLine, Length, result.Length), invalidValue);
				}
			}

			if (IsSpecialReplacingBehaviourOfInvalidCharacterOn)
			{
				var valueWithoutDiacritics = ((ZString)result).RemoveDiacritics();
				result = !HasMultipleInvalidCharacters(valueWithoutDiacritics.Trim(), ApplicationCode) ? Regex.Replace(((string)valueWithoutDiacritics).ToUpper(CultureInfo.CurrentCulture), GetNoneValidCharactersMatchPattern(ApplicationCode), " ") : string.Empty.PadRight(Length);
			}
			else
			{
				result = ReplaceWithValidCharacters(result, ApplicationCode);
			}

			return result;
		}

		static string ReplaceWithValidCharacters(string value, string applicationCode)
		{
			return ReplaceWithValidCharacters(value, applicationCode, applicationCode == CBPEDIInterchange.ApplicationCodes.AMS ? "?" : "*");
		}

		internal static string GetCharacterTypeToReplaceWith(string applicationCode)
		{
			return applicationCode == CBPEDIInterchange.ApplicationCodes.AMS ? ResString.GetMultilingualString("{BD00E4AE-3DFE-4009-A0A7-A0B2D0808B1B}", "a question mark '?'") : ResString.GetMultilingualString("{9383444A-A5D6-4F9A-8D38-E7557F598527}", "an asterisk '*'");
		}

		internal static string ReplaceWithValidCharacters(string value, string applicationCode, string replacement)
		{
			var noneValidCharactersMatchPattern = GetNoneValidCharactersMatchPattern(applicationCode);
			return Regex.Replace(value.ToUpper(CultureInfo.CurrentCulture), noneValidCharactersMatchPattern, replacement);
		}

		static string GetNoneValidCharactersMatchPattern(string applicationCode)
		{
			return applicationCode == CBPEDIInterchange.ApplicationCodes.AMS ? AMSNoneValidCharactersMatchPattern : ABINoneValidCharactersMatchPattern;
		}

		static bool IsNumberSSNFormat(ZString number)
		{
			return Regex.IsMatch(number, @"^[0-9]{3}-[0-9]{2}-[0-9]{4}$", RegexOptions.IgnoreCase);
		}

		// ABI valid characters ! @ # $ % ^ & * ( ) - _ = + [ { ] } \ | ; : ' " , < . > / ? ` ~ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789
		// Note: ABI does support ¢ however Entry Summary doesn't; we will enable ¢ for certain message block if there is a needed
		const string ABINoneValidCharactersMatchPattern = @"[^!""#$%&'*()+,-./0123456789:; <=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\\]^_`{|}~]";
		//AMS valid characteers !"#$%&'()+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`{|}~
		const string AMSNoneValidCharactersMatchPattern = @"[^!""#$%&'()+,-./0123456789:; <=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\\]^_`{|}~]";

		protected override IZType DeSerialiseCore(string value)
		{
			var result = new ZString(ShouldTrimBegining ? value.Trim() : value);
			result = MaskSocialSecurityNumberIfRequired(result);
			return result;
		}

		static bool HasMultipleInvalidCharacters(string elements, string applicationCode)
		{
			var replaceInvalidWithBlank = Regex.Replace(elements.ToUpper(CultureInfo.CurrentCulture), GetNoneValidCharactersMatchPattern(applicationCode), "");
			return elements.Length != 0 && replaceInvalidWithBlank.Length / (double)elements.Length < 0.75;
		}

		string MaskSocialSecurityNumberIfRequired(string socialSecurityNumber)
		{
			var result = socialSecurityNumber;
			if (IsPersonalInformation && !Environment.Env.Security.OrgDetailsViewPersonalInformation.IsAllowed)
			{
				if (IsNumberSSNFormat(result))
				{
					result = "***-**-****";
				}
				else if (MaskButDoNotCheckFormat)
				{
					result = "*********";
				}
			}

			return result;
		}
	}
}
