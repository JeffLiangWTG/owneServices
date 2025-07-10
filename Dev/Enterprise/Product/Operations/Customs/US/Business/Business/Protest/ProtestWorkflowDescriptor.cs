using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Protest
{
	public class ProtestWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code
		{
			get { return WorkflowDescriptors.ProtestWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("C3A1D109-86CA-456C-B39F-49AFE8CAE95C", "Protest declaration job"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(JobDeclaration); }
		}

		public override bool SupportsBufferManagement => false;

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.Protest; }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.Protest }; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsUniversalTemplates => false;

		public override bool RequiresClient
		{
			get { return true; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool RequiresDepartment
		{
			get { return false; }
		}

		public override bool RequiresPort1
		{
			get { return false; }
		}

		public override bool RequiresPort2
		{
			get { return false; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.OrgProxy |
				MessageRecipientPartyType.Email;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				USAddInfoSchema.US_P_PeriodBaseDate,
				USAddInfoSchema.US_P_ApplicationFurtherReview,
				USAddInfoSchema.US_P_AcceleratedDispositionInd,
				USAddInfoSchema.US_P_HardCopySent,
				USAddInfoSchema.US_P_SampleSent,
				USAddInfoSchema.US_P_FaxSent
			};
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			var fieldName = fieldColumn.Name;
			var fieldColumnDescription = string.Empty;
			switch (fieldName)
			{
				case USAddInfoSchema.Constants.US_P_PeriodBaseDate:
					fieldColumnDescription = Res.GetString("5F26BAF6-CA03-4C1D-8AD3-60FAD84F1B13", "Period Base Date");
					break;
				case USAddInfoSchema.Constants.US_P_ApplicationFurtherReview:
					fieldColumnDescription = Res.GetString("B4DB313F-F003-4091-813A-521283797784", "Further Review?");
					break;
				case USAddInfoSchema.Constants.US_P_AcceleratedDispositionInd:
					fieldColumnDescription = Res.GetString("9A8E0299-A64C-4EAF-94C0-47D3DA2F4985", "Accelerated Disposition?");
					break;
				case USAddInfoSchema.Constants.US_P_HardCopySent:
					fieldColumnDescription = Res.GetString("E760FC40-F2B6-40C3-8D4E-AE42C40FFCE4", "Hard Copy Sent?");
					break;
				case USAddInfoSchema.Constants.US_P_SampleSent:
					fieldColumnDescription = Res.GetString("83D321CB-BF32-41A6-A6BD-68A52C789A0E", "Sample Sent?");
					break;
				case USAddInfoSchema.Constants.US_P_FaxSent:
					fieldColumnDescription = Res.GetString("F15E7E16-6072-4870-AB15-9A808E77D442", "FAX Sent?");
					break;
				default:
					fieldColumnDescription = base.GetFieldColumnDescription(factory, fieldColumn);
					break;
			}
			return fieldColumnDescription;
		}
	}
}
