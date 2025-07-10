using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI
{
	public class ReportingBookAccountingJournalPrintOptionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ReportingBookAccountingJournalPrintOptionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new ReportingBookAccountingJournalPrintOptionControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
