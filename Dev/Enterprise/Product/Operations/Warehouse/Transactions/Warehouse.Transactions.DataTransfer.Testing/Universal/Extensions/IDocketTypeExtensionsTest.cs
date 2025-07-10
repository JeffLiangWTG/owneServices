using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class IDocketTypeExtensionsTest : TestCase
	{
		#region TestThrowImportFailureExceptionIfNotEmpty

		public void TestThrowImportFailureExceptionIfNotEmpty()
		{
			var dummy = new DummyDocketType();
			AssertNoExceptionThrown(() => dummy.ThrowImportFailureExceptionIfNotEmpty(new ZStringBuilder()));

			var builder = new ZStringBuilder("Test");
			builder.Append("Hello");
			AssertExceptionThrown(typeof(DataObjectReadFailureException), string.Format("Cannot Import Dummy\r\nTest\r\nHello"),
				() => dummy.ThrowImportFailureExceptionIfNotEmpty(builder));
		}

		class DummyDocketType : IDocketType
		{
			public string DocketType => "Dummy";
		}

		#endregion
	}
}