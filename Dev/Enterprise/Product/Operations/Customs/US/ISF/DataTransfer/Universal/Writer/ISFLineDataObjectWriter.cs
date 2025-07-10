using System.Collections.Generic;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Writer
{
	class ISFLineDataObjectWriter : DataObjectWriter<CusISFLine, CommercialInvoiceLine>
	{
		public ISFLineDataObjectWriter(IDataWritingManager writeManager, ISFDataObjectHelper helper)
			: base(writeManager)
		{
			this.helper = helper;
		}
		readonly ISFDataObjectHelper helper;

		protected override CommercialInvoiceLine PopulateDataObject(CusISFLine lineBO)
		{
			var invoiceLineData = new CommercialInvoiceLine();
			invoiceLineData.LineNo = helper.LineNo;
			invoiceLineData.PartNo = lineBO.BL_TextProductCode;
			invoiceLineData.CountryOfOrigin = ListHelper.GetWithName<Country>(lineBO.BL_RN_NKGoodsOrigin, lineBO.Lookups.GoodsOrigins);
			invoiceLineData.HarmonisedCode = lineBO.BL_FormattedHarmonisedNum;

			var addInfoCollection = new List<AddInfo>();
			if (!lineBO.CustomAttribute1.IsEmpty)
			{
				addInfoCollection.Add(AddInfo.New(ISFConstants.CustomizedFieldConstants.CustomAttribOne, lineBO.CustomAttribute1));
			}
			if (!lineBO.CustomAttribute2.IsEmpty)
			{
				addInfoCollection.Add(AddInfo.New(ISFConstants.CustomizedFieldConstants.CustomAttribTwo, lineBO.CustomAttribute2));
			}
			invoiceLineData.AddInfoCollection = addInfoCollection.Count == 0 ? null : addInfoCollection;

			if (lineBO.ManufacturerDocAddress != null)
			{
				invoiceLineData.OrganizationAddressCollection = ProcessCollection(lineBO.ManufacturerDocAddress, new JobDocAddressDataObjectWriter(writeManager));
			}
			return invoiceLineData;
		}
	}
}
