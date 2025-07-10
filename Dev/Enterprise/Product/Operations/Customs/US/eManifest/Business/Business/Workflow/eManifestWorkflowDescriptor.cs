using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Business
{
	class eManifestWorkflowDescriptor : WorkflowDescriptor
	{
		#region Overrides of WorkflowDescriptor

		public override string Code
		{
			get { return JobInvoicingConsumerTypes.eManifest.Code; }
		}

		public override IMultilingualString Description
		{
			get { return JobInvoicingConsumerTypes.eManifest.MultilingualDescription; }
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.US.eManifest; }
		}

		public override Type WorkflowProviderType
		{
			get { return typeof(Trip); }
		}

		public override BusinessContext[] DocumentBusinessContext
		{
			get { return new[] { BusinessContext.eManifest }; }
		}

		public override bool RequiresBranch
		{
			get { return true; }
		}

		public override bool SupportsEventTracking
		{
			get { return true; }
		}

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business)
		{
			return MessageRecipientPartyType.Email;
		}

		public override SchemaColumn[] GetWorkflowTriggerFieldColumns(IBusiness parent = null)
		{
			return new SchemaColumn[]
			{
				CusInBondHeaderSchema.BH_ReleaseStatus,
				CusInBondBillSchema.B0_ReleaseStatus
			};
		}

		public override string GetFieldColumnDescription(BusinessObjectFactory factory, SchemaColumn fieldColumn)
		{
			switch (fieldColumn.Name)
			{
				case Trip.Schema.BH_ReleaseStatus:
					return "e-Manifest Release Status";
				case Shipment.Schema.B0_ReleaseStatus:
					return "Shipment Release Status";
			}
			return base.GetFieldColumnDescription(factory, fieldColumn);
		}

		#endregion
	}
}
