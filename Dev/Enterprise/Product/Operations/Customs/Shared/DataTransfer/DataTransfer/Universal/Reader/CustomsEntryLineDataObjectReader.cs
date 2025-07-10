using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryLineDataObjectReader : DataObjectReader<UniversalCustoms.EntryLine, CusEntryLine>
	{
		public CustomsEntryLineDataObjectReader(UniversalCustoms.EntryLine entryLineDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, CusEntryHeader entryHeader)
			: base(entryLineDataObject, logger, helper.Factory)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			this.helper = helper;
		}
		protected readonly CusEntryHeader entryHeader;
		protected readonly UniversalDataObjectReaderHelper helper;

		protected override CusEntryLine GetNewBusinessObject()
		{
			return (CusEntryLine)factory.New(entryHeader.MergedLines.TypeOfElements);
		}

		protected override CusEntryLine GetExistingBusinessObject()
		{
			return null; // should not match as we don't want JobComInvoiceLine to be linked to an incorrect CusEntryLine
		}

		protected override void PopulateBusinessObject(CusEntryLine entryLine)
		{
			var entryLineRow = GetColumnIndexer(entryLine);
			var addInfoManager = entryLine as IAddInfoManager;
			var isAddInfoSerialisationEnabled = addInfoManager != null && !IsDefaultingEnabled;
			try
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(false);
				}
				SetValue(entryLineRow, CusEntryLineSchema.CL_CH, entryHeader.PK);
				SetValue(entryLineRow, CusEntryLineSchema.CL_LineNumber, dataObject.LineNumber);
				SetValue(entryLineRow, CusEntryLineSchema.CL_AdValoremTariff, dataObject.HarmonisedCode);
				SetValue(entryLineRow, CusEntryLineSchema.CL_CustomsValue, dataObject.CustomsValue);
				SetValue(entryLineRow, CusEntryLineSchema.CL_DutyPercent, dataObject.DutyRatePercent);
				SetValue(entryLineRow, CusEntryLineSchema.CL_FlatAmount, dataObject.DutyRateFlatAmount);
				SetValue(entryLineRow, CusEntryLineSchema.CL_FlatAmountUQ, dataObject.DutyRateFlatAmountUnit);
				SetValue(entryLineRow, CusEntryLineSchema.CL_Description, dataObject.Description);
				SetValue(entryLineRow, CusEntryLineSchema.CL_CustomsPostedStatus, dataObject.CustomsStatus);

				if (addInfoManager != null)
				{
					FillAddInfos(addInfoManager, entryLineRow);
					if (IsDefaultingEnabled)
					{
						addInfoManager.UpdateRelatedPropertyInfo();
					}
				}
				FillCustomsReferences(entryLineRow, entryLine.IsInDatabase);
				FillEntryLineCharges(entryLine);
				UpdateEntryLineMapping(entryLine);
			}
			finally
			{
				if (isAddInfoSerialisationEnabled)
				{
					addInfoManager.UpdateAddInfoFromString(entryLineRow.GetValue(CusEntryLineSchema.CL_AddInfo));
					addInfoManager.SetUpdateFromAddInfoSerialisationFlag(true);
				}
			}
		}

		protected virtual void FillCustomsReferences(IColumnIndexer entryLineRow, bool entryLineIsInDatabase)
		{
			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryLineRow.GetValue(CusEntryLineSchema.PK), CusEntryLineSchema.Constants.Prefix, entryLineIsInDatabase, dataObject);
		}

		protected virtual void FillAddInfos(IAddInfoManager addInfoManager, IColumnIndexer entryLineRow)
		{
			AddInfoDataObjectReader.New(addInfoManager as BusinessObject, logger, helper, CusEntryLineSchema.CL_AddInfo).ReadIntoRow(addInfoManager, entryLineRow, dataObject, null);
		}

		protected virtual void FillEntryLineCharges(CusEntryLine entryLine)
		{
			if (dataObject.EntryLineChargeCollection != null)
			{
				foreach (var entryLineChargeDataObject in dataObject.EntryLineChargeCollection)
				{
					var entryLineCharge = new CustomsEntryLineChargeDataObjectReader(entryLineChargeDataObject, logger, helper, entryLine).ReadIntoBusinessObject();
				}
			}
		}

		protected virtual ZGuid GetEntryNumberParentID() => entryHeader.PK;

		void UpdateEntryLineMapping(CusEntryLine entryLine)
		{
			var cusEntryNumberQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, GetEntryNumberParentID());
			cusEntryNumberQuery.FetchOnlyFromLocalCache = !entryHeader.IsInDatabase;
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, entryHeader.CountryCode);

			foreach (var entryNumber in factory.Load<Common.CusEntryNumber>(cusEntryNumberQuery))
			{
				helper.AddEntryLineMap(entryLine, entryNumber.CE_EntryType, entryNumber.CE_EntryNum);
			}
		}
	}
}
