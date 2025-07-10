using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCodeListLanguage : AutoRefCusCodeListLanguage
	{
		public RefCusCodeListLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CodeList")]
		public override ZGuid ZXA_ZZD_CodeList
		{
			get => base.ZXA_ZZD_CodeList;
			set => base.ZXA_ZZD_CodeList = value;
		}

		public RefCusCodeList CodeList
		{
			get { return Factory.Load<RefCusCodeList>(ZXA_ZZD_CodeList); }
		}

		[BusinessObjectTestExclude]
		public override ZString ZXA_ZX6_NKLanguage
		{
			get => base.ZXA_ZX6_NKLanguage;
			set => base.ZXA_ZX6_NKLanguage = value;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}
	}
}
