using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.N5116;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	public class N5116EDIMessageDocumentWrapper : DocumentWrapper
	{
		public N5116EDIMessageDocumentWrapper(N5116EDIMessage message) : base(message, message.Factory)
		{
			response = message.IncomingMessageKeyInfomation.Result as Response;
		}

		#region Fields
		readonly Response response;

		ResponseDeclaration Declaration => response?.Declaration;

		ResponseDeclarationBorderTransportMeans DeclarationBorderTransportMeans => Declaration?.BorderTransportMeans;

		ResponseDeclarationGoodsShipment DeclarationGoodsShipment => Declaration?.GoodsShipment;

		ResponseDeclarationGoodsShipmentConsignment DeclarationGoodsShipmentConsignment => DeclarationGoodsShipment?.Consignment;

		ResponseDeclarationGoodsShipmentConsignmentBorderTransportMeans DeclarationGoodsShipmentConsignmentBorderTransportMeans => DeclarationGoodsShipmentConsignment?.BorderTransportMeans;

		Collection<ResponseDeclarationGoodsShipmentConsignmentTransportContractDocument> DeclarationGoodsShipmentConsignmentTransportContractDocumentCollection => DeclarationGoodsShipmentConsignment?.TransportContractDocument;

		Collection<ResponseDeclarationGoodsShipmentConsignmentTransportEquipment> DeclarationGoodsShipmentConsignmentTransportEquipmentCollection => DeclarationGoodsShipmentConsignment?.TransportEquipment;

		ResponseDeclarationImporter DeclarationImporter => Declaration?.Importer;

		ResponseStatus ResponseStatus => response?.Status;

		ResponseDeclarationPackaging DeclarationPackaging => Declaration?.Packaging;

		public ZString TransportMode => DeclarationBorderTransportMeans?.TypeCode?.Value ?? ZString.Empty;

		public ZString DocumentType => Declaration?.TypeCode?.Value ?? ZString.Empty;

		public ZString DeclarationID => CommonHelper.FormattedEntryNumber(Declaration?.Id?.Value ?? ZString.Empty);

		public ZString ClassificationID => DeclarationGoodsShipment?.GovernmentAgencyGoodsItem?.Commodity?.Classification?.Id?.Value ?? ZString.Empty;

		public ZString ReleaseTypeCode => ResponseStatus?.TwReleaseTypeCode?.Value ?? ZString.Empty;

		public ZString ArrivalDateTime => DocumentWrapperHelper.StringAsDateTime(DeclarationBorderTransportMeans?.ArrivalDateTime ?? ZString.Empty).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

		public ZString MasterNumber => DeclarationGoodsShipmentConsignmentTransportContractDocumentCollection?.Where(x => (x.TypeCode.Value == TransportContractDocumentTypeCodes._704 || x.TypeCode.Value == TransportContractDocumentTypeCodes._741)).FirstOrDefault()?.Id?.Value ?? ZString.Empty;

		public ZString HouseNumber => DeclarationGoodsShipmentConsignmentTransportContractDocumentCollection?.Where(x => (x.TypeCode.Value == TransportContractDocumentTypeCodes._703 || x.TypeCode.Value == TransportContractDocumentTypeCodes._714)).FirstOrDefault()?.Id?.Value ?? ZString.Empty;

		public ZString AgentID => Declaration?.Agent?.Id?.Value ?? ZString.Empty;

		public ZString ChineseName => DeclarationImporter?.TwChineseName?.Value ?? ZString.Empty;

		public ZString EnglishName => DeclarationImporter?.Name?.Value ?? ZString.Empty;

		public ZString ImporterID => DeclarationImporter?.Id?.Value ?? ZString.Empty;

		public ZString CustomsControlID => DeclarationImporter?.TwCustomsControlId?.Value ?? ZString.Empty;

		public ZString TransportID => DeclarationGoodsShipmentConsignmentBorderTransportMeans?.Id?.Value ?? ZString.Empty;

		public ZString JourneyID => DeclarationGoodsShipmentConsignmentBorderTransportMeans?.JourneyId?.Value ?? ZString.Empty;

		public ZString GoodsLocationID => DeclarationGoodsShipmentConsignment?.GoodsLocation?.Id?.Value ?? ZString.Empty;

		public ZString WarehouseID => DeclarationGoodsShipment?.Warehouse?.Id?.Value ?? ZString.Empty;

		ZDecimal TotalPackageQuantity => Declaration?.TotalPackageQuantity?.Value ?? ZDecimal.Zero;

		ZString PackageUnit => DeclarationPackaging?.TypeCode?.Value ?? ZString.Empty;

		public ZString TotalPackageQuantityAndUnit => $"{TotalPackageQuantity} {PackageUnit}";

		ZDecimal ReleasedQuantity => ResponseStatus?.TwTotalPackageQuantity?.Value ?? ZDecimal.Zero;

		ZString ReleasedUnit => UnreleasedUnit;

		public ZString ReleasedQuantityAndUnit => $"{ReleasedQuantity} {ReleasedUnit}";

		ZDecimal UnreleasedQuantity => response?.TwUnreleasedPackages?.TwQuantityQuantity?.Value ?? ZDecimal.Zero;

		ZString UnreleasedUnit => response?.TwUnreleasedPackages?.TwTypeCode?.Value ?? ZString.Empty;

		public ZString UnreleasedQuantityAndUnit => $"{UnreleasedQuantity} {UnreleasedUnit}";

		public ZString ReleaseDateTime => DocumentWrapperHelper.StringAsDateTime(ResponseStatus?.ReleaseDateTime ?? ZString.Empty).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No need to be translated")]
		public ZString OtherDeclarations
		{
			get
			{
				var otherDeclarations = new ZStringBuilder();
				if (!marksNumbers.IsEmpty)
				{
					otherDeclarations.Append((NoResString)"標記:");
					otherDeclarations.Append(marksNumbers);
				}
				if (!containerInfo.IsEmpty)
				{
					otherDeclarations.Append((NoResString)"貨櫃資料:");
					otherDeclarations.Append(containerInfo);
				}
				if (!equipmentCurrentCode.IsEmpty)
				{
					otherDeclarations.Append("處理註記:");
					otherDeclarations.Append(equipmentCurrentCode);
				}
				return otherDeclarations.ToStringWithNewLineBetweenAppends();
			}
		}

		ZString marksNumbers => DeclarationPackaging?.MarksNumbers?.Value ?? ZString.Empty;

		ZString containerInfo
		{
			get
			{
				var containerInfo = new ZStringBuilder();
				DeclarationGoodsShipmentConsignmentTransportEquipmentCollection?.Select(c => c.Id?.Value ?? ZString.Empty)?.ForEach(containerId =>
				{
					containerInfo.AppendIfNotEmpty(containerId);
				});
				return containerInfo.ToStringWithDelimiterBetweenAppends("/");
			}
		}

		ZString equipmentCurrentCode
		{
			get
			{
				var containerInfo = new ZStringBuilder();
				DeclarationGoodsShipmentConsignmentTransportEquipmentCollection?.Select(c => c.TwCurrentCode?.Value ?? ZString.Empty)?.ForEach(currentCode =>
				{
					containerInfo.AppendIfNotEmpty(currentCode);
				});
				return containerInfo.ToStringWithDelimiterBetweenAppends("/");
			}
		}

		public ZString CustomsClearance => ResponseStatus?.NameCode?.Value ?? ZString.Empty;

		public ZString IsShortage => DeclarationGoodsShipment?.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;

		public ZString AdditionalCondition
		{
			get
			{
				var additionalCondition = new ZStringBuilder();
				response?.AdditionalInformation?.Select(c => c.StatementCode?.Value ?? ZString.Empty)?.ForEach(statementCode =>
				{
					additionalCondition.AppendIfNotEmpty(statementCode);
				});
				return additionalCondition.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion
	}
}
