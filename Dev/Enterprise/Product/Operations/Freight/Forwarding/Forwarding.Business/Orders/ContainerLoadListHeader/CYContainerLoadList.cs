using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[UniversalDataContext(DataContextType.ContainerLoadList)]
	public class CYContainerLoadList : CommonContainerLoadList
		, ICYContainerLoadList
		, IWorkflowProvider
		, ICustomFieldProvider
		, IDocManagerSupport
		, IExternalRequestGenerationProvider
		, IDocumentSupportable
	{
		public CYContainerLoadList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			if (this.CLH_LoadMode == ZString.Empty)
			{
				this.CLH_LoadMode = Constants.ContainerLoadListHeaderLoadMode.ContainerYard;
			}
		}

		#region IDocManagerSupport

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.ContainerLoadList));

		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocManagerSupport

		public DocumentSupporter DocumentSupporter => new CYContainerLoadListDocumentSupporter(this);

		#endregion

		#region Workflow
		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			return Booking?.GetTemplateSelectionCriteria() ?? new ColumnValueRanker();
		}

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		ZString IWorkflowProviderCore.WorkflowType => WorkflowDescriptors.ContainerLoadListWorkflowDescriptorCode;

		[ChildEditable(true)]
		public ContainerLoadListProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ContainerLoadListProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ContainerLoadListProcessTaskCollection workflowItems;

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
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

		#endregion

		#region ICustomFieldProvider

		public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
		{
			var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
			return new CustomBusinessObject(Factory, this, properties);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted)
			{
				WorkflowItems.RemoveAndDeleteAll();
				LoadListLines.DeleteAll();
				base.Delete();
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransaction

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => CLH_LoadListId;

		public ZString GetRequestTypeCode() => ExternalRequestTypes.Codes.ContainerLoadList;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.LoadListParty => (CLH_OH_LoadListParty, ZGuid.Empty),
			DocAddressTypes.Codes.SupplierDocumentaryAddress or DocAddressTypes.Codes.ControllingCustomer => Booking?.GetRequestSupportedAddressInfo(addressType) ?? (ZGuid.Empty, ZGuid.Empty),
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScripForGetRequestManufacturerAddressInfo, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobDocAddressSchema.Constants.E2_Contact),
			DocAddressTypes.Codes.BuyerDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScriptForGetRequestBuyerDocumentaryAddress, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobOrderHeaderSchema.Constants.JD_OC_BuyerContact, false),
			_ => (ZGuid.Empty, ZGuid.Empty)
		};

		string ScripForGetRequestManufacturerAddressInfo =>
			"""
				SELECT DISTINCT OA_OH, E2_Contact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobDocAddress ON JSL_PK = E2_ParentID AND E2_AddressOverride = 0 AND E2_AddressSequence = 0 AND E2_OA_Address IS NOT NULL AND E2_AddressType = 'MAN'
				INNER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
				WHERE CLL_CLH_LoadListHeader = '{0}'
			""";

		string ScriptForGetRequestBuyerDocumentaryAddress =>
			"""
				SELECT DISTINCT OA_OH, JD_OC_BuyerContact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobOrderLine ON JSL_JO_OrderLine = JO_PK
				INNER JOIN dbo.JobOrderHeader ON JO_JD = JD_PK
				INNER JOIN dbo.OrgAddress ON OA_PK = JD_OA_BuyerAddress
				WHERE CLL_CLH_LoadListHeader = '{0}'
			""";

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			CLH_LoadMode =  CommonContainerLoadListLoadModeList.Codes.CY;
			CLH_PlannedTransportMode = "SEA";
		}

#endif
	}
}
