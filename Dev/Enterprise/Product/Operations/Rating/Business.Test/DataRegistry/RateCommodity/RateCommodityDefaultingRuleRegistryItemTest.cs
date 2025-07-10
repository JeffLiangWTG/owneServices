using System.Text;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateCommodityDefaultingRuleRegistryItem))]
	class RateCommodityDefaultingRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<RateCommodityDefaultingRuleCollection>
	{
		protected override StronglyTypedRegistryItem<RateCommodityDefaultingRuleCollection, RateCommodityDefaultingRuleCollection> GetNewRegistryItem()
		{
			return new RateCommodityDefaultingRuleRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(RateCommodityDefaultingRuleRegistryDataType))]
	class RateCommodityDefaultingRuleRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RateCommodityDefaultingRuleRegistryDataType>
	{
		#region Implementation

		protected override RateCommodityDefaultingRuleRegistryDataType GetNewDataType()
		{
			return new RateCommodityDefaultingRuleRegistryDataType();
		}

		protected override string ExpectedEditorName => "RateCommodityDefaultingRuleRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new RateCommodityDefaultingRuleCollection();

			var rateCommodityDefaultingRule1 = new RateCommodityDefaultingRule();
			first.Add(rateCommodityDefaultingRule1);
			rateCommodityDefaultingRule1.RateCommodityCode = "GEN";
			rateCommodityDefaultingRule1.TransportMode = Core.Constants.TransportModes.Sea;
			rateCommodityDefaultingRule1.ContainerMode = Core.Constants.ContainerModes.LCL;

			var rateCommodityDefaultingRule2 = new RateCommodityDefaultingRule();
			first.Add(rateCommodityDefaultingRule2);
			rateCommodityDefaultingRule2.RateCommodityCode = "GEN";
			rateCommodityDefaultingRule2.TransportMode = Core.Constants.TransportModes.Sea;
			rateCommodityDefaultingRule2.ContainerMode = Core.Constants.ContainerModes.FCL;

			var firstSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRateCommodityDefaultingRule xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><RateCommodityDefaultingRule><RateCommodityCode>GEN</RateCommodityCode><TransportMode>SEA</TransportMode><ContainerMode>LCL</ContainerMode></RateCommodityDefaultingRule><RateCommodityDefaultingRule><RateCommodityCode>GEN</RateCommodityCode><TransportMode>SEA</TransportMode><ContainerMode>FCL</ContainerMode></RateCommodityDefaultingRule></ArrayOfRateCommodityDefaultingRule>";

			var second = new RateCommodityDefaultingRuleCollection();

			var rateCommodityDefaultingRule3 = new RateCommodityDefaultingRule();
			second.Add(rateCommodityDefaultingRule3);
			rateCommodityDefaultingRule3.RateCommodityCode = "GEN";
			rateCommodityDefaultingRule3.TransportMode = Core.Constants.TransportModes.Air;
			rateCommodityDefaultingRule3.ContainerMode = Core.Constants.ContainerModes.Loose;

			var rateCommodityDefaultingRule4 = new RateCommodityDefaultingRule();
			second.Add(rateCommodityDefaultingRule4);
			rateCommodityDefaultingRule4.RateCommodityCode = "GEN";
			rateCommodityDefaultingRule4.TransportMode = Core.Constants.TransportModes.Sea;
			rateCommodityDefaultingRule4.ContainerMode = Core.Constants.ContainerModes.BuyersConsol;

			var secondSerialised = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRateCommodityDefaultingRule xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><RateCommodityDefaultingRule><RateCommodityCode>GEN</RateCommodityCode><TransportMode>AIR</TransportMode><ContainerMode>LSE</ContainerMode></RateCommodityDefaultingRule><RateCommodityDefaultingRule><RateCommodityCode>GEN</RateCommodityCode><TransportMode>SEA</TransportMode><ContainerMode>BCN</ContainerMode></RateCommodityDefaultingRule></ArrayOfRateCommodityDefaultingRule>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(first, Encoding.Unicode.GetBytes(firstSerialised)),
				new ValidSampleAndBinaryValueInDB(second, Encoding.Unicode.GetBytes(secondSerialised)),
			};
		}

		#endregion
	}
}
