using System;
using Enterprise.NumberFountain.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class NumberFountainWithStartNumForTest : NumberFountainForTest
	{
		public NumberFountainWithStartNumForTest(int minNumber)
			: base(Guid.Empty, minNumber)
		{
		}
	}
}
