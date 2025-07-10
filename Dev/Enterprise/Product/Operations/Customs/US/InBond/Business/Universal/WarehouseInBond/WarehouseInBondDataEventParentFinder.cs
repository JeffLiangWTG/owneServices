using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.InBond.Business.Universal
{
	public class WarehouseInBondDataEventParentFinder : EventParentFinder
	{
		public WarehouseInBondDataEventParentFinder(BusinessObjectFactory factory, WarehouseInBondDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			BusinessObject[] result = null;

			var eventValueObject = (IXmlEventValueObject)xmlEvent;
			if (!eventValueObject.Context.EntryNumber.IsEmpty && eventValueObject.Context.EntryNumberType == Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond
				&& eventValueObject.Context.EntryNumberCountryOfIssue == Core.Constants.CountryCodes.UnitedStates)
			{
				var headerQuery = new ZDBOnlySubQuery(typeof(CusInBondHeader), CusInBondHeaderSchema.PK);
				headerQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.InBond);
				var moveHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondMoveHeader));
				var inBondNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond);
				inBondNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, eventValueObject.Context.EntryNumber);
				moveHeaderQuery.AddSubQuery(inBondNumberQuery, JoinCondition.And);
				moveHeaderQuery.AddSubQuery(CusInBondMoveHeaderSchema.BM_BH, headerQuery, JoinCondition.And);
				result = factory.Load<CusInBondMoveHeader>(moveHeaderQuery);
			}

			return result;
		}
	}
}
