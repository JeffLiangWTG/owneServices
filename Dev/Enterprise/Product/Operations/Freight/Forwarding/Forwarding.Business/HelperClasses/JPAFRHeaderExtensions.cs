using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class JPAFRHeaderExtensions
	{
		public static Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader GetAFRHeader(this ForwardingConsol consol, bool alwaysLoadFromDb = true)
		{
			Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader result = null;
			if (consol != null)
			{
				var query = new ZQuery(JPAFRHeaderSchema.JPH_ParentId, consol.PK);
				query.AddToFilter(JPAFRHeaderSchema.JPH_IsShippingLineEntry, ZBool.False);
				query.AddToFilter(JPAFRHeaderSchema.JPH_ParentTableCode, consol.TablePrefix);
				query.OrderBy = JPAFRHeaderSchema.JPH_SystemCreateTimeUtc.Name;
				query.ReLoadExistingRows = alwaysLoadFromDb;
				result = consol.Factory.LoadTop1<Enterprise.Integration.Customs.JP.AFR.IJPAFRHeader>(query);
			}
			return result;
		}

		public static ZBool IsEligibleForJPAFR(this ForwardingConsol consol)
		{
			return consol != null && !consol.IsDeleted && consol.IsSea && consol.IsGoingViaIgnoringDomesticRoute(Core.Constants.CountryCodes.Japan);
		}
	}
}
