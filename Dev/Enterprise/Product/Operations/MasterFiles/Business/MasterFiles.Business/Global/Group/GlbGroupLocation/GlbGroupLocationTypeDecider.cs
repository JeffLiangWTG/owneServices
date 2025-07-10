using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbGroupLocationTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(GlbGroupLocation);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var portOrCountry = row[GlbGroupLocationSchema.Constants.GGL_PortOrCountry] as string;
			if (portOrCountry.Length == 2)
			{
				return typeof(GlbGroupCountry);
			}
			else if (portOrCountry.Length == 5)
			{
				return typeof(GlbGroupUnloco);
			}
			else
			{
				return null;
			}
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
