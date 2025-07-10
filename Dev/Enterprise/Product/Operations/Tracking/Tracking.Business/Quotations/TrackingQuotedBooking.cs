using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	[TestExcludeWorkflowProviderHasTestCase]
	public class TrackingQuotedBooking : QuotedBooking, IBizOChangesEmailNotification, IContainerListProvider
	{
		protected internal TrackingQuotedBooking(ZGuid quotePK, ZGuid bookingPK, bool attemptToLoadFromOther, BusinessObjectFactory factory)
			: base(quotePK, bookingPK, attemptToLoadFromOther, factory)
		{
		}

		#region GetNewQuotation

		public static TrackingQuotedBooking GetNewQuotation(BusinessObjectFactory factory, TrackingSiteUser siteUser)
		{
			Quote quote = CreateNewQuote(factory, QuoteState.NotApprovedAndNotAccepted);
			return GetNewQuotation(quote, siteUser);
		}

		public static TrackingQuotedBooking GetNewQuotation(Quote quote, TrackingSiteUser siteUser)
		{
			quote.Logs.AutoCreatedLogDefaultSL_Reference = siteUser.ContactAndCompanyReference;
			quote.TH_FollowUpDate = ZDateTime.Empty;
			quote.TH_OH = siteUser.CurrentOrg;
			quote.TH_QuoteEndDate = ZDate.Today.AddMonths(Env.Registry.Rating.WebRateValidityPeriod);
			quote.ShowApprovalDialog += (sender, e) => e.Cancel = true;

			if (!quote.Lookups.Companies.Contains(quote.Factory.Load<GlbCompany>(quote.TH_GC)) && quote.Lookups.Companies.Count > 0)
			{
				quote.TH_GC = quote.Lookups.Companies[0].PK;
			}

			var newDelegate = new NewDelegate((quotePK, bookingPK, attemptToLoadFromOther, factory) => new TrackingQuotedBooking(quotePK, bookingPK, attemptToLoadFromOther, factory));

			TrackingQuotedBooking spotQuote = null;

			using (new QuotedBookingsCreationContext(newDelegate))
			{
				spotQuote = (TrackingQuotedBooking)New(quote.PK, ZGuid.Empty, quote.Factory);
			}

			if (spotQuote != null)
			{
				spotQuote.WebServiceLevelCollection = () => new WebServiceLevelCollection(quote.Factory);
				spotQuote.Logs.AutoCreatedLogDefaultSL_Reference = siteUser.ContactAndCompanyReference;
			}

			if (WebDataRegistry.Instance.GetQuotesBasedOnPostalCodes.Value)
			{
				spotQuote.ConsignorDocumentaryAddress.SuspendValidation();
				spotQuote.ConsigneeDocumentaryAddress.SuspendValidation();
			}

			spotQuote.SiteUser = siteUser;

			return spotQuote;
		}

		class QuotedBookingsCreationContext : IDisposable
		{
			public QuotedBookingsCreationContext(NewDelegate newDelegate)
			{
				originalNewDelegate = OverridableNewDelegate.Value;
				OverridableNewDelegate.Value = newDelegate;
			}

			readonly NewDelegate originalNewDelegate;

			void IDisposable.Dispose()
			{
				OverridableNewDelegate.Value = originalNewDelegate;
			}
		}

		#endregion GetNewQuotation

		#region QuoteContainers

		[ChildEditable(true)]
		public TrackingQuoteContainerDependentCollection QuoteContainers
		{
			get
			{
				if (containers == null && Quote != null)
				{
					containers = new TrackingQuoteContainerDependentCollection(this, false);
					containers.Load();

					Quote.RegisterEditableChildObject(containers);
				}

				return containers;
			}
		}
		TrackingQuoteContainerDependentCollection containers;

		#endregion

		#region IBizOChangesEmailNotification Members

		ZGuid IBizOChangesEmailNotification.PK
		{
			get { return Quote.PK; }
		}

		ZString IBizOChangesEmailNotification.HumanReadableName
		{
			get { return Res.GetString("71bb5f9a-a427-4b5d-b2c4-1b5a81147987", "Quote {0}", Quote.TH_QuoteNumber); }
		}

		ZString IBizOChangesEmailNotification.Number
		{
			get { return Quote.TH_QuoteNumber; }
		}

		OrgContact IBizOChangesEmailNotification.LoggedInContact
		{
			get { return SiteUser.LoggedInUser; }
		}

		ZBool IBizOChangesEmailNotification.IsCancelled
		{
			get { return Quote.IsCancelled; }
		}

		GlbBranch IBizOChangesEmailNotification.EventBranch
		{
			get
			{
				var controllingBranch = GlbBranch.FindControllingBranchWithFallBackToAnyCompany(SiteUser.LoggedInOrganisation);
				var shouldUseControllingBranch = controllingBranch != null &&
					(Quote.Company == null ||
					!Quote.Company.ActiveBranches.Any() ||
					Quote.Company.ActiveBranches.Any(b => b.GB_Code == controllingBranch.GB_Code));

				return shouldUseControllingBranch ? controllingBranch : Quote.Company?.FirstActiveBranch;
			}
		}

		GuidRegistryItem IBizOChangesEmailNotification.EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.QuoteNotificationEmailGroup; }
		}

		bool IBizOChangesEmailNotification.IsInDatabase
		{
			get { return Quote.IsInDatabase; }
		}

		bool IBizOChangesEmailNotification.IsDeleted
		{
			get { return Quote.IsDeleted; }
		}

		bool IBizOChangesEmailNotification.HasChanges
		{
			get { return Quote.HasChanges; }
		}

		void IBizOChangesEmailNotification.AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("2ddb170f-834f-425c-8ed2-e5650c8fc92a", "Request quote from"), Quote.Company != null ? Quote.Company.GC_Name : ZString.Empty);

			if (ConsignorDocumentaryAddress != null)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("29180690-cfc7-4ddf-bcde-a88967289f51", "Pickup"), ConsignorDocumentaryAddress.E2_CompanyNameTruncated);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("0490ceb7-193a-402e-b6d7-b3ebf747a8cd", "Pickup Address"), ConsignorDocumentaryAddress.AddressAsASingleLine);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("0383543f-6b36-4923-8e58-4cb62f8e3b85", "Pickup Contact"), ConsignorDocumentaryAddress.E2_Contact);
			}

			if (ConsigneeDocumentaryAddress != null)
			{
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("0d168721-dcfa-493d-87ae-5a67999e341b", "Delivery"), ConsigneeDocumentaryAddress.E2_CompanyNameTruncated);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("8a8a83d6-7d3c-443d-8801-f0d6562b93cd", "Delivery Address"), ConsigneeDocumentaryAddress.AddressAsASingleLine);
				PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f4d4c1b9-054a-4f57-8716-aa0b0b2f02fa", "Delivery Contact"), ConsigneeDocumentaryAddress.E2_Contact);
			}

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("04771e96-281d-41d8-a79d-85ad3a6fe899", "Origin"), Origin);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("272a23e5-427b-4e9b-af10-177256288cfd", "Destination"), Destination);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("51e34d1a-2a17-4f43-a73f-5599998edc02", "Mode"), Mode, Modes);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("7d0a8b3d-fb26-4e10-b4e5-cb474fd5e9e5", "Payment Term"), PaymentTerms);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("626abd98-dfa4-4fe9-9917-ef7e5cf368ac", "Service Level"), ServiceLevel);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("397bc5bd-da4f-44dc-8552-929270d53e88", "Weight"), ZString.Format("{0} {1}", Weight, WeightUnit));
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("bd5fb87a-f77a-4ad9-b23c-b6f4026a9506", "Volume"), ZString.Format("{0} {1}", Volume, VolumeUnit));
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("1d746ce0-5e58-4d40-879b-1d512d1ae243", "Commodity"), Commodity);

			if (Quote.CurrentOneOffQuote != null)
			{
				AddContainersForEmailReporting(state, Quote.CurrentOneOffQuote.Containers);
				AddLooseCargoForEmailReporting(state, Quote.CurrentOneOffQuote.LooseCargo);
			}
		}

		PropertyChangeInfo[] IBizOChangesEmailNotification.GetPropertiesForEmailReporting()
		{
			return PropertiesForEmailReporting.GetValuesAsArray();
		}

		ControllerID IBizOChangesEmailNotification.ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.QuotedBookings; }
		}

		OrgHeader IBizOChangesEmailNotification.RelatedOrg
		{
			get { return SiteUser.LoggedInOrganisation; }
		}

		ZGuid IBizOChangesEmailNotification.GetStaffGuid(OrgStaffAssignmentsCollection staffAssignments, CodeDescriptionBool role)
		{
			var staffNK = staffAssignments.GetStaffAssignment(role.Code,
				this.IsImport() ? OrgStaffAssignmentsCollection.Direction.Import : OrgStaffAssignmentsCollection.Direction.Export,
				TransportMode == Core.Constants.TransportModes.Air ? OrgStaffAssignmentsCollection.AirSea.Air : OrgStaffAssignmentsCollection.AirSea.Sea);

			var staff = staffAssignments.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);

			return staff == null ? ZGuid.Empty : staff.PK;
		}

		CodeDescriptionBoolRegistryItem IBizOChangesEmailNotification.StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.QuoteNotificationStaffRoles; }
		}

		CodePairRegistryItem IBizOChangesEmailNotification.NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.QuoteNotificationOptions; }
		}

		#region Implementation

		PropertyChangeInfoCollection PropertiesForEmailReporting
		{
			get { return propertiesForEmailReporting ?? (propertiesForEmailReporting = new PropertyChangeInfoCollection()); }
		}
		PropertyChangeInfoCollection propertiesForEmailReporting;

		OrgContactWebUser SiteUser
		{
			get { return siteUser ?? (OrgContactWebUser)WebEnv.AppInstance.SiteUser; }
			set { siteUser = value; }
		}
		OrgContactWebUser siteUser;

		void AddContainersForEmailReporting(DataState state, RateOneOffContainersCollection collection)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				RateOneOffContainers container = collection[i];

				var name = ResString.GetMultilingualString("e4947c55-53f7-4395-98cf-2d109a247919", "Container #{0}", i + 1);
				var value = ResString.GetMultilingualString("d36a297d-0deb-4fb3-b839-c46afead6d43", "Count={0}, Type={1}", container.TC_ContainerCount, container.Container.RC_Code);

				PropertiesForEmailReporting.Add(state, name, value);
			}
		}

		void AddLooseCargoForEmailReporting(DataState state, RateOneOffPackLineCollection collection)
		{
			for (var i = 0; i < collection.Count; i++)
			{
				var container = collection[i];

				var name = ResString.GetMultilingualString("a2d78f41-3cdf-409f-aac5-40ab52902bfe", "Loose Cargo #{0}", i + 1);
				var value = ResString.GetMultilingualString("e2794461-5b01-4842-a9f7-bb2873126ab2", "Count={0}, Length={1}, Width={2}, Height={3}", container.TPL_PackLineCount, container.TPL_Length, container.TPL_Width, container.TPL_Height);

				PropertiesForEmailReporting.Add(state, name, value);
			}
		}

		#endregion

		#endregion

		[BusinessObjectTestExclude]
		public GlbCompanyCollection Companies
		{
			get
			{
				if (companies == null)
				{
					TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
					if (siteUser != null && siteUser.LoggedInOrganisation != null)
					{
						ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_GC);
						subQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, siteUser.LoggedInOrganisation.PK);
						subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, true);

						ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(GlbCompany));
						filter.AddSubQuery(GlbCompanySchema.PK, subQuery, JoinCondition.And);

						companies = new GlbCompanyCollection(Factory);
						companies.AdditionalFilter = filter;
						if (companies.Count == 0)
						{
							companies = Quote.Lookups.Companies;
						}
					}
				}
				return companies;
			}
		}
		GlbCompanyCollection companies;

		#region IContainerListProvider

		public RefContainerCollection Container_List
		{
			get { return new ContainerHelper(Factory).List(TransportMode); }
		}

		#endregion
	}
}
