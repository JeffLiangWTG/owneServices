using System.Globalization;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5116MessageHelper : TWMessageHelper
	{
		public N5116MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.N5116.Response;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.N5116.Response response;

		protected override void WriteTable(HtmlTableCreator table)
		{
			if (response != null)
			{
				WriteRow(table, Captions.FunctionCode, GetFunctionCode(FunctionCode));

				var additionalInformations = response?.AdditionalInformation;
				if (additionalInformations != null)
				{
					foreach (var addInfo in additionalInformations)
					{
						WriteRow(table, Captions.ExtraRequirement, GetCPT_025_ExtraCondition(addInfo.StatementCode.Value));
					}
				}

				WriteRow(table, Captions.ModeOfCustomsClearance, GetModeofCustomsClearance(ModeOfCustomsClearance));
				WriteRow(table, Captions.ReleaseDateTime, ReleaseDateTime);
				WriteRow(table, Captions.TypeOfReleaseNote, GetTypeofReleaseNote(TypeOfReleaseNote));
				WriteRow(table, Captions.PackageReleased, PackageReleased);
				WriteRow(table, Captions.PackageUnreleased, PackageUnreleased);
				WriteRow(table, Captions.PackageUnit, PackageUnit);
				WriteRow(table, Captions.ShortlandedNote, ShortlandedNote);
				WriteRow(table, Captions.ExaminationNote, ExaminationNote);

				var transportEquipments = response.Declaration?.GoodsShipment?.Consignment?.TransportEquipment;
				if (transportEquipments != null)
				{
					foreach (var equipment in transportEquipments)
					{
						WriteRow(table, Captions.FullContainerNo, equipment.Id?.Value);
						var processCode = equipment.TwCurrentCode?.Value;
						if (!string.IsNullOrEmpty(processCode))
						{
							WriteRow(table, Captions.ProcessCode, GetProcessCode(processCode));
						}
					}
				}
			}
		}

		ZString ShortlandedNote => response?.Declaration?.GoodsShipment?.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;

		ZString ExaminationNote => response?.Declaration?.GoodsShipment?.Consignment?.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;

		ZString FunctionCode => response?.FunctionCode?.Value ?? ZString.Empty;

		ZString ModeOfCustomsClearance => response?.Status?.NameCode?.Value ?? ZString.Empty;

		ZString ReleaseDateTime => response?.Status?.ReleaseDateTime ?? ZString.Empty;

		ZString TypeOfReleaseNote => response?.Status?.TwReleaseTypeCode?.Value ?? ZString.Empty;

		ZString PackageReleased => response?.Status?.TwTotalPackageQuantity?.Value.ToString(CultureInfo.CurrentCulture) ?? ZString.Empty;

		ZString PackageUnreleased => response?.TwUnreleasedPackages?.TwQuantityQuantity?.Value.ToString(CultureInfo.CurrentCulture) ?? ZString.Empty;

		ZString PackageUnit => response?.TwUnreleasedPackages?.TwTypeCode?.Value ?? ZString.Empty;
	}
}
