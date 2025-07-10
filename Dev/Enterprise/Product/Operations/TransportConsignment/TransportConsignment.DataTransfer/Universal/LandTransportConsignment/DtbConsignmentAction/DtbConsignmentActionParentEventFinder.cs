using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	internal class DtbConsignmentActionParentEventFinder : EventParentFinder
	{
		internal DtbConsignmentActionParentEventFinder(DtbConsignmentActionDataContextManager manager, BusinessObjectFactory factory, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalDataBuss.DataObjects.Universal.Event eventDataObject)
		{
			var result = new List<BusinessObject>();
			var matchingFields = new TransportMatchingFields(eventDataObject, factory);
			var eventCode = matchingFields.EventType;

			if (eventCode == AutoEvents.ArrivalCode || eventCode == AutoEvents.DepartureCode)
			{
				var packageId = matchingFields.PackageID;
				if (!packageId.IsEmpty)
				{
					var isDelivering = eventCode == AutoEvents.ArrivalCode;
					var actionTypeCode = isDelivering ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp;
					var matchingAction = FindMatchingAction(packageId, actionTypeCode, DocAddressTypes.GetCode(factory, matchingFields.FacilityAddressType).ToString(), matchingFields.City);
					if (matchingAction != null)
					{
						result.Add(matchingAction);

						var package = matchingAction.Consignment.LoosePackages.FirstOrDefault(p => p.KP_PackageID == packageId);
						if (package != null)
						{
							result.Add(package);
						}
					}
				}
			}

			return result.ToArray();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DtbConsignmentAction FindMatchingAction(ZString packageId, string actionType, string addressType, string city)
		{
			// The reason of splitting this query into two parts is, we need an Action as the result but this comes from a instructions list that are DESC sorted which is only supported on the top level of the query result.
			// First part to get the instruction that contains the required actions by sorting by KG_StartTime of Runsheet DESC.
			var zQueryRunSheet = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet)) { OrderBy = DtbConsignmentRunSheetSchema.KG_StartTime.Name + " DESC" };
			zQueryRunSheet.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTimeOffset.Now.ToDateTimeOffset());
			var runSheetInstructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
			var dtbConsignmentActionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAction), DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction);

			var divotSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentActionPackageDivot), DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction);
			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), DtbConsignmentActionPackageDivotSchema.LTP_KP_Package);
			var packageHeaderSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageHeaderSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, packageId);
			packageSubQuery.AddSubQuery(packageHeaderSubQuery, JoinCondition.And);
			divotSubQuery.AddSubQuery(packageSubQuery, JoinCondition.And);
			dtbConsignmentActionSubQuery.AddSubQuery(divotSubQuery, JoinCondition.And);

			var dtbConsignmentAddressQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAddress), DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress);
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), DtbConsignmentAddressSchema.PK, JobDocAddressSchema.E2_ParentID);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, "LTS");
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			var cityFilter = $@"
EXISTS (
	SELECT 1
	FROM
		dbo.JobDocAddress JobAddress
	LEFT OUTER JOIN dbo.OrgAddress ON dbo.JobDocAddress.E2_OA_Address = dbo.OrgAddress.OA_PK
	AND dbo.JobDocAddress.E2_AddressOverride = 0
	WHERE
		COALESCE(dbo.OrgAddress.OA_City, dbo.JobDocAddress.E2_City) = '{city}')";
			jobDocAddressSubQuery.AddFilterAndZSQLParameterCollection(cityFilter, new ZSqlParameterCollection());
			dtbConsignmentAddressQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			dtbConsignmentActionSubQuery.AddToFilter(DtbConsignmentActionSchema.LTA_ActionType, actionType);
			dtbConsignmentActionSubQuery.AddSubQuery(dtbConsignmentAddressQuery, JoinCondition.And);
			runSheetInstructionSubQuery.AddSubQuery(dtbConsignmentActionSubQuery, JoinCondition.And);
			zQueryRunSheet.AddSubQuery(runSheetInstructionSubQuery, JoinCondition.And);

			var resultRunSheet = factory.LoadTop1<DtbConsignmentRunSheet>(zQueryRunSheet);
			if (resultRunSheet == null)
			{
				return null;
			}

			// Second part to get the required action in the retrieved instruction by using packageId.
			var zQueryAction = new ZDBOnlyQuery(typeof(DtbConsignmentAction));
			var runSheetInstructionSubQuery2 = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction);
			var runSheetSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheet), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
			runSheetSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.PK, SQLComparisonOperator.Equal, resultRunSheet.PK);
			runSheetInstructionSubQuery2.AddSubQuery(runSheetSubQuery, JoinCondition.And);
			zQueryAction.AddSubQuery(runSheetInstructionSubQuery2, JoinCondition.And);
			zQueryAction.AddSubQuery(divotSubQuery, JoinCondition.And); // Add Divot back to determine required action when multiple actions exist in one instruction.
			zQueryAction.AddToFilter(DtbConsignmentActionSchema.LTA_ActionType, actionType);

			return factory.LoadTop1<DtbConsignmentAction>(zQueryAction);
		}
	}
}


