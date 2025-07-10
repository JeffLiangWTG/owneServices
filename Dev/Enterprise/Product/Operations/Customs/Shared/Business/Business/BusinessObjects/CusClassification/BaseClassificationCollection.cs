using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public interface IBaseClassificationCollection<out T> : IBusinessObjectCollection<T>
		where T : BaseCusClassification
	{
	}

	[ModuleID(ModuleId.Classification)]
	[CodeProperty(CusClassificationSchema.Constants.CC_Description)]
	[DescriptionProperty(CusClassificationSchema.Constants.CC_LookupCode)]
	public class BaseClassificationCollection<TClassification> : BusinessObjectCollection<TClassification>, IBaseClassificationCollection<TClassification>
		where TClassification : BaseCusClassification
	{
		public BaseClassificationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public BaseClassificationCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public BaseClassificationCollection(BusinessObjectFactory factory, ZString countryCode)
			: base(factory)
		{
			this.countryCode = countryCode;
		}

		public BaseClassificationCollection(BusinessObjectFactory factory, ZQuery filter, ZString countryCode)
			: base(factory, filter)
		{
			this.countryCode = countryCode;
		}

		public ZString DefaultCountryCode => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

		protected ZString countryCode;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusClassificationSchema.CC_RN_NKCountryCode, string.IsNullOrEmpty(countryCode) ? DefaultCountryCode : countryCode);
			return result;
		}

		public IEnumerator<TClassification> GetEnumerator() => Elements.Cast<TClassification>().GetEnumerator();

		protected override IFindBoxListProvider FindBoxListProvider => new ListProvider(this);

		public class ListProvider : FindBoxListProvider
		{
			public ListProvider(BaseClassificationCollection<TClassification> collection)
				: base(collection)
			{
				this.collection = collection;
			}

			readonly BaseClassificationCollection<TClassification> collection;

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				if (!string.IsNullOrEmpty(code))
				{
					ZQuery query = new ZQuery(CusClassificationSchema.CC_LookupCode, code);
					query.AddToFilter(collection.RelationshipFilter);
					return collection.Factory.Load(collection.TypeOfElements, query);
				}
				else
				{
					return base.BizObjsFromCodeWithRelationshipFilter(code);
				}
			}
		}
	}
}
