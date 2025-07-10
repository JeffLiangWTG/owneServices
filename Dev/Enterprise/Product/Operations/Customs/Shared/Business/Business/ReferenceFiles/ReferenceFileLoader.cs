using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Subclass this as a nested class inside a reference file business object.
	/// Call the subclass 'Loader'. See US or SG for examples.
	/// </summary>
	public abstract class ReferenceFileLoader<T> : ReferenceFileLoader where T : BusinessObject
	{
		protected ReferenceFileLoader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new T LoadBestMatch(ZString code, ZDateTime assessmentDate)
		{
			return (T)base.LoadBestMatch(code, assessmentDate);
		}

		public new T LoadExact(ZString code, ZDateTime dateFrom, ZDateTime dateTo)
		{
			return (T)base.LoadExact(code, dateFrom, dateTo);
		}

		protected sealed override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(T);
		}
	}

	public abstract class ReferenceFileLoader : BusinessObject.Loader
	{
		internal ReferenceFileLoader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected abstract internal SchemaColumn CodeSchema { get; }
		protected abstract internal SchemaColumn DateFromSchema { get; }
		protected abstract internal SchemaColumn DateToSchema { get; }

		public ZQuery GetLoadBestMatchFilter(ZString code, ZDateTime assessmentDate)
		{
			var result = new ZQuery();
			var formattedCode = TariffFormatter.Format(code);
			if (!formattedCode.IsEmpty && assessmentDate.IsValidSmallDateTime)
			{
				result.AddToFilter(CodeSchema, formattedCode);
				result.AddToFilter(DateFromSchema, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, assessmentDate);
				result.AddToFilter(DateToSchema, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, assessmentDate);
			}
			return result;
		}

		public BusinessObject LoadBestMatch(ZString code, ZDateTime assessmentDate)
		{
			return LoadBestMatch(GetLoadBestMatchFilter(code, assessmentDate));
		}

		public BusinessObject LoadBestMatch(ZQuery filter)
		{
			BusinessObject result = null;
			if (filter != null && !filter.IsEmpty)
			{
				var restrictiveDateTo = ZDateTime.Empty;
				foreach (var possibleMatch in Factory.Load(GetTypeOfBusinessObjectToLoad(), filter).OrderByDescending(x => x[DateFromSchema]))
				{
					if (restrictiveDateTo.IsEmpty || restrictiveDateTo > (ZDateTime)possibleMatch[DateToSchema.Name])
					{
						result = possibleMatch;
						restrictiveDateTo = (ZDateTime)possibleMatch[DateToSchema.Name];
					}
				}
			}
			return result;
		}

		public BusinessObject LoadExact(ZString code, ZDateTime dateFrom, ZDateTime dateTo)
		{
			BusinessObject result = null;
			var formattedCode = TariffFormatter.Format(code);
			if (!formattedCode.IsEmpty && dateFrom.IsValid && dateTo.IsValid)
			{
				var filter = new ZQuery(CodeSchema, formattedCode);
				filter.AddToFilter(DateFromSchema, SQLComparisonOperator.EqualToDatePartOnly, dateFrom);
				filter.AddToFilter(DateToSchema, SQLComparisonOperator.EqualToDatePartOnly, dateTo);
				result = Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), filter);
			}
			return result;
		}

		public TariffFormatter TariffFormatter
		{
			get { return GetNewTariffFormatter(); }
		}

		protected virtual TariffFormatter GetNewTariffFormatter()
		{
			return new TariffFormatter();
		}
	}
}
