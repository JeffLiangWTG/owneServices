using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgHeaderDeduplicationProvider : IDeduplicationBizoProvider
	{
		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public Type GlowType => typeof(IOrgHeader);

		public string GroupNameForType => DeduplicationProvider.Constants.Organisations;

		public Type BusinessObjectType => typeof(OrgHeader);

		public string TablePrefix => OrgHeaderSchema.Constants.Prefix;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			switch (headerParent)
			{
				case DeduplicationOrgHeader deduplicationOrgHeader:
					return deduplicationOrgHeader;
				case DeduplicationGlbPerson deduplicationGlbPerson:
					return (deduplicationGlbPerson.OrgContacts?.FirstOrDefault()?.OrgHeader) as DeduplicationOrgHeader;
			}

			return null;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			if (master.First().BizoType == typeof(OrgHeader))
			{
				return master.Cast<DeduplicationOrgHeader>()?.First(x => x.OH_PK == pk);
			}
			else if (master.First().BizoType == typeof(GlbPerson))
			{
				foreach (DeduplicationGlbPerson person in master)
				{
					if (person?.OrgContacts != null)
					{
						foreach (var contact in person.OrgContacts)
						{
							if (contact.OrgHeader?.OH_PK == pk)
							{
								return contact.OrgHeader as DeduplicationOrgHeader;
							}
						}
					}
				}
			}

			return null;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			var orgHeader = source as DeduplicationOrgHeader;
			return orgHeader.OH_Code;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType)
		{
			var targets = master as IEnumerable<IDeduplicationGlowObject>;
			var orgHeader = targets.First(x => x.PK == pk) as DeduplicationOrgHeader;

			var header = string.Empty;
			var name = orgHeader.OH_FullName;
			var code = orgHeader.OH_Code;

			if (headerType == HeaderType.Short)
			{
				header = code;
			}
			else if (headerType == HeaderType.Long)
			{
				header = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", code, name);
			}

			return header;
		}
	}
}
