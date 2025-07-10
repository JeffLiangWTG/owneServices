using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IJobCostingPlugInHelperTest : TestCaseWithFactory
	{
		public void TestGetDirectionCode()
		{
			var expectedList = new[] {
				(Directions.Unknown, (string)ZString.Empty),
				(Directions.Import, Core.Constants.FreightShipmentDirection.Code.Import),
				(Directions.Export, Core.Constants.FreightShipmentDirection.Code.Export),
				(Directions.Domestic, Core.Constants.FreightShipmentDirection.Code.Domestic),
				(Directions.CrossTrade, Core.Constants.FreightShipmentDirection.Code.Other),
			};
			AssertArrayEqualsByElements(
				Enum.GetNames(typeof(Directions)).OrderBy(x => x).ToArray(),
				expectedList.Select(x => x.Item1.ToString()).OrderBy(x => x).ToArray()
			);

			foreach (var expectedSetting in expectedList)
			{
				AssertEquals(expectedSetting.Item2, IJobCostingPlugInHelper.GetDirectionCode(expectedSetting.Item1));
			}

			AssertEquals(ZString.Empty, IJobCostingPlugInHelper.GetDirectionCode(null));
		}
	}
}
