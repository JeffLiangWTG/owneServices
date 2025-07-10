using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonCartageLegDocManagerInfo : DocManagerInfo
	{
		public CommonCartageLegDocManagerInfo(CommonCartageLeg leg, ZString docManagerCode)
			: base(leg, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			var cartageLeg = (CommonCartageLeg)BusinessEntity;

			var cartage = cartageLeg.Cartage;
			if (cartage != null)
			{
				result.Add(cartage);
			}
			return result.ToArray();
		}
	}
}
