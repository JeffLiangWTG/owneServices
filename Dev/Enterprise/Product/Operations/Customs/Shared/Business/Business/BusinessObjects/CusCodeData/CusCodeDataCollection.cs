using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusCodeDataCollection<out T> : IBusinessObjectCollection<T>
		where T : CusCodeData
	{
		new T this[int index] { get; }
		new T AddNew();

		ZString CY_Type { get; }
		T AddNew(ZString code);
		T AddNew(ZString code, ZString data);
		ZString GetStringDataHaving(ZString code);
		void SetStringValueHavingCodeOrDeleteIfValueEmpty(ZString code, ZString value, bool deleteDuplicate = false);
		bool ContainsCode(string code);
		T GetFirstElementHaving(string code);
		T[] GetElementsHaving(string code);
		bool Any();
		T First();
		T FirstOrDefault();
		T Last();
		ZString AsString { get; set; }
	}

	public class CusCodeDataCollection<T> : DependentBusinessObjectCollection<T, BusinessObject>, ICusCodeDataCollection<T>
		where T : CusCodeData
	{
		public CusCodeDataCollection(BusinessObject parent, ZString cY_Type)
			: base(parent)
		{
			this.CY_Type = cY_Type;
		}

		public ZString CY_Type { get; }

		/// <summary>
		/// Copy this collection to the cloneResult collection
		/// </summary>
		public virtual void CloneElementsTo(CusCodeDataCollection<T> cloneResult)
		{
			foreach (CusCodeData data in this)
			{
				cloneResult.Add(data.Clone());
			}
		}

		public T AddNew(ZString code)
		{
			T result = AddNew();
			result.CY_Code = code;
			return result;
		}

		public T AddNew(ZString code, ZString data)
		{
			T result = AddNew(code);
			result.CY_Data = data;
			return result;
		}

		public ZString GetStringDataHaving(ZString code)
		{
			CusCodeData[] result = GetElementsHaving(code);

			if (result.Length == 1)
			{
				return result[0].CY_Data;
			}
			else if (result.Length > 1)
			{
				ErrorReporter.ReportOnce("GetStringDataHaving with " + code, "There are " + result.Length + " elements in this collection having code, " + code);
			}
			return ZString.Empty;
		}

		public void SetStringValueHavingCodeOrDeleteIfValueEmpty(ZString code, ZString value, bool deleteDuplicate = false)
		{
			CusCodeData[] result = GetElementsHaving(code);

			if (result.Length > 1 && !deleteDuplicate)
			{
				ErrorReporter.ReportOnce("SetStringValueToCodeHaving with " + code, "There are " + result.Length + " elements in this collection having code, " + code);
			}

			if (value.IsEmpty)
			{
				foreach (CusCodeData codeData in result)
				{
					codeData.Delete();
				}
			}
			else
			{
				if (result.Length == 0)
				{
					result = new CusCodeData[] { AddNew(code, value) };
				}
				else
				{
					if (deleteDuplicate)
					{
						var list = result.ToList();
						var first = list.First();
						first.CY_Data = value;
						list.Remove(first);
						list.ForEach(x => RemoveAndDelete(x));
					}
					else
					{
						result[0].CY_Data = value;
					}
				}
			}
		}

		public bool ContainsCode(string code)
		{
			return GetFirstElementHaving(code) != null;
		}

		public T GetFirstElementHaving(string code)
		{
			foreach (T codeData in this)
			{
				if (codeData.CY_Code == code)
				{
					return codeData;
				}
			}
			return null;
		}

		public T[] GetElementsHaving(string code)
		{
			List<T> result = new List<T>();

			foreach (T codeData in this)
			{
				if (codeData.CY_Code == code)
				{
					result.Add(codeData);
				}
			}

			return result.ToArray();
		}

		public IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();
		public bool Any() => Count > 0;
		public T First() => this[0];
		public T FirstOrDefault() => this.Cast<T>().FirstOrDefault();
		public T Last() => this[Count - 1];

		public ZString AsString
		{
			get => Factory.GetValue(ref asStringCached, () =>
			{
				return string.Join(",", this.Cast<T>().OrderBy(x => x.CY_Order).Select(x => x.CY_Code).Where(x => !x.IsEmpty));
			});
			set
			{
				RemoveAndDeleteAll();
				foreach (string code in value.ToString().Replace(" ", ",").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
				{
					var item = AddNew();
					item.CY_Code = new ZString(code).Left(4).Trim();
				}
			}
		}
		CachedProperty<ZString> asStringCached;

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusCodeDataSchema.CY_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusCodeDataSchema.CY_Type, SQLComparisonOperator.Equal, CY_Type);
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			CusCodeData child = (CusCodeData)dependent;
			child.CY_Type = CY_Type;
			child.Parent = Master;
		}
	}
}
