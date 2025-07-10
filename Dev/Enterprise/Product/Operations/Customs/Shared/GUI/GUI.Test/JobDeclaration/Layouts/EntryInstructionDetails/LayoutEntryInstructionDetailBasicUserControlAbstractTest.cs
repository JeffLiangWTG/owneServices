using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class LayoutEntryInstructionDetailBasicUserControlAbstractTest<TDeclaration> : TestCaseWithFactory
		where TDeclaration : BaseJobDeclaration
	{
		public void TestBasicDetailsControl()
		{
			var declaration = GetDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = CreateUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions("Details Controls", () =>
				{
					foreach (var controlName in ExpectedControls)
					{
						var controls = control.Controls.Find(controlName, true);
						AssertEquals($"Control Found: {controlName}", true, controls.Any());
					}
				});
			}
		}

		protected abstract LayoutEntryInstructionDetailBasicUserControl CreateUserControl();

		protected abstract IReadOnlyCollection<string> ExpectedControls { get; }

		protected virtual TDeclaration GetDeclaration() => Factory.New<TDeclaration>();
	}
}
