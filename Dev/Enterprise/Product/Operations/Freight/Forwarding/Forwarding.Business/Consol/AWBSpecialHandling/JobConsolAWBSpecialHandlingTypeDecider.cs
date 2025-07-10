using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class JobConsolAWBSpecialHandlingTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			var code = (row != null) ? (ZString)row[JobConsolAWBSpecialHandling.Schema.JKH_Code].ToString().Trim() : ZString.Empty;
			if (code.IsEmpty || AWBSpecialHandlingCodeDescriptionPairList.IsCargoSecurityStatusCode(code))
			{
				return typeof(SecurityJobConsolAWBSpecialHandling);
			}

			return typeof(NonSecurityJobConsolAWBSpecialHandling);
		}

		public override Type GetTypeForNew() => null;

		public override Type GetTypeForBinding() => null;
	}
}
