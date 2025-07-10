using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.Universal
{
	public sealed class TariffAttributeView : AutoTariffAttributeView
	{
		public TariffAttributeView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoTariffAttributeView.Schema
		{
			public const string AdditionalDescription = "AdditionalDescription";
		}

		[ResourceStringData("Enterprise.Customs.Universal.TariffAttributeView|ZZ3_Value", Caption = "Value")]
		public override ZString ZZ3_Value { get => base.ZZ3_Value; set => base.ZZ3_Value = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffAttributeView|ZZ3_Name", Caption = "Name")]
		public override ZString ZZ3_Name { get => base.ZZ3_Name; set => base.ZZ3_Name = value; }

		[ResourceStringData("Enterprise.Customs.Universal.TariffAttributeView|AdditionalDescription", Caption = "Additional Description")]
		public ZString AdditionalDescription => CusTariff.AdditionalAttributeInformationProvider.AdditionalDescription(ZZ3_Name, ZZ3_Value);

		public ZPropertyInfo AdditionalDescriptionInfo => GetZPropertyInfo(Schema.AdditionalDescription);

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZZ3_ZZ1_ParentTariffOrNationalCode
		{
			get { return base.ZZ3_ZZ1_ParentTariffOrNationalCode; }
			set { base.ZZ3_ZZ1_ParentTariffOrNationalCode = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZZ3_ZZ1_ParentTariffOrNationalCode);

#if DEBUG
		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new UniversalReferenceBOTestDataHelper();
		}
#endif
	}
}
