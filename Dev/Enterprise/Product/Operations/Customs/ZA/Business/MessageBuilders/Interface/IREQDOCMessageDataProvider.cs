using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface IREQDOCMessageDataProvider
	{
		IBranch Branch { get; }

		#region Fields For BGM

		ZString MessageType { get; }
		ZString LocalReferenceNumber { get; }
		ZString SenderReference { get; }
		ZString MessageFunction { get; }

		#endregion

		#region Fields For DOC

		ZString MessageTypeForDoc { get; }
		ZString FinancialAccountNumber { get; }
		ZString FinalMRN { get; }
		ZString DocumentMessageSource { get; }
		ZString ManifestDocumentType { get; }

		#endregion

		#region Fields For DTM

		ZDateTime RequestDate { get; }
		ZDateTime StartDate { get; }
		ZDateTime EndDate { get; }

		#endregion

		#region Fields For SG6

		ZString MessageSender { get; }
		ZString ReleaseAuthority { get; }

		#endregion
	}
}
