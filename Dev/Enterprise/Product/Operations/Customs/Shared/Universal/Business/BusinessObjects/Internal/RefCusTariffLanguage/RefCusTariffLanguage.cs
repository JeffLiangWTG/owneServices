using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	internal sealed class RefCusTariffLanguage : AutoRefCusTariffLanguage
	{
		public RefCusTariffLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("Tariff")]
		public override ZGuid ZX7_ZZ1_Tariff
		{
			get => base.ZX7_ZZ1_Tariff;
			set => base.ZX7_ZZ1_Tariff = value;
		}

		public TariffView Tariff
		{
			get { return Factory.Load<TariffView>(ZX7_ZZ1_Tariff); }
		}

		[BusinessObjectTestExclude]
		public override ZString ZX7_ZX6_NKLanguage
		{
			get => base.ZX7_ZX6_NKLanguage;
			set => base.ZX7_ZX6_NKLanguage = value;
		}
	}
}
