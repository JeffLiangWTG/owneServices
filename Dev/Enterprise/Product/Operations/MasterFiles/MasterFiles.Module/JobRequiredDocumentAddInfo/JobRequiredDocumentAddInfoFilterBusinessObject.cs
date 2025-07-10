using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class JobRequiredDocumentAddInfoFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public static class Constants
		{
			public const string Company = "Company";
			public const string ApplicationCode = "Application Code";
			public const string DocumentNumber = "Document Number";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();
			AddHiddenFilters(collection);
			var documentNoFilter = collection.AddTextFilter(Constants.DocumentNumber, DocumentNumberFilterQuery);
			documentNoFilter.Category = FilterCategories.NumbersAndReferences;
			documentNoFilter.MultilingualDescription = ResString.GetMultilingualString("A88CCCBE-D515-4F07-B68D-49A3DD0347CE", "Document Number");
			return collection;
		}

		ZQuery DocumentNumberFilterQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobRequiredDocumentAddInfo));
			var requiredDocumentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
			requiredDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_DocNumber, comparisonOperator, value);
			result.AddSubQuery(requiredDocumentQuery, JoinCondition.And);
			return result;
		}

		public CodeDescriptionPairList ApplicationCode_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList();
				list.AddPair(Core.Constants.Customs.DocumentImageSystemIDs.US_DIS);
				list.AddPair(Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF);
				return list;
			}
		}

		void AddHiddenFilters(ModuleFilterCollection collection)
		{
			ModuleGuidFilter hiddenGuidFilter = collection.AddGuidFilter(Constants.Company, ModuleIDs.GlbCompany, JobRequiredDocumentAddInfoSchema.EX_GC_Company, Companies);
			hiddenGuidFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			hiddenGuidFilter.Property = GlbCompany.CurrentCompany.PK;
			hiddenGuidFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
			hiddenGuidFilter.MultilingualDescription = ResString.GetMultilingualString("8E02802B-4F98-4B48-807B-A9D68D918ADD", "Company");

			var categoryFilter = collection.AddTextFilter(Constants.ApplicationCode, JobRequiredDocumentAddInfoSchema.EX_ApplicationCode, ApplicationCode_List);
			categoryFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			categoryFilter.Category = FilterCategories.ModesAndTypes;
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("6B5CF7B6-A232-46AD-B65B-2A283369F1A2", "Application Code");
		}

		GlbCompanyCollection Companies
		{
			get { return fCompanies ?? (fCompanies = new GlbCompanyCollection(Factory)); }
		}

		GlbCompanyCollection fCompanies;
	}
}
