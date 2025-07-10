using System;
using System.Collections.Generic;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgCompanyDataComparer : IEqualityComparer<OrgCompanyData>
	{
		public static OrgCompanyDataComparer CompareByOrganization => lazyByOrganization.Value;

		[ThreadSafe]
		static readonly Lazy<OrgCompanyDataComparer> lazyByOrganization = new Lazy<OrgCompanyDataComparer>(() => new OrgCompanyDataComparerByOrganization());

		public abstract bool Equals(OrgCompanyData x, OrgCompanyData y);

		public abstract int GetHashCode(OrgCompanyData obj);

		class OrgCompanyDataComparerByOrganization : OrgCompanyDataComparer
		{
			public override bool Equals(OrgCompanyData x, OrgCompanyData y) =>
				x != null && y != null && x.OB_OH == y.OB_OH;

			public override int GetHashCode(OrgCompanyData orgCompanyData) =>
				orgCompanyData.OB_OH.GetHashCode();
		}
	}
}
