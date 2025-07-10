using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public abstract class ZZRefCusCodeListWrapperCollection<T> : NonPersistentBusinessObjectCollection<T>,
		IZZRefCusCodeListWrapperCollection,
		ICodeDescriptionPairList
		where T : ZZRefCusCodeListWrapper
	{
		protected ZZRefCusCodeListWrapperCollection(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date)
			: base(factory)
		{
			DataGroupingCode = Argument.NotNullOrEmpty(dataGroupingCode, nameof(dataGroupingCode));
			CodeType = Argument.NotNullOrEmpty(codeType, nameof(codeType));
			EffectiveDate = date.IsEmpty ? ZDateTime.Today : date;

			CusCodeListCollection = new ZZRefCusCodeListCombinedCollection(factory, dataGroupingCode, codeType, date);
		}
		protected readonly ZZRefCusCodeListCombinedCollection CusCodeListCollection;
		public readonly ZString DataGroupingCode;
		public readonly ZString CodeType;
		public readonly ZDateTime EffectiveDate;

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		public override void Load()
		{
			Load(new ZQuery());
		}

		public BusinessObject[] LoadCusCodeList(ZQuery query)
		{
			CusCodeListCollection.Load(query);
			return CusCodeListCollection.Select(x => CreateNew(x)).ToArray();
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			CusCodeListCollection.Load(alternativeAdditionalFilter);
			BuildSubLocationCollection();
		}

		void BuildSubLocationCollection()
		{
			RemoveAll();
			AddRange(CusCodeListCollection.Select(x => CreateNew(x)));
		}

		T CreateNew(ZZRefCusCodeListCombined cusCodeList)
		{
			return (T)Activator.CreateInstance(typeof(T), cusCodeList);
		}

		T LoadFromCode(ZString code)
		{
			var cusCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, DataGroupingCode, CodeType, EffectiveDate);
			return cusCodeList == null ? null : CreateNew(cusCodeList);
		}

		public override int GetEstimatedLoadCount(ZQuery initialFilter)
		{
			return CusCodeListCollection.GetEstimatedLoadCount(initialFilter);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CountryOrGrouping = DataGroupingCode;
			cusCodeList.ZZD_CodeType = CodeType;
			cusCodeList.ZZD_StartDate = EffectiveDate.AddYears(-1);
			cusCodeList.ZZD_EndDate = EffectiveDate.AddYears(1);
			return CreateNew(cusCodeList);
		}

		protected override IFindBoxListProvider FindBoxListProvider => new ZZRefCusCodeListFindBoxListProvider<T>(this);

		#region Implementation of ICodeDescriptionPairList

		public bool ContainsCode(object code)
		{
			return LoadFromCode((ZString)code) != null;
		}

		public string GetDescriptionFromCode(string code)
		{
			return LoadFromCode(code)?.Description ?? ZString.Empty;
		}

		#endregion
	}

	public class ZZRefCusCodeListFindBoxListProvider<T> : FindBoxListProvider where T : ZZRefCusCodeListWrapper
	{
		public ZZRefCusCodeListFindBoxListProvider(ZZRefCusCodeListWrapperCollection<T> collection) : base(collection)
		{
		}

		public override (string, bool) NearestMatchCore(string code, bool explicitAutoComplete)
		{
			var collection = List as ZZRefCusCodeListWrapperCollection<T>;
			var query = ZZRefCusCodeListCombined.Loader.GetFilter(collection.Factory, collection.DataGroupingCode, collection.CodeType, collection.EffectiveDate, new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_Code, SQLComparisonOperator.StartsWith, code), true);
			query.OrderBy = ZZRefCusCodeListCombinedSchema.Constants.ZZD_Code;
			var bizO = collection.Factory.LoadTop1<ZZRefCusCodeListCombined>(query);

			return (bizO?.ZZD_Code ?? code, bizO != null);
		}

		public override string DescriptionFromCode(string code)
		{
			return (List as ICodeDescriptionPairList).GetDescriptionFromCode(code);
		}
	}
}
