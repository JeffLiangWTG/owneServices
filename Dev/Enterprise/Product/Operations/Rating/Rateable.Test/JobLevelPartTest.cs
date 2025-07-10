using Enterprise.Rating.Integration;
using NUnit.Framework;

namespace Enterprise.Rating.Rateable.Test;

public class JobLevelPartTest : TestCase
{
	public void TestSetQuantityForDeclarationLineCount()
	{
		MeasureType[] measureTypes = { MeasureType.HTS9902Line, MeasureType.HTS9903Line };
		var part = new JobLevelPart();

		foreach (var measureType in measureTypes)
		{
			AssertEquals(0, part.GetDeclarationLineCount(measureType));
			part.SetQuantity(measureType, 100, "unit");
			AssertEquals(100, part.GetDeclarationLineCount(measureType));
		}
	}
}
