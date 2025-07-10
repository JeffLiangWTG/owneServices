using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	public class NZJobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.NZ.IJobDeclarationFilterBusinessObject
	{
		public new NZJobDeclarationFilterLookups Lookups
		{
			get { return (NZJobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new NZJobDeclarationFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetModuleFiltersCore();

			ModuleTextFilter mafStatusFilter = new ModuleTextFilter(MAFStatusFilterName, GetMAFStatusQuery, new MessagingStatusList());
			mafStatusFilter.Category = FilterCategories.StatusAndFlags;
			mafStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.FormalEntry.NZJobDeclarationFilterBusinessObject|MAFStatusFilterName", "MPI Status");
			result.AddFilter(mafStatusFilter);

			StartsWithOnlyModuleTextFilter mafConsignmentFilter = new StartsWithOnlyModuleTextFilter(MAFConsignmentNumberFilterName, GetMAFConsignmentNumberQuery);
			mafConsignmentFilter.Category = FilterCategories.NumbersAndReferences;
			mafConsignmentFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.FormalEntry.NZJobDeclarationFilterBusinessObject|MAFConsignmentNumberFilterName", "MPI Consignment Number");
			result.AddCustomFilter(mafConsignmentFilter);

			var tswStatusFilter = new ModuleTextFilter(TSWCombinedStatusFilterName, GetTSWCombinedStatusQuery, new TSWEntryStatusList());
			tswStatusFilter.Category = FilterCategories.StatusAndFlags;
			tswStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.Declaration.FormalEntry.NZJobDeclarationFilterBusinessObject|TSWCombinedStatusFilterName", "TSW Combined Status");
			result.AddFilter(tswStatusFilter);

			return result;
		}

		public const string MAFStatusFilterName = "MPI Status";
		public const string MAFConsignmentNumberFilterName = "MPI Consignment Number";
		public const string ApplicationCodeName = "Message Mode";
		public const string TSWCombinedStatusFilterName = "TSW Combined Status";
		public override MultilingualString ApplicationCodeFilterCaption => ResString.GetMultilingualString("NZCustoms|DeclarationFilter|SubmitType", ApplicationCodeName);

		ZQuery GetAddInfoQuery(ZString key, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_AddInfo, SQLComparisonOperator.Contains, key + "=" + value);
		}

		ZQuery GetMAFStatusQuery(ZString value)
		{
			return GetAddInfoQuery("MAF_MessagingStatus", value);
		}

		ZQuery GetMAFConsignmentNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddInfoQuery("MAF_ConsignmentNumber", value);
		}

		ZQuery GetTSWCombinedStatusQuery(ZString value)
		{
			return GetAddInfoQuery("TSWCombinedStatus", value);
		}

		internal class StartsWithOnlyModuleTextFilter : ModuleTextFilter
		{
			public StartsWithOnlyModuleTextFilter(ZString description, GetTextQueryWithOperator queryDelegate)
				: base(description, queryDelegate)
			{
				ComparisonOperator_List.Clear();
				ComparisonOperator_List.AddPair(ComparisonConstants.StartsWith);
			}
		}
	}
}
