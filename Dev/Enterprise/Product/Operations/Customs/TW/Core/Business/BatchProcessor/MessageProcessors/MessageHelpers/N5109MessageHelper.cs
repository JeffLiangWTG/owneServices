using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5109MessageHelper : TWMessageHelper
	{
		public N5109MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.N5109.Response;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.N5109.Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Captions.NoticeNumber, NoticeNumber);
				WriteRow(table, Captions.ExaminationDispatchedDateAndTime, ExaminationDispatchedDateAndTime);

				var transportEquipments = response.Declaration?.GoodsShipment?.Consignment?.TransportEquipment;
				if (transportEquipments != null)
				{
					foreach (var equipment in transportEquipments)
					{
						WriteRow(table, Captions.ContainerNumber, equipment.Id.Value);
					}
				}

				WriteRow(table, Captions.InstrumentInspectionStationCoded, GetICIRefCusCodeList(InstrumentInspectionStationCoded));
				WriteRow(table, Captions.PlaceOfPhysicalExaminationCoded, GetICIRefCusCodeList(PlaceOfPhysicalExaminationCoded));
				WriteRow(table, Captions.ExaminerCoded, ExaminerCoded);
			}
		}

		ZString NoticeNumber => response?.FunctionalReferenceId?.Value ?? ZString.Empty;

		ZString ExaminationDispatchedDateAndTime => response?.Control?.InspectionStartDateTime ?? ZString.Empty;

		ZString InstrumentInspectionStationCoded => response.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem?.Commodity?.ExaminationPlace?.Id?.Value ?? ZString.Empty;

		ZString PlaceOfPhysicalExaminationCoded => response.Declaration?.GoodsShipment?.GovernmentAgencyGoodsItem?.ExaminationPlace?.Id?.Value ?? ZString.Empty;

		ZString ExaminerCoded => response.Declaration?.ResponsibleGovernmentAgency?.GovernmentOfficer?.Id?.Value ?? ZString.Empty;
	}
}
