using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class StaffCapabilityGroupHelper
	{
		public static IEnumerable<string> GetStaffCodesByPKs(BusinessObjectFactory factory, IEnumerable<ZGuid> pks)
		{
			var query = new ZDBOnlyQuery(typeof(GlbStaff));
			query.AddToFilter(GlbStaffSchema.PK, pks);
			return factory.Load<GlbStaff>(query).Select(s => s.GS_Code.ToString());
		}

		public static IDictionary<ZGuid, GlbGroup> GetGroupsByPKs(BusinessObjectFactory factory, IEnumerable<ZGuid> pks)
		{
			var query = new ZDBOnlyQuery(typeof(GlbGroup));
			query.AddToFilter(GlbGroupSchema.PK, pks);
			return factory.Load<GlbGroup>(query).ToDictionary(g => g.PK, g => g);
		}

		public static IDictionary<ZGuid, GlbCapability> GetCapabilitiesByPKs(BusinessObjectFactory factory, IEnumerable<ZGuid> pks)
		{
			var query = new ZDBOnlyQuery(typeof(GlbCapability));
			query.AddToFilter(GlbCapabilitySchema.PK, pks);
			return factory.Load<GlbCapability>(query).ToDictionary(c => c.PK, c => c);
		}

		public static IDictionary<ZGuid, IEnumerable<ZGuid>> GetStaffGroupedByCapabilitiesForGroup(BusinessObjectFactory factory, GlbGroup group)
		{
			var query = new ZDBOnlyQuery(typeof(GlbResourceCapabilityPivot));

			var capabilitySubQuery = new ZDBOnlySubQuery(typeof(GlbCapability), GlbCapabilitySchema.PK);
			capabilitySubQuery.AddToFilter(GlbCapabilitySchema.G4_CapacityScope, GlbCapabilityScopeList.Codes.GroupScope);
			query.AddSubQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, capabilitySubQuery, JoinCondition.And);

			var groupLinkSubQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
			groupLinkSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GG, group.PK);
			groupLinkSubQuery.AddToFilter(GlbGroupLinkSchema.GK_GS, group.Staff.Select(s => s.PK));
			query.AddSubQuery(GlbResourceCapabilityPivotSchema.G5_GS_Resource, groupLinkSubQuery, JoinCondition.And);

			var resourceCapabilities = factory.Load<GlbResourceCapabilityPivot>(query);
			return resourceCapabilities.ToLookup(pivot => pivot.G5_G4_Capability, pivot => pivot.G5_GS_Resource)
				.ToDictionary(lookup => lookup.Key, lookup => (IEnumerable<ZGuid>)lookup.ToList());
		}

		public static IDictionary<ZGuid, IEnumerable<ZGuid>> GetStaffGroupedByGroupsForCapability(BusinessObjectFactory factory, GlbCapability capability)
		{
			var resourceCapabilitySubQuery = new ZDBOnlySubQuery(typeof(GlbResourceCapabilityPivot), GlbResourceCapabilityPivotSchema.G5_GS_Resource);
			resourceCapabilitySubQuery.AddToFilter(GlbResourceCapabilityPivotSchema.G5_G4_Capability, capability.PK);
			resourceCapabilitySubQuery.AddToFilter(GlbResourceCapabilityPivotSchema.G5_GS_Resource, capability.ResourcesWithCapability.Select(s => s.PK));

			var capabilitySubQuery = new ZDBOnlySubQuery(typeof(GlbCapability), GlbCapabilitySchema.PK);
			capabilitySubQuery.AddToFilter(GlbCapabilitySchema.G4_CapacityScope, GlbCapabilityScopeList.Codes.GroupScope);
			resourceCapabilitySubQuery.AddSubQuery(GlbResourceCapabilityPivotSchema.G5_G4_Capability, capabilitySubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(GlbGroupLink));
			query.AddSubQuery(GlbGroupLinkSchema.GK_GS, resourceCapabilitySubQuery, JoinCondition.And);

			var staffGroups = factory.Load<GlbGroupLink>(query);
			return staffGroups.ToLookup(link => link.GK_GG, pivot => pivot.GK_GS)
				.ToDictionary(lookup => lookup.Key, lookup => (IEnumerable<ZGuid>)lookup.ToList());
		}

		public static IDictionary<string, IEnumerable<string>> GetLastStaffInGroupPerGRPCapability(BusinessObjectFactory factory, BusinessObject[] staffList, GlbGroup group)
		{
			var lastStaffInGroupCapabilities = new Dictionary<string, IEnumerable<string>>();
			var staffGroupedByCapabilities = GetStaffGroupedByCapabilitiesForGroup(factory, group);

			var capabilities = GetCapabilitiesByPKs(factory, staffGroupedByCapabilities.Keys);

			foreach (var staffByCapabilitySet in staffGroupedByCapabilities)
			{
				var matchingStaffSet = staffByCapabilitySet.Value.Where(v => staffList.Any(s => s.PK == v));
				if (staffByCapabilitySet.Value.Count() == matchingStaffSet.Count())
				{
					capabilities.TryGetValue(staffByCapabilitySet.Key, out var capability);
					if (capability == null)
					{
						continue;
					}

					lastStaffInGroupCapabilities[capability.G4_Code] = GetStaffCodesByPKs(factory, matchingStaffSet);
				}
			}

			return lastStaffInGroupCapabilities;
		}

		public static IDictionary<string, IEnumerable<string>> GetLastStaffWithGRPCapabilityPerGroup(BusinessObjectFactory factory, BusinessObject[] staffList, GlbCapability capability, bool disregardSecurityGroups = false)
		{
			var lastStaffInGroups = new Dictionary<string, IEnumerable<string>>();
			var staffGroupedByGroups = GetStaffGroupedByGroupsForCapability(factory, capability);
			var groups = GetGroupsByPKs(factory, staffGroupedByGroups.Keys);

			foreach (var staffByGroupSet in staffGroupedByGroups)
			{
				groups.TryGetValue(staffByGroupSet.Key, out var group);
				if (group == null || (disregardSecurityGroups && group.GG_IsSecurityEnabled))
				{
					continue;
				}

				var matchingStaffSet = staffByGroupSet.Value.Where(v => staffList.Any(s => s.PK == v));
				if (staffByGroupSet.Value.Count() == matchingStaffSet.Count())
				{
					lastStaffInGroups[group.GG_Code] = GetStaffCodesByPKs(factory, matchingStaffSet);
				}
			}

			return lastStaffInGroups;
		}

		public static IDictionary<string, IEnumerable<string>> GetGRPCapabilitiesWithLastStaffPerGroup(BusinessObjectFactory factory, BusinessObject[] groupList, ZGuid staffPK)
		{
			var groupCapabilities = new Dictionary<string, IEnumerable<string>>();

			foreach (var group in groupList)
			{
				var capabilitiesList = new List<string>();
				var staffGroupedByCapabilities = GetStaffGroupedByCapabilitiesForGroup(factory, group as GlbGroup);
				var capabilities = GetCapabilitiesByPKs(factory, staffGroupedByCapabilities.Keys);

				foreach (var staffByCapabilitySet in staffGroupedByCapabilities)
				{
					capabilities.TryGetValue(staffByCapabilitySet.Key, out var capability);
					if (capability == null)
					{
						continue;
					}

					var matchingStaffSet = staffByCapabilitySet.Value.Where(v => v == staffPK);
					if (staffByCapabilitySet.Value.Count() == matchingStaffSet.Count())
					{
						capabilitiesList.Add(capability.G4_Code);
					}
				}

				if (!capabilitiesList.IsNullOrEmpty())
				{
					groupCapabilities.Add((group as GlbGroup).GG_Code, capabilitiesList);
				}
			}

			return groupCapabilities;
		}

		public static IDictionary<string, IEnumerable<string>> GetGroupsWithLastStaffPerGRPCapability(BusinessObjectFactory factory, BusinessObject[] capabilityList, ZGuid staffPK, bool disregardSecurityGroups = false)
		{
			var groupCapabilities = new Dictionary<string, IEnumerable<string>>();

			foreach (var capability in capabilityList)
			{
				var groupsList = new List<string>();
				var staffGroupedByGroups = GetStaffGroupedByGroupsForCapability(factory, capability as GlbCapability);
				var groups = GetGroupsByPKs(factory, staffGroupedByGroups.Keys);

				foreach (var staffByGroupSet in staffGroupedByGroups)
				{
					groups.TryGetValue(staffByGroupSet.Key, out var group);
					if (group == null || (disregardSecurityGroups && group.GG_IsSecurityEnabled))
					{
						continue;
					}

					var matchingStaffSet = staffByGroupSet.Value.Where(v => v == staffPK);
					if (staffByGroupSet.Value.Count() == matchingStaffSet.Count())
					{
						groupsList.Add(group.GG_Code);
					}
				}

				if (!groupsList.IsNullOrEmpty())
				{
					groupCapabilities.Add((capability as GlbCapability).G4_Code, groupsList);
				}
			}

			return groupCapabilities;
		}
	}
}
