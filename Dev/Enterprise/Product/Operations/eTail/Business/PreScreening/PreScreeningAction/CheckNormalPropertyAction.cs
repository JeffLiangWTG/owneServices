using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	class CheckNormalPropertyAction : PreScreeningAction
	{
		public CheckNormalPropertyAction(HVLVPreScreeningField field, HVLVConsignment consignment) : base(field, consignment)
		{
		}

		protected virtual IEnumerable<ZString> PropertyValues
		{
			get
			{
				if (field.FieldName.StartsWith(HVLVConsignmentSchema.Constants.Prefix))
				{
					yield return consignment.FindPropertyInfo(field.FieldName).Value.ToString();
				}
				else if (field.FieldName.StartsWith(HVLVItemLineSchema.Constants.Prefix))
				{
					var itemLines = consignment.Items.OfType<HVLVItem>().SelectMany(item => item.Lines).OfType<HVLVItemLine>();
					if (itemLines != null && itemLines.Any())
					{
						foreach (var itemLine in itemLines)
						{
							yield return itemLine.FindPropertyInfo(field.FieldName).Value.ToString();
						}
					}
				}
			}
		}

		protected override void PerformPreScreeningAndPopulateResult(HVLVConsignmentPreScreeningResult result)
		{
			foreach (var propertyValue in PropertyValues)
			{
				GetResultOfCheckRegularStringProperty(result, propertyValue);
			}
		}

		void GetResultOfCheckRegularStringProperty(HVLVConsignmentPreScreeningResult result, string fieldValue)
		{
			if (field.IsMandatory && string.IsNullOrEmpty(fieldValue))
			{
				AddPreScreeningDetailsCore(GetFieldMandatoryPreScreeningMessage(), result, messageLevelOverride: MessageLevel.Error);
			}
			else
			{
				if (TryRegularStringPropertyPreScreeningCore(
					fieldValue,
					out var defaultMessageText,
					out var userConfiguredMessageText))
				{
					AddPreScreeningDetailsCore(GetRegularStringFieldPreScreeningMessage(defaultMessageText, userConfiguredMessageText), result);
				}
			}
		}

		bool TryRegularStringPropertyPreScreeningCore(
				ZString fieldValue,
				out ZString defaultMessageText,
				out ZString userConfiguredMessageText)
		{
			defaultMessageText = ZString.Empty;
			userConfiguredMessageText = ZString.Empty;
			var anyValueMatched = false;
			var matchedScreeningValues = field.ScreeningValues.OfType<HVLVPreScreeningValue>().Where(s => field.IsHSCodeField ? s.IsMatchedForHSCode(fieldValue) : s.IsMatched(fieldValue));

			if (matchedScreeningValues.Any())
			{
				var needFallBackToFieldLevelMessageText = false;

				foreach (var item in matchedScreeningValues)
				{
					if (item.MessageTextPerValue.IsEmpty)
					{
						needFallBackToFieldLevelMessageText = true;
						defaultMessageText += "," + item.ScreeningValue;
					}
					else
					{
						userConfiguredMessageText += "\r\n" + item.MessageTextPerValue;
					}
				}

				defaultMessageText = defaultMessageText.TrimStart(',');
				userConfiguredMessageText = userConfiguredMessageText.TrimStart("\r\n".ToCharArray());

				if (needFallBackToFieldLevelMessageText)
				{
					userConfiguredMessageText = field.MessageText
						+ (userConfiguredMessageText.IsEmpty ? ZString.Empty : new ZString("\r\n" + userConfiguredMessageText));
				}

				anyValueMatched = true;
			}

			return anyValueMatched;
		}

		string GetFieldMandatoryPreScreeningMessage()
		{
			return string.Format(CultureInfo.CurrentCulture, "{0} {1} - {2}", field.FieldDescription, ScreeningError, MandatoryMessage);
		}

		string GetRegularStringFieldPreScreeningMessage(ZString defaultMessageText, ZString userConfiguredMessageText)
		{
			var fieldDescription = field.FieldDescription;
			var needFallBackToDefaultMessageText = userConfiguredMessageText.IsEmpty;
			var messageTextToDisplay = needFallBackToDefaultMessageText ?
				string.Format(CultureInfo.CurrentCulture, "{0} {1}", defaultMessageText, NormalNotificationMessage)
				: userConfiguredMessageText.ToString();

			return string.Format(CultureInfo.CurrentCulture, "{0} {1} - {2}.",
				fieldDescription,
				ScreeningError,
				messageTextToDisplay
				);
		}

		string MandatoryMessage => Res.GetString("ba3070be-f704-48a4-bf61-81d2a250f30a", "Please enter a value");
	}
}
