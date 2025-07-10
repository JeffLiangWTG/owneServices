using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[DependentBusinessObject(typeof(CommonContainerLoadList), "LoadListLines")]
	public class ContainerLoadListLine : AutoContainerLoadListLine, IExternalRequestGenerationProvider, IWorkflowProvider
	{
		public ContainerLoadListLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ForwardingContainer Container => Factory.Load<ForwardingContainer>(CLL_JC_Container);

		public CommonContainerLoadList LoadListHeader => Factory.Load<CommonContainerLoadList>(CLL_CLH_LoadListHeader);

		public JobSupplierBookingLine SupplierBookingLine => Factory.Load<JobSupplierBookingLine>(CLL_JSL_BookingLine);

		public override ZDecimal CLL_PackedQuantity
		{
			get => base.CLL_PackedQuantity;
			set
			{
				if (base.CLL_PackedQuantity != value)
				{
					var orginalActiveQuantity = ActiveQuantity();
					var originalPackedQuantity = PackedQuantity();

					base.CLL_PackedQuantity = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActiveQuantity != ActiveQuantity())
					{
						SupplierBookingLine.JSL_RemainingQuantityToBePacked -= ActiveQuantity() - orginalActiveQuantity;
					}

					if (UpdateQtyPacked(LoadListHeader.CLH_Status))
					{
						SupplierBookingLine.OrderLine.JO_QtyPacked += PackedQuantity() - originalPackedQuantity;
					}
				}
			}
		}

		public override ZInt CLL_Packages
		{
			get => base.CLL_Packages;
			set
			{
				if (base.CLL_Packages != value)
				{
					var orginalActivePackages = ActivePackages();

					base.CLL_Packages = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActivePackages != ActivePackages())
					{
						SupplierBookingLine.JSL_RemainingPackagesToBePacked -= ActivePackages() - orginalActivePackages;
					}
				}
			}
		}

		public override ZDecimal CLL_Weight
		{
			get => base.CLL_Weight;
			set
			{
				if (base.CLL_Weight != value)
				{
					var orginalActiveWeight = ActiveWeight();
					base.CLL_Weight = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActiveWeight != ActiveWeight())
					{
						SupplierBookingLine.JSL_RemainingWeightToBePacked -= ActiveWeight() - orginalActiveWeight;
					}
				}
			}
		}

		public override ZDecimal CLL_Volume
		{
			get => base.CLL_Volume;
			set
			{
				if (base.CLL_Volume != value)
				{
					var orginalActiveVolume = ActiveVolume();
					base.CLL_Volume = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActiveVolume != ActiveVolume())
					{
						SupplierBookingLine.JSL_RemainingVolumeToBePacked -= ActiveVolume() - orginalActiveVolume;
					}
				}
			}
		}

		public override ZDecimal CLL_PlannedQuantity
		{
			get => base.CLL_PlannedQuantity;
			set
			{
				if (base.CLL_PlannedQuantity != value)
				{
					var orginalActiveQuantity = ActiveQuantity();

					base.CLL_PlannedQuantity = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActiveQuantity != ActiveQuantity())
					{
						SupplierBookingLine.JSL_RemainingQuantityToBePacked -= ActiveQuantity() - orginalActiveQuantity;
					}
				}
			}
		}

		public override ZInt CLL_PlannedPackages
		{
			get => base.CLL_PlannedPackages;
			set
			{
				if (base.CLL_PlannedPackages != value)
				{
					var orginalActivePackages = ActivePackages();

					base.CLL_PlannedPackages = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActivePackages != ActivePackages())
					{
						SupplierBookingLine.JSL_RemainingPackagesToBePacked -= ActivePackages() - orginalActivePackages;
					}
				}
			}
		}

		public override ZDecimal CLL_PlannedWeight
		{
			get => base.CLL_PlannedWeight;
			set
			{
				if (base.CLL_PlannedWeight != value)
				{
					var orginalActiveWeight = ActiveWeight();
					base.CLL_PlannedWeight = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActiveWeight != ActiveWeight())
					{
						SupplierBookingLine.JSL_RemainingWeightToBePacked -= ActiveWeight() - orginalActiveWeight;
					}
				}
			}
		}

		public override ZDecimal CLL_PlannedVolume
		{
			get => base.CLL_PlannedVolume;
			set
			{
				if (base.CLL_PlannedVolume != value)
				{
					var orginalActiveVolume = ActiveVolume();
					base.CLL_PlannedVolume = value;

					if (LoadListHeader.ShouldUpdateToBePackedColumns() && orginalActiveVolume != ActiveVolume())
					{
						SupplierBookingLine.JSL_RemainingVolumeToBePacked -= ActiveVolume() - orginalActiveVolume;
					}
				}
			}
		}

		public override ZGuid CLL_JSL_BookingLine
		{
			get => base.CLL_JSL_BookingLine;
			set
			{
				if (base.CLL_JSL_BookingLine != value)
				{
					if (!CLL_JSL_BookingLine.IsEmpty && LoadListHeader.ShouldUpdateToBePackedColumns())
					{
						SupplierBookingLine.JSL_RemainingQuantityToBePacked += ActiveQuantity();
						SupplierBookingLine.JSL_RemainingPackagesToBePacked += ActivePackages();
						SupplierBookingLine.JSL_RemainingWeightToBePacked += ActiveWeight();
						SupplierBookingLine.JSL_RemainingVolumeToBePacked += ActiveVolume();
					}

					if (!CLL_JSL_BookingLine.IsEmpty && UpdateQtyPacked(LoadListHeader.CLH_Status))
					{
						SupplierBookingLine.OrderLine.JO_QtyPacked -= CLL_PackedQuantity;
					}

					base.CLL_JSL_BookingLine = value;

					if (!CLL_JSL_BookingLine.IsEmpty && UpdateQtyPacked(LoadListHeader.CLH_Status))
					{
						SupplierBookingLine.OrderLine.JO_QtyPacked += CLL_PackedQuantity;
					}

					if (LoadListHeader.ShouldUpdateToBePackedColumns())
					{
						SupplierBookingLine.JSL_RemainingQuantityToBePacked -= ActiveQuantity();
						SupplierBookingLine.JSL_RemainingPackagesToBePacked -= ActivePackages();
						SupplierBookingLine.JSL_RemainingWeightToBePacked -= ActiveWeight();
						SupplierBookingLine.JSL_RemainingVolumeToBePacked -= ActiveVolume();
					}
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				WorkflowItems.RemoveAndDeleteAll();
				base.Delete();
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();

			if (UpdateQtyPacked(LoadListHeader.CLH_Status))
			{
				SupplierBookingLine.OrderLine.JO_QtyPacked -= PackedQuantity();
			}

			if (LoadListHeader.ShouldUpdateToBePackedColumns())
			{
				SupplierBookingLine.JSL_RemainingQuantityToBePacked += ActiveQuantity();
				SupplierBookingLine.JSL_RemainingPackagesToBePacked += ActivePackages();
				SupplierBookingLine.JSL_RemainingWeightToBePacked += ActiveWeight();
				SupplierBookingLine.JSL_RemainingVolumeToBePacked += ActiveVolume();
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public ZDecimal ActiveQuantity(string status = null) => UsePlanned(status) ? WrapPlanned(CLL_PlannedQuantity) : WrapPacked(CLL_PackedQuantity);
		public ZInt ActivePackages(string status = null) => UsePlanned(status) ? WrapPlanned(CLL_PlannedPackages) : WrapPacked(CLL_Packages);
		public ZDecimal ActiveWeight(string status = null) => UsePlanned(status) ? WrapPlanned(CLL_PlannedWeight) : WrapPacked(CLL_Weight);
		public ZDecimal ActiveVolume(string status = null) => UsePlanned(status) ? WrapPlanned(CLL_PlannedVolume) : WrapPacked(CLL_Volume);
		public ZDecimal PackedQuantity() => WrapPacked(CLL_PackedQuantity);

		bool UsePlanned(string loadListStatus) => string.IsNullOrEmpty(loadListStatus)
			? CommonContainerLoadList.CheckUsePlanned(LoadListHeader.CLH_LoadMode, LoadListHeader.CLH_Status)
			: CommonContainerLoadList.CheckUsePlanned(LoadListHeader.CLH_LoadMode, loadListStatus);

		bool UpdateQtyPacked(string loadListStatus) => CommonContainerLoadList.IsConvertedOrShipped(loadListStatus);

		T WrapPlanned<T>(T value) => CLL_JC_Container.IsEmpty ? value : default(T);

		T WrapPacked<T>(T value) => !CLL_JC_Container.IsEmpty ? value : default(T);

		#region Workflow Provider

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

		ProcessTaskCollection IWorkflowProvider.WorkflowItems => WorkflowItems;

		[ChildEditable]
		public ContainerLoadListLineProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ContainerLoadListLineProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ContainerLoadListLineProcessTaskCollection workflowItems;

		public ZString WorkflowType =>
			CLL_LoadMode == CommonContainerLoadListLoadModeList.Codes.CY
				? WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode
				: WorkflowDescriptors.CargoLoadPlanLineWorkflowDescriptorCode;

		public IWorkflowInformationProvider GetWorkflowInformationProvider() => null;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			ZString GetTransportMode() =>
				CLL_LoadMode == CommonContainerLoadListLoadModeList.Codes.CY
					? SupplierBookingLine.SupplierBooking.JSB_TransportMode
					: LoadListHeader.CLH_PlannedTransportMode;

			var result = new ColumnValueRanker();

			result.Add(ProcessTaskTemplateSchema.P0_OH_Client, SupplierBookingLine.GetClientsInTemplateSelectionOrder());
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, GetTransportMode(), ZString.Empty);

			return result;
		}

		#endregion

		#region IExternalRequestGenerationProvider

		public ZString GetRequestJobID() => LoadListHeader?.CLH_LoadListId ?? ZString.Empty;

		public ZString GetRequestTypeCode() => CLL_LoadMode == Constants.ContainerLoadListHeaderLoadMode.ContainerYard ? ExternalRequestTypes.Codes.ContainerLoadListLine : ExternalRequestTypes.Codes.ContainerLoadPlanLine;

		public (ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfo(ZString addressType)
		{
			return CLL_LoadMode == Constants.ContainerLoadListHeaderLoadMode.ContainerYard ? GetRequestSupportedAddressInfoForContainerLoadListLine(addressType) : GetRequestSupportedAddressInfoForContainerLoadPlanLine(addressType);
		}

		(ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfoForContainerLoadPlanLine(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.ControllingCustomer => (LoadListHeader?.ControllingCustomerAddress?.OrganisationPK ?? ZGuid.Empty, LoadListHeader?.ControllingCustomerAddress?.ContactPK ?? ZGuid.Empty),
			DocAddressTypes.Codes.SupplierDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScripForGetLoadPlanLineRequestSupplierAddressInfo, PK), OrgAddressSchema.Constants.OA_OH, JobDocAddressSchema.Constants.E2_Contact),
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScripForGetLoadPlanLineRequestManufacturerAddressInfo, PK), OrgAddressSchema.Constants.OA_OH, JobDocAddressSchema.Constants.E2_Contact),
			DocAddressTypes.Codes.BuyerDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScriptForGetLoadPlanLineRequestBuyerDocumentaryAddress, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobOrderHeaderSchema.Constants.JD_OC_BuyerContact, false),
			_ => (ZGuid.Empty, ZGuid.Empty)
		};

		string ScripForGetLoadPlanLineRequestSupplierAddressInfo =>
			"""
				SELECT TOP 1 OA_OH, E2_Contact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobSupplierBooking ON JSB_PK = JSL_JSB_Booking
				INNER JOIN dbo.JobDocAddress ON JSB_PK = E2_ParentID AND E2_AddressOverride = 0 AND E2_AddressSequence = 0 AND E2_AddressType = 'SUD'
				INNER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
				WHERE CLL_PK = '{0}'
			""";

		string ScripForGetLoadPlanLineRequestManufacturerAddressInfo =>
			"""
				SELECT TOP 1 OA_OH, E2_Contact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobDocAddress ON JSL_PK = E2_ParentID AND E2_AddressOverride = 0 AND E2_AddressSequence = 0 AND E2_AddressType = 'MAN'
				INNER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
				WHERE CLL_PK = '{0}'
			""";

		string ScriptForGetLoadPlanLineRequestBuyerDocumentaryAddress => 
			"""
				SELECT TOP 1 OA_OH, JD_OC_BuyerContact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobOrderLine ON JSL_JO_OrderLine = JO_PK
				INNER JOIN dbo.JobOrderHeader ON JO_JD = JD_PK
				INNER JOIN dbo.OrgAddress ON OA_PK = JD_OA_BuyerAddress
				WHERE CLL_PK = '{0}'
				
			""";

		(ZGuid OrginzationPK, ZGuid ContactPK) GetRequestSupportedAddressInfoForContainerLoadListLine(ZString addressType) => addressType.ToString() switch
		{
			DocAddressTypes.Codes.LoadListParty or DocAddressTypes.Codes.SupplierDocumentaryAddress or DocAddressTypes.Codes.ControllingCustomer => (LoadListHeader as CYContainerLoadList)?.GetRequestSupportedAddressInfo(addressType) ?? (ZGuid.Empty, ZGuid.Empty),
			DocAddressTypes.Codes.Manufacturer => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScripForGetLoadListLineRequestManufacturerAddressInfo, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobDocAddressSchema.Constants.E2_Contact),
			DocAddressTypes.Codes.BuyerDocumentaryAddress => RequestGenerationProviderHelper.GetRequestSupportedAddressInfo(Factory, string.Format(ScriptForGetLoadListLineRequestBuyerDocumentaryAddress, PK.ToString()), OrgAddressSchema.Constants.OA_OH, JobOrderHeaderSchema.Constants.JD_OC_BuyerContact, false),
			_ => (ZGuid.Empty, ZGuid.Empty)
		};

		string ScripForGetLoadListLineRequestManufacturerAddressInfo =>
			"""
				SELECT TOP 1 OA_OH, E2_Contact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobDocAddress ON JSL_PK = E2_ParentID AND E2_AddressOverride = 0 AND E2_AddressSequence = 0 AND E2_AddressType = 'MAN'
				INNER JOIN dbo.OrgAddress ON OA_PK = E2_OA_Address
				WHERE CLL_PK = '{0}'
			""";

		string ScriptForGetLoadListLineRequestBuyerDocumentaryAddress =>
			"""
				SELECT TOP 1 OA_OH, JD_OC_BuyerContact
				FROM dbo.ContainerLoadListLine
				INNER JOIN dbo.JobSupplierBookingLine ON JSL_PK = CLL_JSL_BookingLine
				INNER JOIN dbo.JobOrderLine ON JSL_JO_OrderLine = JO_PK
				INNER JOIN dbo.JobOrderHeader ON JO_JD = JD_PK
				INNER JOIN dbo.OrgAddress ON OA_PK = JD_OA_BuyerAddress
				WHERE CLL_PK = '{0}'
			""";

		#endregion
	}
}
