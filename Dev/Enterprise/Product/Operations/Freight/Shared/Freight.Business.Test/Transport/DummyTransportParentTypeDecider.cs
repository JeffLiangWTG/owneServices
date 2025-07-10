using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	class DummyTransportParentTypeDecider : TransportParentTypeDecider
	{
		public override sealed Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			if ((string)row[JobConsolTransportSchema.JW_ParentType.Name] == "DMY")
			{
				return typeof(DummyTransportParent);
			}
			return base.GetTypeForLoad(row, factory);
		}
	}
}
