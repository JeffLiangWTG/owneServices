using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UniversalDataMenuItemDescriptorTest : TestCaseWithFactory
	{
		public void TestNewUniversalDataMenuItemDescriptor()
		{
			MultilingualString caption = (NoResString)"caption";
			EventHandler handler = (s, e) => { };

			var menuItemDescriptor = new UniversalDataMenuItemDescriptor(caption, handler);

			AssertEquals("Caption", caption, menuItemDescriptor.Caption);
			AssertEquals("Handler", handler, menuItemDescriptor.Handler);
		}
	}
}
