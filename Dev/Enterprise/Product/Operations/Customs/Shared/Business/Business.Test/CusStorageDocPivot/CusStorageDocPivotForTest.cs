using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusStorageDocPivotForTest : BaseCusStorageDocPivot
	{
		public CusStorageDocPivotForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override TypeLoaderCollection parentLoaders
		{
			get
			{
				var result = new TypeLoaderCollection();
				result.Add(typeof(BaseJobComInvoiceHeader));
				result.Add(typeof(CusEntryInstructionAsTypeSupporter));
				return result;
			}
		}
	}
}
