using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderWorkflowDescriptor : WorkflowDescriptor
	{
		#region Overrides of WorkflowDescriptor

		public override string Code
		{
			get { return WorkflowDescriptors.CusInBondHeaderWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("US.InBond|CusInBondHeaderWorkflowDescriptor|Description", "US InBond"); }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(CusInBondHeader); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.CusInBondHeader }; }
		}

		#region Names

		public override ZString Port1Name
		{
			get { return "Port Of Loading"; }
		}

		public override ZString Port2Name
		{
			get { return "Port Of Arrival"; }
		}

		public override ZString ClientName
		{
			get { return "Importer"; }
		}

		#endregion

		#region Requires

		public override bool RequiresPort1
		{
			get { return true; }
		}

		public override bool RequiresPort2
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

		public override bool RequiresClient
		{
			get { return true; }
		}

		#endregion

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override bool SupportsUniversalTemplates => false;

		protected override bool SupportsTaskLineTriggersCore => false;

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return
				MessageRecipientPartyType.Email |
				MessageRecipientPartyType.OrgProxy;
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.InBond; }
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				CusInBondMoveHeaderSchema.BM_CustomsStatus
			};
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			return fieldColumn.Name == CusInBondMoveHeader.Schema.BM_CustomsStatus ? "Message Status" : base.GetFieldColumnDescription(factory, fieldColumn);
		}

		#endregion
	}
}
