using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class NonPersistentAddressOverrideColumnStyleTest : MultiEmailColumnStyleTest
	{
		MultiEmailColumnStyle columnStyle;
		protected override MultiEmailColumnStyle ColumnStyle => columnStyle ?? (columnStyle = new NonPersistentAddressOverrideColumnStyle<DocDeliveryContact>(new NonPersistentAddressOverrideColumnStyleInfo<DocDeliveryContact>()));

		protected override Type ExpectedControlType { get; } = typeof(NonPersistentAddressOverrideCombinationControl<DocDeliveryContact>);
	}
}
