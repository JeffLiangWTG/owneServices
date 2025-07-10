using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Internal
{
	public class RefCusRate : AutoRefCusRate
	{
		public RefCusRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusTariff")]
		public override ZGuid ZZ2_ZZ1_Tariff
		{
			get { return base.ZZ2_ZZ1_Tariff; }
			set { base.ZZ2_ZZ1_Tariff = value; }
		}

		public TariffView CusTariff => Factory.Load<TariffView>(ZZ2_ZZ1_Tariff);

		public CusRefPreferenceView Preference => Factory.Load<CusRefPreferenceView>(ZZ2_ZZS_Preference);
		[RelatedBusinessObject("Preference")]
		public override ZGuid ZZ2_ZZS_Preference { get => base.ZZ2_ZZS_Preference; set => base.ZZ2_ZZS_Preference = value; }

		[ChildEditable]
		internal RefCusRateUOMCollection UnitsOfMeasure
		{
			get
			{
				if (unitsOfMeasure == null)
				{
					unitsOfMeasure = new RefCusRateUOMCollection(this);
					RegisterEditableChildObject(unitsOfMeasure);
				}
				return unitsOfMeasure;
			}
		}
		RefCusRateUOMCollection unitsOfMeasure;
	}
}
