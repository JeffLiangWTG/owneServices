using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CusBondDetail : AutoCusBondDetail
	{
		public CusBondDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public BusinessObject Parent
		{
			get { return ParentLoaders.LoadBusinessObject(Factory, PW_ParentTableCode, PW_ParentID); }
			set { ParentLoaders.SetTablePrefixAndPK(value, PW_ParentTableCodeInfo, PW_ParentIDInfo); }
		}

		protected virtual TypeLoaderCollection ParentLoaders
		{
			get { return new TypeLoaderCollection(typeof(OrgHeader)); }
		}
	}
}
