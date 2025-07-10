using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemCollection : DependentBusinessObjectCollection<CusUSLVItem, CusUSLVConsignment>
	{
		public CusUSLVItemCollection(CusUSLVConsignment cusUSLVConsignment)
			: base(cusUSLVConsignment)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusUSLVItemSchema.ULI_ULB; }
		}

		public CusUSLVItem AddNew(Action<CusUSLVItem> initializeAction)
		{
			var newItem = Factory.New<CusUSLVItem>();
			initializeAction?.Invoke(newItem);
			Add(newItem);
			return newItem;
		}
	}
}
