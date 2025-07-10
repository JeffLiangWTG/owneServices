using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Business
{
	public class HRHiringRequestDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.HRHiringRequestDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("50167c02-9df1-4e12-bc96-5ebf1a9d35b0", "Hiring Request");

		public override ControllerID ControllerID => ControllerIDs.HRHiringRequest;

		public override Type WorkflowProviderType => typeof(HRHiringRequest);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => true;
		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Email;
		}

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypesList == null)
				{
					subTypesList = new[]
					{
						new ProcessTemplateSubType(Res.GetString("f2b33516-fcaa-4048-b7d5-075253a0f13c", "Team"), Teams),
					};
				}
				return subTypesList;
			}
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
					teamsList.AddRange(factory.Load<GlbTeam>(new ZQuery()));
					return teamsList;
				});
			}
		}

		public override bool RequiresClient => false;

		public override bool RequiresBranch => true;

		public override bool RequiresDepartment => true;
	}
}
