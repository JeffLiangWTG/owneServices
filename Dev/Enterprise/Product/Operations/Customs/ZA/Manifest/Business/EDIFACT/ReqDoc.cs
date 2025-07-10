using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public class ReqDoc : IREQDOCMessageDataProvider
	{
		public ReqDoc(CUSCAREDIMessage message, AsycudaManifestHeader header)
		{
			this.message = message;
			this.header = header;
		}

		IBranch IREQDOCMessageDataProvider.Branch => header?.Branch ?? GlbBranch.CurrentBranch;

		ZString IREQDOCMessageDataProvider.MessageType => DocumentNameCodeList.CargoManifest.ToString();

		ZString IREQDOCMessageDataProvider.LocalReferenceNumber => message.LocalReferenceNumber;

		ZString IREQDOCMessageDataProvider.SenderReference => ZString.Empty;

		ZString IREQDOCMessageDataProvider.MessageFunction => Universal.MessageFunctionCodeList.Codes.Original;

		ZString IREQDOCMessageDataProvider.MessageTypeForDoc => DocumentNameCodeList.MasterBillOfLading.ToString();

		ZString IREQDOCMessageDataProvider.FinancialAccountNumber => ZString.Empty;

		ZString IREQDOCMessageDataProvider.FinalMRN => message.CUSCARD16AHelper.MasterTransportDocumentNumber;

		ZString IREQDOCMessageDataProvider.DocumentMessageSource => ZString.Empty;

		ZString IREQDOCMessageDataProvider.ManifestDocumentType => message.CUSCARD16AHelper.ManifestType;

		ZDateTime IREQDOCMessageDataProvider.RequestDate => ZDateTime.Today;

		ZDateTime IREQDOCMessageDataProvider.StartDate => ZDateTime.Invalid;

		ZDateTime IREQDOCMessageDataProvider.EndDate => ZDateTime.Invalid;

		ZString IREQDOCMessageDataProvider.MessageSender => message.CUSCARD16AHelper.MessageSender;

		ZString IREQDOCMessageDataProvider.ReleaseAuthority => ZString.Empty;

		readonly CUSCAREDIMessage message;
		readonly AsycudaManifestHeader header;
	}
}
