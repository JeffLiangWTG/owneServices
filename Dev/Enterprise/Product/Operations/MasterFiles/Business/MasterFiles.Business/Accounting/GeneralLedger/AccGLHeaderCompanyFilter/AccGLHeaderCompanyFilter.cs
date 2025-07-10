using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderCompanyFilter : AutoAccGLHeaderCompanyFilter
	{
		public AccGLHeaderCompanyFilter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Delete

		bool IsCompanyFilterUsedInTransactionHeaders
		{
			get
			{
				var transactionQuery = new ZQuery(AccTransactionHeaderSchema.AH_GC, ACF_GC_Company);
				transactionQuery.AddToFilter(AccTransactionHeaderSchema.AH_AG, ACF_AG_Header);
				return Factory.LoadTop1<AccTransactionHeader>(transactionQuery) != null;
			}
		}

		bool IsCompanyFilterUsedInTransactionLines
		{
			get
			{
				var lineQuery = new ZDBOnlyQuery(typeof(AccTransactionLines));
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_AG, ACF_AG_Header);
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, ACF_GC_Company);
				return Factory.LoadTop1<AccTransactionLines>(lineQuery) != null;
			}
		}

		public List<AccChargeCode> GetChargeCodeUseGLAccountWithCompanyNotSet(ZGuid[] companyPKs)
		{
			var chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, companyPKs);
			var chargeCodeQuery2 = new ZQuery(AccChargeCodeSchema.AC_AG_AccrualAccount, ACF_AG_Header);
			chargeCodeQuery2.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_AG_CostAccount, ACF_AG_Header);
			chargeCodeQuery2.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_AG_RevenueAccount, ACF_AG_Header);
			chargeCodeQuery2.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_AG_WIPAccount, ACF_AG_Header);
			chargeCodeQuery.AddToFilter(chargeCodeQuery2);
			return Factory.Load<AccChargeCode>(chargeCodeQuery).ToList();
		}

		public bool GetIsCompanyFilterUsedInChargeCode(ZGuid[] companyPKs)
		{
			return GetChargeCodeUseGLAccountWithCompanyNotSet(companyPKs).Any();
		}

		public List<AccBankAccount> GetBankAccountUseGLAccountWithCompanyNotSet(ZGuid[] companyPKs)
		{
			var bankAccountQuery = new ZQuery(AccBankAccountSchema.AB_GC, companyPKs);
			bankAccountQuery.AddToFilter(AccBankAccountSchema.AB_AG, ACF_AG_Header);
			return Factory.Load<AccBankAccount>(bankAccountQuery).ToList();
		}

		public bool GetIsCompanyFilterUsedInBankAccount(ZGuid[] companyPKs)
		{
			return GetBankAccountUseGLAccountWithCompanyNotSet(companyPKs).Any();
		}

		public override bool CanDelete
		{
			get
			{
				return !IsInDatabase || (!GetIsCompanyFilterUsedInChargeCode(new ZGuid[] { ACF_GC_Company }) && !GetIsCompanyFilterUsedInBankAccount(new ZGuid[] { ACF_GC_Company }) && !GLHeader.IsGLAccountUsedInCompanyLevelRegistry);
			}
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			List<string> areasGLAccountUsed = new List<string>();
			if (IsCompanyFilterUsedInTransactionHeaders)
			{
				areasGLAccountUsed.Add(ResString.GetMultilingualString("a38c3578-132d-4793-b719-e705b61694fb", "Transaction Headers"));
			}
			if (IsCompanyFilterUsedInTransactionLines)
			{
				areasGLAccountUsed.Add(ResString.GetMultilingualString("df1750e3-4ad8-4333-a987-5f567f4c3978", "Transaction Lines"));
			}
			string areasGLAccountUsedAsString = string.Join(", ", areasGLAccountUsed.Distinct().ToList());

			return string.IsNullOrEmpty(areasGLAccountUsedAsString) ? (NoResString)string.Empty : ResString.GetMultilingualString("fe4e3196-7ed4-4313-8fa7-fd0b0119a8cc", "This GL Account has been used ({0}) in Company {1}.", areasGLAccountUsedAsString, Company.GC_Code);
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				List<MultilingualString> areasGLAccountUsed = new List<MultilingualString>();

				if (GetIsCompanyFilterUsedInChargeCode(new ZGuid[] { ACF_GC_Company }))
				{
					areasGLAccountUsed.Add(ResString.GetMultilingualString("dca788e8-0b3f-4d5c-b6c4-419114ea063f", "Charge Codes"));
				}
				if (GetIsCompanyFilterUsedInBankAccount(new ZGuid[] { ACF_GC_Company }))
				{
					areasGLAccountUsed.Add(ResString.GetMultilingualString("45f8a5f6-d7b5-47dc-a585-81776614cc58", "Bank Accounts"));
				}
				if (GLHeader.IsGLAccountUsedInCompanyLevelRegistry)
				{
					areasGLAccountUsed.Add(ResString.GetMultilingualString("ba61aefe-4408-4de4-b053-1777f133eace", "Registries"));
				}
				MultilingualString areasGLAccountUsedAsString = MultilingualString.Join(", ", areasGLAccountUsed.Distinct().ToArray());

				return ResString.GetMultilingualString("beadcca0-a735-4716-9226-818c8b630614", "Company {0} cannot be removed as this GL Account has been used ({1}).", Company.GC_Code, areasGLAccountUsedAsString);
			}
		}

		AccGLHeader GLHeader
		{
			get { return Factory.Load<AccGLHeader>(ACF_AG_Header); }
		}

		#endregion
	}
}
