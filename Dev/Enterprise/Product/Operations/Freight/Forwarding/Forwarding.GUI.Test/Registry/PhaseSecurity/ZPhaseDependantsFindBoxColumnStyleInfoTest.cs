using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ZPhaseDependantsFindBoxColumnStyleInfoTest : TestCase
	{
		public void TestColumnStyleType()
		{
			AssertEquals(typeof(ZPhaseDependantsFindBoxColumnStyle), new ZPhaseDependantsFindBoxColumnStyleInfo().ColumnStyleType);
		}

		public void TestCharacterCasing()
		{
			AssertEquals(CharacterCasing.Normal, new ZPhaseDependantsFindBoxColumnStyleInfo().CharacterCasing);
		}
	}
}
