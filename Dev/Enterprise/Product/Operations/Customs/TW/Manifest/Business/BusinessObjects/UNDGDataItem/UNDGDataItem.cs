using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class UNDGDataItem : MasterFiles.Business.UNDGDataItem
	{
		public UNDGDataItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new UNDGDataItemValidation Validation => (UNDGDataItemValidation)base.Validation;

		protected override MasterFiles.Business.UNDGDataItemValidation GetNewValidation()
		{
			return new UNDGDataItemValidation(this);
		}
	}
}
