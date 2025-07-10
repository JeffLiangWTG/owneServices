using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	class DummyCustomFieldBizo : DummyBusinessObjectWithUserDefinedFields, IStmALogParent
	{
		public DummyCustomFieldBizo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool DeferFiringWorkflow => false;
		public ZGuid LogsParentPK => PK;
		public string LogsParentTableName => DummyBizoSchema.Constants.TableName;
		void IStmALogParent.ProcessLog(IStmALog log) { }
	}
}
