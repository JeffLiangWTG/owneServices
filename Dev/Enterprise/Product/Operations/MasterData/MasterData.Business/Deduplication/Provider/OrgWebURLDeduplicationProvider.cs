using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgWebUrlDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(OrgWebURL);

		public Type GlowType => typeof(IOrgWebURL);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public string GroupNameForType => DeduplicationProvider.Constants.Websites;

		public string TablePrefix => OrgWebURLSchema.Constants.Prefix;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			var orgHeader = headerParent as DeduplicationOrgHeader;
			return (orgHeader?.OrgWebURLs.FirstOrDefault(x => x.PU_PK == childObjectPK)) as DeduplicationOrgWebURL;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return master.Cast<DeduplicationOrgHeader>().SelectMany(x => x.OrgWebURLs).FirstOrDefault(y => y.PU_PK == pk) as DeduplicationOrgWebURL;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			return ((DeduplicationOrgWebURL)source).PU_URL;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IOrgHeader>;
			var url = targets?.SelectMany(x => x.OrgWebURLs).FirstOrDefault(y => y.PU_PK == pk);
			return url?.PU_URL;
		}
	}
}
