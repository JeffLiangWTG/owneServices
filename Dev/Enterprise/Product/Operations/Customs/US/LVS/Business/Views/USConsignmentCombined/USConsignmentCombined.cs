using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class USConsignmentCombined : AutoUSConsignmentCombined
	{
		public USConsignmentCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public CusUSLVConsignment Consignment => IsConsignment ? Factory.Load<CusUSLVConsignment>(PK) : null;

		public JobDeclaration Declaration => IsDeclaration ? Factory.Load<JobDeclaration>(PK) : null;

		#region Property Overrides

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.Clients))]
		public override ZGuid UBV_OH_Client => base.UBV_OH_Client;

		public OrgHeader Importer => Factory.Load<OrgHeader>(UBV_OH_Importer);

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.Importers))]
		public override ZGuid UBV_OH_Importer => base.UBV_OH_Importer;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.Consignees))]
		public override ZGuid UBV_OH_Consignee => base.UBV_OH_Consignee;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.Sellers))]
		public override ZGuid UBV_OH_Seller => base.UBV_OH_Seller;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.TransportModes))]
		public override ZString UBV_TransportMode => base.UBV_TransportMode;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.MessageStatuses))]
		public override ZString UBV_MessageStatus => base.UBV_MessageStatus;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.Branches))]
		public override ZGuid UBV_GB => base.UBV_GB;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.PortOfDischargeList))]
		public override ZString UBV_PortOfDischarge => base.UBV_PortOfDischarge;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.PortOfLoadingList))]
		public override ZString UBV_PortOfLoading => base.UBV_PortOfLoading;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.PortOfEntryList))]
		public override ZString UBV_PortOfEntry => base.UBV_PortOfEntry;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.ReleaseStatuses))]
		public override ZString UBV_ReleaseStatus => base.UBV_ReleaseStatus;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.JobTypes))]
		public override ZString UBV_JobType => base.UBV_JobType;

		#endregion

		#region Non-Schema Properties

		public ZString CarrierSCAC => IsConsignment ? Consignment.Shipment?.ULH_CarrierSCAC ?? ZString.Empty : Declaration.US_UI_NKCarrierSCAC;

		public ZString ContactName => IsConsignment ? Consignment.Shipment?.ULH_ContactName ?? ZString.Empty : ZString.Empty;
		public ZString ContactPhone => IsConsignment ? Consignment.Shipment?.ULH_ContactPhone ?? ZString.Empty : ZString.Empty;

		public ZString ContainerMode => IsConsignment ? Consignment.Shipment?.ULH_ContainerMode ?? ZString.Empty : Declaration.JE_ContainerMode;

		public ZString EntryFilerCode => IsConsignment ? Consignment.Shipment?.ULH_EntryFilerCode ?? ZString.Empty : Declaration.US_EntryFilerCode;

		OrgCusCode IORCusCode => OrgHeaderWrapper.GetCustomsRelatedOrgCusCode(Importer, OrgMatchedCustomsRegNoType.EIN);

		public ZString IORReference => (IsConsignment ? Consignment.Shipment?.ULH_IORReference : IORCusCode?.OK_CustomsRegNo) ?? ZString.Empty;

		public ZString IORType => (IsConsignment ? Consignment.Shipment?.ULH_IORType : IORCusCode?.OK_CodeType) ?? ZString.Empty;

		public ZString MasterBillIssuerSCAC => IsConsignment ? Consignment.Shipment?.ULH_MasterBillIssuerSCAC ?? ZString.Empty : Declaration.JE_MasterBillIssuerSCAC;

		public ZString HouseBillIssuerSCAC => IsConsignment ? Consignment.ULB_HouseBillIssuerSCAC : Declaration.JE_HouseBillIssuerSCAC;

		public ZString OwnerReferenceNumber => IsConsignment ? Consignment.ULB_OwnerReferenceNumber : Declaration.JE_OwnerRef;

		ZString DeclarationContainerNumber
		{
			get
			{
				var result = ZString.Empty;
				if (IsDeclaration)
				{
					if (Declaration.CusContainers.Count == 1)
					{
						result = Declaration.CusContainers.Single().CO_ContainerNumber;
					}
					else if (Declaration.CusContainers.Count > 1)
					{
						result = Res.GetString("da222693-b923-45f6-8ae9-2bce4c59c5c8", "MULTIPLE");
					}
				}
				return result;
			}
		}

		public ZString EquipmentNumber => IsConsignment ? Consignment.ULB_EquipmentNumber : DeclarationContainerNumber;

		public ZDecimal GoodsValue => IsConsignment ? Consignment.ULB_GoodsValue : Declaration.CustomsValue;

		public ZString Currency => IsConsignment ? Consignment.ULB_Currency : (ZString)CurrencyCodes.UnitedStates;

		public ZInt NumberOfPacks => IsConsignment ? Consignment.ULB_NumberOfPacks : Declaration.JE_TotalNoOfPacks;

		public ZString PackType => IsConsignment ? Consignment.ULB_PackType : Declaration.JE_TotalNoOfPacksPackType;

		public ZBool NonAMSIndicator => IsConsignment ? Consignment.ULB_NonAMSIndicator : Declaration.US_NonAMS;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.ConsigneeAddresses))]
		public ZGuid ConsigneeAddress => IsConsignment ? Consignment.ULB_OA_Consignee : Declaration.JE_OA_ConsigneeAddress;

		[List(nameof(Lookups) + "." + nameof(USConsignmentCombinedLookups.SellerAddresses))]
		public ZGuid SellerAddress => IsConsignment ? Consignment.ULB_OA_Seller : Declaration.JE_OA_SellerAddress;

		public ZString RailReferenceNumber => IsConsignment ? Consignment.CE_RailReferenceNumber : Declaration.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN).FirstOrDefault();

		#endregion

		public ZBool IsConsignment => UBV_JobType == USConsignmentCombinedJobTypes.Codes.Consignment;

		public ZBool IsDeclaration => UBV_JobType == USConsignmentCombinedJobTypes.Codes.Declaration;

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => false;
	}
}
