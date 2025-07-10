using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public abstract class ReservedFieldCollection<T> : CusCodeDataCollection<T> where T : ReservedField
	{
		protected ReservedFieldCollection(BusinessObject parent) : base(parent, CusCodeDataTypeList.Codes.ReservedField)
		{
		}

		protected override bool AllowNewCore => Count < 10;

		public IEnumerable<ReservedField> OrderList => this.Cast<ReservedField>().ToList().OrderBy(x => x.CY_Code);
	}
}
