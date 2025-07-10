using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class ContainerNumberValidation
	{
		public static void ErrorIfInvalid(ZPropertyInfo info)
		{
			AddNotificationIfInvalid(info, NotificationTypes.Error);
		}

		public static void WarnIfInvalid(ZPropertyInfo info)
		{
			AddNotificationIfInvalid(info, NotificationTypes.Warning);
		}

		public static void AddNotificationIfInvalid(ZPropertyInfo info, NotificationTypes notificationType)
		{
			var containerNumber = (ZString)info.Value;

			if (!containerNumber.IsEmpty && notificationType != NotificationTypes.None)
			{
				var message = GetContainerNumberError(containerNumber);

				if (message != null)
				{
					switch (notificationType)
					{
						case NotificationTypes.Error:
							info.AddError(message);
							break;

						case NotificationTypes.Warning:
							info.AddWarning(message);
							break;

						case NotificationTypes.MessageError:
							info.AddMessageError(message);
							break;
					}
				}
			}
		}

		public static bool IsValidContainerNumber(string containerNumber)
		{
			if (containerNumber == null)
			{
				throw new ArgumentNullException(nameof(containerNumber));
			}

			char checkDigit;
			return TryGetCheckDigit(containerNumber, out checkDigit) && checkDigit == containerNumber[10];
		}

		public static string GetContainerNumberError(string containerNumber)
		{
			if (containerNumber == null)
			{
				throw new ArgumentNullException(nameof(containerNumber));
			}

			char checkDigit;

			if (!TryGetCheckDigit(containerNumber, out checkDigit))
			{
				return Res.GetString("20bd0383-6333-4211-8a29-d6ac265f4f4c", "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
			}
			else if (checkDigit != containerNumber[10])
			{
				return Res.GetString("d7cdbaa3-5057-467e-879b-0dbda3e24772", "Container number does not have a valid check (last) digit. The check digit should be {0:G}.", checkDigit);
			}
			else
			{
				return null;
			}
		}

		static bool TryGetCheckDigit(string containerNumber, out char checkDigit)
		{
			if (containerNumber.Length != 11)
			{
				checkDigit = '\0';
				return false;
			}

			int result = 0;

			for (int charPos = 0; charPos < 4; charPos++)
			{
				char c = containerNumber[charPos];
				int charVal = (c >= 97) ? (c - 87) : (c - 55);

				if (charVal > 35 || charVal < 10)
				{
					checkDigit = '\0';
					return false;
				}
				if (charVal > 30)
				{
					charVal += 3;
				}
				else if (charVal > 20)
				{
					charVal += 2;
				}
				else if (charVal > 10)
				{
					charVal += 1;
				}

				result += charVal << charPos;
			}

			for (int charPos = 4; charPos < 10; charPos++)
			{
				int charVal = containerNumber[charPos] - 48;

				if (charVal < 0 || charVal > 9)
				{
					checkDigit = '\0';
					return false;
				}
				else
				{
					result += charVal << charPos;
				}
			}

			result %= 11;
			if (result == 10)
			{
				result = 0;
			}

			char charValue = containerNumber[10];
			if (charValue < 48 || charValue >= 58)
			{
				checkDigit = '\0';
				return false;
			}
			else
			{
				checkDigit = (char)(result + 48);
				return true;
			}
		}
	}
}
