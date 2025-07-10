using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public sealed class ExportAWBHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew()
		{
			throw new NotSupportedException();
		}

		public override Type GetTypeForBinding()
		{
			return typeof(ExportAWBHeader);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			string parentTableName = (string)row[ExportAWBHeaderSchema.EH_Table.Name];
			if (parentTableName == JobConsolSchema.Constants.TableName)
			{
				result = typeof(ConsolExportAWBHeader);
			}
			else if (parentTableName == JobShipmentSchema.Constants.TableName)
			{
				result = typeof(ShipmentExportAWBHeader);
			}
#if DEBUG
			else if (parentTableName == DummyBizoSchema.Constants.TableName)
			{
				result = Type.GetType("Enterprise.Freight.Forwarding.Business.AWB.Testing.MockExportAWBHeader,Enterprise.Freight.Forwarding.Business.Test");
			}
#endif
			return result;
		}
	}
}
