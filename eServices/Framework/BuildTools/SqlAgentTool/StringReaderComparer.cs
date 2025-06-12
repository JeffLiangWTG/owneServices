using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;

namespace eServices.BuildTools.SqlAgentTool;

internal class StringReaderComparer(RootComparer rootComparer) : BaseTypeComparer(rootComparer)
{
	public override bool IsTypeMatch(Type type1, Type type2)
	{
		return type1 == typeof(string);
	}

	public override void CompareType(CompareParms parms)
	{
		if (!AreEqual((string)parms.Object1, (string)parms.Object2))
		{
			AddDifference(parms);
		}
	}

	public static bool AreEqual(string string1, string string2)
	{
		using var reader1 = new StringReader(string1);
		using var reader2 = new StringReader(string2);
		string? line1, line2;
		while ((line1 = reader1.ReadLine()) is not null | (line2 = reader2.ReadLine()) is not null)
		{
			if (line1 != line2)
			{
				return false;
			}
		}
		return true;
	}
}
