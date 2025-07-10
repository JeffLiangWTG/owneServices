using System.Data;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.Tests
{
	sealed class StmMenuItemExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetDocumentDirection()
		{
			var menuItem = new Mock<IStmMenuItem>();

			menuItem.Setup(m => m.SU_DocumentDirection).Returns(ZString.Empty);
			AssertEquals("menuItem.GetDocumentDirection()", DocumentDirection.ANY, menuItem.Object.GetDocumentDirection());

			menuItem.Setup(m => m.SU_DocumentDirection).Returns("DEP");
			AssertEquals("menuItem.GetDocumentDirection()", DocumentDirection.DEP, menuItem.Object.GetDocumentDirection());

			menuItem.Setup(m => m.SU_DocumentDirection).Returns("ARV");
			AssertEquals("menuItem.GetDocumentDirection()", DocumentDirection.ARV, menuItem.Object.GetDocumentDirection());

			menuItem.Setup(m => m.SU_DocumentDirection).Returns("XXX");
			AssertEquals("menuItem.GetDocumentDirection()", DocumentDirection.ANY, menuItem.Object.GetDocumentDirection());

			menuItem.Setup(m => m.SU_DocumentDirection).Returns("ANY");
			AssertEquals("menuItem.GetDocumentDirection()", DocumentDirection.ANY, menuItem.Object.GetDocumentDirection());
		}

		public void TestIsApplicable_Document()
		{
			var parent = Factory.New<DocDummyBusinessObject>();
			parent.Z0_Code = "AAA";

			var command = Factory.New<DocumentCommand>();
			command.Parent = parent;
			command.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			command.SU_BusinessContext = nameof(BusinessContext.Test);
			command.SU_IsPublished = true;
			command.SU_MenuName = "Test Menu Name";
			command.SU_MenuIndex = 0;
			command.SU_FilterList = "\"<Z0_Code>\" == \"AAA\"";

			AssertEquals("Command is applicable as matches the filter", true, command.IsApplicable((BusinessObject)parent));
			AssertEquals("Command is applicable as matches the filter", true, command.IsApplicable((IDocumentSupportable)parent));
		}

		public void TestIsApplicable_Form()
		{
			var parent = (DummyBusinessObject)ObjectFactory.Get<IDocumentVisualizerTestHelper>().CreateBusinessObjectWithDocumentVisualiserSupport(Factory);
			parent.Z0_Code = "AAA";

			var command = Factory.New<DocumentCommand>();
			command.Parent = (IDocumentSupportable)parent;
			command.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			command.SU_BusinessContext = nameof(BusinessContext.Test);
			command.SU_IsPublished = true;
			command.SU_MenuName = "Test Menu Name";
			command.SU_MenuIndex = 0;
			command.SU_FilterList = "Z0_Code == \"AAA\"";

			AssertEquals("Command is applicable as matches the filter", true, command.IsApplicable(parent));
			AssertEquals("Command is applicable as matches the filter", true, command.IsApplicable((IDocumentSupportable)parent));
		}
	}

	class DocDummyBusinessObject : DummyBusinessObject, IDocumentSupportable
	{
		public DocDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IDocumentSupportable Members

		public virtual DocumentSupporter DocumentSupporter
		{
			get => documentSupporter ??= new Mock<DocumentSupporter>(this).Object;
		}

		DocumentSupporter documentSupporter;

		#endregion
	}
}
