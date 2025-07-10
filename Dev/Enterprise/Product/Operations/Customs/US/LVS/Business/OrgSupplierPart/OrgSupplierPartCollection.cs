using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using ClassificationTypeList = Enterprise.Customs.US.Business.ClassificationTypeList;
using OrgSupplierPart = Enterprise.Customs.US.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.LVS.Business
{
	public class OrgSupplierPartCollection : US.Business.OrgSupplierPartCollection
	{
		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, CusUSLVItem item)
			: base(factory)
		{
			fItem = item;
		}

		readonly CusUSLVItem fItem;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var part = (OrgSupplierPart)child;
			if (fItem != null)
			{
				AddAdditionalLineDetails(part, fItem);
				AddPivotWithAdditionalLineDetails(part, fItem);
			}
		}

		void AddAdditionalLineDetails(OrgSupplierPart part, CusUSLVItem item)
		{
			if (item != null)
			{
				part.OP_Desc = item.ULI_GoodsDescription;
			}
		}

		void AddPivotWithAdditionalLineDetails(OrgSupplierPart part, CusUSLVItem item)
		{
			if (item != null)
			{
				var pivot = part.PivotsForBinding.AddNew();
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				pivot.CI_TariffNum = item.ULI_Tariff.Left(CusClassPartPivot.Schema.CI_TariffNumMaxLength);
				pivot.CD_UC_NKCountryOfOrigin = item.ULI_RN_NKCountryOfOrigin;
				pivot.CD_ADDApplicable = item.ULI_AntiDumping;
				pivot.CD_CVDApplicable = item.ULI_Countervailing;
				AddPGAInfo(pivot, item);
			}
		}

		void AddPGAInfo(CusClassPartPivot pivot, CusUSLVItem item)
		{
			pivot.CD_ACEFDAIndicator = item.ACEFDAWrapper.Indicator;
			pivot.CD_ACEFDADisclaimReason = item.ACEFDAWrapper.DisclaimReason;

			pivot.CD_AMSIndicator = item.AMSWrapper.Indicator;
			pivot.CD_AMSDisclaimReason = item.AMSWrapper.DisclaimReason;

			pivot.CD_NOPIndicator = item.NOPWrapper.Indicator;
			pivot.CD_NOPDisclaimReason = item.NOPWrapper.DisclaimReason;

			pivot.CD_APHISIndicator = item.APHISWrapper.Indicator;
			pivot.CD_APHISDisclaimReason = item.APHISWrapper.DisclaimReason;

			pivot.CD_CPSCIndicator = item.CPSCWrapper.Indicator;
			pivot.CD_CPSCDisclaimReason = item.CPSCWrapper.DisclaimReason;

			pivot.CD_DEAIndicator = item.DEAWrapper.Indicator;
			pivot.CD_DEADisclaimReason = item.DEAWrapper.DisclaimReason;

			pivot.CD_FWSIndicator = item.FWSWrapper.Indicator;
			pivot.CD_FWSDisclaimReason = item.FWSWrapper.DisclaimReason;

			pivot.CD_LaceyActIndicator = item.LaceyActWrapper.Indicator;
			pivot.CD_LaceyActDisclaimReason = item.LaceyActWrapper.DisclaimReason;

			pivot.CD_NHTSAIndicator = item.NHTSAWrapper.Indicator;
			pivot.CD_NHTSADisclaimReason = item.NHTSAWrapper.DisclaimReason;

			pivot.CD_NMFS370Indicator = item.NMFS370Wrapper.Indicator;
			pivot.CD_NMFS370DisclaimReason = item.NMFS370Wrapper.DisclaimReason;

			pivot.CD_NMFSAMRIndicator = item.NMFSAMRWrapper.Indicator;
			pivot.CD_NMFSAMRDisclaimReason = item.NMFSAMRWrapper.DisclaimReason;

			pivot.CD_NMFSHMSIndicator = item.NMFSHMSWrapper.Indicator;
			pivot.CD_NMFSHMSDisclaimReason = item.NMFSHMSWrapper.DisclaimReason;

			pivot.CD_ODSIndicator = item.ODSWrapper.Indicator;
			pivot.CD_ODSDisclaimReason = item.ODSWrapper.DisclaimReason;

			pivot.CD_OMCIndicator = item.OMCWrapper.Indicator;
			pivot.CD_OMCDisclaimReason = item.OMCWrapper.DisclaimReason;

			pivot.CD_PSTIndicator = item.PSTWrapper.Indicator;
			pivot.CD_PSTDisclaimReason = item.PSTWrapper.DisclaimReason;

			pivot.CD_TSCAIndicator = item.TSCAWrapper.Indicator;
			pivot.CD_TSCADisclaimReason = item.TSCAWrapper.DisclaimReason;

			pivot.CD_TTBIndicator = item.TTBWrapper.Indicator;
			pivot.CD_TTBDisclaimReason = item.TTBWrapper.DisclaimReason;

			pivot.CD_VNEIndicator = item.VNEWrapper.Indicator;
			pivot.CD_VNEDisclaimReason = item.VNEWrapper.DisclaimReason;
		}

		protected override (OrgHeader Org, string Relationship)? GetNewRelationship()
		{
			(OrgHeader Org, string Relationship)? result = null;

			if (fItem != null && fItem.Consignment is CusUSLVConsignment consignment)
			{
				var helper = new DeclarationForProductCreationHelper(fItem);
				var option = helper.CalculateDefaultOptionForCreateProduct();
				if (option == ProductRelationDefaultOption.OptionForImporter)
				{
					result = (EffectiveOwner(consignment.Consignee.Header), OrgPartRelation.RelationshipTypes.Owner);
				}
				else if (option == ProductRelationDefaultOption.OptionForSupplier)
				{
					result = (EffectiveSupplier(consignment.Seller.Header), OrgPartRelation.RelationshipTypes.Supplier);
				}
			}

			return result;
		}
	}
}
