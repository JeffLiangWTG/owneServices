using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class AdditionalDutiesTariffTypeList : ICodeDescriptionPairList
	{
		public AdditionalDutiesTariffTypeList(BusinessObjectFactory factory, string countryCode)
			: this(factory, countryCode, new ZQuery())
		{
		}

		public AdditionalDutiesTariffTypeList(BusinessObjectFactory factory, string countryCode, ZQuery additionalFilter)
		{
			this.factory = factory;
			this.additionalFilter = additionalFilter.DeepClone();
			this.additionalFilter.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, countryCode);
			this.additionalFilter.OrderBy = RefCusTariffTypeSchema.Constants.ZZI_TariffType;
			additionalFilterCacheKey = string.Join("|", "AdditionalDutiesTariffTypeList|List", countryCode, additionalFilter.LiteralTextSqlFormatted);
		}
		readonly BusinessObjectFactory factory;
		readonly ZQuery additionalFilter;
		readonly string additionalFilterCacheKey;

		protected List<RefCusTariffType> List
		{
			get
			{
				return factory.GetCachedValue(additionalFilterCacheKey, (() =>
				{
					var list = new List<RefCusTariffType>();
					var selectedTariffTypes = factory.Load<RefCusTariffType>(additionalFilter);
					if (selectedTariffTypes != null)
					{
						list.AddRange(selectedTariffTypes);
					}

					return list;
				}));
			}
		}

		public RefCusTariffType this[string tariffType]
		{
			get { return List.FirstOrDefault(x => x.ZZI_TariffType == tariffType); }
		}

		public bool ContainsCode(object code)
		{
			var tariffType = code.ToString();
			return this[tariffType] != null;
		}

		public string GetDescriptionFromCode(string code)
		{
			var record = this[code];
			return record?.ZZI_Description;
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException();
		}

		bool IList.Contains(object value)
		{
			return List.Contains(value as RefCusTariffType);
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		int IList.IndexOf(object value)
		{
			return List.IndexOf(value as RefCusTariffType);
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return List.GetEnumerator();
		}

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		public int Count
		{
			get { return List.Count; }
		}

		object ICollection.SyncRoot
		{
			get { return null; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object IList.this[int index]
		{
			get { return List[index]; }
			set { }
		}
	}
}
