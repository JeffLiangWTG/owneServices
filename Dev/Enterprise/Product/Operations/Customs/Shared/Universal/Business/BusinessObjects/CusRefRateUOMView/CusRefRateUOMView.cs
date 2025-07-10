using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class CusRefRateUOMView : AutoCusRefRateUOMView
	{
		public CusRefRateUOMView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[RelatedBusinessObject("CusRate")]
		public override ZGuid ZXG_ZZ2_Rate
		{
			get => base.ZXG_ZZ2_Rate;
			set => base.ZXG_ZZ2_Rate = value;
		}

		public RateView CusRate => Factory.Load<RateView>(ZXG_ZZ2_Rate);

		[ReadOnly(true)]
		public override ZString ZXG_DataSet
		{
			get => base.ZXG_DataSet;
			set => base.ZXG_DataSet = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZXG_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
		}
	}
}
