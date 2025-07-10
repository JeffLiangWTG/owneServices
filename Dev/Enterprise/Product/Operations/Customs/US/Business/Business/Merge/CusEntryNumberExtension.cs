using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public static class CusEntryNumberExtension
	{
		public static ZString GetEntryFilerCode(this CusEntryNumber entryNumber)
		{
			JobDeclaration declaration = entryNumber.GetJobDeclaration();

			return declaration != null ? declaration.US_EntryFilerCode : ZString.Empty;
		}

		public static GlbBranch GetBranch(this CusEntryNumber entryNumber)
		{
			GlbBranch result = null;
			JobDeclaration declaration = entryNumber.GetJobDeclaration();
			if (declaration != null)
			{
				result = declaration.Branch;
			}
			return result;
		}

		public static GlbCompany GetCompany(this CusEntryNumber entryNumber)
		{
			var branch = entryNumber.GetBranch();
			return branch != null ? branch.Company : null;
		}

		public static JobDeclaration GetJobDeclaration(this CusEntryNumber entryNumber)
		{
			JobDeclaration declaration = null;

			if (entryNumber.CE_ParentTable == JobDeclarationSchema.Constants.TableName)
			{
				declaration = entryNumber.Factory.Load<JobDeclaration>(entryNumber.CE_ParentID);
			}
			else if (entryNumber.CE_ParentTable == CusEntryHeaderSchema.Constants.TableName)
			{
				CusEntryHeader entryHeader = entryNumber.Factory.Load<CusEntryHeader>(entryNumber.CE_ParentID);
				declaration = entryHeader != null ? entryHeader.Declaration : null;
			}

			return declaration;
		}

		public static JobDeclaration GetJobDeclarationByEntryNumberAndFilerCode(BusinessObjectFactory factory, ZString entryNumber, ZString entryFilerCode)
		{
			if (!entryNumber.IsEmpty && !entryFilerCode.IsEmpty)
			{
				var declaratioQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				declaratioQuery.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);
				var modelViewHelper = new ModelViewColumnQueryHelper<JobDeclaration>();
				var entryFilerQuery = modelViewHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, JobDeclaration.ModelViewSchema.PK, JobDeclaration.ModelViewSchema.TableName, "JE_EntryFilerCode", SQLComparisonOperator.Equal, entryFilerCode);
				entryFilerQuery.AddToFilter(modelViewHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, JobDeclaration.ModelViewSchema.PK, JobDeclaration.ModelViewSchema.TableName_Recon, "JE_EntryFilerCode", SQLComparisonOperator.Equal, entryFilerCode), JoinCondition.Or);
				declaratioQuery.AddToFilter(entryFilerQuery, JoinCondition.And);

				var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
				entryNumberSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);

				declaratioQuery.AddSubQuery(entryNumberSubQuery, JoinCondition.And);
				declaratioQuery.OrderBy = JobDeclarationSchema.JE_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				return factory.LoadTop1<JobDeclaration>(declaratioQuery);
			}

			return null;
		}
	}
}
