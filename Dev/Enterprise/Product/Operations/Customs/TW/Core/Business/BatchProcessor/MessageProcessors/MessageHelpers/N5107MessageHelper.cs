using System.Globalization;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5107MessageHelper : TWMessageHelper
	{
		public N5107MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.N5107.Response;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.N5107.Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Captions.DateAndTimeOfNotification, DateAndTimeOfNotification);
				WriteRow(table, Captions.Deadline, Deadline);
				WriteRow(table, Captions.ModeOfCustomsClearance, GetModeofCustomsClearance(ModeOfCustomsClearance));

				var goodsItems = response?.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem;
				foreach (var goodsItem in goodsItems)
				{
					var sequenceNumeric = goodsItem.SequenceNumeric;
					var sequenceNumericString = sequenceNumeric.HasValue ? sequenceNumeric.Value.ToString(CultureInfo.CurrentCulture) : string.Empty;
					WriteRow(table, Captions.GoodsItemNumber, sequenceNumericString);
					foreach (var error in goodsItem.Error)
					{
						WriteRow(table, Captions.RequiredFormalities, GetRequiredFormalities(error?.ValidationCode?.Value ?? string.Empty));
					}
				}
			}
		}

		readonly protected CTP_017_ErrorDocumentsOrRequiredFormalities fValidationCodeList = new CTP_017_ErrorDocumentsOrRequiredFormalities();
		public override string GetRequiredFormalities(string code)
		{
			var result = GetDescriptionFromCode(code, (itemCode) => fValidationCodeList.GetDescriptionFromCode(itemCode));
			if (result == code)
			{
				result = base.GetRequiredFormalities(code);
			}
			return result;
		}

		ZString DateAndTimeOfNotification => response?.IssueDateTime ?? ZString.Empty;

		ZString Deadline => response?.AdditionalInformation?.LimitDateTime ?? ZString.Empty;

		internal ZString ModeOfCustomsClearance => response.Status?.NameCode?.Value ?? ZString.Empty;
	}
}
