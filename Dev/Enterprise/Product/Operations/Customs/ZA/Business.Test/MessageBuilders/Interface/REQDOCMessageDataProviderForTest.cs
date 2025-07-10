using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.Business.MessageBuilders.Testing
{
	sealed class REQDOCMessageDataProviderForTest : IREQDOCMessageDataProvider
	{
		public REQDOCMessageDataProviderForTest()
		{
		}

		public IBranch Branch => GlbBranch.CurrentBranch;
		#region Fields For BGM
		public ZString MessageType
		{
			get => messageType;
			set
			{
				messageType = value;
				MessageTypeForDoc = MessageType;
			}
		}

		ZString messageType;
		public ZString LocalReferenceNumber { get; set; }

		public ZString SenderReference { get; set; }

		public ZString MessageFunction { get; set; }

		#endregion
		#region Fields For DOC
		public ZString MessageTypeForDoc { get; set; }

		public ZString FinancialAccountNumber { get; set; }

		public ZString FinalMRN { get; set; }

		public ZString DocumentMessageSource { get; set; }

		public ZString ManifestDocumentType { get; set; }

		#endregion
		#region Fields For DTM
		public ZDateTime RequestDate { get; set; }

		public ZDateTime StartDate { get; set; }

		public ZDateTime EndDate { get; set; }

		#endregion
		#region Fields For SG6
		public ZString MessageSender { get; set; }

		public ZString ReleaseAuthority { get; set; }
		#endregion
	}
}
