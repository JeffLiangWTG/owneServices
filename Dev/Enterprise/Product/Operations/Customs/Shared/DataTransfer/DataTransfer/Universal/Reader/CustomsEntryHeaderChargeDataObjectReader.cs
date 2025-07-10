using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryHeaderChargeDataObjectReader : DataObjectReader<UniversalCustoms.EntryHeaderCharge, CusEntryHeaderCharges>
	{
		public CustomsEntryHeaderChargeDataObjectReader(UniversalCustoms.EntryHeaderCharge entryHeaderChargeDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, CusEntryHeader entryHeader)
			: base(entryHeaderChargeDataObject, logger, helper.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			this.helper = helper;
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly UniversalDataObjectReaderHelper helper;

		protected override CusEntryHeaderCharges GetNewBusinessObject()
		{
			return (CusEntryHeaderCharges)factory.New(entryHeader.Charges.TypeOfElements);
		}

		protected override CusEntryHeaderCharges GetExistingBusinessObject()
		{
			var type = dataObject.Type.GetCodeAsUpperCase();
			CusEntryHeaderCharges result = null;
			if (!type.IsEmpty)
			{
				var query = new ZQuery(CusEntryHeaderChargesSchema.C1_CH, entryHeader.PK);
				query.AddToFilter(CusEntryHeaderChargesSchema.C1_ClusterKey, entryHeader.CH_ClusterKey);
				query.AddToFilter(CusEntryHeaderChargesSchema.C1_ChargeType, type);
				query.FetchOnlyFromLocalCache = !entryHeader.IsInDatabase;
				result = factory.LoadTop1<CusEntryHeaderCharges>(query);
			}
			return result;
		}

		protected override void PopulateBusinessObject(CusEntryHeaderCharges entryHeaderCharge)
		{
			var entryHeaderChargeRow = GetColumnIndexer(entryHeaderCharge);
			SetValue(entryHeaderChargeRow, CusEntryHeaderChargesSchema.C1_CH, entryHeader.PK);
			SetValue(entryHeaderChargeRow, CusEntryHeaderChargesSchema.C1_ClusterKey, entryHeader.CH_ClusterKey);
			SetValue(entryHeaderChargeRow, CusEntryHeaderChargesSchema.C1_ChargeType, dataObject.Type);
			SetValue(entryHeaderChargeRow, CusEntryHeaderChargesSchema.C1_ChargeAmount, dataObject.Amount);
		}
	}
}
