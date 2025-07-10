using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class WarehouseInBondDataObjectReader : ShipmentDataObjectReader<CusInBondMoveHeader>
	{
		public WarehouseInBondDataObjectReader(Shipment headerDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(headerDataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.WarehouseInBond; }
		}

		protected override CusInBondMoveHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			existingMatched = null;
			CusInBondMoveHeader result = null;
			if (!InBondNumber.IsEmpty)
			{
				var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.InBond);
				var moveHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
				var inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, InBondNumber);
				moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
				moveHeaderQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, headerQuery, JoinCondition.And);
				moveHeaderQuery.OrderBy = CusInBondMoveHeaderSchema.BM_SystemCreateTimeUtc.Name;
				existingMatched = factory.Load<CusInBondMoveHeader>(moveHeaderQuery);
				if (existingMatched.Length == 1)
				{
					result = existingMatched[0];
				}
			}
			return result;
		}
		CusInBondMoveHeader[] existingMatched;

		ZString InBondNumber
		{
			get
			{
				if (!inBondNumberCached.HasValue)
				{
					EntryNumber entryNumber = null;
					if (dataObject.InBondMoveHeaderCollection != null && dataObject.InBondMoveHeaderCollection.Count == 1 && dataObject.InBondMoveHeaderCollection[0].EntryNumberCollection != null)
					{
						entryNumber = dataObject.InBondMoveHeaderCollection[0].EntryNumberCollection.FirstOrDefault(y => y.Type.GetCodeAsUpperCase() == CusEntryHeaderMessageTypeList.Codes.InBond && !y.Number.GetValueOrDefault().IsEmpty);
					}
					inBondNumberCached = entryNumber == null ? ZString.Empty : entryNumber.Number.GetValueOrDefault();
				}
				return inBondNumberCached.Value;
			}
		}
		ZString? inBondNumberCached;

		protected override IMatchingBusinessEntityFinder<CusInBondMoveHeader> GetCombinedReferenceMatcher()
		{
			return null; // No Combined Reference MAtching has been implemented for InBond. Considering we're looking at replacing this with Reference and Party ID matching, is best not to implement.
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusInBondMoveHeader targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty && existingMatched != null && existingMatched.Length > 1)
			{
				result = "Multiple jobs were matched with InBond Number (" + InBondNumber + ")";
			}
			else if (dataObject.InBondMoveHeaderCollection != null && dataObject.InBondMoveHeaderCollection.Count != 1)
			{
				result = "Warehouse InBond must have one element in InBondMoveHeaderCollection";
			}
			else if (InBondNumber.IsEmpty)
			{
				result = "Warehouse InBond must have an InBond Number specified";
			}
			return result;
		}

		protected override void PopulateBusinessObject(CusInBondMoveHeader moveHeader)
		{
			var moveHeaderRow = GetColumnIndexer(moveHeader);
			var headerPK = moveHeaderRow.GetValue(CusInBondMoveHeaderSchema.BM_BH);
			var inBondHeaderDataObjectReader = new CusInBondHeaderDataObjectReader(dataObject, logger, factory, null);
			var header = factory.Load<CusInBondHeader>(headerPK) ?? factory.New<CusInBondHeader>();
			if (inBondHeaderDataObjectReader.CheckUpdateDataIsAllowed(header))
			{
				inBondHeaderDataObjectReader.PopulateMainData(header, moveHeader);
			}
			else if (!moveHeader.IsInDatabase)
			{
				moveHeader.Delete();
			}
		}
	}
}
