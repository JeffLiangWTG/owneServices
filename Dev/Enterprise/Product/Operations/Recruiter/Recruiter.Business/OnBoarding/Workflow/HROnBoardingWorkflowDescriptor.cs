using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HROnBoardingWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.HROnBoardingWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("8C8128F6-74D8-4100-8D91-64C553EA5BE9", "On-boarding");

		public override ControllerID ControllerID => ControllerIDs.HROnBoarding;

		public override Type WorkflowProviderType => typeof(HROnBoarding);

		public override bool SupportsBufferManagement => true;

		public override bool SupportsEventTracking => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email | MessageRecipientPartyType.OnBoardingEmail;

		public override ProcessTemplateSubType[] SubTypeInformation => subTypesList ?? (subTypesList = new[]
		{
			new ProcessTemplateSubType(Res.GetString("f2b33516-fcaa-4048-b7d5-075253a0f13c", "Team"), Teams),
			new ProcessTemplateSubType(Res.GetString("17211041-785a-416a-98b7-60f3d1cd8a22", "Contract Status"), GetContractStatus())
		});

		CodeDescriptionPairList GetContractStatus()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(string.Empty, Res.GetString("44e5d757-3b36-4fa9-a534-a58e0fc7d277", "All"));
			result.AddPair("NST", ResString.GetMultilingualString("6f052c80-7f7c-4cec-a06e-ccd82c02fd5f", "Not Sent"));
			result.AddPair("SNT", ResString.GetMultilingualString("1c616acc-e507-4fe9-886a-25c31c078d5f", "Sent"));
			result.AddPair("REJ", ResString.GetMultilingualString("0332aa8b-627b-4552-a465-c6dfda25f6b2", "Rejected"));
			result.AddPair("ACP", ResString.GetMultilingualString("02f62152-3da2-498f-a291-adbf137aea0e", "Accepted"));
			return result;
		}

		ProcessTemplateSubType[] subTypesList;

		public CodeDescriptionPairList Teams
		{
			get
			{
				var factory = LastProcessTaskTemplate != null ? LastProcessTaskTemplate.Factory : CreateNewFactory();
				return factory.GetCachedValue("All.Teams", () =>
				{
					var teamsList = new CodeDescriptionPairList();
					teamsList.AddRange(factory.Load<GlbTeam>(new ZQuery(GlbTeamSchema.GST_IsActive, true)));
					return teamsList;
				});
			}
		}

		protected override bool IsDirectEmailRecipient(string triggerParty)
		{
			return base.IsDirectEmailRecipient(triggerParty)
				|| triggerParty == MessageRecipientPartyTypeList.Codes.OnBoardingEmail;
		}

		protected override string[] GetNotificationEmailAddresses(ProcessTaskNotification action, BusinessObject parent)
		{
			switch (action.PQ_Calc_TriggerParty)
			{
				case MessageRecipientPartyTypeList.Codes.OnBoardingEmail:
					var onboarding = (HROnBoarding)parent;
					return new[] { onboarding.JobApplicant.Email };
				default:
					return base.GetNotificationEmailAddresses(action, parent);
			}
		}

		public new BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		public override bool RequiresClient => false;

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => true;
	}
}
