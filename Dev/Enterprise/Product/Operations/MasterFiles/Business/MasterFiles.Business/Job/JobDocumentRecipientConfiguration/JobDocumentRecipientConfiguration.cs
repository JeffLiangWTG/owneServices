using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class JobDocumentRecipientConfiguration : NonPersistentBusinessObject
	{
		public JobDocumentRecipientConfiguration(BusinessObjectFactory factory, IDocumentSupportable documentSupportable)
			: base(factory)
		{
			DocumentSupportable = documentSupportable;
		}
		public readonly IDocumentSupportable DocumentSupportable;

		public static JobDocumentRecipientConfiguration New(IDocumentSupportable documentSupportable)
		{
			return new JobDocumentRecipientConfiguration(new BusinessObjectFactory() { NameForDebugging = "JobDocumentRecipientConfiguration.New" }, documentSupportable);
		}

		#region Job Document Recipients

		public JobDocumentRecipientCollection JobDocumentRecipients
		{
			get
			{
				if (jobDocumentRecipients == null)
				{
					jobDocumentRecipients = new JobDocumentRecipientCollection(Factory, this, true);

					var jobDocumentDeliveries = new JobDocumentDeliveryCollection(Factory, DocumentSupportable.DocumentSupporter.BusinessObject);
					foreach (var jobDocumentDelivery in jobDocumentDeliveries)
					{
						jobDocumentRecipients.Add(new JobDocumentRecipientWrapperForJobDocumentDelivery(jobDocumentDelivery, this));
					}

					var jobDocumentExclusions = new JobDocumentExclusionCollection(Factory, DocumentSupportable.DocumentSupporter.BusinessObject);
					foreach (var jobDocumentExclusion in jobDocumentExclusions)
					{
						jobDocumentRecipients.Add(new JobDocumentRecipientWrapperForJobDocumentExclusion(jobDocumentExclusion, this));
					}
					RegisterEditableChildObject(jobDocumentRecipients);
				}
				return jobDocumentRecipients;
			}
		}
		JobDocumentRecipientCollection jobDocumentRecipients;

		#endregion

		#region Org Document Recipients

		public JobDocumentRecipientCollection OrgDocumentRecipients
		{
			get
			{
				if (orgDocumentRecipients == null)
				{
					orgDocumentRecipients = new JobDocumentRecipientCollection(Factory.GetCachedReadOnlyFactory(), this, false);
				}
				return orgDocumentRecipients;
			}
		}
		JobDocumentRecipientCollection orgDocumentRecipients;

		#endregion

		public SuggestedOrganisationCollection SuggestedOrganisations
		{
			get
			{
				if (suggestedOrganisations == null)
				{
					suggestedOrganisations = new SuggestedOrganisationCollection();
					if (DocumentSupportable.DocumentSupporter.BusinessObject is ISupportJobDocumentRecipient supporter && supporter.SuggestedOrganisations != null)
					{
						var suggestions = supporter.SuggestedOrganisations.Where(s => s.orgHeader != null).ToList();
						foreach (var suggestion in suggestions)
						{
							suggestedOrganisations.Add(new SuggestedOrganisation(suggestion.organisationType, suggestion.orgHeader));
						}
					}
				}
				return suggestedOrganisations;
			}
		}
		SuggestedOrganisationCollection suggestedOrganisations;

		public bool SupportsSuggestions => SuggestedOrganisations.Count > 0;

		#region Filters

		public void FilterOrgDocumentRecipients()
		{
			if (ShouldPerformSearch)
			{
				SortInfo info = OrgDocumentRecipients.SortInformation;
				OrgDocumentRecipients.RemoveAll();
				AddOrgDocumentRecipients();

				if (info != null)
				{
					OrgDocumentRecipients.Sort(info);
				}
			}
			else if (AreAllFiltersEmpty)
			{
				OrgDocumentRecipients.RemoveAll();
			}
		}

		void AddOrgDocumentRecipients()
		{
			if (DocumentFromFilter != null && OrganisationPKFilter.IsEmpty && DocumentGroupFilter.IsEmpty)
			{
				AddAutoDeliveryOrgDocumentRecipients();
			}
			else if (OrganisationFromFilter != null)
			{
				AddFilteredOrgDocumentRecipients();
			}
		}

		void AddAutoDeliveryOrgDocumentRecipients()
		{
			if (DocumentFromFilter == null)
			{
				return;
			}

			var docAutoDelivery = new DocAutoDelivery();
			var contacts = docAutoDelivery.GetDeliveryContacts(DocumentFromFilter, DocumentSupportable.DocumentSupporter);

			foreach (DocDeliveryContact contact in contacts)
			{
				if (contact.DocumentDeliveryType != null)
				{
					OrgDocumentRecipients.Add(new JobDocumentRecipientWrapperForOrgDocument(contact.DocumentDeliveryType, this));
				}
			}
		}

		void AddFilteredOrgDocumentRecipients()
		{
			if (OrganisationFromFilter == null)
			{
				return;
			}

			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, OrganisationFromFilter.PK);

			var orgContactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgDocumentSchema.OD_OC);
			orgContactSubQuery.AddToFilter(OrgContactSchema.OC_IsActive, true);
			orgContactSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			var orgDocumentQuery = new ZDBOnlyQuery(typeof(OrgDocument));
			if (DocumentFromFilter != null)
			{
				orgDocumentQuery.AddToFilter(OrgDocumentSchema.OD_SU_MenuItem, DocumentFromFilter.PK);
			}
			if (!DocumentGroupFilter.IsEmpty)
			{
				orgDocumentQuery.AddToFilter(OrgDocumentSchema.OD_DocumentGroup, DocumentGroupFilter);
			}
			orgDocumentQuery.AddSubQuery(orgContactSubQuery, JoinCondition.And);

			var orgDocuments = Factory.GetCachedReadOnlyFactory().Load<OrgDocument>(orgDocumentQuery);
			foreach (var orgDocument in orgDocuments)
			{
				OrgDocumentRecipients.Add(new JobDocumentRecipientWrapperForOrgDocument(orgDocument, this));
			}
		}

		public void ClearOrgDocumentRecipientsFilters()
		{
			DocumentPKFilter = ZGuid.Empty;
			OrganisationPKFilter = ZGuid.Empty;
			DocumentGroupFilter = ZString.Empty;
			OrgDocumentRecipients.RemoveAll();
		}

		bool ShouldPerformSearch => (DocumentPKFilter.IsValid || OrganisationPKFilter.IsValid)
			&& !DocumentPKFilterInfo.HasErrors()
			&& !OrganisationPKFilterInfo.HasErrors()
			&& !DocumentGroupFilterInfo.HasErrors();

		bool AreAllFiltersEmpty => DocumentPKFilter.IsEmpty && OrganisationPKFilter.IsEmpty && DocumentGroupFilter.IsEmpty;

		[List(nameof(Organisations))]
		public ZGuid OrganisationPKFilter
		{
			get => organisationPKFilter;
			set
			{
				if (organisationPKFilter != value)
				{
					organisationPKFilter = value;
					OrganisationPKFilterInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateOrganisationPKFilter();
						Validation.ValidateDocumentGroupFilter();
					}
				}
			}
		}
		ZGuid organisationPKFilter;

		public ZPropertyInfo OrganisationPKFilterInfo => GetZPropertyInfo(nameof(OrganisationPKFilter));

		internal OrgHeader OrganisationFromFilter => Factory.GetCachedReadOnlyFactory().Load<OrgHeader>(OrganisationPKFilter);

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		[List(nameof(DocumentGroups))]
		[MaxLength(3)]
		public ZString DocumentGroupFilter
		{
			get => documentGroupFilter;
			set
			{
				if (documentGroupFilter != value)
				{
					CheckMaximumLength(DocumentGroupFilterInfo, value);
					documentGroupFilter = value;
					DocumentGroupFilterInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentGroupFilter();
					}
				}
			}
		}
		ZString documentGroupFilter;

		public ZPropertyInfo DocumentGroupFilterInfo => GetZPropertyInfo(nameof(DocumentGroupFilter));

		public CodeDescriptionPairList DocumentGroups => OrgCodeLists.ContactType_List;

		[List(nameof(Documents))]
		public ZGuid DocumentPKFilter
		{
			get => documentPKFilter;
			set
			{
				if (documentPKFilter != value)
				{
					documentPKFilter = value;
					DocumentPKFilterInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateDocumentPKFilter();
						Validation.ValidateDocumentGroupFilter();
					}
				}
			}
		}
		ZGuid documentPKFilter;

		public ZPropertyInfo DocumentPKFilterInfo => GetZPropertyInfo(nameof(DocumentPKFilter));

		internal StmMenuItem DocumentFromFilter => Factory.GetCachedReadOnlyFactory().Load<StmMenuItem>(DocumentPKFilter);

		public UniqueDocumentCollectionView Documents
		{
			get
			{
				if (documents == null)
				{
					var applicableCommandPKs = GetApplicableDocumentCommandPKs(DocumentSupportable);
					documents = new UniqueDocumentCollectionView(Factory.GetCachedReadOnlyFactory(), applicableCommandPKs, DocumentSupportable.DocumentSupporter.BusinessContext.ToString());
				}
				return documents;
			}
		}
		UniqueDocumentCollectionView documents;

		List<ZGuid> GetApplicableDocumentCommandPKs(IDocumentSupportable parent)
		{
			var collection = ObjectFactory.Get<IDocumentCommandCollection>(nameof(IDocumentCommandCollection), parent);
			collection.Load();
			return GetApplicableDocumentCommandPKs(collection);
		}

		List<ZGuid> GetApplicableDocumentCommandPKs(IDocumentCommandCollection collection)
		{
			var filterEvaluatedResult = new Dictionary<string, bool>();
			var result = new List<ZGuid>();
			bool isDocumentCommandApplicable;

			foreach (IDocumentCommand documentCommand in collection)
			{
				if (filterEvaluatedResult.ContainsKey(documentCommand.SU_FilterList))
				{
					isDocumentCommandApplicable = filterEvaluatedResult[documentCommand.SU_FilterList];
				}
				else
				{
					isDocumentCommandApplicable = documentCommand.IsApplicable;
					filterEvaluatedResult.Add(documentCommand.SU_FilterList, isDocumentCommandApplicable);
				}

				if (isDocumentCommandApplicable)
				{
					result.Add(documentCommand.PK);
				}
			}
			return result;
		}

		#endregion

		public JobDocumentRecipientConfigurationValidation Validation => new JobDocumentRecipientConfigurationValidation(this);
	}
}
