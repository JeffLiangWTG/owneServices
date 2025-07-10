using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(GlbGroup);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var isSales = (bool)row[GlbGroupSchema.Constants.GG_IsSales];
			if (isSales)
			{
				return typeof(SalesTeam);
			}
			else
			{
				return typeof(GlbGroup);
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(GlbGroup);
		}
	}
}
