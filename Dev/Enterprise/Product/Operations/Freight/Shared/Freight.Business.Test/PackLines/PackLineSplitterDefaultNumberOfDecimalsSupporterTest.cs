using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineSplitterDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public void TestDefaultNumberOfDecimalsAttribute_PackLineRelatedAttributes()
		{
			AssertDefaultNumberOfDecimalsAttribute(splitter, PackLineSplitter.Schema.SplitWeight, splitter.Line.JL_ActualWeightUQ);
			AssertDefaultNumberOfDecimalsAttribute(splitter, PackLineSplitter.Schema.SplitVolume, splitter.Line.JL_ActualVolumeUQ);
		}

		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("No change in transport mode expected during the lifecycle of this bizObj", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var line = shipment.OuterPackLines.AddNew();
			line.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			line.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			var container = Factory.New<CommonContainer>();
			consol.Containers.Add(container);

			splitter = new PackLineSplitter(line, container);
		}

		public override BusinessObject BizObj
		{
			get { return splitter; }
		}
		PackLineSplitter splitter;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get { return new Dictionary<ZString, ZString>(); }
		}

		public override List<ZString> PropertiesWithExternalUnitsToExcludeFromTesting
		{
			get
			{
				if (propertiesWithExternalUnitsToExcludeFromTesting == null)
				{
					propertiesWithExternalUnitsToExcludeFromTesting = new List<ZString>();
					propertiesWithExternalUnitsToExcludeFromTesting.Add(PackLineSplitter.Schema.SplitWeight);
					propertiesWithExternalUnitsToExcludeFromTesting.Add(PackLineSplitter.Schema.SplitVolume);
				}

				return propertiesWithExternalUnitsToExcludeFromTesting;
			}
		}
		List<ZString> propertiesWithExternalUnitsToExcludeFromTesting;

		#endregion

	}
}
