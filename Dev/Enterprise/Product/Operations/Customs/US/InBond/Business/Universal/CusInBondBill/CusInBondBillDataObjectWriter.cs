using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class CusInBondBillDataObjectWriter : DataTransfer.Universal.CusInBondBillDataObjectWriter
	{
		public CusInBondBillDataObjectWriter(IDataWritingManager manager, InBondDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected new InBondDataObjectWriterHelper Helper
		{
			get { return (InBondDataObjectWriterHelper)base.Helper; }
		}

		protected override void PopulateInBondSpecificData(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			base.PopulateInBondSpecificData(billBO, billData);
			var inBondBillBO = (CusInBondBill)billBO;
			var isAir = inBondBillBO.IsAir;
			if (isAir)
			{
				billData.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };
				billData.BillNumber = inBondBillBO.B0_HouseBillNumber;
				billData.ParentBillNumber = inBondBillBO.B0_MasterBillNumber;
			}
			else
			{
				billData.BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master, Description = WayBillTypeList.Descriptions.Master };
				billData.BillNumber = inBondBillBO.B0_MasterBillNumber;
				PopulateAddInfosData(inBondBillBO, billData);
			}
		}

		protected override List<OrganizationAddress> PopulateOrganizations(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			var organizationCollection = base.PopulateOrganizations(billBO, billData) ?? new List<OrganizationAddress>();
			var inBondBillBO = (CusInBondBill)billBO;
			organizationCollection = ProcessCollection(inBondBillBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager));
			return organizationCollection;
		}

		protected override List<UniversalCustoms.CustomsReference> PopulateCustomsReferenceData(Customs.Business.CusInBondBill billBO, AdditionalBill billData)
		{
			var customsReferenceCollection = base.PopulateCustomsReferenceData(billBO, billData) ?? new List<UniversalCustoms.CustomsReference>();
			var inBondBillBO = (CusInBondBill)billBO;
			foreach (var additionalReferenceBO in Helper.Load<CusInbondBillAddRef>(inBondBillBO.AdditionalReferences.CompleteFilter).OrderBy(x => x.BR_Qualifier + x.BR_ReferenceNum))
			{
				customsReferenceCollection.Add(new UniversalCustoms.CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = Constants.AdditionalReference.Type, Description = Constants.AdditionalReference.TypeDescription },
					SubType = ListHelper.GetWithDescription<CodeDescriptionPair35Char>(additionalReferenceBO.BR_Qualifier, additionalReferenceBO.Lookups.AdditionalReferenceList),
					Reference = additionalReferenceBO.BR_ReferenceNum
				});
			}
			return customsReferenceCollection;
		}

		void PopulateAddInfosData(CusInBondBill billBO, AdditionalBill billData)
		{
			var list = new List<AddInfo>();
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.ManifestUnit,
				Value = billBO.B0_ManifestUQ
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.Weight,
				Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(billBO.B0_Weight)
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.WeightUnit,
				Value = billBO.B0_WeightUQ
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.Volume,
				Value = Enterprise.Customs.Business.BaseAddInfo.GetStringRepresentation(billBO.B0_Volume)
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.VolumeUnit,
				Value = billBO.B0_VolumeUQ
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.PortOfLadingScheduleK,
				Value = billBO.B0_PortOfLadingKCode
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.PlaceOfReceiptScheduleD,
				Value = billBO.B0_PlaceOfReceipt
			});
			list.Add(new AddInfo()
			{
				Key = Constants.Bill.AddInfo.IssuerCode,
				Value = billBO.B0_IssuerCode
			});
			billData.AddInfoCollection = list;
		}
	}
}
