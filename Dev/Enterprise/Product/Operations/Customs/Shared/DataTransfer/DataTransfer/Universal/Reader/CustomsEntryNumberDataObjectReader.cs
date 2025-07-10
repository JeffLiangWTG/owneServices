using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryNumberDataObjectReader<T> : DataObjectReader<UniversalCustoms.EntryNumber, CusEntryNumber>
		where T : BusinessObject
	{
		public CustomsEntryNumberDataObjectReader(UniversalCustoms.EntryNumber entryNumberDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, T parent)
			: base(entryNumberDataObject, logger, helper.Factory)
		{
			this.parent = Argument.NotNull(parent, "parent");
			countryCode = helper.TargetCountryCode;
		}

		public CustomsEntryNumberDataObjectReader(UniversalCustoms.EntryNumber entryNumberDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, T parent, ZString countryCode)
			: base(entryNumberDataObject, logger, factory)
		{
			this.parent = Argument.NotNull(parent, "parent");
			this.countryCode = countryCode;
		}
		protected readonly T parent;
		protected readonly ZString countryCode;

		protected override CusEntryNumber GetExistingBusinessObject()
		{
			var type = dataObject.Type.GetCodeAsUpperCase();

			CusEntryNumber result = null;
			if (!type.IsEmpty)
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, parent.PK);
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, type);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
				query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
				result = factory.LoadTop1<CusEntryNumber>(query);
			}

			return result;
		}

		protected override void PopulateBusinessObject(CusEntryNumber entryNumber)
		{
			var entryNumberRow = GetColumnIndexer(entryNumber);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_ParentID, parent.PK);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_ParentTable, parent.TableName);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_RN_NKCountryCode, countryCode);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryType, dataObject.Type);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryNum, dataObject.Number);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryIsSystemGenerated, dataObject.EntryIsSystemGenerated);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryStatus, dataObject.EntryStatus);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_IssueDate, dataObject.IssueDate);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_ExpiryDate, dataObject.ExpiryDate);
		}
	}
}
