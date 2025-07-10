using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentProcessTask : ProcessTask
	{
		public DtbConsignmentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbConsignment); }
		}

		public new DtbConsignment Parent
		{
			get { return (DtbConsignment)base.Parent; }
		}
	}
}
