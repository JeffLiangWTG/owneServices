using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCarrierCodeLanguage : AutoRefCarrierCodeLanguage
	{
		public RefCarrierCodeLanguage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("LanguageType")]
		public override ZString ZCL_ZX6_NKLanguage
		{
			get => base.ZCL_ZX6_NKLanguage;
			set => base.ZCL_ZX6_NKLanguage = value;
		}

		public RefLanguageType LanguageType => Factory.LoadFromNaturalKey<RefLanguageType>(RefLanguageTypeSchema.ZX6_Language, ZCL_ZX6_NKLanguage);

		[RelatedBusinessObject("CarrierCode")]
		public override ZGuid ZCL_ZZ4_CarrierCode
		{
			get => base.ZCL_ZZ4_CarrierCode;
			set => base.ZCL_ZZ4_CarrierCode = value;
		}

		public RefCarrierCode CarrierCode => Factory.Load<RefCarrierCode>(ZCL_ZZ4_CarrierCode);
	}
}
