using System.Globalization;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class N5204MessageHelper : TWMessageHelper
	{
		public N5204MessageHelper(TWMessage message) : base(message)
		{
			response = Message.IncomingMessageKeyInfomation.Result as CargoWise.Customs.TW.MessageDefinitions.N5204.Response;
		}

		readonly CargoWise.Customs.TW.MessageDefinitions.N5204.Response response;

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
				WriteRow(table, Captions.PackageReleased, PackageReleased);
				WriteRow(table, Captions.PackageUnreleased, PackageUnreleased);
				WriteRow(table, Captions.PackageUnit, PackageUnit);

				var transportEquipments = response.Declaration?.GoodsShipment?.Consignment?.TransportEquipment;
				if (transportEquipments != null)
				{
					foreach (var equipment in transportEquipments)
					{
						WriteRow(table, Captions.FullContainerNo, equipment.Id?.Value);
						WriteRow(table, Captions.ProcessCode, GetProcessCode(equipment.TwCurrentCode?.Value));
					}
				}
			}
		}

		ZString FunctionCode => response?.FunctionCode?.Value ?? ZString.Empty;

		ZString ModeOfCustomsClearance => response?.Status?.NameCode?.Value ?? ZString.Empty;

		ZString ReleaseDateTime => response?.Status?.ReleaseDateTime ?? ZString.Empty;

		ZString PackageReleased => response?.Status?.TwTotalPackageQuantity?.Value.ToString(CultureInfo.CurrentCulture) ?? ZString.Empty;

		ZString PackageUnreleased => response?.TwUnreleasedPackages?.TwQuantityQuantity?.Value.ToString(CultureInfo.CurrentCulture) ?? ZString.Empty;

		ZString PackageUnit => response?.TwUnreleasedPackages?.TwTypeCode?.Value ?? ZString.Empty;
	}
}
