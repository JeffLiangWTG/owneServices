using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.RulesEngine.Facts;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.ProductionRules.Business.Common;

namespace Enterprise.TransportConsignment.ProductionRulesEngine
{
	class BranchSelectionFactAccumulator
	{
		public BranchSelectionFactAccumulator()
		{
			OrgFacts = new Dictionary<ZGuid, IOrganisationWithMainAddressFact>();
			UnlocoFacts = new Dictionary<ZGuid, IUNLOCOFact>();
			CountryFacts = new Dictionary<ZGuid, ICountryFact>();
			AddressFacts = new Dictionary<ZGuid, IAddressFact>();
			StaffFacts = new Dictionary<ZGuid, IStaffFact>();
			DepartmentFacts = new Dictionary<Guid, IDepartmentFact>();
			BranchFacts = new Dictionary<Guid, IBranchFact>();
		}

		public IUNLOCOFact CreateUniqueUNLOCOFact(RefUNLOCO unloco)
		{
			var unlocoCountry = unloco.Country;
			if (unlocoCountry == null)
			{
				return null;
			}

			if (!UnlocoFacts.TryGetValue(unloco.PK, out var unlocoFact))
			{
				if (!CountryFacts.TryGetValue(unloco.Country.PK, out var originCountryFact))
				{
					CountryFacts[unloco.Country.PK] = originCountryFact = new CountryFact(unloco.Country);
				}
				UnlocoFacts[unloco.PK] = unlocoFact = new UNLOCOFact(unloco, originCountryFact);
			}

			return unlocoFact;
		}

		public IOrganisationWithMainAddressFact CreateUniqueOrganisationFact(OrgHeader org)
		{
			var address = org.MainAddress;
			var addressCountry = address.Country;
			if (addressCountry == null)
			{
				return null;
			}

			if (!OrgFacts.TryGetValue(org.PK, out var orgFact))
			{
				if (!CountryFacts.TryGetValue(addressCountry.PK, out var countryFact))
				{
					CountryFacts[addressCountry.PK] = countryFact = new CountryFact(addressCountry);
				}
				if (!AddressFacts.TryGetValue(address.PK, out var addressFact))
				{
					AddressFacts[address.PK] = addressFact = new AddressFact(address, countryFact);
				}
				OrgFacts[org.PK] = orgFact = new OrganisationWithMainAddressFact(org, addressFact);
			}

			return orgFact;
		}

		public IStaffFact CreateUniqueStaffFact(GlbStaff staff)
		{
			var homeDepartmentFact = CreateUniqueDepartmentFact(staff.HomeDepartment);

			if (!StaffFacts.TryGetValue(staff.PK, out var staffFact))
			{
				StaffFacts[staff.PK] = staffFact = new StaffFact(staff, homeDepartmentFact);
			}

			return staffFact;
		}

		public IDepartmentFact CreateUniqueDepartmentFact(IDepartment department)
		{
			if (department != null)
			{
				if (!DepartmentFacts.TryGetValue(department.PK, out var departmentFact))
				{
					DepartmentFacts[department.PK] = departmentFact = new DepartmentFact(department.PK, department.Code);
				}
				return departmentFact;
			}

			return null;
		}

		public IBranchFact CreateUniqueBranchFact(IBranch branch)
		{
			if (branch != null)
			{
				if (!BranchFacts.TryGetValue(branch.PK, out var branchFact))
				{
					BranchFacts[branch.PK] = branchFact = new BranchFact(branch.PK, branch.Code);
				}
				return branchFact;
			}
			return null;
		}

		Dictionary<ZGuid, IOrganisationWithMainAddressFact> OrgFacts { get; }
		Dictionary<ZGuid, IUNLOCOFact> UnlocoFacts { get; }
		Dictionary<ZGuid, ICountryFact> CountryFacts { get; }
		Dictionary<ZGuid, IAddressFact> AddressFacts { get; }
		Dictionary<ZGuid, IStaffFact> StaffFacts { get; }
		Dictionary<Guid, IDepartmentFact> DepartmentFacts { get; }
		Dictionary<Guid, IBranchFact> BranchFacts { get; }
	}
}
