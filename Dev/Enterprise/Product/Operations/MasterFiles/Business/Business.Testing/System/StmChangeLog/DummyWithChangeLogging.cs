using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DummyWithChangeLogging : DummyBusinessObject
	{
		public DummyWithChangeLogging(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public StmChangeLogCollection FieldChangeLogs
		{
			get
			{
				if (fieldChangeLogs == null)
				{
					fieldChangeLogs = new StmChangeLogCollection(this);
				}
				return fieldChangeLogs;
			}
		}
		StmChangeLogCollection fieldChangeLogs;
	}
}
