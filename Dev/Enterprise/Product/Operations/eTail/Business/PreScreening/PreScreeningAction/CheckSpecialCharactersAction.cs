using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.eTail.Business
{
	class CheckSpecialCharactersAction : PreScreeningAction
	{
		public CheckSpecialCharactersAction(HVLVPreScreeningField field, HVLVConsignment consignment)
			: base(field, consignment)
		{
		}

		protected override void PerformPreScreeningAndPopulateResult(HVLVConsignmentPreScreeningResult result)
		{
			CheckSpecialCharactersAndPopulateResult(consignment.HVC_GoodsDescription,
				DataBoundResourceStrings.GetDataForProperty(consignment.HVC_GoodsDescriptionInfo).Caption, result);

			CheckSpecialCharactersAndPopulateResult(consignment.HVC_ConsigneeInstructions,
				DataBoundResourceStrings.GetDataForProperty(consignment.HVC_ConsigneeInstructionsInfo).Caption, result);
		}

		void CheckSpecialCharactersAndPopulateResult(ZString fieldValue, ZString caption, HVLVConsignmentPreScreeningResult result)
		{
			var matchedScreeningValues = field.SpecialCharacters.OfType<HVLVPreScreeningSpecialCharacterValue>().Where(s => s.IsMatched(fieldValue));

			if (matchedScreeningValues.Any())
			{
				var messageText = string.Join(",", matchedScreeningValues.Select(i => i.CharacterValue));

				AddPreScreeningDetailsCore(GetSpecialCharactersFieldPreScreeningMessage(messageText, caption), result);
			}
		}

		string GetSpecialCharactersFieldPreScreeningMessage(string defaultMessageText, string fileldName)
		{
			var messageTextToDisplay = field.MessageText.IsEmpty ? string.Format(CultureInfo.CurrentCulture, "{0} {1}", defaultMessageText, NormalNotificationMessage)
				: field.MessageText.ToString();

			return string.Format(CultureInfo.CurrentCulture, "{0} {1} - {2}.",
				fileldName,
				ScreeningError,
				messageTextToDisplay);
		}
	}
}
