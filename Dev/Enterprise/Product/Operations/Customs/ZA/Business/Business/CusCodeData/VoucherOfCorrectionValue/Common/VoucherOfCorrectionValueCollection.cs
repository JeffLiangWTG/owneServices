using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Business
{
	public abstract class VoucherOfCorrectionValueCollection<T, TMaster> : CusCodeDataCollection<T>
		where T : VoucherOfCorrectionValue
		where TMaster : BusinessObject
	{
		protected VoucherOfCorrectionValueCollection(TMaster master, ZString cy_type)
			: base(master, cy_type)
		{
		}

		public ZDecimal GetValue(ZString code)
		{
			return GetFirstElementOrderByPKHaving(code)?.CY_Value ?? ZDecimal.Zero;
		}

		protected virtual bool ShouldDeleteCodeOnSet(ZString code, ZDecimal value) => value.IsEmpty;

		public void SetValue(ZString code, ZDecimal value, ZPropertyInfo info)
		{
			var oldValue = ZDecimal.Zero;
			if (ShouldDeleteCodeOnSet(code, value))
			{
				this.OfType<T>().Where(x => x.CY_Code == code).DeleteAll();
			}
			else
			{
				var vocBizObj = GetFirstElementOrderByPKHaving(code);
				if (vocBizObj == null)
				{
					vocBizObj = AddNew(code);
				}
				else
				{
					oldValue = vocBizObj.CY_Value;
				}
				vocBizObj.CY_Value = value;
			}
			info.RefreshBinding(oldValue);
		}

		T GetFirstElementOrderByPKHaving(ZString code)
		{
			return this.OfType<T>().Where(x => x.CY_Code == code).OrderBy(x => x.PK.ToStringKey()).FirstOrDefault();
		}
	}
}
