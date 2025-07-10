using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class JobSupplierBookingLineWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.JobSupplierBookingLineWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("Forwarding|JobSupplierBookingLineWorkflowDescriptor|Description", "Supplier Booking Line");

		public override ControllerID ControllerID => null;

		public override Type WorkflowProviderType => typeof(JobSupplierBookingLine);

		protected override bool SupportsReleaseGroupRulesCore => false;

		protected override ValidationToolSettings GetValidationToolSettings() => new JobSupplierBookingLineValidationToolSettings(this);

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => false;

		public override bool SupportsWorkflowTriggerActionXML => false;

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback => false;

		public override bool SupportsTasks => false;

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => false;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsScreenLayout => false;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("bb528eb3-4dd6-4644-bfc2-1c44e6acdaa9", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		CodeDescriptionPairList TransportModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("1006bc6a-aabd-442e-8697-1671a3501b6e", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}

		public override bool RequiresPort1 => false;

		public override bool RequiresPort2 => false;
	}
}

