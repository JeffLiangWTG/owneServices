using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Internal
{
	[CodeProperty(RefCusPreferenceSchema.Constants.ZZS_Preference), DescriptionProperty(RefCusPreferenceSchema.Constants.ZZS_Description)]
	internal sealed class RefCusPreference : AutoRefCusPreference
	{
		public RefCusPreference(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		[RelatedBusinessObject("DataGrouping")]
		public override ZString ZZS_ZZZ_NKDataGrouping
		{
			get { return base.ZZS_ZZZ_NKDataGrouping; }
			set { base.ZZS_ZZZ_NKDataGrouping = value; }
		}

		public RefDataGrouping DataGrouping
		{
			get { return Factory.LoadFromNaturalKey<RefDataGrouping>(RefDataGroupingSchema.ZZZ_DataGrouping, ZZS_ZZZ_NKDataGrouping); }
		}
	}
}
