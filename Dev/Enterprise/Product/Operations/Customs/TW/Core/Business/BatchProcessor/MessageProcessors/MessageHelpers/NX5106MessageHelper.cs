using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class NX5106MessageHelper : TWMessageHelper
	{
		public NX5106MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.NX5106.Response;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.NX5106.Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Captions.Remark, Remark);
				WriteRow(table, Captions.ResponseCode, GetResponseCode(ResponseCode));
				WriteRow(table, Captions.DateAndTimeOfNotification, DateAndTimeOfNotification);

				WriteGoodsItemRows(table);
				WriteApplicationResponseRows(table);
			}
		}

		void WriteGoodsItemRows(HtmlTableCreator table)
		{
			var goodsItems = response?.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem;
			foreach (var goodsItem in goodsItems)
			{
				WriteRow(table, Captions.GoodsItemNumber, goodsItem.SequenceNumeric.ToString(CultureInfo.CurrentCulture));
				foreach (var nameCode in goodsItem.Status.Select(x => x.NameCode.Value))
				{
					if (!string.IsNullOrEmpty(nameCode))
					{
						WriteRow(table, Captions.ResponseCode, GetRejectionReason(nameCode));
					}
				}
			}
		}

		void WriteApplicationResponseRows(HtmlTableCreator table)
		{
			var applicationResponses = response.Declaration?.TwApplicationResponse;
			if (applicationResponses != null)
			{
				foreach (var applicationResponse in applicationResponses)
				{
					WriteRow(table, Captions.DateAndTimeOfNotification, applicationResponse.TwIssueDateTime);
					WriteRow(table, Captions.Description, applicationResponse.AdditionalInformation?.Content?.Value ?? string.Empty);
					WriteRow(table, Captions.OfficeOfDeclaration, applicationResponse.ContactOffice?.Id?.Value ?? string.Empty);
					WriteRow(table, Captions.ResponseCode, string.Join(";", applicationResponse.Status?.Select(x => x.NameCode.Value).ToArray()));
					WriteRow(table, Captions.ProcessingDateAndTime, applicationResponse.TwApplication?.TwAcceptanceDateTime ?? string.Empty);
					WriteRow(table, Captions.ProcessingNumber, applicationResponse.TwApplication?.AdditionalInformation?.TwProcessNumber?.Value);

					WriteApplicationResponseGoodsItemRows(table, applicationResponse);
				}
			}
		}

		void WriteApplicationResponseGoodsItemRows(HtmlTableCreator table, CargoWise.Customs.TW.MessageDefinitions.NX5106.ResponseDeclarationTwApplicationResponse applicationResponse)
		{
			var applicationGoodsItems = applicationResponse.TwApplication?.GoodsShipment?.GovernmentAgencyGoodsItem;
			foreach (var goodsItem in applicationGoodsItems)
			{
				var sequenceNumeric = goodsItem.SequenceNumeric;
				string sequenceNumericString = sequenceNumeric.HasValue ? sequenceNumeric.Value.ToString(CultureInfo.CurrentCulture) : string.Empty;
				WriteRow(table, Captions.GoodsItemNumber, sequenceNumericString);
				WriteRow(table, Captions.Description, goodsItem.AdditionalInformation?.Content?.Value);

				foreach (var nameCode in goodsItem.Status.Select(x => x.NameCode.Value))
				{
					if (!string.IsNullOrEmpty(nameCode))
					{
						WriteRow(table, Captions.ResponseCode, nameCode);
					}
				}
			}
		}

		readonly protected CPT_016_RejectionReasons fResponseCodeList = new CPT_016_RejectionReasons();

		public override string GetRejectionReason(string code)
		{
			var result = GetDescriptionFromCode(code, (itemCode) => fResponseCodeList.GetDescriptionFromCode(itemCode));
			if (result == code)
			{
				result = base.GetRejectionReason(code);
			}
			return result;
		}

		ZString Remark => response?.AdditionalInformation?.StatementDescription?.Value ?? ZString.Empty;

		ZString ResponseCode => response?.Status?.NameCode?.Value ?? ZString.Empty;

		ZString DateAndTimeOfNotification => response.Declaration?.AdditionalInformation?.TwIssueDateTime ?? ZString.Empty;
	}
}
