using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class CusCodeDataWithOrderCollection<T> : CusCodeDataCollection<T>
		where T : CusCodeDataWithOrder
	{
		protected CusCodeDataWithOrderCollection(ZPropertyInfo info, ZString type, short size, short startOrder = 1)
			: base(info.BizObj, type)
		{
			this.info = info;
			MaxCountValidationEnable(size);
			this.startOrder = startOrder;
			Sort(CusCodeDataWithOrder.Schema.CY_Order);
		}

		readonly ZShort startOrder;
		readonly ZPropertyInfo info;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(CusCodeDataSchema.CY_Order, SQLComparisonOperator.GreaterThanOrEqualTo, startOrder);
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var code = (T)child;
			code.CY_Order = Count > ZShort.Zero ? this.Cast<T>().Max(x => x.CY_Order) + 1 : startOrder;
		}

		protected override bool AllowNewCore => base.AllowNewCore && Count < MaxCount;

		void ApplyCurrentSortOrder()
		{
			short index = startOrder;
			foreach (T code in this)
			{
				code.CY_Order = index++;
			}
		}

		public IEnumerable<ZString> GetAllCodes() => this.Cast<T>().Select(x => x.CY_Code).Where(x => !x.IsEmpty);

		#region Event handling

		protected virtual void OnCodesChanged(EventArgs e)
		{
			info.RefreshBinding();
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			var code = ((T)bizOAdded);
			code.CY_CodeInfo.ValueChanged -= new EventHandler(OnCodesInfo_ValueChanged);
			code.CY_CodeInfo.ValueChanged += new EventHandler(OnCodesInfo_ValueChanged);
			OnCodesChanged(EventArgs.Empty);
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			ApplyCurrentSortOrder();
			((T)bizO).CY_CodeInfo.ValueChanged -= new EventHandler(OnCodesInfo_ValueChanged);
			OnCodesChanged(EventArgs.Empty);
			((ICusCodeDataWithOrderSupporter)Master).OnCodesChanged();
		}

		void OnCodesInfo_ValueChanged(object sender, EventArgs e)
		{
			OnCodesChanged(EventArgs.Empty);
		}

		#endregion
	}
}
