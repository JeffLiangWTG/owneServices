using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Scope = "type")]
	class CartageProcessTask : ProcessTask, Integration.ICartageProcessTask
	{
		public CartageProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString JobNumber
		{
			get { return Parent.JJ_ConsignmentID; }
		}

		protected override Type ParentType
		{
			get { return typeof(CommonCartage); }
		}

		public new CommonCartage Parent
		{
			get { return (CommonCartage)base.Parent; }
		}
	}
}
