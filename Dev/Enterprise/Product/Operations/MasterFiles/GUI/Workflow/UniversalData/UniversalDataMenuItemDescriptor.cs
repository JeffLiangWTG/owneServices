using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	public class UniversalDataMenuItemDescriptor
	{
		public UniversalDataMenuItemDescriptor(MultilingualString caption, EventHandler handler)
		{
			Caption = caption;
			Handler = handler;
		}

		public MultilingualString Caption { get; private set; }
		public EventHandler Handler { get; private set; }
	}
}
