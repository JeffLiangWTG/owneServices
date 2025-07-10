using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffChangeRequestWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("ea0eead0-abeb-48a2-9734-60fcc51ae08f", "Change Request");

		public override ControllerID ControllerID => ControllerIDs.GlbStaffChangeRequest;

		public override Type WorkflowProviderType => typeof(GlbStaffChangeRequest);

		public override bool SupportsBufferManagement => true;

		public override bool SupportsEventTracking => true;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;

		public override bool RequiresClient => false;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				if (subTypesList == null)
				{
					subTypesList = new ProcessTemplateSubType[] {
						new ProcessTemplateSubType(Res.GetString("EF516A8E-180D-4468-B8E5-4CADD7D5F261", "Change Request Templates"), StaffChangeRequestTemplates),
					};
				}
				return subTypesList;
			}
		}
		ProcessTemplateSubType[] subTypesList;

		CodeDescriptionPairList StaffChangeRequestTemplates
		{
			get
			{
				var factory = LastProcessTaskTemplate != null ? LastProcessTaskTemplate.Factory : CreateNewFactory();

				return factory.GetCachedValue("All.StaffChangeRequestTemplates", () =>
				{
					var changeRequestTemplates = new CodeDescriptionPairList();
					changeRequestTemplates.AddRange(factory.Load<GlbStaffChangeRequestTemplate>(new ZQuery()));
					return changeRequestTemplates;
				});
			}
		}
	}
}
