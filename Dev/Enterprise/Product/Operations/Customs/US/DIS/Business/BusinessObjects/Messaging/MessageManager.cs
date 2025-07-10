using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	public class MessageManager : DISMessageManagerBase
	{
		public MessageManager(DISDocument disDocument)
			: base(disDocument)
		{
			this.disDocument = disDocument;
		}

		new readonly DISDocument disDocument;

		protected override void SendSubmissionCore()
		{
			var wrapper = (IDISDocument)new DISDocumentWrapper(disDocument);
			new DISMessageBuilder(wrapper).BuildDocumentSubmissionPackage();
			disDocument.Status = wrapper.ActionCodeForSubmission == ActionCodeList.Codes.Add ? StatusList.Codes.AOS : StatusList.Codes.ARS;
			PopulateSubmitDate();
		}

		protected override void SendWithdrawlCore()
		{
			new DISMessageBuilder(new DISDocumentWrapper(disDocument)).BuildDocumentWithdrawalUsingDocSubmissionPackage();
			disDocument.Status = StatusList.Codes.AWS;
			PopulateSubmitDate();
		}

		void PopulateSubmitDate()
		{
			if (disDocument.SubmitDateUTC.IsEmpty)
			{
				disDocument.SubmitDateUTC = ZDateTime.UtcNow;
			}
		}
	}
}
