using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business
{
	public partial class MessageSendingObjectForREQDOC : IREQDOCMessageDataProvider
	{
		#region IREQDOCMessageDataProvider

		ZString IREQDOCMessageDataProvider.MessageType => Declaration.IsImport ? MessageTypeList.Codes.Import : MessageTypeList.Codes.Export;

		ZString IREQDOCMessageDataProvider.LocalReferenceNumber => LocalReferenceNumber;

		ZString IREQDOCMessageDataProvider.SenderReference => ZString.Empty;

		ZString IREQDOCMessageDataProvider.MessageFunction => MessageFunctionCodeList.Codes.Original;

		ZString IREQDOCMessageDataProvider.MessageTypeForDoc => ((IREQDOCMessageDataProvider)this).MessageType;

		ZString IREQDOCMessageDataProvider.ManifestDocumentType => ZString.Empty;

		ZString IREQDOCMessageDataProvider.FinancialAccountNumber => ZString.Empty;

		ZString IREQDOCMessageDataProvider.FinalMRN => MovementReferenceNumber;

		ZString IREQDOCMessageDataProvider.DocumentMessageSource => DocumentMessageSource;

		ZDateTime IREQDOCMessageDataProvider.RequestDate => ZDateTime.Today;

		ZDateTime IREQDOCMessageDataProvider.StartDate => ZDateTime.Empty;

		ZDateTime IREQDOCMessageDataProvider.EndDate => ZDateTime.Empty;

		ZString IREQDOCMessageDataProvider.MessageSender => Header.AgentCode + Header.AgentDualProfileCode;

		ZString IREQDOCMessageDataProvider.ReleaseAuthority => ZString.Empty;

		IBranch IREQDOCMessageDataProvider.Branch => Header?.Declaration?.Branch;

		#endregion
	}
}
