using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public static class AsycudaHelper
	{
		public static ZDecimal ConvertWeightQty(ZDecimal weight, ZString sourceUnitCode, ZString targetUnitCode)
		{
			var result = ZDecimal.Zero;
			if (!sourceUnitCode.IsEmpty && !targetUnitCode.IsEmpty && Constants.Weight.ContainsCode(sourceUnitCode) && Constants.Weight.ContainsCode(targetUnitCode))
			{
				result = Constants.Weight.Convert(weight, sourceUnitCode, targetUnitCode);
			}
			return result;
		}

		public static void CreateManifestStatement(AsycudaManifestHeader header)
		{
			var registrationDate = header.RegistrationDate;
			var factory = header.Factory;
			var companyPK = header.Branch.Company.PK;
			if (!registrationDate.IsEmpty)
			{
				var statementHeader = new CusStatementHeader.Loader(factory).LoadMonthlyStatementWithPeriodStartDate(CusStatementHeaderTypes.Codes.GlobalManifest, registrationDate, companyPK);
				if (statementHeader == null)
				{
					statementHeader = factory.New<CusStatementHeader>();
					statementHeader.B2_IsMonthlyStatement = true;
					statementHeader.B2_Status = StatementHeaderStatusList.Codes.PRE;
					statementHeader.B2_StatementType = CusStatementHeaderTypes.Codes.GlobalManifest;
					statementHeader.B2_DueDate = new DateTime(registrationDate.Year, registrationDate.Month, 20).AddMonths(1);
					statementHeader.B2_PaymentParty = PaymentPartyCodeDescriptionList.Codes.Broker;
					statementHeader.B2_PeriodStartDate = new ZDate(registrationDate.Year, registrationDate.Month, 1);
					statementHeader.B2_PeriodEndDate = new ZDate(registrationDate.Year, registrationDate.Month, 1).AddMonths(1).AddDays(-1);
					statementHeader.B2_GC = companyPK;
				}

				CreateCusStatementLine(statementHeader, header, header.MasterBill);

				foreach (AsycudaBill bill in header.Bills)
				{
					CreateCusStatementLine(statementHeader, header, bill);
				}
			}
		}

		static void CreateCusStatementLine(CusStatementHeader statementHeader, AsycudaManifestHeader header, AsycudaBill bill)
		{
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryType = StatementLineEntryTypeList.Codes.MAN;
			statementLine.B3_BrokerReference = header.AMA_JobReference;
			statementLine.B3_AssociatedEntry = bill.ABL_BillNumber;
			statementLine.B3_EntryNum = header.RegistrationNumber;
			statementLine.B3_EntryDate = header.RegistrationDate.Date;

			foreach (AsycudaTax tax in bill.AsycudaTaxes)
			{
				var charge = statementLine.Charges.AddNew();
				charge.B4_ChargeType = tax.AET_ChargeType;
				charge.B4_ChargeAmount = tax.AET_ChargeAmount;
			}
		}

		static ZDBOnlyQuery GetManifestHeadersQuery(ZString billNumber, ZString transportMode, ZGuid currentJobPK, ZString applicationCode)
		{
			var masterbillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
			masterbillQuery.AddToFilter(AsycudaBillSchema.ABL_BillNumber, billNumber);
			masterbillQuery.AddToFilter(AsycudaBillSchema.ABL_BolType, AsycudaBill.ChildBolCode);

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Turkey);

			var currentYear = (ZString)ZDateTime.Today.Year.ToString();
			currentYear = currentYear.SubstringSafe(2);

			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.StartsWith, currentYear);

			var manifestHeaderQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, applicationCode);
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, Constants.CountryCodes.Turkey);
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_TransportMode, transportMode);
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_Nature, ShipmentTypeList.Codes.Import23);
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.PK, SQLComparisonOperator.NotEqual, currentJobPK);
			manifestHeaderQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_IsActive, true);

			manifestHeaderQuery.AddSubQuery(masterbillQuery, JoinCondition.And);
			manifestHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			return manifestHeaderQuery;
		}

		public static AsycudaManifestHeader LoadManifestHeadersForMasterBill(BusinessObjectFactory factory, ZString masterBillNumber, ZString transportMode, ZGuid currentJobPK, ZString applicationCode)
		{
			var manifestQuery = GetManifestHeadersQuery(masterBillNumber, transportMode, currentJobPK, applicationCode);
			return factory.LoadTop1<AsycudaManifestHeader>(manifestQuery);
		}
	}
}
