using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;

namespace Enterprise.Customs.PL.GUI.Helpers;

internal class NegateBoolBinding : KBinding
{
	public NegateBoolBinding(string propertyName, object dataSource, string dataMember) : base(propertyName, dataSource, dataMember)
	{
		AddNegateValueEvents();
	}

	public NegateBoolBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled) : base(propertyName, dataSource, dataMember, formattingEnabled)
	{
		AddNegateValueEvents();
	}

	public NegateBoolBinding(string propertyName, object dataSource, string dataMember, bool formattingEnabled, DataSourceUpdateMode dataSourceUpdateMode) : base(propertyName, dataSource, dataMember, formattingEnabled, dataSourceUpdateMode)
	{
		AddNegateValueEvents();
	}

	void AddNegateValueEvents()
	{
		Parse += SwitchBool;
		Format += SwitchBool;
	}

	void SwitchBool(object sender, ConvertEventArgs e)
	{
		if (e.Value is ZBool)
		{
			e.Value = new ZBool(!(ZBool)e.Value);
		}
		else if (e.Value is bool)
		{
			e.Value = !(bool)e.Value;
		}
	}
}
