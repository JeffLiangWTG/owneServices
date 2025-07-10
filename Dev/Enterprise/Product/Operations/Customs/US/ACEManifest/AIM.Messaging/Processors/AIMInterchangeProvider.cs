using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.US.AIM.Messaging.Constants;
using IOriginatorCodeProvider = Enterprise.Integration.Customs.ASYCUDA.ACEManifest.IOriginatorCodeProvider;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMInterchangeProvider : Enterprise.Messaging.InterchangeProviders.InterchangeProviderBase
	{
		public AIMInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override Type InterchangeType => typeof(AIMEDIInterchange);
		protected override string InstructionHowToSetInterchangeSenderID => "Originator code can be added under CFS Address > Details > Config > Registration Numbers / Codes, (using type 'AMO') or added under Current Branch ({0}) or Current Company ({1}) Organization Proxy > Details > Config > Registration Numbers / Codes, (using type 'AMO')";
		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var message = messages.OfType<AIMEDIMessage>().Single();
			var branchPK = message.EM_GB;
			using (branchPK.IsValid && branchPK != GlbBranch.CurrentBranch.PK ? DisposableEnvironment.ForBranch(branchPK.ToGuid()) : null)
			{
				var originatorCode = ZString.Empty;
				if (message.EM_LinkedObject is IOriginatorCodeProvider provider && !provider.AirAMSOriginatorCode.IsEmpty)
				{
					originatorCode = provider.AirAMSOriginatorCode;
				}

				if (!originatorCode.IsEmpty)
				{
					interchange.EI_ApplicationCode = message.EM_ApplicationCode;
					interchange.EI_InterchangeType = message.EM_MessageType;
					interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
					interchange.EI_Status = EDIInterchange.Status.eHubQueued;
					interchange.ContainedMessages.Add(message);
					interchange.EI_GB = branchPK;
					interchange.EI_To = AIMMessageRecipients.USCustoms;
					interchange.EI_From = originatorCode;
					interchange.EI_HeaderText = BuildInterchangeHeader(originatorCode);
					interchange.EI_BodyText = message.EM_MessageText;

					message.EM_Status = EDIMessage.Status.Sent;
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Discarded;
					var stringBuilder = new ZStringBuilder();
					stringBuilder.AppendLine(ZString.Format("Message {0} will be discarded for the following reason:", message.EM_MessageNum));
					stringBuilder.AppendLine("There is no Customs Originator Code set up");
					stringBuilder.AppendLine(ZString.Format("for Company - {0}, Branch - {1}", message.Company.GC_Code, message.Branch.GB_Code));
					stringBuilder.AppendLine(ZString.Format(InstructionHowToSetInterchangeSenderID, message.Branch.GB_BranchName, message.Company.CompanyName));
					message.Notes.AddNew(true, ProcessingLogDescription, stringBuilder.ToString());
					interchange.Delete();
				}
			}
		}

		ZString BuildInterchangeHeader(string originatorCode)
		{
			var isProductionSystem = Env.Instance.IsProductionSystem;
			var systemTypeCode = isProductionSystem ? AIMSystemTypes.WASUSCR : AIMSystemTypes.WASUCCR;

			return ZString.Format("{0}\x0D\x0A\x2E{1}", systemTypeCode, originatorCode);
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}
	}
}
