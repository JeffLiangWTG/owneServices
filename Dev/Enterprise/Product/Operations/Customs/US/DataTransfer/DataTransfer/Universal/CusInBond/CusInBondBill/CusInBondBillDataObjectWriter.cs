using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CusInBondBillDataObjectWriter : DataObjectWriter<CusInBondBill, AdditionalBill>
	{
		public CusInBondBillDataObjectWriter(IDataWritingManager manager, InBondDataObjectWriterHelper helper)
			: base(manager)
		{
			this.writerHelper = Argument.NotNull(helper, "helper");
		}
		readonly InBondDataObjectWriterHelper writerHelper;

		protected InBondDataObjectWriterHelper Helper
		{
			get { return writerHelper; }
		}

		protected override AdditionalBill PopulateDataObject(CusInBondBill billBO)
		{
			var billData = new AdditionalBill(writeManager.WriterStrategy);
			billData.NoOfPacks = (ZDecimal)billBO.B0_ManifestQty;
			billData.SetCustomsReferenceCollection(() => PopulateCustomsReferenceData(billBO, billData));
			billData.SetAddInfoCollection(() => PopulateAddInfosData(billBO, billData));
			billData.SetOrganizationAddressCollection(() => PopulateOrganizations(billBO, billData));
			Helper.AllocateBillLink(billBO, billData);
			PopulateInBondSpecificData(billBO, billData);
			return billData;
		}

		protected virtual void PopulateInBondSpecificData(CusInBondBill billBO, AdditionalBill billData)
		{
		}

		protected virtual List<OrganizationAddress> PopulateOrganizations(CusInBondBill billBO, AdditionalBill billData)
		{
			return billData.OrganizationAddressCollection ?? new List<OrganizationAddress>();
		}

		protected virtual List<CustomsReference> PopulateCustomsReferenceData(CusInBondBill billBO, AdditionalBill billData)
		{
			return billData.CustomsReferenceCollection ?? new List<CustomsReference>();
		}

		protected virtual List<AddInfo> PopulateAddInfosData(CusInBondBill billBO, AdditionalBill billData)
		{
			return billData.AddInfoCollection ?? new List<AddInfo>();
		}
	}
}
