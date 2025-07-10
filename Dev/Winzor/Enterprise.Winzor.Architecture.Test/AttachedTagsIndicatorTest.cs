using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Moq;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class AttachedTagsIndicatorTest
{
	BusinessObjectFactory ObjectFactory { get; set; }

	[OneTimeSetUp]
	public void InitFactory() => ObjectFactory = new BusinessObjectFactory();

	[TestCase(BMBoardSectionOrientation.Horizontal, "background:linear-gradient(to right,Aqua 100%,Aqua 0.1%);")]
	[TestCase(BMBoardSectionOrientation.Vertical, "background:linear-gradient(to bottom,Aqua 100%,Aqua 0.1%);")]
	public async Task AttachedTagsWithOneColor(BMBoardSectionOrientation orientation, string expectStyle)
	{
		using var ctx = new EnterpriseTestContext();
		var parent = new Mock<ITaskCardComponentParent>();
		parent.Setup(m => m.IsPreview).Returns(false);
		parent.Setup(m => m.Task).Returns(ObjectFactory.New<ProcessTask>());
		parent.Setup(m => m.CardContent).Returns(MockCardContent(ObjectFactory, SizedButtonBorderStyle.Default, Color.Aqua));

		var actualStyle = string.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var control = new AttachedTagsIndicator(parent.Object, orientation);
			control.SetupTags();
			actualStyle = control.ExtraStyleString;
		});

		Assert.That(actualStyle, Contains.Substring(expectStyle));
	}

	[TestCase(BMBoardSectionOrientation.Horizontal, "background:linear-gradient(to right,Aqua 25%,Red 0.1%,Red 50%,Black 0.1%,Black 75%,Green 0.1%,Green 100%);")]
	[TestCase(BMBoardSectionOrientation.Vertical, "background:linear-gradient(to bottom,Aqua 25%,Red 0.1%,Red 50%,Black 0.1%,Black 75%,Green 0.1%,Green 100%);")]
	public async Task AttachedTagsOverOneColorHorizontal(BMBoardSectionOrientation orientation, string expectStyle)
	{
		using var ctx = new EnterpriseTestContext();
		var parent = new Mock<ITaskCardComponentParent>();
		parent.Setup(m => m.IsPreview).Returns(false);
		parent.Setup(m => m.Task).Returns(ObjectFactory.New<ProcessTask>());
		parent.Setup(m => m.CardContent).Returns(MockCardContent(ObjectFactory, SizedButtonBorderStyle.Default, Color.Aqua, Color.Red, Color.Black, Color.Green));

		var actualStyle = string.Empty;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using var control = new AttachedTagsIndicator(parent.Object, orientation);
			control.SetupTags();
			actualStyle = control.ExtraStyleString;
		});

		Assert.That(actualStyle, Contains.Substring(expectStyle));
	}

	[Test]
	public async Task AttachedTagsHeightExcludeBordersHeight([Values] bool isLarge)
	{
		using var ctx = new EnterpriseTestContext();
		var parent = new Mock<ITaskCardComponentParent>();
		parent.Setup(m => m.IsPreview).Returns(false);
		parent.Setup(m => m.Task).Returns(ObjectFactory.New<ProcessTask>());
		parent.Setup(m => m.CardContent).Returns(MockCardContent(ObjectFactory, new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Outset, isLarge), Color.Aqua));

		var controlHeight = 0;
		var actualStyle = string.Empty;
		var group = VisualBoardsTestHelper.CreateGroup(ObjectFactory, "AAA");
		var workflow = VisualBoardsTestHelper.CreateWorkflowAndParents<OrgHeader>(ObjectFactory, "Container", releaseGroupPK: group.PK);
		var task = VisualBoardsTestCase.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, description: "Task 1");
		var system = VisualBoardsTestHelper.GetOrCreateSystem(ObjectFactory, "XBC");
		var bucket = VisualBoardsTestHelper.CreateBucket(system);
		var section = VisualBoardsTestHelper.CreateBoardSection(bucket);
		var viewModel = VisualBoardsTestHelper.CreateViewModel(section, workflow);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var taskCard = new TaskCardControl(parent.Object.CardContent, viewModel, new CellContent(0, 0, CellContentType.Cards), false);
			using var control = new AttachedTagsIndicator(parent.Object);
			control.SetupTags();
			control.Parent = taskCard;
			controlHeight = control.Height;
			actualStyle = control.ExtraStyleString;
		});
		var expectedHeight = isLarge ? controlHeight - 6 : controlHeight - 2;

		Assert.That(actualStyle, Contains.Substring($"height: {expectedHeight}px"));
	}

	ICardContent MockCardContent(BusinessObjectFactory factory, SizedButtonBorderStyle borderStyle, params Color[] colors)
	{
		var definition = VisualBoardsTestHelper.CreateTagDefinition(factory, "AZE");

		for (var i = 0; i < colors.Length; i++)
		{
			VisualBoardsTestHelper.CreateTagMagnitude(definition, "CZ" + i, color: colors[i]);
		}

		return new TinyCardContent
		{
			BorderStyle = borderStyle,
			ApplicableTagMagnitudes = new HashSet<ZGuid>(definition.Magnitudes.Select(m => m.PK)).ToImmutableHashSet(),
			Definitions = new TagDefinitionCache(factory),
		};
	}

	class TinyCardContent : ICardContent
	{
		public ImmutableHashSet<ZGuid> ApplicableTagMagnitudes { get; set; }
		public object Bindable { get; set; }
		public Color BorderColor { get; set; }
		public SizedButtonBorderStyle BorderStyle { get; set; }
		public ICardCapacityDto CapacityDto { get; set; }
		public CardType CardType { get; set; }
		public TagDefinitionCache Definitions { get; set; }
		public ZGuid Identifier { get; set; }
		public bool IsCurrent { get; set; }
		public ZDateTime LastEditTime { get; set; }
		public ZString NoteText { get; set; }
		public ZGuid TaskIdentifier { get; set; }
		public ITaskOrderable TaskOrderable { get; set; }
		public ZGuid WorkflowIdentifier { get; set; }
		public ZString WorkflowType { get; set; }
		public TValue GetCustomAttribute<TValue>(StaticControlProperty key) => default;

		public ZString DisplayTextForDebugging { get; set; }
	}
}
