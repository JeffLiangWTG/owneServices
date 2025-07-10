using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyCountry : AutoAgencyCountry
	{
		public AgencyCountry(JobVoyage voyage, RefCountry country)
			: base(voyage == null ? null : voyage.Factory)
		{
			if (voyage == null)
			{
				throw new ArgumentNullException(nameof(voyage));
			}

			if (country == null)
			{
				throw new ArgumentNullException(nameof(country));
			}

			this.voyage = voyage;
			this.country = country;
		}

		#region Properties

		public override ZString CountryCode
		{
			get { return country.RN_Code; }
		}

		public override ZString CountryName
		{
			get { return country.RN_DescMultilingual; }
		}

		[ReadOnlyMember(nameof(IsScheduleAllocationEditDenied))]
		[List("Lookups.AllocationMethods")]
		public override ZString J0_AllocationMethod
		{
			get { return VoyageCountry.J0_AllocationMethod; }
			set
			{
				VoyageCountry.J0_AllocationMethod = value;
				J0_AllocationMethodInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateJ0_AllocationMethod();
				}
			}
		}

		[ReadOnlyMember(nameof(IsScheduleAllocationEditDenied))]
		public override ZBool J0_AllocationsByPrincipal
		{
			get { return VoyageCountry.J0_AllocationsByPrincipal; }
			set
			{
				VoyageCountry.J0_AllocationsByPrincipal = value;
				J0_AllocationsByPrincipalInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateJ0_AllocationsByPrincipal();
				}
			}
		}

		public bool IsScheduleAllocationEditDenied
		{
			get { return !Env.Security.SailingScheduleAllocationEdit.IsAllowed; }
		}

		#endregion

		#region Related BusinessObjects

		public VoyageCountry VoyageCountry
		{
			get { return voyage.Countries.GetCountry(country.RN_Code, true); }
		}

		public JobVoyage Voyage
		{
			get { return voyage; }
		}

		public OrgHeaderCollection Principals
		{
			get
			{
				if (principals == null)
				{
					CreatePrincipalCollections();
				}

				return principals;
			}
		}
		OrgHeaderCollection principals;

		public AgencyPrincipalCollection WrappedPrincipals
		{
			get
			{
				if (wrappedPrincipals == null)
				{
					CreatePrincipalCollections();
				}

				return wrappedPrincipals;
			}
		}
		AgencyPrincipalCollection wrappedPrincipals;

		public AgencyPrincipal GenericPrincipal
		{
			get
			{
				if (genericPrincipal == null)
				{
					genericPrincipal = new AgencyPrincipal(Factory);
					GenericPrincipal.Schedule = this;
					RegisterEditableChildObject(GenericPrincipal);
				}

				return genericPrincipal;
			}
		}
		AgencyPrincipal genericPrincipal;

		#endregion

		#region Operations

		public void RefreshAllUsageData()
		{
			GenericPrincipal.RefreshUsageData();

			foreach (AgencyPrincipal principal in WrappedPrincipals)
			{
				principal.RefreshUsageData();
			}
		}

		#endregion

		#region Lookups

		public AgencyCountryLookups Lookups
		{
			get { return lookups ?? (lookups = new AgencyCountryLookups(this)); }
		}
		AgencyCountryLookups lookups;

		#endregion

		#region Implementation

		void CreatePrincipalCollections()
		{
			principals = new OrgHeaderCollectionWithoutNew(Factory);
			LoadPrincipals();
			principals.CountChanged += new CollectionCountChangedEventHandler(principals_CountChanged);

			wrappedPrincipals = new AgencyPrincipalCollection(this);
			wrappedPrincipals.Load();
			RegisterEditableChildObject(wrappedPrincipals);
		}

		void principals_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateJ0_AllocationsByPrincipal();
			}
		}

		void LoadPrincipals()
		{
			if (VoyageCountry != null)
			{
				LoadPrincipalsFromSlotAllocations(VoyageCountry.SlotAllocations);
			}

			foreach (VoyageOrigin origin in voyage.Origins)
			{
				if (origin.JA_RL_NKPortOfLoading.Left(2) == country.Code)
				{
					LoadPrincipalsFromSlotAllocations(origin.SlotAllocations);
				}
			}

			foreach (JobSailing sailing in voyage.Sailings)
			{
				if (sailing.Origin != null && sailing.Origin.JA_RL_NKPortOfLoading.Left(2) == country.Code)
				{
					LoadPrincipalsFromSlotAllocations(sailing.SlotAllocations);
				}
			}
		}

		void LoadPrincipalsFromSlotAllocations(SlotAllocationDependentCollection collection)
		{
			foreach (SlotAllocation allocation in collection)
			{
				if (allocation.Principal != null)
				{
					Principals.Add(allocation.Principal);
				}

				Factory.AddFetchHint(JobSlotAllocationAspectSchema.D5_E0, allocation.PK);
			}
		}

		readonly JobVoyage voyage;
		readonly RefCountry country;

		#region OrgHeaderCollectionWithoutNew
		public class OrgHeaderCollectionWithoutNew : OrgHeaderCollection
		{
			public OrgHeaderCollectionWithoutNew(BusinessObjectFactory factory)
				: base(factory) { }

			protected override bool AllowNewCore
			{
				get { return false; }
			}
		}

		#endregion

		#endregion
	}
}
