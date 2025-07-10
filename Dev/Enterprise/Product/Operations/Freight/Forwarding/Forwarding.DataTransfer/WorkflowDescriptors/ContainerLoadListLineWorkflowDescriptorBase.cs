using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public abstract class ContainerLoadListLineWorkflowDescriptorBase : WorkflowDescriptor
	{
		public override ControllerID ControllerID => null;

		public override Type WorkflowProviderType => typeof(ContainerLoadListLine);

		protected override bool SupportsReleaseGroupRulesCore => false;

		public override bool SupportsBufferManagement => false;

		public override bool SupportsEventTracking => false;

		public override bool SupportsWorkflowTriggerActionXML => false;

		public override bool SupportsWorkflowTriggerActionXMLWithJobFallback => false;

		public override bool SupportsTasks => false;

		public override bool SupportsSetFieldTriggerAction(IBaseTrigger trigger, IBusiness bizo) => false;

		public override bool SupportsWorkflowTemplates => true;

		public override bool SupportsScreenLayout => false;

		public override bool RequiresPort1 => false;

		public override bool RequiresPort2 => false;

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				var result = new List<ProcessTemplateSubType>(base.SubTypeInformation);
				result.Add(new ProcessTemplateSubType(Res.GetString("0282C502-601F-47FA-8647-CE244EB3A87D", "Transport Mode"), TransportModeList));
				return result.ToArray();
			}
		}

		CodeDescriptionPairList TransportModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("", Res.GetString("A9F90C6E-7753-474F-91A1-F9873DD475F1", "All"));
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.TransportType));
				return result;
			}
		}
	}
}

