using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationAttemptFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddAttemptFilters(result);
			AddPersonFilters(result);
			AddAccreditationFilters(result);
			return result;
		}

		void AddAttemptFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.CommencementDate, GlbAccreditationAttemptSchema.HAA_CommencementDate).MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|CommencementDate", GlbAccreditationAttemptFilterProvider.FilterDescription.CommencementDate);
			filters.AddDateFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.CompletionDueDate, GlbAccreditationAttemptSchema.HAA_CompletionDueDate).MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|CompletionDueDate", GlbAccreditationAttemptFilterProvider.FilterDescription.CompletionDueDate);
			filters.AddDateFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.CompletionDate, GlbAccreditationAttemptSchema.HAA_CompletionDate).MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|CompletionDate", GlbAccreditationAttemptFilterProvider.FilterDescription.CompletionDate);
			filters.AddDateFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.ExpiryDate, GlbAccreditationAttemptSchema.HAA_ExpiryDate).MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|ExpiryDate", GlbAccreditationAttemptFilterProvider.FilterDescription.ExpiryDate);
			filters.AddTextFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.Status, AccreditationAttemptFilterProvider.GetStatusQuery, AccreditationAttemptFilterProvider.StatusList).MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|Status", GlbAccreditationAttemptFilterProvider.FilterDescription.Status);
		}

		void AddPersonFilters(ModuleFilterCollection filters)
		{
			var personSubGroup = new GlbAccreditationAttemptFilterProvider.PersonSubGroup();

			var personFilter = filters.AddGuidFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.Person, ModuleIDs.GlbPerson, GlbAccreditationAttemptSchema.HAA_PER, AccreditationAttemptFilterProvider.Persons);
			personFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|Person", GlbAccreditationAttemptFilterProvider.FilterDescription.Person);

			var personFullNameFilter = filters.AddTextFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.PersonFullName, GlbPersonSchema.PER_FullName);
			personFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|PersonFullName", GlbAccreditationAttemptFilterProvider.FilterDescription.PersonFullName);
			personFullNameFilter.SubGroup = personSubGroup;

			var emailFilter = filters.AddTextFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.EmailAddress, AccreditationAttemptFilterProvider.GetEmailQuery());
			emailFilter.MaxLength = GlbPersonSchema.PER_EmailAddress.MaxLength;
			emailFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|EmailAddress", GlbAccreditationAttemptFilterProvider.FilterDescription.EmailAddress);
			emailFilter.SubGroup = personSubGroup;

			var orgFilter = filters.AddTextFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.RelatedOrganizationName, AccreditationAttemptFilterProvider.GetOrganizationQuery());
			orgFilter.MaxLength = OrgAddressSchema.OA_CompanyNameOverride.MaxLength;
			orgFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|RelatedOrganizationName", GlbAccreditationAttemptFilterProvider.FilterDescription.RelatedOrganizationName);
			orgFilter.SubGroup = personSubGroup;

			var locationFilter = filters.AddNkFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.WorkingLocation, AccreditationAttemptFilterProvider.GetLocationQuery, ModuleIDs.Location, AccreditationAttemptFilterProvider.Locations);
			locationFilter.MaxLength = Math.Min(OrgAddressSchema.OA_RL_NKRelatedPortCode.MaxLength, OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength);
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|WorkingLocation", GlbAccreditationAttemptFilterProvider.FilterDescription.WorkingLocation);
			locationFilter.SubGroup = personSubGroup;
		}

		void AddAccreditationFilters(ModuleFilterCollection filters)
		{
			var accreditationSubGroup = new GlbAccreditationAttemptFilterProvider.AccreditationSubGroup();
			var accreditationCodeFilter = filters.AddNkFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.AccreditationCode, GlbAccreditationSchema.HAC_Code, ModuleIDs.GlbAccreditation, AccreditationAttemptFilterProvider.Accreditations);
			accreditationCodeFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|AccreditationCode", GlbAccreditationAttemptFilterProvider.FilterDescription.AccreditationCode);
			accreditationCodeFilter.SubGroup = accreditationSubGroup;

			var groupFilter = new GlbAccreditationHighestLevelByProgramFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram)
			{
				MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|HighestLevelReachedByProgram", GlbAccreditationAttemptFilterProvider.FilterDescription.HighestLevelReachedByProgram)
			};
			filters.AddCustomFilter(groupFilter);

			var webPublishedFilter = filters.AddFlagsFilter(GlbAccreditationAttemptFilterProvider.FilterDescription.WebPublished, new string[] { ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|WebPublished", "Web Published") }, new GetFlagsQuery[] { GetWebPublishedFilter });
			webPublishedFilter.SubGroup = accreditationSubGroup;
			webPublishedFilter.MultilingualDescription = ResString.GetMultilingualString("Recruiter|GlbAccreditationAttempt|WebPublished", "Web Published");
		}

		ZQuery GetWebPublishedFilter(ZBool value)
		{
			return new ZQuery(GlbAccreditationSchema.HAC_IsWebPublished, value);
		}

		public GlbAccreditationAttemptFilterProvider AccreditationAttemptFilterProvider
		{
			get { return accreditationAttemptFilterProvider ?? (accreditationAttemptFilterProvider = new GlbAccreditationAttemptFilterProvider()); }
		}
		GlbAccreditationAttemptFilterProvider accreditationAttemptFilterProvider;
	}
}
