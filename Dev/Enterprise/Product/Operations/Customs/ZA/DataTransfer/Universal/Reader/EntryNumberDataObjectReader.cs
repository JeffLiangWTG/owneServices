using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class EntryNumberDataObjectReader : DataObjectReader<EntryNumber, CusEntryNumber>
	{
		public EntryNumberDataObjectReader(EntryNumber entryNumberData, IXmlImportLogger logger, UniversalObjectFactory factory, BusinessObject entryNumberParent)
			: base(entryNumberData, logger, factory)
		{
			this.entryNumberData = Argument.NotNull(entryNumberData, "EntryNumberData");
			Argument.NotNull(logger, "Logger");
			Argument.NotNull(factory, "Factory");
			this.entryNumberParent = Argument.NotNull(entryNumberParent, "BusinessObject EntryNumberParent");
		}

		#region Implementation

		protected override void PopulateBusinessObject(CusEntryNumber entryNumberBO)
		{
			var entryNumberRow = GetColumnIndexer(entryNumberBO);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_ParentID, entryNumberParent.PK);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_ParentTable, entryNumberParent.TableName);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryNum, entryNumberData.Number);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryType, entryNumberData.Type);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_RN_NKCountryCode, entryNumberData.CountryOfIssue);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryIsSystemGenerated, entryNumberData.EntryIsSystemGenerated);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryLineReference, entryNumberData.EntryLineReference);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_EntryStatus, entryNumberData.EntryStatus);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_ExpiryDate, entryNumberData.ExpiryDate);
			SetValue(entryNumberRow, CusEntryNumSchema.CE_IssueDate, entryNumberData.IssueDate);
		}

		protected override CusEntryNumber GetExistingBusinessObject()
		{
			var type = entryNumberData.Type.GetCodeAsUpperCase();

			CusEntryNumber result = null;
			if (!type.IsEmpty)
			{
				var query = new ZQuery(CusEntryNumSchema.CE_ParentID, entryNumberParent.PK);
				query.AddToFilter(CusEntryNumSchema.CE_EntryType, type);
				query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica);
				query.FetchOnlyFromLocalCache = !entryNumberParent.IsInDatabase;
				result = factory.LoadTop1<CusEntryNumber>(query);
			}

			return result;
		}

		#endregion

		readonly EntryNumber entryNumberData;
		readonly BusinessObject entryNumberParent;
	}
}
