using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class NonPersistentAddressOverrideColumnStyleInfoTest : MultiEmailColumnStyleInfoTest
	{
		protected override MultiEmailColumnStyleInfo ColumnStyleInfo { get; } = new NonPersistentAddressOverrideColumnStyleInfo<DocDeliveryContact>();

		protected override Type ExpectedColumnStyleType { get; } = typeof(NonPersistentAddressOverrideColumnStyle<DocDeliveryContact>);
	}
}
