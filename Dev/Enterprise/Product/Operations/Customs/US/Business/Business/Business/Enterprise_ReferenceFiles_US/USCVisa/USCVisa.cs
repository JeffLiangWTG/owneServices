using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCVisa : AutoUSCVisa
	{
		public USCVisa(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoUSCVisa.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public USCVisa Load(ZString code, ZString country, ZDateTime beginDate, ZDateTime endDate)
			{
				ZQuery query = GetQueryFor(code, country);
				query.AddToFilter(USCVisaSchema.UO_BeginDate, beginDate);
				query.AddToFilter(USCVisaSchema.UO_EndDate, endDate);
				return (USCVisa)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), query);
			}

			public USCVisa Load(ZString code, ZString country, ZDateTime date)
			{
				return (USCVisa)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), GetLoadFilter(code, country, date));
			}

			public ZQuery GetLoadFilter(ZString code, ZString country, ZDateTime date)
			{
				ZQuery query = GetQueryFor(code, country);
				query.AddToFilter(JoinCondition.And, USCVisaSchema.UO_BeginDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
				query.AddToFilter(JoinCondition.And, USCVisaSchema.UO_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				return query;
			}

			ZQuery GetQueryFor(ZString code, ZString country)
			{
				ZQuery query = new ZQuery(USCVisaSchema.UO_TextileCategoryNo, code);
				query.AddToFilter(USCVisaSchema.UO_UC_NKOriginCountry, country);
				return query;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(USCVisa);
			}
		}

		[ChildEditable(true)]
		public USCVisaTariffCollection Tariffs
		{
			get
			{
				if (fTariffs == null)
				{
					fTariffs = new USCVisaTariffCollection(this);
					fTariffs.Load();
					RegisterEditableChildObject(fTariffs);
				}
				return fTariffs;
			}
		}
		USCVisaTariffCollection fTariffs;

		public override void Delete()
		{
			Tariffs.RemoveAndDeleteAll();
			base.Delete();
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			UO_BeginDate = ZDateTime.BrettsBirthday;
			UO_EndDate = ZDateTime.BrettsBirthday;
		}

#endif
	}
}
