using System;
using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Res = Enterprise.Customs.DataTransfer.Res;
using ResString = Enterprise.Customs.DataTransfer.ResString;

namespace Enterprise.Customs.Business
{
	public class CommercialInvoiceLineWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description / ControllerID

		public override string Code => WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("CommercialInvoiceLineWorkflowDescriptor|Description", "Commercial Invoice Line");

		public override ControllerID ControllerID => null; // Does not have Controller ID

		public override ZString MilestoneTemplateHintCaption => string.Empty;

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation => new List<ProcessTemplateSubType>(base.SubTypeInformation)
		{
			new ProcessTemplateSubType(Res.GetString("cd805ce8-6a32-4f7c-8407-24a61cf710e7", "Job Type"), JobDeclarationWorkflowDescriptor.GetJobMessageTypeList(this))
		}.ToArray();

		#endregion

		#region Criteria Requirements

		public override bool RequiresBranch => true;
		public override bool RequiresDepartment => false;
		public override bool RequiresPort2 => false;
		public override bool SupportsEventTracking => false;
		public override bool RequiresPort1 => false;
		public override bool SupportsTasks => false;
		public override bool SupportsScreenLayout => false;
		public override bool SupportsUniversalTemplates => false;
		public override bool SupportsBufferManagement => false;

		#endregion

		public override Type WorkflowProviderType => typeof(BaseJobComInvoiceLine);
	}
}
