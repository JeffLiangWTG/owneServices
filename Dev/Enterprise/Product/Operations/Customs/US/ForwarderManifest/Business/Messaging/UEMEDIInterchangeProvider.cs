using System;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMEDIInterchangeProvider : Enterprise.Messaging.InterchangeProviders.InterchangeProviderBase
	{
		public UEMEDIInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override Type InterchangeType => typeof(UEMEDIInterchange);

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get
			{
				return "Please add a carrier code in company organization proxy > Config > Registration Numbers / Codes, (using type 'CCC' or 'CCP').";
			}
		}

		protected override string GetCollationKey(EDIMessage message) => DoNotCollateType;

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count > 0)
			{
				var message = messages[0];
				var branch = message.Branch ?? GlbBranch.CurrentBranch;
				var company = branch.Company;

				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					var header = message.EM_LinkedObject is AsycudaBill bill ? bill.Header : null;

					if (header != null)
					{
						var senderID = company.GetSenderIDFromOrgProxy(header.IsAir);
						if (!senderID.IsEmpty)
						{
							SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, "USC", senderID);
							header.AMA_MessageStatus = message.EM_Status == EDIMessage.Status.Sent ? MessageStatusCodeList.Codes.Sent : ZString.Empty;

							interchange.EI_GB = branch.PK;
						}
						else
						{
							message.EM_Status = EDIMessage.Status.Discarded;
						}
					}
					else
					{
						message.EM_Status = EDIMessage.Status.Discarded;
					}

					if (message.EM_Status == EDIMessage.Status.Discarded)
					{
						var stringBuilder = new ZStringBuilder();
						stringBuilder.AppendLine(ZString.Format("\r\nMessage {0} will be discarded for the following reason:", message.EM_MessageNum));
						if (header != null)
						{
							header.AMA_MessageStatus = EDIMessage.Status.Error;
							var codeType = header.IsAir ? OrgCusCode.CodeTypes.ControlledPremisesID : OrgCusCode.CodeTypes.CarrierCode;
							stringBuilder.AppendLine("There is no Customs Interchange Sender ID set up.");
							stringBuilder.AppendLine(ZString.Format("Please add a carrier code in company '{0}' organization proxy > Config > Registration Numbers / Codes, (using type '{1}').", company.GC_Code, codeType));
						}
						else
						{
							stringBuilder.AppendLine("There is no header being related to this message.");
						}
						message.Notes.AddNew(true, ProcessingLogDescription, stringBuilder.ToString());
						interchange.Delete();
					}
				}
			}
		}

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange)
		{
			return EDIInterchange.Status.eHubQueued;
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}
	}
}
