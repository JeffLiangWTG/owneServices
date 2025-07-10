using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(FWSEditForm))]
	sealed class FWSEditFormTest : ZFormBasherTest
	{
		protected override System.Collections.Generic.IEnumerable<Form> FormsToBash
		{
			get
			{
				yield return GetFormToBashCore();
				var header = Factory.New<FWSHeader>();
				header.Data.HasChanges = false;
				yield return new FWSEditForm(header);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<FWSHeader>();
			header.Data.HasChanges = false;
			return new FWSEditForm(header);
		}
	}
}
