using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface ICusCodeDataWithOrder
	{
		public ZShort CY_Order { get; set; }
		public ZPropertyInfo CY_OrderInfo { get; }
		public ZString CY_Code { get; set; }
		public ZPropertyInfo CY_CodeInfo { get; }
	}

	public abstract class CusCodeDataWithOrder : CusCodeData, ICusCodeDataWithOrder
	{
		protected CusCodeDataWithOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		protected abstract string CusCodeDataType { get; }

		public ICusCodeDataWithOrderSupporter Supporter => Parent as ICusCodeDataWithOrderSupporter;

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public ZQuery GetZQuery(ZGuid pk, ZString type)
			{
				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, pk);
				query.AddToFilter(CusCodeDataSchema.CY_Type, type);
				return query;
			}

			public T Load<T, TParent>(TParent parent, ZShort order, ZString type)
				where T : CusCodeDataWithOrder
				where TParent : BusinessObject
			{
				T result = null;
				if (parent != null)
				{
					var query = GetZQuery(parent.PK, type);
					query.FetchOnlyFromLocalCache = !parent.IsInDatabase;
					result = Factory.Load<T>(query).Where(x => x.CY_Order == order).OrderBy(x => x.CY_SystemCreateTimeUtc).FirstOrDefault();
				}
				return result;
			}

			public T LoadOrCreate<T, TParent>(TParent parent, ZShort order, ZString type)
				where T : CusCodeDataWithOrder
				where TParent : BusinessObject
			{
				T result = null;
				if (parent != null)
				{
					result = Load<T, TParent>(parent, order, type);
					if (result == null)
					{
						result = Factory.New<T>();
						result.CY_ParentID = parent.PK;
						result.CY_ParentTableCode = parent.TablePrefix;
						result.CY_Order = order;
						result.CY_Type = type;
					}
				}
				return result;
			}

			public CusCodeData Load<TParent>(TParent parent, ZShort order, ZString type) where TParent : BusinessObject
				=> Load<CusCodeDataWithOrder, TParent>(parent, order, type);

			public CusCodeData LoadOrCreate<TParent>(TParent parent, ZShort order, ZString type) where TParent : BusinessObject
				=> LoadOrCreate<CusCodeDataWithOrder, TParent>(parent, order, type);

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CusCodeData);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataType;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("BEB741E0-8C56-47A2-A48C-18BB2060852B", "Code with order");
	}
}
