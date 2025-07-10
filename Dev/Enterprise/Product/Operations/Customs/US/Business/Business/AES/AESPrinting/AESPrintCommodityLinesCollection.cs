using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.AES
{
	public class AESPrintCommodityLinesCollection : NonPersistentBusinessObjectCollection<AESPrintCommodityLine>
	{
		public AESPrintCommodityLinesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}
	}
}
