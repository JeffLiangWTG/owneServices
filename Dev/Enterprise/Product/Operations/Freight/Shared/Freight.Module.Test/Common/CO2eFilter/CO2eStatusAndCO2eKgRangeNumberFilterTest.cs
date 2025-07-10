using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(CO2eStatusAndCO2eKgRangeNumberFilter))]
	internal class CO2eStatusAndCO2eKgRangeNumberFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCO2eStatusAndCO2eKgRangeNumberFilterProperties()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				Filter.CO2eStatus = CO2eStatusList.Codes.NotCalculated;

				AssertEquals("DefaultCO2eStatus", CO2eStatusList.Codes.Current, Filter.DefaultCO2eStatus);
				AssertEquals("CO2eStatus", CO2eStatusList.Codes.NotCalculated, Filter.CO2eStatus);
				AssertEquals("CO2StatusList CodesAsString", Filter.CO2eStatusList.CodesAsString, $"{CO2eStatusList.Codes.Current}, {CO2eStatusList.Codes.NotCalculated}, {CO2eStatusList.Codes.NotCurrent}, {CO2eStatusList.Codes.Pending}, {CO2eStatusList.Codes.Rejected}");
				AssertEquals("CO2StatusList ElementsAsString", Filter.CO2eStatusList.ElementsAsString,
					$@"{CO2eStatusList.Codes.Current} - {CO2eStatusList.Descriptions.Current}
{CO2eStatusList.Codes.NotCalculated} - {CO2eStatusList.Descriptions.NotCalculated}
{CO2eStatusList.Codes.NotCurrent} - {CO2eStatusList.Descriptions.NotCurrent}
{CO2eStatusList.Codes.Pending} - {CO2eStatusList.Descriptions.Pending}
{CO2eStatusList.Codes.Rejected} - {CO2eStatusList.Descriptions.Rejected}");
			}
		}

		CO2eStatusAndCO2eKgRangeNumberFilter Filter
		{
			get { return filter ?? (filter = (CO2eStatusAndCO2eKgRangeNumberFilter)GetNewBusinessObject()); }
		}
		CO2eStatusAndCO2eKgRangeNumberFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CO2eStatusAndCO2eKgRangeNumberFilter("CO2e", null);
		}
	}
}
