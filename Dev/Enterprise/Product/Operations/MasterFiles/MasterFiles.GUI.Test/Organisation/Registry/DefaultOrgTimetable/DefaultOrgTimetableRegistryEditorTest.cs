using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(DefaultOrgTimetableRegistryEditor))]
	sealed class DefaultOrgTimetableRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new DefaultOrgTimetableRegistryEditor(new DefaultOrgTimetableDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DefaultOrgTimetableControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(DefaultOrgTimetableControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var defaultOrgtimetableSettings = new DefaultOrgTimetableSettingsCollection();
			defaultOrgtimetableSettings.AddNew();

			return new DefaultOrgTimetableRegistryItem(
				"DefaultOrgTimetable",
				(NoResString)"",
				(NoResString)"Default Pickup & Delivery Timetable",
				(NoResString)"Default Pickup & Delivery Timetable",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				defaultOrgtimetableSettings
				);
		}

		protected override object[] GetValidRegistryValues()
		{
			DefaultOrgTimetableSettingsCollection[] items = new DefaultOrgTimetableSettingsCollection[1];

			items[0] = new DefaultOrgTimetableSettingsCollection();
			var settings = items[0].AddNew();
			DefaultOrgTimetable item = settings.Timetables.AddNew();
			item.Type = OrgTimetableType.Codes.Pickup;
			item.From = new ZDateTime(2015, 1, 1, 9, 0, 0);
			item.To = new ZDateTime(2015, 1, 1, 17, 0, 0);
			item.Day = "MON";

			return items;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
