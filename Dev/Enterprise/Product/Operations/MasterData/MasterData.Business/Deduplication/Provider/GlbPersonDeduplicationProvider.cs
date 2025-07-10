using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterData.Business
{
	[CodeAlive("Required for the creation of Contacts duplicate detection UI")]
	public class GlbPersonDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(GlbPerson);

		public string TablePrefix => GlbPersonSchema.Constants.Prefix;

		public Type GlowType => typeof(IGlbPerson);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public string GroupNameForType => DeduplicationProvider.Constants.Person;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			var person = headerParent as DeduplicationGlbPerson;
			return person;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false) => master;

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false) => master?.First(x => x.PK == pk);

		public string GetHeading(IDeduplicationGlowObject source)
		{
			var orgHeader = source as DeduplicationGlbPerson;
			return orgHeader.PER_FullName;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IDeduplicationGlowObject>;
			var glbPerson = targets.First(x => x.PK == pk) as DeduplicationGlbPerson;

			var header = string.Empty;
			var name = glbPerson.PER_FullName;

			if (headerType == HeaderType.Short)
			{
				header = name;
			}
			else if (headerType == HeaderType.Long)
			{
				var friendlyName = glbPerson.PER_FriendlyName;
				header = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", friendlyName, name);
			}

			return header;
		}
	}
}
