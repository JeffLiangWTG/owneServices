using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;

namespace Enterprise.Customs.US.Messaging.Business
{
	public class CBPInterchangeProvider : InterchangeProviderBase
	{
		public CBPInterchangeProvider(NonDependentEDIMessageCollection messages, bool enableEnviromentSwitching = false)
			: this(messages, "See US Customs registry.", enableEnviromentSwitching)
		{
		}
		public CBPInterchangeProvider(NonDependentEDIMessageCollection messages, string instructionHowToSetInterchangeSenderID, bool enableEnviromentSwitching = false)
			: base(messages)
		{
			this.instructionHowToSetInterchangeSenderID = instructionHowToSetInterchangeSenderID;
			this.enableEnviromentSwitching = enableEnviromentSwitching;
		}

		protected override string GetCollationKey(EDIMessage message)
		{
			return DoNotCollateType;
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return instructionHowToSetInterchangeSenderID; }
		}
		readonly string instructionHowToSetInterchangeSenderID;

		protected override ZString GetInterchangeFooter(int messageCount)
		{
			return generator != null && generator.Z != null ? generator.Z.Serialise() : "";
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			if (messages.Count != 1)
			{
				throw new ArgumentException("Messages collection must have one element only - had " + messages.Count, nameof(messages));
			}

			EDIMessage message = messages[0];
			SwitchEnvironmentIfNeeded(message.EM_GB);
			interchange.EI_GB = message.EM_GB;
			var branch = message.Branch;
			generator = ApplicationControlGenerator.New(message.EM_ApplicationCode, message.EM_MessageType, branch);
			generator?.AddMessage(message);
			var senderIDForInterchange = generator == null ? ZString.Empty : GetFromKey(message, generator.A.FilerID);
			ZString bodyText = generator?.GetBody() ?? ZString.Empty;
			if (!senderIDForInterchange.IsEmpty && !bodyText.IsEmpty)
			{
				message.EM_Status = EDIMessage.Status.Sent;
				interchange.EI_ApplicationCode = message.EM_ApplicationCode;
				interchange.EI_InterchangeType = message.EM_MessageType;
				interchange.EI_Status = CBPEDIInterchange.Status.Queued;
				interchange.EI_ReceiveTransmit = CBPEDIInterchange.Direction.Transmit;
				interchange.ContainedMessages.Add(message);
				interchange.EI_To = "USC";
				interchange.EI_From = senderIDForInterchange;
				interchange.EI_HeaderText = generator.A.Serialise();
				interchange.EI_BodyText = bodyText;
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Discarded;
				ZStringBuilder stringBuilder = new ZStringBuilder();
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, "Message {0} will be discarded for the following reason:", message.EM_MessageNum));
				if (senderIDForInterchange.IsEmpty)
				{
					stringBuilder.Append("There is no Customs Interchange Sender ID set up");
					var settingDetails = generator?.GetSettingDetails(branch);
					if (string.IsNullOrEmpty(settingDetails))
					{
						var company = branch.Company;
						settingDetails = string.Format(CultureInfo.CurrentCulture, "for Company - {0} (OrgProxy:{1}), Branch - {2}", company.GC_Code, company.OrgProxy.OH_Code, branch.GB_Code);
					}
					stringBuilder.Append(settingDetails);
				}
				else if (bodyText.IsEmpty)
				{
					stringBuilder.Append("There is no message text generated.");
				}
				message.Notes.AddNew(true, ProcessingLogDescription, stringBuilder.ToStringWithNewLineBetweenAppends());
				interchange.Delete();
			}
		}

		protected virtual ZString GetFromKey(EDIMessage message, ZString filerID)
		{
			return filerID;
		}

		ApplicationControlGenerator generator;

		protected override Type InterchangeType
		{
			get { return typeof(CBPEDIInterchange); }
		}

		void SwitchEnvironmentIfNeeded(ZGuid messageBranchPK)
		{
			if (enableEnviromentSwitching)
			{
				if (!CompanyBranchPKs.TryGetValue(GlbCompany.CurrentCompany.PK, out var branchPKs))
				{
					SwitchEnvironment(messageBranchPK);
					AddToCompanyBranchPKs(GlbCompany.CurrentCompany);
				}
				else if (!branchPKs.Contains(messageBranchPK))
				{
					SwitchEnvironment(messageBranchPK);
					if (!CompanyBranchPKs.ContainsKey(GlbCompany.CurrentCompany.PK))
					{
						AddToCompanyBranchPKs(GlbCompany.CurrentCompany);
					}
				}
			}
		}
		readonly bool enableEnviromentSwitching;

		void SwitchEnvironment(ZGuid messageBranchPK)
		{
			environmentSwitchDisposable?.Dispose();
			environmentSwitchDisposable = DisposableEnvironment.ForBranch(messageBranchPK.ToGuid());
		}

		void AddToCompanyBranchPKs(GlbCompany currentCompany)
		{
			CompanyBranchPKs.Add(currentCompany.PK, currentCompany.Branches.GetPKs().ToHashSet());
		}

		Dictionary<ZGuid, HashSet<ZGuid>> CompanyBranchPKs => companyBranchPKs ?? (companyBranchPKs = new Dictionary<ZGuid, HashSet<ZGuid>>());
		Dictionary<ZGuid, HashSet<ZGuid>> companyBranchPKs;

		protected override void DisposeCore()
		{
			base.DisposeCore();
			environmentSwitchDisposable?.Dispose();
		}
		IDisposable environmentSwitchDisposable;
	}
}
