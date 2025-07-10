using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Universal.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public class COSTCOHeader : ICOSTCOMessageDataProvider
	{
		public COSTCOHeader(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		#region ICOSTCOMessageDataProvider

		ZString IBGM_BeginningOfMessage.OutturnManifestType => header.AMA_ManifestType;

		ZDateTime IDTM_DocumentDateTime.DocumentIssueDateTime => header.AMA_IssueDate;

		ZDateTime IDTM_ActualArrivalDate.ActualArrivalDateTime => header.MasterBill.ABL_E_ARV;

		ZDateTime IDTM_EstimatedDateOfDeparture.EstimatedDateOfDeparture => header.MasterBill.ABL_E_DEP;

		ZDateTime IDTM_DateTimeFullyUnloadedLoaded.DateTimeFullyUnloadedLoaded => header.FullyLoadedUnloadedDate;

		ZString IFTX_ExcessIndicator.ExcessIndicator => header.ExcessIndicator;

		ZString IFTX_ImportExportTranshipmentIndicator.ImportExportTranshipmentIndicator
		{
			get
			{
				ZString result;
				var nature = Nature;
				switch (nature)
				{
					case NatureList.Codes.Import23:
						result = COSTCO.Contants.ImportExportTranshipmentIndicator.Import23;
						break;
					case NatureList.Codes.Export22:
						result = COSTCO.Contants.ImportExportTranshipmentIndicator.Export22;
						break;
					case NatureList.Codes.Transhipment28:
						result = COSTCO.Contants.ImportExportTranshipmentIndicator.Transhipment28;
						break;
					case NatureList.Codes.Transit24:
						result = COSTCO.Contants.ImportExportTranshipmentIndicator.Transit24;
						break;
					default:
						result = ZString.Empty;
						break;
				}

				return result;
			}
		}

		ZString IRFF_DocumentToBeAmended.DocumentToBeAmended => header.GetLastAcceptedCOSTCOEDIMessage()?.ParentMessageNumber ?? ZString.Empty;

		ZString ITDT_TransportInformation.VoyageFlightNumber => header.AMA_Voyage;

		ZString ITDT_TransportInformation.TransportCode
		{
			get
			{
				var result = ZString.Empty;
				var transportMode = header.AMA_TransportMode;
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Sea:
						result = COSTCO.Contants.TransportCode.Sea;
						break;
					case Core.Constants.TransportModes.Rail:
						result = COSTCO.Contants.TransportCode.Rail;
						break;
					case Core.Constants.TransportModes.Road:
						result = COSTCO.Contants.TransportCode.Road;
						break;
					case Core.Constants.TransportModes.Air:
						result = COSTCO.Contants.TransportCode.Air;
						break;
				}

				return result;
			}
		}

		ZString ITDT_TransportInformation.CarrierCode => header.CarrierCode;

		ZString ITDT_TransportInformation.CallSign => header.Vessel?.RV_RadioCallSign ?? ZString.Empty;

		ZString IRFF_ManifestType.ManifestType
		{
			get
			{
				ZString result;
				var nature = Nature;
				switch (nature)
				{
					case NatureList.Codes.Import23:
						result = COSTCO.Contants.ManifestType.Import23;
						break;
					case NatureList.Codes.Export22:
						result = COSTCO.Contants.ManifestType.Export22;
						break;
					case NatureList.Codes.Transhipment28:
						result = COSTCO.Contants.ManifestType.Transhipment28;
						break;
					case NatureList.Codes.Transit24:
						result = COSTCO.Contants.ManifestType.Transit24;
						break;
					case NatureList.Codes.FreightRemainingOnBoard:
						result = COSTCO.Contants.ManifestType.FreightRemainingOnBoard;
						break;
					default:
						result = ZString.Empty;
						break;
				}

				return result;
			}
		}

		ZString Nature => header.AMA_Nature;

		ZString IRFF_PrincipalCarrierConveyageNumber.PrincipalCarrierConveyageNumber => header.AMA_Voyage;

		ZString ILOC_PlacePortOfDischarge.PlaceOfDicharge => header.MasterBill.ABL_RL_NKPortOfDischarge;

		ZString ILOC_PlacePortOfDischarge.TerminalDepotCode => header.MasterBill.ABL_GoodsLocation;

		ZString ILOC_PlacePortOfLoading.PlaceOfLoading => header.MasterBill.ABL_RL_NKPortOfLoading;

		ZString ILOC_PlacePortOfLoading.TerminalBerth => header.TerminalBerth;

		ZString INAD_MessageSender.MessageSender => ((IInterchangeSenderIdProvider)header).SenderID;

		ZString INAD_OutturnProviderCode.OutturnProviderCode => header.OutturnProvider;

		ZInt ICNT_ControlTotal.TotalNumberOfPackages => ((ICOSTCOMessageDataProvider)this).Bills.SelectMany(x => x.Packs).Sum(x => x.NumberOfPackagesPackedUnpacked);

		IEnumerable<ICOSTCOContainerInformation> ICOSTCOMessageDataProvider.Containers
		{
			get
			{
				var result = header.Containers.Cast<AsycudaContainer>().Select(x => (ICOSTCOContainerInformation)new COSTCOContainer(x)).ToList();
				return result.Any()
					? result
					: new List<ICOSTCOContainerInformation> { new MockCOSTCOContainer(header) };
			}
		}

		IEnumerable<ICOSTCOLineLevelInformation> ICOSTCOMessageDataProvider.Bills => header.Bills.Cast<AsycudaBill>().Select((x, i) => new COSTCOBill(x, i)).ToList();

		ZBool ICOSTCOMessageDataProvider.IsDOR => header.IsDOR;

		ZBool ICOSTCOMessageDataProvider.IsBBB => header.IsBBB;

		ZBool ICOSTCOMessageDataProvider.IsVOR => header.IsVOR;

		ZBool ICOSTCOMessageDataProvider.IsAOR => header.IsAOR;

		ZBool ICOSTCOMessageDataProvider.IsEOR => header.IsEOR;

		ZBool ICOSTCOMessageDataProvider.IsALD => header.IsALD;

		ZBool ICOSTCOMessageDataProvider.IsAir => header.IsAir;

		ZBool ICOSTCOMessageDataProvider.IsImport => header.IsImport;

		ZBool ICOSTCOMessageDataProvider.IsExport => header.IsExport;

		#endregion

		#region IEDIMessageCollectionProvider

		Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => header.Messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => header.Factory;

		void IEDIFACTMessageAttachee.AddMessage(EDIMessage message)
		{
			header.Messages.Add(message);
		}

		ZString IEDIFACTMessageAttachee.MessageStatus { get; set; } = ZString.Empty;

		ZString IEDIFACTMessageAttachee.JobStatus { get; set; } = ZString.Empty;

		bool IEDIFACTMessageAttachee.HasChanges => header.HasChanges;

		ZString IEDIFACTMessageAttachee.JobIdentification => ZString.Empty;

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject => header;

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage => true;

		#endregion

		readonly AsycudaManifestHeader header;
	}
}
