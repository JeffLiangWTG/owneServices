using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaEDIMessage : ASYCUDA.Business.AsycudaEDIMessage
	{
		public AsycudaEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool SupportNoteMessageInterpretationCore()
		{
			var actionPurposeCode = ActionPurposeCode;
			return (actionPurposeCode == Constants.ActionPurpose.AEP
				|| actionPurposeCode == Constants.ActionPurpose.ERR
				|| actionPurposeCode == Constants.ActionPurpose.PIN);
		}

		protected override AsycudaEventMessageInterpretationGenerator GetNewAsycudaEventMessageInterpretationGeneratorCore(UniversalEvent messageUniversalEvent)
		{
			AsycudaEventMessageInterpretationGenerator htmlGenerator = null;
			var dataContext = messageUniversalEvent?.DataContext;
			if (dataContext != null)
			{
				switch (dataContext.ActionPurposeCode)
				{
					case Constants.ActionPurpose.AEP:
						htmlGenerator = new AsycudaEventMessageSuccessInterpretationGenerator(messageUniversalEvent);
						break;
					case Constants.ActionPurpose.PIN:
						htmlGenerator = new AsycudaEventMessagePinInterpretationGenerator(messageUniversalEvent);
						break;
					default:
						htmlGenerator = new AsycudaEventMessageFailureInterpretationGenerator(Factory, messageUniversalEvent);
						break;
				}
			}
			return htmlGenerator;
		}
	}
}
