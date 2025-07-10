using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.TW.MessageDefinitions.N5204;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business
{
	public class N5204EDIMessageDocumentWrapper : DocumentWrapper
	{
		public N5204EDIMessageDocumentWrapper(N5204EDIMessage message) : base(message, message.Factory)
		{
			response = message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly Response response;

		ResponseDeclaration Declaration => response?.Declaration;

		ResponseDeclarationBorderTransportMeans DeclarationBorderTransportMeans => Declaration?.BorderTransportMeans;

		ResponseDeclarationGoodsShipment DeclarationGoodsShipment => Declaration?.GoodsShipment;

		Collection<ResponseDeclarationGoodsShipmentConsignmentTransportContractDocument> DeclarationGoodsShipmentConsignmentTransportContractDocumentCollection => DeclarationGoodsShipmentConsignment?.TransportContractDocument;

		ResponseDeclarationGoodsShipmentExporter GoodsShipmentExporter => Declaration?.GoodsShipment?.Exporter;

		ResponseDeclarationGoodsShipmentConsignmentBorderTransportMeans GoodsShipmentConsignmentBorderTransportMeans => DeclarationGoodsShipmentConsignment?.BorderTransportMeans;

		ResponseStatus ResponseStatus => response?.Status;

		public ZString TransportMode => DeclarationBorderTransportMeans?.TypeCode?.Value ?? ZString.Empty;

		public ZString DeclarationID => CommonHelper.FormattedEntryNumber(Declaration?.Id?.Value ?? ZString.Empty);

		public ZString ClassificationID => DeclarationGoodsShipment?.GovernmentAgencyGoodsItem?.Commodity?.Classification?.Id?.Value ?? ZString.Empty;

		public ZString MasterNumber => DeclarationGoodsShipmentConsignmentTransportContractDocumentCollection?.FirstOrDefault(x => x.TypeCode.Value == TransportContractDocumentTypeCodes._704 || x.TypeCode.Value == TransportContractDocumentTypeCodes._741)?.Id?.Value ?? ZString.Empty;

		public ZString HouseNumber => DeclarationGoodsShipmentConsignmentTransportContractDocumentCollection?.FirstOrDefault(x => x.TypeCode.Value == TransportContractDocumentTypeCodes._703 || x.TypeCode.Value == TransportContractDocumentTypeCodes._714)?.Id?.Value ?? ZString.Empty;

		public ZString CarrierID => DeclarationGoodsShipmentConsignment?.Carrier?.Id?.Value ?? ZString.Empty;

		public ZString ExporterName => GoodsShipmentExporter?.Name?.Value ?? ZString.Empty;

		public ZString ExporterID => GoodsShipmentExporter?.Id?.Value ?? ZString.Empty;

		public ZString AgentID => Declaration?.Agent?.Id?.Value ?? ZString.Empty;

		public ZString CallSignID => GoodsShipmentConsignmentBorderTransportMeans?.TwCallSignId?.Value ?? ZString.Empty;

		public ZString JourneyID => GoodsShipmentConsignmentBorderTransportMeans?.JourneyId?.Value ?? ZString.Empty;

		public ZString GoodsLocationID => DeclarationGoodsShipmentConsignment?.GoodsLocation?.Id?.Value ?? ZString.Empty;

		public ZString ReleasedQuantityAndUnit => $"{ResponseStatus?.TwTotalPackageQuantity?.Value ?? ZDecimal.Zero} {Unit}".Trim();

		public ZString UnreleasedQuantityAndUnit => $"{response?.TwUnreleasedPackages?.TwQuantityQuantity?.Value ?? ZDecimal.Zero} {Unit}".Trim();

		ZString Unit => response?.TwUnreleasedPackages?.TwTypeCode?.Value ?? ZString.Empty;

		public ZString ReleaseDateTime
		{
			get
			{
				var result = ZString.Empty;
				var releaseDateTime = DocumentWrapperHelper.StringAsDateTime(ResponseStatus?.ReleaseDateTime);
				if (releaseDateTime.IsValid)
				{
					result = releaseDateTime.ToString("yyyy/MM/dd HH:mm:ss");
				}
				return result;
			}
		}

		ResponseDeclarationGoodsShipmentConsignment DeclarationGoodsShipmentConsignment => DeclarationGoodsShipment?.Consignment;

		Collection<ResponseDeclarationGoodsShipmentConsignmentTransportEquipment> DeclarationGoodsShipmentConsignmentTransportEquipmentCollection => DeclarationGoodsShipmentConsignment?.TransportEquipment;

		ZString ContainerInfo
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

		public ZString EquipmentCurrentCode
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

		ResponseDeclarationPackaging DeclarationPackaging => Declaration?.Packaging;

		ZString MarksNumbers => DeclarationPackaging?.MarksNumbers?.Value ?? ZString.Empty;

		public ZString MarksNumbersAndTransportEquipment
		{
			get
			{
				var otherDeclarations = new ZStringBuilder();
				var marksNumbers = MarksNumbers;
				var containerInfo = ContainerInfo;
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
				return otherDeclarations.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString NameCode => ResponseStatus?.NameCode?.Value ?? ZString.Empty;

		public ZString Registration => GoodsShipmentConsignmentBorderTransportMeans?.TwRegistration?.Value ?? ZString.Empty;

		public ZString StatementCode
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
	}
}
