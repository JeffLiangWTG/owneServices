using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusConditionValueType : AutoRefCusConditionValueType
	{
		public RefCusConditionValueType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZX4_ZZZ_NKDataGrouping
		{
			get => base.ZX4_ZZZ_NKDataGrouping;
			set => base.ZX4_ZZZ_NKDataGrouping = value;
		}

		public RefDataGrouping DataGrouping => Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZX4_ZZZ_NKDataGrouping);
	}
}
