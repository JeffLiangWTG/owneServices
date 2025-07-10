using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgContactDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(OrgContact);

		public Type GlowType => typeof(IOrgContact);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public string GroupNameForType => DeduplicationProvider.Constants.Contacts;

		public string TablePrefix => OrgContactSchema.Constants.Prefix;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			return (headerParent?.OrgContacts?.FirstOrDefault(x => x.OC_PK == childObjectPK)) as DeduplicationOrgContact;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return master.Cast<IDeduplicationMaster>().SelectMany(x => x.OrgContacts).FirstOrDefault(y => y.OC_PK == pk) as DeduplicationOrgContact;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			return ((DeduplicationOrgContact)source).OC_ContactName;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IOrgHeader>;
			var targetOrg = targets.FirstOrDefault(x => x.OrgContacts.Select(y => y.OC_PK).Contains(pk));

			if (targetOrg != null)
			{
				var contact = targetOrg.OrgContacts.First(x => x.OC_PK == pk);
				return string.Join(" - ", contact.OC_ContactName, targetOrg.OH_FullName + " (" + targetOrg.OH_Code + ")");
			}

			return string.Empty;
		}
	}
}
