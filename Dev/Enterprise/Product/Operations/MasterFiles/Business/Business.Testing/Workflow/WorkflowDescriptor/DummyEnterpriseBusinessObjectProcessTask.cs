using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyEnterpriseBusinessObjectProcessTask : ProcessTask
	{
		public DummyEnterpriseBusinessObjectProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected internal override Type ParentType
		{
			get { return typeof(DummyEnterpriseBusinessObjectWithWorkflow); }
		}
	}
}
