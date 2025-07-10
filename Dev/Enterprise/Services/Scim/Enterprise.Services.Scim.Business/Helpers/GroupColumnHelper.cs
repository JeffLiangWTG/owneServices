using System;
using CargoWise.Schema;
using Enterprise.Services.Scim.Models;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.Scim.Business
{
	internal static class GroupColumnHelper
	{
		internal static SchemaColumn GetGroupColumn(string scimColumn)
		{
			if (scimColumn.Equals(AttributeNames.DisplayName, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.GG_Desc;
			}

			if (scimColumn.Equals(AttributeNames.Active, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.GG_IsActive;
			}

			if (scimColumn.Equals(AttributeNames.ExternalId, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.GG_ExternalId;
			}

			if (scimColumn.Equals(AttributeNames.MetaCreated, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.GG_SystemCreateTimeUtc;
			}

			if (scimColumn.Equals(AttributeNames.MetaLastModified, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.GG_SystemLastEditTimeUtc;
			}

			if (scimColumn.Equals(AttributeNames.Id, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.PK;
			}

			if (scimColumn.Equals(AttributeNames.Category, StringComparison.InvariantCultureIgnoreCase))
			{
				return GlbGroupSchema.GG_Category;
			}

			return null;
		}
	}
}
