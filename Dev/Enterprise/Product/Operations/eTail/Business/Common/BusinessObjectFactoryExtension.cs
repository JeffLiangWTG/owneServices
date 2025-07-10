using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Business
{
	public static class BusinessObjectFactoryExtension
	{
		public static ZString StripNonWesternEuropeanCharactersIfRequired(this BusinessObjectFactory factory, ZString nvarcharValue)
		{
			if (factory.ServiceContainer.GetService<NonWesternEuropeanCharactersRemovalService>() != null)
			{
				nvarcharValue = nvarcharValue.StripNonWesternEuropeanCharacters();
			}

			return nvarcharValue;
		}

		public static IEnumerable<T> GetAddedBusinessObjects<T>(this BusinessObjectFactory factory, bool excludeInactive = true)
		{
			var tableName = BusinessObjectFactory.GetTableNameFromType(typeof(T));
			var table = GetTableIfItHasNewRows(factory, tableName);
			if (table != null)
			{
				var result = table.Rows
					.OfType<DataRow>()
					.Where(x => x.RowState == DataRowState.Added)
					.Select(x => factory.GetBizOsForDataRow(x).First(b => b.TableName == tableName))
					.OfType<T>();

				if (excludeInactive)
				{
					result = result.Where(x => !(x as ICancellable)?.IsCancelled ?? true);
				}

				return result;
			}

			return Enumerable.Empty<T>();
		}

		public static DataTable GetTableIfItHasNewRows(this BusinessObjectFactory factory, string tableName)
		{
			var table = ((INeedDataSet)factory).Data.Tables[tableName];
			if (table != null && table.GetChanges(DataRowState.Added) != null)
			{
				return table;
			}

			return null;
		}

		public static GlbBranch GetBranchFromStaffCode(this BusinessObjectFactory factory, string staffCode)
		{
			var query = GetBranchQuery(staffCode, GlbStaffSchema.GS_GB_LastLogonBranch);
			var branch = factory.LoadTop1<GlbBranch>(query);

			if (branch == null)
			{
				var fallbackQuery = GetBranchQuery(staffCode, GlbStaffSchema.GS_GB_HomeBranch);
				branch = factory.LoadTop1<GlbBranch>(fallbackQuery);
			}

			return branch;
		}

		static ZDBOnlyQuery GetBranchQuery(string staffCode, SchemaGuidColumn subQueryColumn)
		{
			var query = new ZDBOnlyQuery(typeof(GlbBranch));
			var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), subQueryColumn);
			staffSubQuery.AddToFilter(GlbStaffSchema.GS_Code, staffCode);
			query.AddSubQuery(GlbBranchSchema.PK, staffSubQuery, JoinCondition.And);
			return query;
		}
	}
}
