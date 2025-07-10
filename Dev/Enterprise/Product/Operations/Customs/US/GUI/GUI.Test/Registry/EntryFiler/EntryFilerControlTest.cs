using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.GUI.Testing
{
	[TestedType(typeof(EntryFilerControl))]
	sealed class EntryFilerControlTest : RegistryZUserControlTestCase
	{
		public void TestFilerCodeCharacterCase()
		{
			using (EntryFilerControl control = new EntryFilerControl())
			{
				AssertEquals(CharacterCasing.Upper, control.EntryFilerCodeTextBox.CharacterCasing);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new EntryFiler();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			bool result = false;
			EntryFiler filer = control.CurrentDataItem as EntryFiler;
			if (filer != null)
			{
				result = filer.ReadOnly;
			}
			return result;
		}
	}
}
