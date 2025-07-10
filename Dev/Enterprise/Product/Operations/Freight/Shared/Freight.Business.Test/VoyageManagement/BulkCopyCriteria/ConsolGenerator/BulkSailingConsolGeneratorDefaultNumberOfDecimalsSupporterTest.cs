using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Testing
{
	sealed class BulkSailingConsolGeneratorDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("Not required, transport mode is always AIR", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			generator = new BulkSailingConsolGeneratorForTest(Factory);
			generator.WeightUnit = Core.Constants.Weight.Kilograms;
			generator.VolumeUnit = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return generator; }
		}
		BulkSailingConsolGenerator generator;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(BulkSailingConsolGenerator.Schema.Weight, BulkSailingConsolGenerator.Schema.WeightUnit);
					measurePropertiesAndUnits.Add(BulkSailingConsolGenerator.Schema.Volume, BulkSailingConsolGenerator.Schema.VolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
