using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class BillValidator
	{
		public static string AirlinePrefixValidationMessage
		{
			get { return Res.GetString("d78f937a-b02b-4591-94ee-84a4e02a321f", "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number."); }
		}

		public void CheckMandatory(bool isMandatory, ZPropertyInfo billInfo, ZString propertyDescription)
		{
			if (isMandatory && billInfo.Value.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(billInfo, propertyDescription);
			}
		}

		#region Check For Duplicate Declaration

		public void CheckDuplicateDeclaration(ZPropertyInfo billInfo, BaseJobDeclaration declaration, ZString houseBill, ZString masterBill)
		{
			if (!declaration.IsExWarehouse)
			{
				ZString warning = GetDuplicateDeclarationMessage(declaration, houseBill, masterBill);

				if (!warning.IsEmpty)
				{
					billInfo.AddWarning(warning);
				}
			}
		}

		ZString GetDuplicateDeclarationMessage(BaseJobDeclaration declaration, ZString houseBill, ZString masterBill)
		{
			var result = ZString.Empty;

			if (!houseBill.IsEmpty)
			{
				var otherDuplicateDec = declaration.Factory.LoadTop1<BaseJobDeclaration>(GetDuplicateDeclarationQuery(declaration, houseBill, masterBill));

				if (otherDuplicateDec != null)
				{
					if (masterBill.IsEmpty)
					{
						result = AlreadyContainsHouseBill(otherDuplicateDec.JE_DeclarationReference, otherDuplicateDec.Company.GC_Name, (otherDuplicateDec.Branch != null ? otherDuplicateDec.Branch.GB_BranchName : ZString.Empty));
					}
					else
					{
						result = AlreadyContainsMasterAndHouseBills(otherDuplicateDec.JE_DeclarationReference, otherDuplicateDec.Company.GC_Name, (otherDuplicateDec.Branch != null ? otherDuplicateDec.Branch.GB_BranchName : ZString.Empty));
					}
				}
			}

			return result;
		}

		public static string AlreadyContainsHouseBill(string reference, string companyName, string branchName)
		{
			return Res.GetString("e6e5fb5b-4772-463d-a759-2c55c566f646", "Another Declaration already contains the same house bill as this declaration (Declaration: '{0}', Company: '{1}', Branch: '{2}').", reference, companyName, branchName);
		}

		public static string AlreadyContainsMasterAndHouseBills(string reference, string companyName, string branchName)
		{
			return Res.GetString("fe26b1dd-02ab-41ef-8877-1f17485358d3", "Another Declaration already contains the same master and house bill combination as this declaration (Declaration: '{0}', Company: '{1}', Branch: '{2}').", reference, companyName, branchName);
		}

		public static ZDBOnlyQuery GetDuplicateDeclarationQuery(BaseJobDeclaration declaration, ZString houseBill, ZString masterBill)
		{
			var result = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			result.AddToFilter(JobDeclarationFilter.ForCountry(false, declaration.CountryCode, declaration.Factory), JoinCondition.And);
			result.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, declaration.JE_MessageType);

			var houseBillQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
			houseBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.HouseBill);
			houseBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, houseBill);

			if (!masterBill.IsEmpty)
			{
				var masterBillQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.PK);
				masterBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
				masterBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, masterBill);

				houseBillQuery.AddSubQuery(CusDecHouseBillSchema.CU_CU_ParentBill, masterBillQuery, JoinCondition.And);
			}
			else
			{
				houseBillQuery.AddToFilter(CusDecHouseBillSchema.CU_CU_ParentBill, DBNull.Value);
			}

			result.AddSubQuery(houseBillQuery, JoinCondition.And);
			result.IsNoResultQuery = false;
			result.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + " DESC";
			return result;
		}

		public void CheckDeclarationWithSameDirectMasterBill(BaseJobDeclaration declaration, ZPropertyInfo masterBillInfo)
		{
			if (!declaration.IsExWarehouse)
			{
				ZString masterBillNum = (ZString)masterBillInfo.Value;
				if (!masterBillNum.IsEmpty)
				{
					ZString warning = GetDeclarationWithSameDirectMasterBillMessage(declaration, masterBillNum);
					if (!warning.IsEmpty)
					{
						masterBillInfo.AddWarning(warning);
					}
				}
			}
		}

		ZString GetDeclarationWithSameDirectMasterBillMessage(BaseJobDeclaration declaration, ZString masterBillNum)
		{
			var declarationWithBillQuery = GetDeclarationWithSameDirectMasterBillCore(declaration, masterBillNum);
			var result = declarationWithBillQuery.DeclarationQuery;
			result.AddSubQuery(declarationWithBillQuery.MasterBillQuery, JoinCondition.And);
			result.OrderBy = JobDeclarationSchema.Constants.JE_SystemCreateTimeUtc + " DESC";
			var bizO = declaration.Factory.LoadTop1<BaseJobDeclaration>(result);
			return bizO != null ? AlreadyContainsMasterBill(bizO.JE_DeclarationReference, bizO.Company.GC_Name, bizO.Branch.GB_BranchName) : string.Empty;
		}

		protected virtual (ZDBOnlyQuery DeclarationQuery, ZDBOnlySubQuery MasterBillQuery) GetDeclarationWithSameDirectMasterBillCore(BaseJobDeclaration declaration, ZString masterBillNum)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(BaseJobDeclaration));
			declarationQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, declaration.JE_MessageType);
			declarationQuery.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);

			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), JobDeclarationSchema.JE_GC);
			companySubQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, declaration.CountryCode);
			declarationQuery.AddSubQuery(companySubQuery, JoinCondition.And);

			var masterBillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
			masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
			masterBillSubQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, masterBillNum);

			var childBillSubQuery = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_CU_ParentBill, true);
			masterBillSubQuery.AddSubQuery(childBillSubQuery, JoinCondition.And);

			return (declarationQuery, masterBillSubQuery);
		}

		public static string AlreadyContainsMasterBill(string reference, string companyName, string branchName)
		{
			return Res.GetString("2b80f3b6-ce00-4de5-9f3d-2fa7f51aa663", "Another Declaration already contains the same master as this declaration (Declaration: '{0}', Company: '{1}', Branch: '{2}').", reference, companyName, branchName);
		}

		#endregion

		#region CheckAirWayBill

		public void CheckAirWayBill(BaseJobDeclaration declaration, ZPropertyInfo masterBillInfo, INotificationType notificationTypeForAirWayBillNumber)
		{
			ZString billValue = (ZString)masterBillInfo.Value;
			ZString message = GetWarningMessage(billValue, declaration);
			if (!message.IsEmpty)
			{
				masterBillInfo.AddNotification(notificationTypeForAirWayBillNumber, message);
			}

			if (declaration.JE_VoyageFlightNo.Length >= 2 && billValue.Length >= 3)
			{
				if (declaration.JE_VoyageFlightNo != SpecialFlightTerm)
				{
					BusinessObjectFactory factory = masterBillInfo.BizObj.Factory ?? new BusinessObjectFactory();

					RefAirline airline = RefAirline.LoadFromAirline2LetterCode(factory, declaration.AirlinePrefix);
					if (airline != null && airline.RM_EagleAddedAirlinePrefixOrAccountingCode != billValue.Left(3))
					{
						masterBillInfo.AddWarning(AirlinePrefixValidationMessage);
					}
				}
			}
		}

		protected virtual string GetWarningMessage(ZString billValue, BaseJobDeclaration declaration)
		{
			return new AirWayBillValidator().GetWarningMessage(billValue);
		}

		protected virtual ZString SpecialFlightTerm => ZString.Empty;

		#endregion
	}
}
