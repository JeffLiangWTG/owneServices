using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business
{
	public partial class STATACREQDOCSendingObject : IREQDOCMessageDataProvider
	{
		#region IREQDOCMessageDataProvider

		ZString IREQDOCMessageDataProvider.MessageType => MessageTypeList.Codes.StatementOfAccount;

		ZString IREQDOCMessageDataProvider.LocalReferenceNumber => ZString.Empty;

		ZString IREQDOCMessageDataProvider.SenderReference => FinancialAccountNumber;

		ZString IREQDOCMessageDataProvider.MessageFunction => MessageFunctionCodeList.Codes.Original;

		ZString IREQDOCMessageDataProvider.MessageTypeForDoc => ((IREQDOCMessageDataProvider)this).MessageType;

		ZString IREQDOCMessageDataProvider.ManifestDocumentType => ZString.Empty;

		ZString IREQDOCMessageDataProvider.FinancialAccountNumber => FinancialAccountNumber;

		ZString IREQDOCMessageDataProvider.FinalMRN => ZString.Empty;

		ZString IREQDOCMessageDataProvider.DocumentMessageSource => ZString.Empty;

		ZDateTime IREQDOCMessageDataProvider.RequestDate => ZDateTime.Today;

		ZDateTime IREQDOCMessageDataProvider.StartDate => StartDate;

		ZDateTime IREQDOCMessageDataProvider.EndDate => ZDateTime.Today;

		ZString IREQDOCMessageDataProvider.MessageSender => AgentCodeLinkedToFAN + AgentDualProfileCodeLinkedToFAN;

		ZString IREQDOCMessageDataProvider.ReleaseAuthority => ZString.Empty;

		IBranch IREQDOCMessageDataProvider.Branch => GlbBranch.CurrentBranch;

		#endregion
	}
}
