using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public sealed class ZZRefCarrierAttributeCombined : AutoZZRefCarrierAttributeCombined
	{
		public ZZRefCarrierAttributeCombined(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		[RelatedBusinessObject("Carrier")]
		public override ZGuid ZZG_ZZ4_CarrierCode
		{
			get => base.ZZG_ZZ4_CarrierCode;
			set => base.ZZG_ZZ4_CarrierCode = value;
		}

		[List("Lookups.NameList")]
		public override ZString ZZG_Name
		{
			get => base.ZZG_Name;
			set => base.ZZG_Name = value;
		}

		public override bool ReadOnly
		{
			get => (Carrier?.ReadOnly ?? false) || base.ReadOnly;
			set => base.ReadOnly = value;
		}

		public ZZRefCarrierCombined Carrier => Factory.Load<ZZRefCarrierCombined>(ZZG_ZZ4_CarrierCode);
	}
}
