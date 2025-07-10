using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class HRJobApplicantDeduplicationProvider : IDeduplicationBizoProvider
	{
		public Type BusinessObjectType => typeof(Integration.Recruiter.IHRJobApplicant);

		public string TablePrefix => HRJobApplicantSchema.Constants.Prefix;

		public Type GlowType => typeof(IHRJobApplicant);

		public DeduplicationDisplayMode DisplayModeForType => DeduplicationDisplayMode.Detailed;

		public string GroupNameForType => DeduplicationProvider.Constants.Applicant;

		public IDeduplicationGlowObject GetChildObject(IDeduplicationMaster headerParent, Guid childObjectPK)
		{
			return ((headerParent as DeduplicationGlbPerson)?.HRJobApplicants?.FirstOrDefault(u => u.HA_PK == childObjectPK)) as DeduplicationHRJobApplicant;
		}

		public IDeduplicationGlowObject GetComparisonSource(IDeduplicationGlowObject master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			return GetComparisonSource(new List<IDeduplicationGlowObject> { master }, pk, columnNames, standardizeDomains);
		}

		public IDeduplicationGlowObject GetComparisonSource(IEnumerable<IDeduplicationGlowObject> master, Guid pk, IEnumerable<string> columnNames, bool standardizeDomains = false)
		{
			var targets = master.Cast<IGlbPerson>();
			return targets?.SelectMany(x => x.HRJobApplicants).FirstOrDefault(y => y.HA_PK == pk) as DeduplicationHRJobApplicant;
		}

		public string GetHeading(IDeduplicationGlowObject source)
		{
			return (source as DeduplicationHRJobApplicant)?.HA_FullName;
		}

		public string GetHeading(object master, Guid pk, HeaderType headerType = HeaderType.Short)
		{
			var targets = master as IEnumerable<IGlbPerson>;
			var applicant = targets?.SelectMany(u => u.HRJobApplicants).FirstOrDefault(x => x.HA_PK == pk);

			return applicant?.GlbPerson?.PER_FullName;
		}
	}
}
