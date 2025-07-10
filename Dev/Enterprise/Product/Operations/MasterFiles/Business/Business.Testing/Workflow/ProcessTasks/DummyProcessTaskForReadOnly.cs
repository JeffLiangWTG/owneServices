using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyProcessTaskForReadOnly : DummyProcessTask
	{
		public DummyProcessTaskForReadOnly(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected internal override Type ParentType
		{
			get { return OverriddenParentTypeForTest ?? typeof(DummyWithReadOnlyActionField); }
		}
	}
}
