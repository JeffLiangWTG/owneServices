using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Enterprise.Freight.Agency.Business
{
	public class ContainerPinGenerator
	{
		[Flags]
		public enum CharacterTypes
		{
			Alpha = 1,
			Numeric = 2,
			AlphaNumeric = Alpha | Numeric
		}

		// Avoid using letters that look like numbers (eg. O,Q,S,I). 
		public const string Alphabet = "ABCDEFGHJKLMNPRTUVWXYZ";
		public const string Numbers = "0123456789";

		public ContainerPinGenerator()
		{
			emptyContainersShareAPin = true;
			pinLength = 8;
			random = new Random();
			CharacterType = CharacterTypes.AlphaNumeric;
		}

		public int PinLength
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return pinLength; }
			[System.Diagnostics.DebuggerStepThrough]
			set { pinLength = value; }
		}

		public bool EmptyContainersShareAPin
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return emptyContainersShareAPin; }
			[System.Diagnostics.DebuggerStepThrough]
			set { emptyContainersShareAPin = value; }
		}

		public CharacterTypes CharacterType
		{
			get { return characterType; }
			set
			{
				characterType = value;
				characterSet = (characterType.HasFlag(CharacterTypes.Alpha) ? Alphabet : string.Empty)
								+ (characterType.HasFlag(CharacterTypes.Numeric) ? Numbers : string.Empty);
			}
		}

		CharacterTypes characterType;
		string characterSet;

		public void PopulateEmptyPins<T>(IEnumerable<T> containers)
			where T : AgencyShipmentContainer
		{
			string pinForEmptyContainers = null;

			foreach (AgencyShipmentContainer container in containers)
			{
				if (container.JC_ContainerImportDORelease.IsEmpty || !IsCorrectFormat(container.JC_ContainerImportDORelease))
				{
					if (EmptyContainersShareAPin && container.JC_IsEmptyContainer)
					{
						if (pinForEmptyContainers == null)
						{
							pinForEmptyContainers = NewPin();
						}

						container.JC_ContainerImportDORelease = pinForEmptyContainers;
					}
					else
					{
						container.JC_ContainerImportDORelease = NewPin();
					}
				}
			}
		}

		bool IsCorrectFormat(string pin)
		{
			var regex = string.Format(CultureInfo.InvariantCulture, "^[{0}]{{{1}}}$", characterSet, PinLength);
			return Regex.IsMatch(pin, regex);
		}

		public string NewPin()
		{
			var builder = new StringBuilder(PinLength);

			for (int i = 0; i < PinLength; i++)
			{
				builder.Append(characterSet[random.Next() % characterSet.Length]);
			}

			return builder.ToString();
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		bool emptyContainersShareAPin;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		int pinLength;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly Random random;
	}
}
