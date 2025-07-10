using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class PackLineStatus : IPackLineStatus, Integration.Customs.NZ.IPackLineStatus
	{
		public PackLineStatus(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		ZString IPackLineStatus.GetCustomsStatusDescription(ZString masterBill, ZString houseBill, ZString containerNumber)
		{
			ZString result = ZString.Empty;
			if (!masterBill.IsEmpty)
			{
				ZQuery query = new ZQuery();
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.Bill), CusDecHouseBillSchema.CU_JE);
				subQuery.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
				ZString filteredMasterBill = masterBill.Replace("-", "").Replace(" ", "");
				if (filteredMasterBill == masterBill)
				{
					subQuery.AddToFilter(JoinCondition.And, CusDecHouseBillSchema.CU_BillNum, masterBill);
				}
				else
				{
					ZQuery masterBillNumFilter = new ZQuery(CusDecHouseBillSchema.CU_BillNum, masterBill);
					masterBillNumFilter.AddToFilter(JoinCondition.Or, CusDecHouseBillSchema.CU_BillNum, filteredMasterBill);
					subQuery.AddToFilter(masterBillNumFilter);
				}
				ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(Customs.Business.BaseJobDeclaration));
				dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
				query.AddToFilter(dbOnlyQuery);

				if (houseBill.IsEmpty)
				{
					query.AddToFilter(JobDeclarationSchema.JE_HouseBill, ZString.Empty);
				}
				else
				{
					subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.Bill), CusDecHouseBillSchema.CU_JE);
					subQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, houseBill);
					ZQuery billTypeFilter = new ZQuery(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.SubHouseBill);
					billTypeFilter.AddToFilter(JoinCondition.Or, CusDecHouseBillSchema.CU_BillType, SQLComparisonOperator.Equal, BillTypeList.Codes.HouseBill);
					subQuery.AddToFilter(billTypeFilter);
					dbOnlyQuery = new ZDBOnlyQuery(typeof(Customs.Business.BaseJobDeclaration));
					dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(dbOnlyQuery);
				}

				if (!containerNumber.IsEmpty)
				{
					subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.BaseCusContainer), CusContainerSchema.CO_JE);
					subQuery.AddToFilter(JoinCondition.And, CusContainerSchema.CO_ContainerNumber, containerNumber);
					dbOnlyQuery = new ZDBOnlyQuery(typeof(Customs.Business.BaseJobDeclaration));
					dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(dbOnlyQuery);
				}

				query.AddToFilter(JobDeclarationSchema.JE_EntryStatus, SQLComparisonOperator.NotEqual, "");
				query.AddToFilter(JobDeclarationSchema.JE_EntryStatus, SQLComparisonOperator.NotEqual, "NSC");
				query.AddToFilter(JobDeclarationSchema.JE_IsCancelled, ZBool.False);

				JobDeclaration[] candidates = factory.Load<JobDeclaration>(query);
				foreach (JobDeclaration declaration in candidates)
				{
					if (declaration.CountryCode == Enterprise.Core.Constants.CountryCodes.NewZealand)
					{
						result = declaration.JE_EntryStatusDescription;
						break;
					}
				}
			}
			return result;
		}
	}
}
