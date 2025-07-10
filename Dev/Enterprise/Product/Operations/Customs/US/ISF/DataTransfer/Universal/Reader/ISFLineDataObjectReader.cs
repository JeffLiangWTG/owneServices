using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader
{
	class ISFLineDataObjectReader : DataObjectReader<CommercialInvoiceLine, CusISFLine>
	{
		public ISFLineDataObjectReader(CommercialInvoiceLine dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, CusISFHeader header, ISFHeaderDataObjectReader headerReader = null)
			: base(dataObject, logger, factory)
		{
			this.header = header;
			this.headerReader = headerReader;
			this.addressHelper = new OrganizationAddressReaderHelper<CusISFHeader>(logger, factory, header, DocAddressType.Manufacturer);
		}
		readonly CusISFHeader header;
		readonly ISFHeaderDataObjectReader headerReader;
		readonly OrganizationAddressReaderHelper<CusISFHeader> addressHelper;

		protected override CusISFLine GetNewBusinessObject()
		{
			return header.Lines.AddNew();
		}

		protected override CusISFLine GetExistingBusinessObject()
		{
			return null;
		}

		protected override void PopulateBusinessObject(CusISFLine targetBO)
		{
			var lineRow = GetColumnIndexer(targetBO);

			var addressPK = addressHelper.GetOrCreateDocAddress(dataObject.OrganizationAddressCollection, header.ManufacturerAddresses, FillManufacturerAddress, true);
			if (addressPK != ZGuid.Empty)
			{
				SetValue(lineRow, CusISFLineSchema.BL_ManufacturerDocAddressPK, addressPK);
			}
			SetValue(lineRow, CusISFLineSchema.BL_TextProductCode, dataObject.PartNo);
			SetValue(lineRow, CusISFLineSchema.BL_RN_NKGoodsOrigin, dataObject.CountryOfOrigin);
			SetValue(lineRow, CusISFLineSchema.BL_HarmonisedNum, dataObject.HarmonisedCode);
			FillAddInfos(targetBO);
		}

		JobDocAddress FillManufacturerAddress(BusinessObject bObject, DocAddressType docAddressType, OrganizationAddress orgAddressDataObject)
		{
			return headerReader?.FillManufacturerOrganization(header, orgAddressDataObject, header.ManufacturerAddresses?.Count ?? ZInt.Zero);
		}

		void FillAddInfos(CusISFLine targetBO)
		{
			if (dataObject.AddInfoCollection != null)
			{
				var customAttribute1 = dataObject.AddInfoCollection.GetZStringValue(ISFConstants.CustomizedFieldConstants.CustomAttribOne).GetValueOrDefault();
				targetBO.CustomAttribute1 = customAttribute1;

				var customAttribute2 = dataObject.AddInfoCollection.GetZStringValue(ISFConstants.CustomizedFieldConstants.CustomAttribTwo).GetValueOrDefault();
				targetBO.CustomAttribute2 = customAttribute2;
			}
		}
	}
}

#region Implementation
#endregion
