using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MaximumAllowedTransactionAmountControl))]
	class MaximumAllowedTransactionAmountControlTest : Registry.GUI.Testing.RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new MaximumAllowedTransactionAmount() { MaximumAllowedHeaderAmount = 1M, MaximumAllowedLineAmount = 2M };
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			var maximumAllowedTransactionAmountControl = (MaximumAllowedTransactionAmountControl)control;
			var maximumAllowedLineAmountEdit = (ZCalcEdit)maximumAllowedTransactionAmountControl.Controls.Find("LineMaximumAllowedAmountEdit", true)[0];
			var maximumAllowedHeaderAmountEdit = (ZCalcEdit)maximumAllowedTransactionAmountControl.Controls.Find("HeaderMaximumAllowedAmountEdit", true)[0];
			return maximumAllowedLineAmountEdit.ReadOnly && maximumAllowedHeaderAmountEdit.ReadOnly;
		}
	}
}
