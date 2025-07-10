using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class OrgAddressDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(OrgAddress);

		public Type GlowType => typeof(IOrgAddress);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public string GroupNameForType => DeduplicationProvider.Constants.Addresses;

		public string TablePrefix => OrgAddressSchema.Constants.Prefix;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			var orgHeader = headerParent as DeduplicationOrgHeader;

			return (orgHeader?.OrgAddresses.FirstOrDefault(x => x.OA_PK == childObjectPK)) as DeduplicationOrgAddress;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			var targets = master.Cast<IOrgHeader>();
			return targets?.SelectMany(x => x.OrgAddresses).FirstOrDefault(y => y.OA_PK == pk) as DeduplicationOrgAddress;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			return ((DeduplicationOrgAddress)source).OA_Code;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IOrgHeader>;
			var address = targets.SelectMany(x => x.OrgAddresses).FirstOrDefault(y => y.OA_PK == pk);
			return address?.OA_Code;
		}
	}
}
