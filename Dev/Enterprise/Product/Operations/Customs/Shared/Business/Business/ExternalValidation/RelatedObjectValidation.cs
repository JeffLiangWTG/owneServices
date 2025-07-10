using CargoWiseOne.ResourceStrings;
namespace Enterprise.Customs.Business
{
	using System.Collections;
	using System.Text;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.ResourceStrings.Grammar;
	using CargoWise.Types;

	public class RelatedObjectValidation : ValidationProvider
	{
		protected RelatedObjectValidation(ZPropertyInfo notificationInfo)
		{
			this.notificationInfo = notificationInfo;
		}

		#region Mandatory Validation

		/// <summary>
		/// Sets a message error if the field is empty, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="notificationInfo">The property where to add notification to.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void MessageErrorIfNotEntered(ZPropertyInfo propertyInfo, IZPropertyInfo notificationInfo, string propertyDescription = null)
		{
			MandatoryValidationHelper.AddNotificationIfNotEntered(propertyInfo, "", MandatoryValidationMessageType.YouHaveNotEntered, propertyDescription, notificationInfo.AddMessageError);
		}

		/// <summary>
		/// Sets a message error if the field has a value in it, using the given description.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="notificationInfo">The property where to add notification to.</param>
		/// <param name="propertyDescription">The description of the property.</param>
		public static void MessageErrorIfIsEntered(ZPropertyInfo propertyInfo, IZPropertyInfo notificationInfo, string propertyDescription = null)
		{
			MandatoryValidationHelper.AddNotificationIfIsEntered(propertyInfo, "", propertyDescription, notificationInfo.AddMessageError);
		}

		static IMandatoryValidationInternals MandatoryValidationHelper
		{
			get { return new MandatoryValidation(); }
		}

		#endregion

		#region List Validation

		/// <summary>
		/// Sets a message error if the code does not exist in the list.
		/// </summary>
		/// <param name="propertyInfo">The property to validate.</param>
		/// <param name="notificationInfo">The property where to add notification to.</param>
		/// <param name="list">The list to check.</param>
		/// <param name="message">The message to show if the test fails.</param>
		public static void MessageErrorIfInvalidCode(ZPropertyInfo propertyInfo, IZPropertyInfo notificationInfo, IList list = null, IMultilingualString message = null)
		{
			if (!propertyInfo.Value.IsEmpty)
			{
				ListValidationHelper.MessageErrorIfInvalidCode(propertyInfo, list, message ?? ListValidation.InvalidCodeMessageError, notificationInfo.AddMessageError);
			}
		}

		static IListValidationInternals ListValidationHelper
		{
			get { return new ListValidation(); }
		}

		#endregion

		#region Max Length Validation

		public static void MaxLengthValidation(ZPropertyInfo info, int maxLength, IZPropertyInfo notificationInfo = null, string propertyDescription = null, bool mandatory = false)
		{
			if (string.IsNullOrEmpty(propertyDescription))
			{
				propertyDescription = MandatoryValidationHelper.GetErrorFieldFromProperyInfo(info);
			}

			MaxLengthValidation((ZString)info.Value, maxLength, notificationInfo ?? info, propertyDescription, mandatory);
		}

		public static void MaxLengthValidation(ZString field, int maxLength, IZPropertyInfo notificationInfo, string propertyDescription)
		{
			MaxLengthValidation(field, maxLength, notificationInfo, propertyDescription, false);
		}

		public static void MaxLengthValidation(ZString field, int maxLength, IZPropertyInfo notificationInfo, string propertyDescription, bool mandatory)
		{
			if (field.Length > maxLength)
			{
				notificationInfo.AddWarning(Res.GetString("edaad6c3-93cb-449a-89f2-08e78ca368c6", "The length of data entered into {2} ({0}) exceeds the maximum allowed. Only the first {1} characters will be transmitted to Customs.",
														  field.Length, maxLength, propertyDescription));
			}
			if (mandatory && field.IsEmpty)
			{
				notificationInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(propertyDescription));
			}
		}

		public static void SplitMaxLengthValidation(ZString field, int maxLength, int splitLength, IZPropertyInfo notificationInfo, string propertyDescription, bool mandatory)
		{
			MaxLengthValidation(field, maxLength, notificationInfo, propertyDescription, mandatory);
			if (field.Length > splitLength)
			{
				notificationInfo.AddWarning(Res.GetString("21408210-532F-4FC6-BF05-0B4017AC8CEF", "The length of data entered into {1} ({0}) exceeds the maximum allowed. So {1} will be split and the excess data transmitted to Customs in the second line.",
															 field.Length, propertyDescription));
			}
		}

		#endregion

		#region GetDescriptionWithPrefix

		public static string GetDescriptionWithPrefix(ZPropertyInfo info, bool makePrefixCapital = false)
		{
			return GetDescriptionWithPrefix(MandatoryValidationHelper.GetErrorFieldFromProperyInfo(info), makePrefixCapital);
		}

		public static string GetDescriptionWithPrefix(string propertyDescription, bool makePrefixCapital = false)
		{
			ZString prefix = Grammar.Instance.IndefiniteArticlePrefix(propertyDescription);
			return string.Format("{0}{1}", makePrefixCapital ? prefix.ToTitleCase() : prefix, propertyDescription);
		}

		#endregion

		#region Error String Builder

		protected class ErrorStringBuilder : IZPropertyInfo
		{
			public ErrorStringBuilder(IZPropertyInfo info, string caption)
			{
				this.info = info;
				this.caption = caption;
			}

			#region IZPropertyInfo Members

			public void AddMessageError(string message)
			{
				errorBuilder.AppendLine(message.TrimEnd('.') + ";");
			}

			public void AddWarning(string message)
			{
				warningBuilder.AppendLine(message.TrimEnd('.') + ";");
			}

			public IZType Value
			{
				get { return info.Value; }
				set { info.Value = value; }
			}

			#endregion

			#region INotifications Members

			public void Add(INotification notification)
			{
				if (notification.Type.EnumValueName == nameof(NotificationTypes.Warning))
				{
					AddWarning(notification.Message);
				}
				else
				{
					AddMessageError(notification.Message);
				}
			}

			#endregion

			#region FillErrorMessages

			public void FillErrorMessages()
			{
				var hasErrors = errorBuilder.Length > 0;
				if (hasErrors)
				{
					errorBuilder.Insert(0, GetFieldIsInvalidMessageError(caption));
					errorBuilder.Remove(errorBuilder.Length - 3, 3);
					errorBuilder.Append(".");
					info.AddMessageError(errorBuilder.ToString());
				}
				var hasWarnings = warningBuilder.Length > 0;
				if (hasWarnings)
				{
					warningBuilder.Insert(0, GetFieldHasWarningsMessage(caption));
					warningBuilder.Remove(warningBuilder.Length - 3, 3);
					warningBuilder.Append(".");
					info.AddWarning(warningBuilder.ToString());
				}
			}

			public static string GetFieldIsInvalidMessageError(string caption)
			{
				return Res.GetString("aaf1f38e-7c6a-4d20-9f8d-e22c4e00d093", "{0} is invalid and has following errors:", caption) + "\r\n";
			}

			public static string GetFieldHasWarningsMessage(string caption)
			{
				return Res.GetString("BA02BF4B-B9CE-417C-B354-B6EA27AEC4D6", "{0} has the following warnings:", caption) + "\r\n";
			}

			readonly StringBuilder errorBuilder = new StringBuilder();
			readonly StringBuilder warningBuilder = new StringBuilder();
			readonly IZPropertyInfo info;
			readonly string caption;

			#endregion
		}

		#endregion

		protected readonly ZPropertyInfo notificationInfo;
	}
}
