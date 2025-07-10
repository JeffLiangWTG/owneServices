using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class LayoutEntryInstructionDetailBasicUserControlTest : LayoutEntryInstructionDetailBasicUserControlAbstractTest<BaseJobDeclarationWithEntryInstructions>
	{
		protected override LayoutEntryInstructionDetailBasicUserControl CreateUserControl()
			=> new LayoutEntryInstructionDetailBasicUserControl();

		protected override IReadOnlyCollection<string> ExpectedControls
			=> new[]
			{
				"dynamicDetailsPanel",
			};

		public void TestDetailsLayoutChangedOnMessageTypeChange()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var panelLayoutProvider1 = Mock.Of<IPanelLayoutProvider>(x => x.Layout == new PanelLayout());
			var panelLayoutProvider2 = Mock.Of<IPanelLayoutProvider>(x => x.Layout == new PanelLayout());
			var provider = Mock.Of<IDeclarationFormLayoutProvider>();
			Mock.Get(provider).Setup(x => x.GetInstructionDetailsLayoutProvider(It.IsAny<BaseJobDeclaration>()))
				.Returns<BaseJobDeclaration>(x => x.JE_MessageType == JobMessageTypeList.Codes.Import ? panelLayoutProvider1 : panelLayoutProvider2);
			var objectHandle = Mock.Of<ObjectHandle>(x => x.GetObject() == provider);
			var hashtable = new Hashtable { { "Default", objectHandle } };
			ObjectFactory.Substitute("DeclarationFormLayoutProviders", hashtable);

			using (var form = new ZForm(declaration))
			using (var control = new LayoutEntryInstructionDetailBasicUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				Mock.Get(panelLayoutProvider1).VerifyGet(x => x.Layout, Times.Once);
				Mock.Get(panelLayoutProvider2).VerifyGet(x => x.Layout, Times.Never);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Mock.Get(panelLayoutProvider1).VerifyGet(x => x.Layout, Times.Once);
				Mock.Get(panelLayoutProvider2).VerifyGet(x => x.Layout, Times.Once);
			}

			Assert("Tested by mock", true);
		}
	}
}
