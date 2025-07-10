using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(Schema.ZY3_Category), DescriptionProperty(Schema.ZY3_Description)]
	public sealed class RefCusTariffAdditionalCodeCategory : AutoRefCusTariffAdditionalCodeCategory
	{
		public RefCusTariffAdditionalCodeCategory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusTariffAdditionalCodeCategory.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static RefCusTariffAdditionalCodeCategory Load(BusinessObjectFactory factory, string dataGrouping, string category)
			{
				var query = new ZQuery(RefCusTariffAdditionalCodeCategorySchema.ZY3_ZZZ_NKDataGrouping, dataGrouping);
				query.AddToFilter(RefCusTariffAdditionalCodeCategorySchema.ZY3_Category, category);
				return factory.Load<RefCusTariffAdditionalCodeCategory>(query).OrderBy(x => x.PK).FirstOrDefault();
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefCusTariffAdditionalCodeCategory);
		}

		[RelatedBusinessObject(nameof(DataGrouping))]
		public override ZString ZY3_ZZZ_NKDataGrouping
		{
			get { return base.ZY3_ZZZ_NKDataGrouping; }
			set { base.ZY3_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping => Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZY3_ZZZ_NKDataGrouping);
	}
}
