using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public sealed class ExportAWBHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForNew()
		{
			return typeof(ExportAWBHeader);
		}

		public override Type GetTypeForBinding()
		{
			return typeof(ExportAWBHeader);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string parentTableName = (string)row[ExportAWBHeaderSchema.EH_Table.Name];
			if (parentTableName == JobConsolSchema.Constants.TableName)
			{
				return ObjectFactory.GetType<IConsolExportAWBHeader>();
			}
			else if (parentTableName == JobShipmentSchema.Constants.TableName)
			{
				return ObjectFactory.GetType<IShipmentExportAWBHeader>();
			}
#if DEBUG
			else if (parentTableName == DummyBizoSchema.Constants.TableName)
			{
				return Type.GetType("Enterprise.Freight.Forwarding.Business.AWB.Testing.MockExportAWBHeader,Enterprise.Freight.Forwarding.Business.Test");
			}
#endif
			return typeof(ExportAWBHeader);
		}
	}
}
