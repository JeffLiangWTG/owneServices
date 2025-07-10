using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class RegistrationNumberModelTest : TestCase
	{
		public void TestConstructor()
		{
			var model1 = new RegistrationNumberModel("21 002 936 091", "23 112 936 091", IdentifierType.ABN);
			var model2 = new RegistrationNumberModel(string.Empty, "23 112 888 888", IdentifierType.ABN);

			CombineAssertions(() =>
			{
				AssertEquals("21 002 936 091", model1.CurrentRegistryNumber);
				AssertEquals("23 112 936 091", model1.NewRegistryNumber);
				AssertEquals(nameof(IdentifierType.ABN), model1.RegistryNumberType);

				AssertEquals(string.Empty, model2.CurrentRegistryNumber);
				AssertEquals("23 112 888 888", model2.NewRegistryNumber);
				AssertEquals(nameof(IdentifierType.ABN), model2.RegistryNumberType);
			});
		}

		public void TestDefaultMergeAction()
		{
			var model1 = new RegistrationNumberModel("21 002 936 091", "23 112 936 091", IdentifierType.ABN);
			var model2 = new RegistrationNumberModel(string.Empty, "23 112 888 888", IdentifierType.ABN);

			CombineAssertions(() =>
			{
				AssertEquals(MergeAction.Codes.Update, model1.SelectedMergeAction);
				AssertEquals(MergeAction.Codes.Add, model2.SelectedMergeAction);
			});
		}

		public void TestMergeActions()
		{
			var expectedActions1 = new List<string>() { "Update", "Ignore" };
			var expectedActions2 = new List<string>() { "Add", "Ignore" };

			var model1 = new RegistrationNumberModel("21 002 936 091", "23 112 936 091", IdentifierType.ABN);
			var model2 = new RegistrationNumberModel(string.Empty, "23 112 888 888", IdentifierType.ABN);

			CombineAssertions(() =>
			{
				AssertSequencesEqual(expectedActions1, model1.MergeActions.Select(m => m.Value));
				AssertSequencesEqual(expectedActions2, model2.MergeActions.Select(m => m.Value));
			});
		}
	}
}
