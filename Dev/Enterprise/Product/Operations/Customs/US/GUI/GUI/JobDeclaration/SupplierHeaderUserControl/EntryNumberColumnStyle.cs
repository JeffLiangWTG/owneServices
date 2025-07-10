using System;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.GUI
{
	public class EntryNumberColumnStyle : ZCodeFindBoxColumnStyle
	{
		public EntryNumberColumnStyle(EntryNumberColumnStyleInfo columnInfo)
				: this(() => new ZGridEntryNumberUserControl(), columnInfo)
		{ }

		EntryNumberColumnStyle(Func<ZGridEntryNumberUserControl> control, EntryNumberColumnStyleInfo columnInfo)
			: base(control, columnInfo)
		{
		}
	}

	[SuppressCheckControlModuleId]
	[SuppressCheckControlLookupList]
	public class EntryNumberColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType => typeof(EntryNumberColumnStyle);
	}

	class ZGridEntryNumberUserControl : ZGridFindBox
	{
		internal EmbeddedModulePopup CreateEmbeddedPopupInternal(ZFilterModule module) => CreateEmbeddedPopup(module);
		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module) => new CustomsEmbeddedModulePopup(module);
	}
}
