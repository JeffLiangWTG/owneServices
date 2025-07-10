using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPeriodicInvoicingYardUnits(PeriodicInvoicing periodicInvoicing) : ICYDYardUnitsForRating
	{
		#region ICYDYardUnitsForRating

		IEnumerable<CYDYardUnitState> ICYDYardUnitsForRating.YardUnits => yardUnits ??= GetYardUnitsForRating(periodicInvoicing);

		IEnumerable<CYDYardUnitState> yardUnits;

		IReadOnlyList<string> ICYDYardUnitsForRating.ChargeCodeGroupList => [ChargeCodeGroupList.Codes.YardStorage];

		WhsWarehouse ICYDYardUnitsForRating.Yard => periodicInvoicing.Warehouse;

		OrgHeader ICYDYardUnitsForRating.Client => periodicInvoicing.Client;

		#endregion

		CYDYardUnitState[] GetYardUnitsForRating(PeriodicInvoicing periodicInvoicing)
		{
			var startTime = periodicInvoicing.ET_StorageFromDate.ToDateTimeOffset(periodicInvoicing.Warehouse.RelatedCompanyBranch.HomePort);
			var endTime = periodicInvoicing.ET_StorageToDate.AddDays(1).ToDateTimeOffset(periodicInvoicing.Warehouse.RelatedCompanyBranch.HomePort);	// Add 1 day so the whole day is coverred.
			var calculationMethod = periodicInvoicing.Client.CompanyData.OB_ARYardStorageCalcMethod;
			return calculationMethod == OrgCompanyDataLookups.YardStorageMethodYardOut
				? GetYardUnitsHavingGatedOutInCurrentPeriod(startTime, endTime)
				: [.. GetYardUnitsHavingGatedOutOrIsGatingOut(startTime, endTime), .. GetYardUnitsHavingNotGatedOut(endTime)];
		}

		CYDYardUnitState[] GetYardUnitsHavingGatedOutInCurrentPeriod(ZDateTimeOffset startTime, ZDateTimeOffset endTime)
		{
			var yardUnitQuery = GetYardUnitQuery();
			yardUnitQuery.AddSubQuery(GetGateOutTimeSubQueryForGateOutCalculationMethod(startTime, endTime), JoinCondition.And);
			return periodicInvoicing.Factory.Load<CYDYardUnitState>(yardUnitQuery);
		}

		CYDYardUnitState[] GetYardUnitsHavingGatedOutOrIsGatingOut(ZDateTimeOffset startTime, ZDateTimeOffset endTime)
		{
			var yardUnitQuery = GetYardUnitQuery();
			yardUnitQuery.AddSubQuery(GetGateInTimeSubQueryForStandardCalculationMethod(endTime), JoinCondition.And);
			yardUnitQuery.AddSubQuery(GetGateOutTimeSubQueryForStandardCalculationMethod(startTime), JoinCondition.And);
			return periodicInvoicing.Factory.Load<CYDYardUnitState>(yardUnitQuery);
		}

		CYDYardUnitState[] GetYardUnitsHavingNotGatedOut(ZDateTimeOffset endTime)
		{
			var yardUnitQuery = GetYardUnitQuery();
			yardUnitQuery.AddSubQuery(GetGateInTimeSubQueryForStandardCalculationMethod(endTime), JoinCondition.And);
			yardUnitQuery.AddToFilter(CYDYardUnitStateSchema.YUS_YTU_DispatchTransportationUnit, null);
			return periodicInvoicing.Factory.Load<CYDYardUnitState>(yardUnitQuery);
		}

		ZDBOnlyQuery GetYardUnitQuery()
		{
			var yardUnitQuery = new ZDBOnlyQuery(typeof(CYDYardUnitState));
			yardUnitQuery.AddToFilter(CYDYardUnitStateSchema.YUS_WW_CurrentYard, periodicInvoicing.ET_WW);

			var receiveAdviceLineSubQuery = GetReceiveAdviceLineSubQuery(periodicInvoicing);
			yardUnitQuery.AddSubQuery(receiveAdviceLineSubQuery, JoinCondition.And);

			return yardUnitQuery;
		}

		ZDBOnlySubQuery GetReceiveAdviceLineSubQuery(PeriodicInvoicing periodicInvoicing)
		{
			var receiveAdviceLineSubQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdviceLine), CYDYardUnitStateSchema.YUS_YRL_ReceiveLine);
			var receiveAdviceSubQuery = new ZDBOnlySubQuery(typeof(CYDReceiveAdvice), CYDReceiveAdviceLineSchema.YRL_YRA_ReceiveAdvice);
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CYDReceiveAdviceSchema.Constants.Prefix);
			jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, AutoDocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, periodicInvoicing.ET_OH_Client);

			jobDocAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			receiveAdviceSubQuery.AddSubQuery(jobDocAddressSubQuery, JoinCondition.And);
			receiveAdviceLineSubQuery.AddSubQuery(receiveAdviceSubQuery, JoinCondition.And);
			return receiveAdviceLineSubQuery;
		}

		ZDBOnlySubQuery GetGateOutTimeSubQueryForGateOutCalculationMethod(ZDateTimeOffset startTime, ZDateTimeOffset endTime)
		{
			var gateOutTimeQuery = new ZQuery(CYDTransportationUnitSchema.YTU_GateOutTime, SQLComparisonOperator.GreaterThanOrEqualTo, startTime)
				.AddToFilter(JoinCondition.And, CYDTransportationUnitSchema.YTU_GateOutTime, SQLComparisonOperator.LessThan, endTime);
			var dispatchTransportationUnitSubQuery = new ZDBOnlySubQuery(typeof(CYDTransportationUnit), CYDYardUnitStateSchema.YUS_YTU_DispatchTransportationUnit);
			dispatchTransportationUnitSubQuery.AddToFilter(gateOutTimeQuery);
			return dispatchTransportationUnitSubQuery;
		}

		ZDBOnlySubQuery GetGateOutTimeSubQueryForStandardCalculationMethod(ZDateTimeOffset startTime)
		{
			var gateOutTimeQuery = new ZQuery(CYDTransportationUnitSchema.YTU_GateOutTime, SQLComparisonOperator.GreaterThanOrEqualTo, startTime)
				.AddToFilter(JoinCondition.Or, CYDTransportationUnitSchema.YTU_GateOutTime, null);
			var dispatchTransportationUnitSubQuery = new ZDBOnlySubQuery(typeof(CYDTransportationUnit), CYDYardUnitStateSchema.YUS_YTU_DispatchTransportationUnit);
			dispatchTransportationUnitSubQuery.AddToFilter(gateOutTimeQuery);
			return dispatchTransportationUnitSubQuery;
		}

		ZDBOnlySubQuery GetGateInTimeSubQueryForStandardCalculationMethod(ZDateTimeOffset endTime)
		{
			var gateInTimeQuery = new ZQuery(CYDTransportationUnitSchema.YTU_GateInTime, SQLComparisonOperator.LessThan, endTime);
			var receiveTransportationUnitSubQuery = new ZDBOnlySubQuery(typeof(CYDTransportationUnit), CYDYardUnitStateSchema.YUS_YTU_ReceiveTransportationUnit);
			receiveTransportationUnitSubQuery.AddToFilter(gateInTimeQuery);
			return receiveTransportationUnitSubQuery;
		}
	}
}
