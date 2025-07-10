using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class CFSLoadListConsolDocManagerInfo : ConsolDocManagerInfo
	{
		public CFSLoadListConsolDocManagerInfo(CFSLoadListConsol parent, ZString docManagerCode)
			: base(parent, docManagerCode)
		{
		}

		protected new CFSLoadListConsol Consol
		{
			get { return (CFSLoadListConsol)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());

			if (Consol is ICartageParent)
			{
				result.AddRange((BusinessObject[])Consol.Factory.Load<LocalCartage.Integration.ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, Consol.PK)));
			}

			return result.ToArray();
		}
	}
}
