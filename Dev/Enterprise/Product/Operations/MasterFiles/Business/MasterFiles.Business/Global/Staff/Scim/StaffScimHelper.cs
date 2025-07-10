using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	internal class StaffScimHelper
	{
		public StaffScimHelper(GlbStaff staff)
		{
			Staff = staff;
		}

		GlbStaff Staff { get; set; }

		List<ZGuid> scimGroupPKs;
		List<ZGuid> ScimGroupPKs
		{
			get
			{
				if (scimGroupPKs == null)
				{
					scimGroupPKs = Staff.Groups.Select(g => g.PK).ToList();
				}

				return scimGroupPKs;
			}
		}

		public bool IsPropertyMappedForScim(ZString columnName)
		{
			return SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.Value.Cast<StaffColumnToGroupDescriptionScimMapping>().Any(m => m.StaffColumnName.EqualsIgnoringCase(columnName));
		}

		public void SetScimGroupIfRequired(ZBool value, ZPropertyInfo info)
		{
			if (RegistryCollection.Count == 0)
			{
				return;
			}

			var mappedGroups = GetGroupsMappedToProperty(info);
			var mappedGroupPKs = new HashSet<ZGuid>(mappedGroups.Select(x => x.PK));

			if (!mappedGroups.Any())
			{
				return;
			}

			if (value)
			{
				var missingGroups = mappedGroupPKs.Except(ScimGroupPKs);

				foreach (var groupPK in missingGroups)
				{
					var glbGroupLink = Staff.Factory.New<GlbGroupLink>();
					glbGroupLink.GK_GG = groupPK;
					glbGroupLink.GK_GS = Staff.PK;
					Staff.RegisterEditableChildObject(glbGroupLink);
					ScimGroupPKs.Add(groupPK);
				}
			}
			else
			{
				var existingGroupPKs = ScimGroupPKs.Intersect(mappedGroupPKs);

				var glbGroupLinkQuery = new ZQuery(GlbGroupLinkSchema.GK_GS, Staff.PK);
				glbGroupLinkQuery.AddToFilter(GlbGroupLinkSchema.GK_GG, existingGroupPKs);

				var glbGroupLinks = Staff.Factory.Load<GlbGroupLink>(glbGroupLinkQuery);
				foreach(var link in glbGroupLinks)
				{
					ScimGroupPKs.Remove(link.GK_GG);
					Staff.RegisterEditableChildObject(link);
					link.Delete();
				}
			}

			info.RefreshBinding();
		}

		StaffColumnToGroupDescriptionScimMappingCollection registryCollection;
		StaffColumnToGroupDescriptionScimMappingCollection RegistryCollection
		{
			get
			{
				if (registryCollection == null)
				{
					registryCollection = SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.Value;
				}

				return registryCollection;
			}
		}

		IEnumerable<GlbGroup> GetGroupsMappedToProperty(ZPropertyInfo info)
		{
			var groupDescription = RegistryCollection
				.Cast<StaffColumnToGroupDescriptionScimMapping>()
				.Where(m => m.StaffColumnName.EqualsIgnoringCase(info.Name))
				.Select(i => i.GroupDescriptionMapping.ToUpperInvariant())
				.FirstOrDefault();

			if (string.IsNullOrEmpty(groupDescription))
			{
				return Enumerable.Empty<GlbGroup>();
			}

			var result = new List<GlbGroup>();

			var query = new ZQuery(GlbGroupSchema.GG_Desc, groupDescription);
			query.AddToFilter(GlbGroupSchema.GG_IsActive, true);

			return info.BizObj.Factory.Load<GlbGroup>(query);
		}
	}
}
