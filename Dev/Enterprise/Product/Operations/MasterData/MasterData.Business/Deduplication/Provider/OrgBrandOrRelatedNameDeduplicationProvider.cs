using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgBrandOrRelatedNameDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(OrgBrandOrRelatedName);

		public Type GlowType => typeof(IOrgBrandOrRelatedName);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Undefined;

		public string GroupNameForType => string.Empty;

		public string TablePrefix => OrgBrandOrRelatedNameSchema.Constants.Prefix;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			object brandOrRelatedName = null;

			switch (headerParent)
			{
				case DeduplicationOrgHeader deduplicationOrgHeader:
					brandOrRelatedName = deduplicationOrgHeader.OrgBrandOrRelatedNames?.FirstOrDefault(x => x.P1_PK == childObjectPK);
					break;
				case DeduplicationGlbPerson deduplicationGlbPerson:
					brandOrRelatedName = deduplicationGlbPerson.OrgContacts?.FirstOrDefault()?.OrgHeader?.OrgBrandOrRelatedNames?.FirstOrDefault(x => x.P1_PK == childObjectPK);
					break;
			}
			return brandOrRelatedName as DeduplicationOrgBrandOrRelatedName;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			DeduplicationOrgBrandOrRelatedName brand = null;
			if (master.First().BizoType == typeof(OrgHeader))
			{
				brand = master.Cast<DeduplicationOrgHeader>()?.SelectMany(x => x.OrgBrandOrRelatedNames).FirstOrDefault(y => y.P1_PK == pk) as DeduplicationOrgBrandOrRelatedName;
			}
			else if (master.First().BizoType == typeof(GlbPerson))
			{
				brand = master.Cast<DeduplicationGlbPerson>().Where(x => x.OrgContacts?.FirstOrDefault()?.OrgHeader?.OrgBrandOrRelatedNames != null).SelectMany(x => x.OrgContacts?.FirstOrDefault()?.OrgHeader?.OrgBrandOrRelatedNames).FirstOrDefault(y => y.P1_PK == pk) as DeduplicationOrgBrandOrRelatedName;
			}

			return brand;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			return ((DeduplicationOrgBrandOrRelatedName)source).P1_RelatedName;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IOrgHeader>;
			var brand = targets.SelectMany(x => x.OrgBrandOrRelatedNames).FirstOrDefault(y => y.P1_PK == pk);
			return brand?.P1_RelatedName;
		}
	}
}
