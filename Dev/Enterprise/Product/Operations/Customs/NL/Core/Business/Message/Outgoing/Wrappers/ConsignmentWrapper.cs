using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class ConsignmentWrapper : IConsignment
{
	public ConsignmentWrapper(Declaration.CusEntryHeader entryHeader, JobDeclarationMessageSendingObject messageSendingObject)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		randomHeader = Argument.NotNull(entryHeader.RandomHeader, nameof(entryHeader.RandomHeader));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
	}
	readonly Declaration.CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly JobComInvoiceHeader randomHeader;
	readonly Declaration.CusEntryInstruction entryInstruction;
	readonly JobDeclarationMessageSendingObject messageSendingObject;

	public string ContainerCode
	{
		get
		{
			var cntModes = new List<string>()
			{
				Core.Constants.ContainerModes.LCL,
				Core.Constants.ContainerModes.FCL,
				Core.Constants.ContainerModes.ULD,
				Core.Constants.ContainerModes.Containerised
			};
			var cntMode = entryHeader.Declaration.ContainerMode;
			return cntModes.Contains(cntMode) ? "1" : (cntMode is null || cntMode.Length == 0 ? null : "0");
		}
	}

	public decimal TotalGrossMassMeasure => entryHeader.TotalInvoiceLinesGrossWeightInKG;

	public IArrivalTransportMeans ArrivalTransportMeans => new ArrivalTransportMeansWrapper(declaration);

	public IBorderTransportMeans BorderTransportMeans => new BorderTransportMeansWrapper(declaration);

	public IGoodsLocation GoodsLocation => entryInstruction.GoodsLocation is CusGoodsLocation goodsLocation ? GoodsLocationWrapper.New(goodsLocation) : null;

	public IParty Carrier => PartyWrapper.New(declaration.CarrierEUBorderDocAddress.Organisation);

	public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () => IncludeConsignee ? PartyWrapper.New(declaration.ImporterDocumentaryAddress.Organisation) : null);
	CachedValue<IParty> consignee;

	bool IncludeConsignee
	{
		get
		{
			if (messageSendingObject.MessageType == ExportSendMessageTypes.Codes.DEC && declaration.IsExport)
			{
				return entryHeader.AllLineConsigneesAreEmpty() && !entryHeader.MergedLines.Any(l => l.AdditionalInfos.Any((AdditionalInfo x) => x.IsAnAdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600))
					&& !entryInstruction.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.IsAnAdditionalInformation && x.CSI_Code == EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600);
			}
			return true;
		}
	}

	public IParty Consignor => CachedValueHelper.GetValue(ref consignor, () => IncludeConsignor ? PartyWrapper.New(declaration.SupplierDocumentaryAddress.Organisation) : null);
	CachedValue<IParty> consignor;

	bool IncludeConsignor => !(messageSendingObject.MessageType == ExportSendMessageTypes.Codes.DEC && declaration.IsExport) || entryHeader.AllLineConsignorsAreEmpty();
	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??= GetDepartureTransportMeans().ToArray();
	IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

	IEnumerable<IDepartureTransportMeans> GetDepartureTransportMeans()
	{
		var sequence = 1;
		var departureTransportMeansWrapper = DepartureTransportMeansWrapper.New(declaration, sequence, declaration.JE_TransportIDInland, declaration.JE_TransportMeans, declaration.JE_RN_NKTransportNationalityInland);
		if (departureTransportMeansWrapper != null)
		{
			yield return departureTransportMeansWrapper;
			sequence++;
		}

		if (declaration.JE_TransportModeInland == TransportTypeList.Codes.Road)
		{
			if (!declaration.JE_Trailer1RegNo.IsEmpty)
			{
				var trailer1 = DepartureTransportMeansWrapper.New(declaration, sequence, declaration.JE_Trailer1RegNo, NLConstants.ProcedureCodes._31, declaration.JE_RN_NKTrailer1Nationality);
				if (trailer1 != null)
				{
					yield return trailer1;
					sequence++;
				}
			}

			if (!declaration.JE_Trailer2RegNo.IsEmpty)
			{
				var trailer2 = DepartureTransportMeansWrapper.New(declaration, sequence, declaration.JE_Trailer2RegNo, NLConstants.ProcedureCodes._31, declaration.JE_RN_NKTrailer2Nationality);
				if (trailer2 != null)
				{
					yield return trailer2;
					sequence++;
				}
			}
		}
	}

	public IFreight Freight => new FreightWrapper(declaration);

	public IReadOnlyCollection<IItinerary> Itineraries => itineraries ??= declaration.ItineraryCountries.Cast<ItineraryCountry>().Select((x, index) => new ItineraryWrapper(x, index + 1)).ToList();
	IReadOnlyCollection<IItinerary> itineraries;

	public IReadOnlyCollection<IAdditionalReference> TransportContractDocuments => transportContractDocuments ??= entryInstruction.AdditionalInfos.Where(x => x.IsATransportDocument).Select((x, index) => new AdditionalReferenceWrapper(x, index + 1)).ToList();
	IReadOnlyCollection<IAdditionalReference> transportContractDocuments;

	public IReadOnlyCollection<IDMSOutgoingTransportEquipment> TransportEquipments => transportEquipments ??= entryHeader.Containers.Select((x, index) => new TransportEquipmentWrapper((EU.Business.Declaration.CusContainer)x, index + 1)).ToList();
	IReadOnlyCollection<IDMSOutgoingTransportEquipment> transportEquipments;

	public string UCR => randomHeader.JZ_UCR;
}
