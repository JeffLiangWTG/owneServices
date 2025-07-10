using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class AsycudaBillDataObjectReader : ShipmentDataObjectReader<AsycudaBill>
	{
		public AsycudaBillDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, AsycudaManifestHeader header, UniversalDataObjectReaderHelper helper)
			: base(dataObject, logger, factory)
		{
			billData = Argument.NotNull(dataObject, "ShipmentDataObject");
			headerBO = Argument.NotNull(header, "Header");
			this.helper = Argument.NotNull(helper, "Helper");
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.AsycudaBill; }
		}

		protected override AsycudaBill GetNewBusinessObject()
		{
			return headerBO.Bills.AddNew();
		}

		protected override AsycudaBill GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			AsycudaBill result = null;
			if (billData.WayBillNumber.HasValue)
			{
				var bills = GetBills();
				result = bills.Where(x => x.ABL_BillNumber == billData.WayBillNumber.Value).OrderBy(x => x.ABL_SystemCreateTimeUtc).FirstOrDefault();
			}
			return result;
		}

		AsycudaBill[] GetBills()
		{
			var query = new ZQuery(AsycudaBillSchema.ABL_AMA, headerBO.PK);
			query.AddToFilter(AsycudaBillSchema.ABL_BolType, SQLComparisonOperator.NotEqual, AsycudaBill.ChildBolCode);
			return factory.Load<AsycudaBill>(query);
		}

		protected override IMatchingBusinessEntityFinder<AsycudaBill> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override void PopulateBusinessObject(AsycudaBill billBO)
		{
			var billRow = GetColumnIndexer(billBO);

			SetValue(billRow, AsycudaBillSchema.ABL_BillNumber, billData.WayBillNumber);
			SetValue(billRow, AsycudaBillSchema.ABL_BolType, AsycudaBill.HouseBillCode);
			SetValue(billRow, AsycudaBillSchema.ABL_BillIssuer, billData.AddInfoCollection?.GetZStringValue(AddInfoConstants.AsycudaBill.BillIssuer, logger));
			SetValue(billRow, AsycudaBillSchema.ABL_BillStatus, billData.AddInfoCollection?.GetZStringValue(AddInfoConstants.AsycudaBill.BillStatus, logger));

			new GenAddOnColumnCollectionDataObjectReader(logger).ReadIntoBusinessObject(dataObject.AddInfoCollection, helper.GetAsycudaBillGenAddOnColumnList(billBO), billBO);

			SetCustomsEntryNumber(billBO, AddInfoConstants.AsycudaBill.MRN, billData.AddInfoCollection?.GetZStringValue(AddInfoConstants.AsycudaBill.MRN, logger).ToString());
			SetCustomsEntryNumber(billBO, AddInfoConstants.AsycudaBill.LRN, billData.AddInfoCollection?.GetZStringValue(AddInfoConstants.AsycudaBill.LRN, logger).ToString());

			FillPackingLines(billBO);
		}

		void FillPackingLines(AsycudaBill billBO)
		{
			if (billData.PackingLineCollection != null && billData.PackingLineCollection?.Count > 0)
			{
				helper.PacksReaderHelper.MarkUnprocessedExistingObjectFor(factory, billBO);
				foreach (var packingLineDataObject in billData.PackingLineCollection)
				{
					var pack = new AsycudaPackDataObjectReader(packingLineDataObject, logger, factory, billBO, helper).ReadIntoBusinessObject();
					helper.PacksReaderHelper.MarkProcessed(pack);
				}
				helper.PacksReaderHelper.DeleteUnprocessedObjectsFor(billBO, logger);
			}
		}

		void SetCustomsEntryNumber(AsycudaBill billBO, ZString type, ZString value)
		{
			var entryNumberDataObject = new EntryNumber()
			{
				Type = new EntryType() { Code = type, Description = type },
				Number = value,
				EntryIsSystemGenerated = false,
				CountryOfIssue = new Country() { Code = Core.Constants.CountryCodes.SouthAfrica, Name = "South Africa" }
			};
			new EntryNumberDataObjectReader(entryNumberDataObject, logger, factory, billBO).ReadIntoBusinessObject();
		}

		readonly Shipment billData;
		readonly AsycudaManifestHeader headerBO;
		readonly UniversalDataObjectReaderHelper helper;
	}
}
