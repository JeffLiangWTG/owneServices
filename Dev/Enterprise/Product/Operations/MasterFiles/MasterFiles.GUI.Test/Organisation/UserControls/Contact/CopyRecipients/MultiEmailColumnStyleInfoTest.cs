using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	abstract class MultiEmailColumnStyleInfoTest : TestCaseWithFactory
	{
		protected abstract MultiEmailColumnStyleInfo ColumnStyleInfo { get; }
		protected abstract Type ExpectedColumnStyleType { get; }

		public void TestReturnColumnStyleType()
		{
			AssertEquals("ColumnStyle is AddressOverrideColumnStyle", ExpectedColumnStyleType, ColumnStyleInfo.ColumnStyleType);
		}
	}
}
