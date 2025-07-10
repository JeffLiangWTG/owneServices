using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(DefaultStatementPrintDateControl))]
	sealed class DefaultStatementPrintDateControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DefaultStatementPrintDate();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			DefaultStatementPrintDate statementPrintDateData = control.CurrentDataItem as DefaultStatementPrintDate;
			if (statementPrintDateData != null)
			{
				result = statementPrintDateData.ReadOnly;
			}
			return result;
		}
	}
}
