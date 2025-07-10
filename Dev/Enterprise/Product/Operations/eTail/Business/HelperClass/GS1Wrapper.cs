using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.Ecommerce.Business;

namespace Enterprise.eTail.Business
{
	public class GS1Wrapper
	{
		public GS1Wrapper(OrgHeader gs1Org, ZString gs1Prefix, INumberFountainProxy numberFountain)
		{
			GS1Org = Argument.NotNull(gs1Org, nameof(gs1Org));
			GS1Prefix = gs1Prefix;
			SSCCNumberFountain = Argument.NotNull(numberFountain, nameof(numberFountain));
		}

		public OrgHeader GS1Org { get; }
		public ZString GS1Prefix { get; }
		public INumberFountainProxy SSCCNumberFountain { get; }

		public string GenerateSSCCNumber(BusinessObjectFactory factory)
		{
			return SSCCNumberFountain?.GetNextFormatted(factory);
		}

		public static GS1Wrapper GetGS1Info(OrgAddress orgAddress, BusinessObjectFactory factory) => GetGS1Info(orgAddress?.Header, orgAddress, factory);

		public static GS1Wrapper GetGS1Info(OrgHeader consignorOrg, OrgAddress consignorOrgAddress, BusinessObjectFactory factory)
		{
			GS1Wrapper result = null;

			if (consignorOrgAddress != null)
			{
				result = GetGS1InfoForOrg(consignorOrg, consignorOrgAddress);
			}

			return result ?? GetGS1InfoForOrg(factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy));
		}

		static GS1Wrapper GetGS1InfoForOrg(OrgHeader orgHeader, OrgAddress preferredOrgAddress = null)
		{
			if (orgHeader != null)
			{
				var connection = Db.NewExtraConnectionToMainDb() as IDbConnectionInternals;

				var numberFountainFactory = HVLVSSCCNumberFountainSupporter.GetSSCCNumberFountain(orgHeader.PK.ToGuid(),
																		preferredOrgAddress?.PK.ToGuid(),
																		Env.CurrentBranch.NKUNLOCO,
																		connection.InternalDbConnection);

				if (numberFountainFactory != null)
				{
					var numberFountain = numberFountainFactory.New();
					return new GS1Wrapper(orgHeader, numberFountainFactory.Prefix, numberFountain.Wrap());
				}
			}

			return null;
		}
	}
}
