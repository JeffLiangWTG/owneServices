using System;
using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class MeasureTypeDescriptionTest : TestCase
	{
		public void TestMeasurementDescriptionShouldCoverAllMeasureTypes()
		{
			var measureTypes = (MeasureType[])Enum.GetValues(typeof(MeasureType));
			foreach (var measureType in measureTypes)
			{
				var description = MeasureTypeDescriptions.GetDescription(measureType);
				AssertNotNullOrEmpty($"{measureType} need description", description);
			}
		}
	}
}
