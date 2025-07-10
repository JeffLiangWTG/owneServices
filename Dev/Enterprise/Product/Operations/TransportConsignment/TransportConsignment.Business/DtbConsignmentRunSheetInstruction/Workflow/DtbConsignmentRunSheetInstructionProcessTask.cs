using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbConsignmentRunSheetInstructionProcessTask : ProcessTask
	{
		public DtbConsignmentRunSheetInstructionProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(DtbConsignmentRunSheetInstruction); }
		}
	}
}
