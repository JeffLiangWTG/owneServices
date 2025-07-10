using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Yard.Business
{
	[CodeAlive("New bizo for Container Yard project")]
	public class CYDDeliveryHeader : AutoCYDDeliveryHeader,
		IWorkflowProvider
	{
		public CYDDeliveryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("Yard")]
		public override ZGuid YDH_WW_Yard { get => base.YDH_WW_Yard; set => base.YDH_WW_Yard = value; }

		public WhsWarehouse Yard
		{
			get => Factory.Load<WhsWarehouse>(YDH_WW_Yard);
		}

		OrgAddress client;

		public OrgAddress Client => GetOrLoadAddress(ref client, "BKD");

		OrgAddress transportProvider;

		public OrgAddress TransportProvider => GetOrLoadAddress(ref transportProvider, "TRA");

		OrgAddress pickupLocation;

		public OrgAddress PickupLocation => GetOrLoadAddress(ref pickupLocation, "PUA");

		OrgAddress GetOrLoadAddress(ref OrgAddress addressField, string addressType)
		{
			if (addressField == null)
			{
				var orgAddressQuery = new ZDBOnlyQuery(typeof(OrgAddress));

				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_OA_Address);
				jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_ParentID, PK);
				jobDocAddressSubQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, addressType);

				orgAddressQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
				addressField = Factory.LoadTop1<OrgAddress>(orgAddressQuery);
			}
			return addressField;
		}

		#endregion

		#region RelatedCollections

		[ChildEditable]
		public CYDDeliveryCollection DeliveryCollection
		{
			get
			{
				if (deliveryCollection == null)
				{
					deliveryCollection = new CYDDeliveryCollection(this);
					deliveryCollection.Load();
					RegisterEditableChildObject(deliveryCollection);
				}

				return deliveryCollection;
			}
		}
		CYDDeliveryCollection deliveryCollection;

		#endregion RelatedCollections

		#region Implementation

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("4e5ac516-1aa5-4b10-b81c-26127ffba76e", "Bulk runs in");
			}
		}

		#endregion

		public override void Delete()
		{
			DeliveryCollection.RemoveAndDeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(YDH_JobNumberInfo, Env.NumberFountains.CYDDeliveryHeaderJobNumber);
			}

			base.OnSaving();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			YDH_WW_Yard = Factory.NewWithValidTestData<WhsWarehouse>().PK;
		}
#endif

		#endregion

		#region IWorkflowProvider Members

		[ChildEditable(true)]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}
				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new CYDDeliveryHeaderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.CYDDeliveryHeaderWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_WW, YDH_WW_Yard, null);
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		protected override AutologState AutoLoggingState => AutologState.NotLogged;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			base.OnFactorySavingBeforeTransactionCore();
		}
	}
}
