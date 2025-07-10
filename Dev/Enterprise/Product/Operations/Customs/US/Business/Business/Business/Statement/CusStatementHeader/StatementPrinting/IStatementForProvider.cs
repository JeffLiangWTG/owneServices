using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	interface IStatementForProvider : IFactoryProvider
	{
		OrgHeader Importer { get; }
		ZString B2_PaymentType { get; }
		ZString B2_BranchDesignation { get; }
		GlbCompany Company { get; }
	}

	static class IStatementForProviderExtensions
	{
		public static ZString GetStatementFor(this IStatementForProvider provider)
		{
			IAddressDetails result = null;
			var paymentType = provider.B2_PaymentType;

			if (PaymentTypeList.IsPaidByImporter(paymentType))
			{
				var importer = provider.Importer;
				result = importer != null ? (IAddressDetails)importer.GetCustomsAddressDetailsFallingBackToMainAddress() : null;
			}
			else
			{
				var company = provider.Company;
				if (!provider.B2_BranchDesignation.IsEmpty)
				{
					var branch = provider.FindBranchByBranchDesignationFromRegistry();

					if (branch != null)
					{
						result = branch.OrgProxy != null ? branch.OrgProxy.GetCustomsAddressDetailsFallingBackToMainAddress() : branch;
					}
				}

				if (result == null)
				{
					result = company.OrgProxy != null ? company.OrgProxy.GetCustomsAddressDetailsFallingBackToMainAddress() : company;
				}
			}

			return result != null ? result.CompanyName : ZString.Empty;
		}

		public static GlbBranch FindBranchByBranchDesignationFromRegistry(this IStatementForProvider provider)
		{
			var company = provider.Company;
			if (!provider.B2_BranchDesignation.IsEmpty && company != null)
			{
				var activeBranches = company.ActiveBranches;
				var query = new ZDBOnlyQuery(typeof(StmData));
				query.AddToFilter(StmDataSchema.SD_Name, USCustomsDataRegistry.Instance.ClientBranchDesignation.Name);

				query.AddToFilter(StmDataSchema.SD_Owner, activeBranches.Select(x => x.PK));
				var branchDesignationBinary = new ZBlob(Encoding.Unicode.GetBytes(provider.B2_BranchDesignation));
				query.AddToFilter(JoinCondition.And, StmDataSchema.SD_BinaryValue, branchDesignationBinary);

				var item = provider.Factory.LoadTop1<StmData>(query);
				return item != null ? activeBranches.FirstOrDefault(x => x.PK == item.SD_Owner) : null;
			}
			return null;
		}

		public static GlbBranch GetRelatedBranch(this CusStatementHeader statement)
		{
			var result = statement.BranchByBranchDesignationFromRegistry;

			if (result == null)
			{
				if (statement.IsMonthlyStatement)
				{
					foreach (CusStatementHeader dailyStatement in statement.DailyStatements)
					{
						result = GetRelatedBranch(dailyStatement);
						if (result != null)
						{
							break;
						}
					}
				}

				if (result == null && statement.IsDailyStatement)
				{
					result = GetFirstRelatedDeclarationBranch(statement);
				}
			}

			return result;
		}

		static GlbBranch GetFirstRelatedDeclarationBranch(this CusStatementHeader dailyStatement)
		{
			foreach (CusStatementLine statementLine in dailyStatement.StatementLines)
			{
				var declaration = statementLine.Declaration;
				if (declaration != null && declaration.Branch != null)
				{
					return declaration.Branch;
				}
			}

			return null;
		}
	}
}
