using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyTopLevelPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJC_ContainerMode()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_ContainerModeInfo);
			container.JC_ContainerMode = Constants.ContainerModes.Liquid;
			AssertNoErrors(container.JC_ContainerModeInfo);
			container.JC_ContainerMode = "XXX";
			AssertHasErrors(container.JC_ContainerModeInfo);
		}

		public void TestJC_ContainerNum()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "XXX";
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "";
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "1234";
			container.JC_ContainerCount = 2;
			container.Validation.ValidateJC_ContainerNum();
			AssertNoErrors(container.JC_ContainerNumInfo);
		}

		public void TestJC_ContainerCount()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_ContainerCountInfo);
			container.JC_ContainerCount = 10;
			AssertNoErrors(container.JC_ContainerCountInfo);
			container.JC_ContainerCount = 0;
			AssertNoErrors(container.JC_ContainerCountInfo);
			container.JC_ContainerCount = -1;
			AssertEquals("Can't set container count to a negative number, behaviour set and tested in AgencyShipmentContainer", (short)1, container.JC_ContainerCount);
		}

		public void TestJC_RC()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_RCInfo);
			container.JC_RC = ZGuid.NewZGuid();
			AssertNoErrors(container.JC_RCInfo);
			container.JC_RC = ZGuid.Empty;
			AssertNoErrors(container.JC_RCInfo);
		}

		public void TestJC_F3_NKPackType()
		{
			var commodity = Factory.New<RefPackType>();
			commodity.F3_Code = "AAA";
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			container.Validation.ValidateJC_F3_NKPackType();
			AssertNoErrors(container.JC_F3_NKPackTypeInfo);
			container.JC_F3_NKPackType = "AAA";
			AssertNoErrors(container.JC_F3_NKPackTypeInfo);
			container.JC_F3_NKPackType = "XXX";
			AssertHasErrors(container.JC_F3_NKPackTypeInfo);
		}

		public void TestJC_RH_NKContainerCommodityCode()
		{
			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_RH_NKContainerCommodityCodeInfo);
			container.JC_RH_NKContainerCommodityCode = "AAA";
			AssertNoErrors(container.JC_RH_NKContainerCommodityCodeInfo);
			container.JC_RH_NKContainerCommodityCode = "XXX";
			AssertHasErrors(container.JC_RH_NKContainerCommodityCodeInfo);
		}

		public void TestJC_GrossWeight()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_GrossWeightInfo);
			container.JC_GrossWeight = 10m;
			AssertNoErrors(container.JC_GrossWeightInfo);
			container.JC_GrossWeight = 0m;
			AssertNoErrors(container.JC_GrossWeightInfo);
			AssertHasWarning(container.JC_GrossWeightInfo, "You have not entered weight.");
			container.JC_GrossWeight = -10m;
			AssertHasErrors(container.JC_GrossWeightInfo);
			container.JC_GrossWeight = 10m;
			AssertNoErrors(container.JC_GrossWeightInfo);
		}

		public void TestJC_GrossWeightUQ()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_GrossWeightUQInfo);
			container.JC_GrossWeightUQ = "";
			AssertNoErrors(container.JC_GrossWeightUQInfo);
			container.JC_GrossWeight = 10m;
			container.Validation.ValidateJC_GrossWeightUQ();
			AssertHasErrors(container.JC_GrossWeightUQInfo);
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			AssertNoErrors(container.JC_GrossWeightUQInfo);
			container.JC_GrossWeightUQ = "XX";
			AssertHasErrors(container.JC_GrossWeightUQInfo);
		}

		public void TestJC_GrossVolume()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_GrossVolumeInfo);
			container.JC_GrossVolume = 10m;
			AssertNoErrors(container.JC_GrossVolumeInfo);
			container.JC_GrossVolume = 0m;
			AssertNoErrors(container.JC_GrossVolumeInfo);
			AssertHasWarning(container.JC_GrossVolumeInfo, "You have not entered volume.");
			container.JC_GrossVolume = -10m;
			AssertHasErrors(container.JC_GrossVolumeInfo);
			container.JC_GrossVolume = 10m;
			AssertNoErrors(container.JC_GrossVolumeInfo);
		}

		public void TestJC_GrossVolumeUQ()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_GrossVolumeUQInfo);
			container.JC_GrossVolumeUQ = "";
			AssertNoErrors(container.JC_GrossVolumeUQInfo);
			container.JC_GrossVolume = 10m;
			container.Validation.ValidateJC_GrossVolumeUQ();
			AssertHasErrors(container.JC_GrossVolumeUQInfo);
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			AssertNoErrors(container.JC_GrossVolumeUQInfo);
			container.JC_GrossVolumeUQ = "XX";
			AssertHasErrors(container.JC_GrossVolumeUQInfo);
		}

		public void TestJC_TotalUnitOfMeasure()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_TotalUnitOfMeasureInfo);
			container.JC_TotalUnitOfMeasure = "";
			AssertNoErrors(container.JC_TotalUnitOfMeasureInfo);
			container.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			AssertNoErrors(container.JC_TotalUnitOfMeasureInfo);
			container.JC_TotalUnitOfMeasure = "XX";
			AssertHasErrors(container.JC_TotalUnitOfMeasureInfo);
		}

		public void TestJC_TotalLength()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_TotalLengthInfo);
			container.JC_TotalLength = 10m;
			AssertNoErrors(container.JC_TotalLengthInfo);
			container.JC_TotalLength = -1m;
			AssertHasErrors(container.JC_TotalLengthInfo);
			container.JC_TotalLength = 0m;
			AssertNoErrors(container.JC_TotalLengthInfo);
		}

		public void TestJC_TotalHeight()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_TotalHeightInfo);
			container.JC_TotalHeight = 10m;
			AssertNoErrors(container.JC_TotalHeightInfo);
			container.JC_TotalHeight = -1m;
			AssertHasErrors(container.JC_TotalHeightInfo);
			container.JC_TotalHeight = 0m;
			AssertNoErrors(container.JC_TotalHeightInfo);
		}

		public void TestJC_TotalWidth()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = ContainerModeForTesting;
			AssertNoErrors(container.JC_TotalWidthInfo);
			container.JC_TotalWidth = 10m;
			AssertNoErrors(container.JC_TotalWidthInfo);
			container.JC_TotalWidth = -1m;
			AssertHasErrors(container.JC_TotalWidthInfo);
			container.JC_TotalWidth = 0m;
			AssertNoErrors(container.JC_TotalWidthInfo);
		}

		#region Implementation
		protected virtual ZString ContainerModeForTesting
		{
			get
			{
				return Constants.ContainerModes.BreakBulk;
			}
		}
		#endregion
	}
}
