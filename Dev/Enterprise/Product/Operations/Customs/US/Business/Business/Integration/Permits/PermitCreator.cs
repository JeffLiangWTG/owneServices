using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.US.Business
{
	class PermitCreator
	{
		public PermitCreator(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public void CreatePermits(IEnumerable<IEntryLineGroup> lineGroup)
		{
			if (declaration != null && declaration.Importer != null && declaration.US_PresentationDate.IsValid && declaration.WarehouseAddress != null)
			{
				ZInt seqNo = 1;
				foreach (var line in lineGroup)
				{
					var permit = CreatePermitHeader(seqNo, line.UnitOfMeasure, line.ManufacturerAddress);
					CreatePermitRule(permit, line);
					CreatePermitTransaction(permit, line);
					seqNo++;
				}
			}
		}

		CusPermitHeader CreatePermitHeader(ZInt seqNo, ZString unitOfMeasure, ZGuid manufacturerAddress)
		{
			var permit = declaration.Factory.New<CusPermitHeader>();

			permit.CPH_Number = declaration.US_EntryFilerCode + "-" + declaration.ImportEntryNumber + "-" + seqNo.ToString("D3", CultureInfo.InvariantCulture);
			permit.CPH_OH_PermitHolder = declaration.Importer.PK;
			permit.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			permit.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			permit.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Permit;

			permit.CPH_StartDate = declaration.US_PresentationDate.Date;
			permit.CPH_EndDate = permit.CPH_StartDate.AddDays(6);
			permit.CPH_Type = PermitTypeList.Codes.FTZ;
			permit.CPH_SubType = ZString.Empty;
			permit.CPH_UnitOfMeasure = unitOfMeasure;

			if (manufacturerAddress.IsValid)
			{
				permit.CPH_OA_AppliesTo = manufacturerAddress;
			}
			return permit;
		}

		void CreatePermitRule(CusPermitHeader permit, IEntryLineGroup lineGroup)
		{
			if (!lineGroup.TariffNumber.IsEmpty)
			{
				var rule = permit.CusPermitRules.AddNew();
				rule.CPR_RuleCode = USPermitRuleCodeList.Codes.TAR;
				rule.CPR_ValueFrom = lineGroup.TariffNumber;
				rule.CPR_ValueTo = lineGroup.TariffNumber;
			}

			if (!lineGroup.FirmRegNo.IsEmpty)
			{
				var rule = permit.CusPermitRules.AddNew();
				rule.CPR_RuleCode = USPermitRuleCodeList.Codes.FRM;
				rule.CPR_ValueFrom = lineGroup.FirmRegNo;
				rule.CPR_ValueTo = ZString.Empty;
			}

			if (!lineGroup.ProductCode.IsEmpty)
			{
				var rule = permit.CusPermitRules.AddNew();
				rule.CPR_RuleCode = USPermitRuleCodeList.Codes.PRD;
				rule.CPR_ValueFrom = lineGroup.ProductCode;
				rule.CPR_ValueTo = ZString.Empty;
			}

			if (!lineGroup.CountryOfOrigin.IsEmpty)
			{
				var rule = permit.CusPermitRules.AddNew();
				rule.CPR_RuleCode = USPermitRuleCodeList.Codes.COO;
				rule.CPR_ValueFrom = lineGroup.CountryOfOrigin;
				rule.CPR_ValueTo = ZString.Empty;
			}

			if (!lineGroup.ZoneStatus.IsEmpty)
			{
				var rule = permit.CusPermitRules.AddNew();
				rule.CPR_RuleCode = USPermitRuleCodeList.Codes.ZST;
				rule.CPR_ValueFrom = lineGroup.ZoneStatus;
				rule.CPR_ValueTo = ZString.Empty;
			}
		}
		void CreatePermitTransaction(CusPermitHeader permit, IEntryLineGroup lineGroup)
		{
			var lineTransaction = permit.CusPermitLineTransactions.AddNew();
			lineTransaction.CPL_Reference = lineGroup.EntryNumber;
			lineTransaction.CPL_TranQty = lineGroup.FTZCustomsQty;
			lineTransaction.CPL_TranValue = lineGroup.FTZCustomsValue;
			lineTransaction.CPL_TransactionDate = ZDateTime.Now;
			lineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			lineTransaction.CPL_TransactionCategory = PermitTransactionCategoryList.Codes.CUM;
			lineTransaction.CPL_AppId = EDIMessage.ApplicationCodes.USCustomsImport;
			lineTransaction.CPL_Comment = ZString.Empty;
		}
	}
}
