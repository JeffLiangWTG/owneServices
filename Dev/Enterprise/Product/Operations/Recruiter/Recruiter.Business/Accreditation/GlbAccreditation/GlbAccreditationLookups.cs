//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbAccreditationLookups
//
//    This class should be used for overriding collections in AutoGlbAccreditationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationLookups : AutoGlbAccreditationLookups
	{
		public GlbAccreditationLookups(AutoGlbAccreditation parent) : base(parent)
		{
		}

		public GlbAccreditationCollection AccreditationList
		{
			get
			{
				return Factory.GetCachedValue("GlbAccreditationLookups.GlbAccreditationCollection", () =>
				{
					return new GlbAccreditationCollection(Factory, new ZQuery(GlbAccreditationSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK));
				});
			}
		}

		public CodeDescriptionBoolCollection CertificateCodesList
		{
			get { return RecruiterDataRegistry.Instance.CertificateTypesExtra.Value; }
		}

		public CodeDescriptionPairList RefresherCertExpirationTypesList => Factory.GetCachedValue("GlbAccreditationLookups.RefresherCertExpirationTypesList", () => new RefresherCertExpirationTypes());
	}
}
