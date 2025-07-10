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
	public class GlbStaffDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(GlbStaff);

		public string TablePrefix => GlbStaffSchema.Constants.Prefix;

		public Type GlowType => typeof(IGlbStaff);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public string GroupNameForType => DeduplicationProvider.Constants.Staff;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			return ((headerParent as DeduplicationGlbPerson)?.GlbStaffs?.FirstOrDefault(u => u.GS_PK == childObjectPK)) as DeduplicationGlbStaff;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			var targets = master.Cast<IGlbPerson>();
			return targets?.SelectMany(x => x.GlbStaffs).FirstOrDefault(y => y.GS_PK == pk) as DeduplicationGlbStaff;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			return (source as DeduplicationGlbStaff)?.GS_FullName;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IGlbPerson>;
			var staff = targets?.SelectMany(u => u.GlbStaffs).FirstOrDefault(x => x.GS_PK == pk);

			var header = string.Empty;
			var name = staff?.GS_FullName;

			if (headerType == HeaderType.Short)
			{
				header = name;
			}
			else if (headerType == HeaderType.Long)
			{
				var friendlyName = staff?.GS_FriendlyName;

				header = string.IsNullOrEmpty(friendlyName) ? name : string.Format(CultureInfo.InvariantCulture, "{0}: {1}", friendlyName, name);
			}

			return header;
		}
	}
}
