using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public GlbCompanyCollection Companies => Factory.GetCachedValue("TW.AsycudaManifestHeaderLookups.Companies", () => new GlbCompanyCollection(Factory, new ZQuery(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK)));

		public ICollection Locations => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<TWMessageStatusCodeList>();
	}
}
