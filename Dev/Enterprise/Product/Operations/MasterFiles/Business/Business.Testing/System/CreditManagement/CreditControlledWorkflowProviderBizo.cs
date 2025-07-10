using System;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class CreditControlledWorkflowProviderBizo : CreditControlledBizo, IWorkflowProvider, IDocumentSupportable
	{
		public CreditControlledWorkflowProviderBizo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CreditControlledWorkflowProviderBizoProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		public CargoWise.Integration.IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		public ZString WorkflowType
		{
			get { return "DUM"; }
		}

		public JobDocAddress ConsigneeDocumentaryAddress
		{
			get
			{
				return consigneeDocumentaryAddress ?? (consigneeDocumentaryAddress = Factory.New<JobDocAddress>());
			}
		}
		JobDocAddress consigneeDocumentaryAddress;

		class CreditControlledWorkflowProviderBizoProcessTaskCollection : ProcessTaskCollection
		{
			public CreditControlledWorkflowProviderBizoProcessTaskCollection(CreditControlledWorkflowProviderBizo parent)
				: base(parent)
			{ }

			public new CreditControlledWorkflowProviderBizoProcessTask this[int index]
			{
				get { return (CreditControlledWorkflowProviderBizoProcessTask)Elements[index]; }
			}

			public new CreditControlledWorkflowProviderBizoProcessTask AddNew()
			{
				return (CreditControlledWorkflowProviderBizoProcessTask)base.AddNew();
			}
		}

		class CreditControlledWorkflowProviderBizoProcessTask : ProcessTask
		{
			public CreditControlledWorkflowProviderBizoProcessTask(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected internal override Type ParentType
			{
				get { return typeof(CreditControlledWorkflowProviderBizo); }
			}
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new CreditControlledWorkflowProviderBizoDocumentSupporter(this)); }
			set
			{
				documentSupporter = value;
			}
		}
		DocumentSupporter documentSupporter;

		class CreditControlledWorkflowProviderBizoDocumentSupporter : DocumentSupporter
		{
			public CreditControlledWorkflowProviderBizoDocumentSupporter(CreditControlledWorkflowProviderBizo parentBusinessObject)
				: base(parentBusinessObject)
			{ }

			public override BusinessContext BusinessContext
			{
				get { return BusinessContext.Test; }
			}

			protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return Array.Empty<DocumentEngineCore.DocWrappers.DocumentWrapper>();
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return Array.Empty<Core.Constants.DataContext>();
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { return null; }
			}

			CreditControlledWorkflowProviderBizo Bizo
			{
				get { return (CreditControlledWorkflowProviderBizo)BusinessObject; }
			}

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
			{
				return new OrgHeaderContact(Bizo.ConsigneeDocumentaryAddress.Organisation, null);
			}
		}
	}
}
