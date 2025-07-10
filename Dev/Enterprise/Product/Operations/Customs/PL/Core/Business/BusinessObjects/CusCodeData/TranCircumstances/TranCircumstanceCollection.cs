using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business.Declaration;

public class TranCircumstanceCollection : CusCodeDataCollection<TranCircumstance>
{
	public TranCircumstanceCollection(BusinessObject master, short startOrder = 1)
		: base(master, CusCodeDataTypeList.Codes.DV1)
	{
		this.startOrder = startOrder;
		Sort(TranCircumstance.Schema.CY_Order);
	}

	readonly ZShort startOrder;

	public static TranCircumstanceCollection New<T>(T master, ZPropertyInfo info, short startOrder = 1)
		where T : BusinessObject, ITranCircumstanceSupporter
	{
		var tranCircumstancesCollection = new TranCircumstanceCollection(master, startOrder);
		tranCircumstancesCollection.Load();
		tranCircumstancesCollection.TranCircumstancesChanged += (object sender, EventArgs e) => info.RefreshBinding();
		return tranCircumstancesCollection;
	}

	protected override ZQuery CreateRelationshipFilter()
	{
		var result = base.CreateRelationshipFilter();
		result.AddToFilter(CusCodeDataSchema.CY_Order, SQLComparisonOperator.GreaterThanOrEqualTo, startOrder);
		return result;
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var code = (TranCircumstance)child;
		code.CY_Type = CusCodeDataTypeList.Codes.DV1;
		code.CY_Order = Count > ZShort.Zero ? this.OfType<TranCircumstance>().Max(x => x.CY_Order) + 1 : startOrder;
	}

	void ApplyCurrentSortOrder()
	{
		short index = startOrder;
		foreach (TranCircumstance code in this)
		{
			code.CY_Order = index++;
		}
	}

	public new ZString AsString
	{
		get
		{
			StringBuilder result = new StringBuilder();
			if (SortInformation == null || SortInformation.PropertyName != CusCodeData.Schema.CY_Order)
			{
				Sort(CusCodeData.Schema.CY_Order, ListSortDirection.Ascending);
			}
			foreach (TranCircumstance item in this)
			{
				if (result.Length > 0)
				{
					result.Append(",");
				}

				result.Append(item.CY_Code);
			}
			return result.ToString();
		}
		set
		{
			RemoveAndDeleteAll();
			string[] codes = value.ToString().Replace(" ", ",").Split(',');
			foreach (string code in codes)
			{
				if (!string.IsNullOrWhiteSpace(code))
				{
					var item = AddNew();
					item.CY_Code = new ZString(code).SubstringSafe(0, TranCircumstance.Schema.CY_CodeMaxLength).Trim();
				}
			}
		}
	}

	#region Event handling

	public event EventHandler TranCircumstancesChanged;

	protected virtual void OnTranCircumstancesValueChanged(EventArgs e)
	{
		TranCircumstancesChanged?.Invoke(this, e);
	}

	protected override void OnAdded(BusinessObject bizOAdded)
	{
		base.OnAdded(bizOAdded);
		var code = ((TranCircumstance)bizOAdded);
		code.CY_CodeInfo.ValueChanged -= new EventHandler(OnTranCircumstancesInfo_ValueChanged);
		code.CY_CodeInfo.ValueChanged += new EventHandler(OnTranCircumstancesInfo_ValueChanged);
		OnTranCircumstancesValueChanged(EventArgs.Empty);
	}

	protected override void OnRemoved(BusinessObject bizO)
	{
		base.OnRemoved(bizO);
		ApplyCurrentSortOrder();
		((TranCircumstance)bizO).CY_CodeInfo.ValueChanged -= new EventHandler(OnTranCircumstancesInfo_ValueChanged);
		OnTranCircumstancesValueChanged(EventArgs.Empty);
	}

	void OnTranCircumstancesInfo_ValueChanged(object sender, EventArgs e)
	{
		OnTranCircumstancesValueChanged(EventArgs.Empty);
	}

	#endregion
}
