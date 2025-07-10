using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CusInBondParentExtensions
	{
		public static Enterprise.Integration.Customs.ICusInBondHeader GetInBondHeader(this ICusInBondParent parent, string applicationCode, bool alwaysLoadFromDb = true)
		{
			Enterprise.Integration.Customs.ICusInBondHeader result = null;
			if (parent != null)
			{
				var inBondQuery = new ZQuery(CusInBondHeaderSchema.BH_ParentID, parent.PK);
				inBondQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, applicationCode);
				inBondQuery.AddToFilter(CusInBondHeaderSchema.BH_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				inBondQuery.AddToFilter(CusInBondHeaderSchema.BH_ParentTableCode, parent.TablePrefix);
				inBondQuery.OrderBy = CusInBondHeaderSchema.BH_SystemCreateTimeUtc.Name;
				inBondQuery.ReLoadExistingRows = alwaysLoadFromDb;
				result = parent.Factory.LoadTop1<Enterprise.Integration.Customs.ICusInBondHeader>(inBondQuery);
			}
			return result;
		}
	}
}
