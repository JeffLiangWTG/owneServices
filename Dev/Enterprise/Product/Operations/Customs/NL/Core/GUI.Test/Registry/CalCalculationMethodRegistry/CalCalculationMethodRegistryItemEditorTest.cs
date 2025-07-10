using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(CalCalculationMethodRegistryItemEditor))]
sealed class CalCalculationMethodRegistryItemEditorTest : RegistryItemEditorTestCase
{
	protected override RegistryItemEditor GetEditor() => new CalCalculationMethodRegistryItemEditor(new CalCalculationMethodRegistryDataType(), new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), new BusinessObjectFactory());

	protected override bool GetEditorPaneEnabledState(Control editorPane)
	{
		var control = (CalCalculationMethodRegistryUserControl)editorPane;
		var calCalculationMethodGrid = control.FindSingle<ZArchitecture.ZGrid>("CalCalculationMethodGrid");
		return !calCalculationMethodGrid.ReadOnly;
	}

	protected override Type GetExpectedEditorPaneType() => typeof(CalCalculationMethodRegistryUserControl);

	protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new CalCalculationMethodRegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, CalCalculationMethodRegistryCollection.DefaultCollection);

	protected override object[] GetValidRegistryValues() => new object[] { new CalCalculationMethodRegistryCollection { new CalCalculationMethodRegistry { CalculationMethodName = CalculationMethodList.Codes.DUT, CalculationMethodValue = CalCalculationMethodRegistry.BasedOnDutiesVatValue, CalculationMethodDefault = CalCalculationMethodRegistry.BasedOnDutiesVatDefault } } };

	protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.TopLeftRight;
}
