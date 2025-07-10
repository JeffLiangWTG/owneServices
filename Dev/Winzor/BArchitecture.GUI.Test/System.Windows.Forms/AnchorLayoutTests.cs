using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

public class AnchorLayoutTests
{
	WinzorTestContext context;
	static IEnumerable<AnchorStyles> AnchorCombinations => GetAllAnchors();

	[SetUp]
	public void SetUp()
	{
		context = new WinzorTestContext();
	}

	[TearDown]
	public void TearDown()
	{
		context.Dispose();
	}

	[TestCaseSource(nameof(AnchorCombinations))]
	public async Task TestAfterSettingAnchorGetReturnsCorrectValue(AnchorStyles anchor)
	{
		var control = await GetControlRenderedOnFormAsync<Control>(anchor);
		control.Invoke(() => control.Anchor = anchor);

		Assert.That(control.Anchor, Is.EqualTo(anchor));
	}

	[TestCaseSource(nameof(AnchorCombinations))]
	public async Task TestAnchoredControlsStillAutoSizes(AnchorStyles anchor)
	{
		var control = await GetControlRenderedOnFormAsync<Button>(anchor, new Size(1, 10), "This is a long string");
		control.Invoke(() => control.AutoSize = true);

		Assert.That(control.Size.Width, Is.GreaterThan(1));
	}

	[TestCaseSource(nameof(AnchorCombinations))]
	public async Task TestAnchoredControlsCanBeRelocated(AnchorStyles anchor)
	{
		var control = await GetControlRenderedOnFormAsync<Control>(anchor, new Size(1, 10));
		var initialLocation = control.Location;

		var targetLocation = new Point(1000, 1000);
		Assume.That(initialLocation, Is.Not.EqualTo(targetLocation));

		control.Invoke(() => control.Location = targetLocation);

		Assert.That(control.Location, Is.EqualTo(targetLocation));
	}

	[Test]
	// When a button is anchored left and right it will grow with it's parent
	// However it will NOT shrink with it's parent if its autosize is set to GrowOnly
	public async Task TestAnchoredButtonAutoSizeGrowOnlyRespected()
	{
		var button = await GetControlRenderedOnFormAsync<Button>(AnchorStyles.Left | AnchorStyles.Right, new Size(1, 10));
		button.Invoke(() => button.AutoSize = true);
		var form = button.Parent!;

		Assume.That(button.Width, Is.EqualTo(6));
		Assume.That(button.AutoSizeMode, Is.EqualTo(AutoSizeMode.GrowOnly));

		var initialFormSize = form.Width;
		button.Invoke(() => form.Width = 10000);
		Assume.That(button.Width, Is.GreaterThan(initialFormSize));

		button.Invoke(() => form.Width = initialFormSize);
		Assert.That(button.Width, Is.GreaterThan(initialFormSize));
	}

	[Test]
	public async Task TestControlAnchoredToAllSidesResizesWithForm()
	{
		var control = await GetControlRenderedOnFormAsync<Control>(AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, new Size(100, 100));
		var parent = control.Parent!;
		var initialParentSize = parent.Size;
		var initialControlSize = control.Size;

		Assume.That(control.Size, Is.EqualTo(new Size(100, 100)));

		parent.Invoke(() =>
		{
			parent.Width += 1000;
			parent.Height += 500;
		});

		Assume.That(parent.Width, Is.EqualTo(initialParentSize.Width + 1000));
		Assume.That(parent.Height, Is.EqualTo(initialParentSize.Height + 500));

		Assert.That(control.Width, Is.GreaterThan(initialControlSize.Width));
		Assert.That(control.Height, Is.GreaterThan(initialControlSize.Height));
	}

	[TestCaseSource(nameof(AnchorCombinations))]
	public async Task TestAnchorMaintainsDistanceBetweenControlAndAnchorPositions(AnchorStyles anchor)
	{
		var control = await GetControlRenderedOnFormAsync<Control>(AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, Size.Empty);
		var parent = control.Parent!;

		var initialLeft = control.Left;
		var initialTop = control.Top;
		var initialRightDistance = parent.Right - control.Right;
		var initialBottomDistance = parent.Bottom - control.Bottom;
		var initialParentSize = parent.Size;
		var initialControlSize = control.Size;

		parent.Invoke(() =>
		{
			parent.Width += 1000;
			parent.Height += 500;
		});

		Assume.That(parent.Width, Is.EqualTo(initialParentSize.Width + 1000));
		Assume.That(parent.Height, Is.EqualTo(initialParentSize.Height + 500));

		if (IsAnchored(AnchorStyles.Left, anchor))
		{
			Assert.That(control.Left, Is.EqualTo(initialLeft));
		}

		if (IsAnchored(AnchorStyles.Top, anchor))
		{
			Assert.That(control.Top, Is.EqualTo(initialTop));
		}

		if (IsAnchored(AnchorStyles.Right, anchor))
		{
			Assert.That(parent.Right - control.Right, Is.EqualTo(initialRightDistance));
		}

		if (IsAnchored(AnchorStyles.Bottom, anchor))
		{
			Assert.That(parent.Bottom - control.Bottom, Is.EqualTo(initialBottomDistance));
		}

		if (IsAnchored(AnchorStyles.Top | AnchorStyles.Bottom, anchor))
		{
			Assert.That(control.Height, Is.GreaterThan(initialControlSize.Height));
		}

		if (IsAnchored(AnchorStyles.Left | AnchorStyles.Right, anchor))
		{
			Assert.That(control.Width, Is.GreaterThan(initialControlSize.Width));
		}
	}

	[Test]
	public async Task TestThatAnchorPlacementIsPerformedBeforeSizeChangedEvent()
	{
		Form form = null;
		ListBox listBox = null;
		Label label = null;

		await context.RenderFormAsync(() =>
		{
			form = new Form() { Size = new Size(200, 200) };
			label = new Label() { Text = "Text" };
			listBox = new ListBox()
			{
				Anchor = AnchorStyles.Top | AnchorStyles.Bottom,
				Size = new Size(100, 100)
			};

			form.Controls.Add(label);
			form.Controls.Add(listBox);

			form.SizeChanged += (object sender, EventArgs e) =>
			{
				label.Top = listBox.Bottom + 30;
			};

			return form;
		});

		form.Invoke(() => { form.Height += 100; });
		Assert.That(listBox.Height, Is.EqualTo(182));
		Assert.That(label.Top, Is.EqualTo(listBox.Bottom + 30));
	}

	static bool IsAnchored(AnchorStyles desiredStyles, AnchorStyles anchorStyles) => (anchorStyles & desiredStyles) == desiredStyles;

	async Task<T> GetControlRenderedOnFormAsync<T>(AnchorStyles anchor, Size? size = null, string text = "") where T : Control, new()
	{
		T control = null;
		await context.RenderControlOnFormAsync(() =>
		{
			control = new T { Anchor = anchor, Size = size ?? Size.Empty, Text = text };
			return control;
		});

		return control;
	}

	static IEnumerable<AnchorStyles> GetAllAnchors()
	{
		var anchorStyles = new[] { AnchorStyles.Bottom, AnchorStyles.Left, AnchorStyles.Right, AnchorStyles.Top };

		IEnumerable<AnchorStyles> GetCombinationsWithoutRepetition(AnchorStyles[] styles)
		{
			if (styles.Length == 1)
			{
				return styles.Select(s => s);
			}

			var lastStyle = styles[^1];
			var combinations = GetCombinationsWithoutRepetition(styles[..^1]).ToArray();
			return combinations.Concat(combinations.Select(s => s | lastStyle)).Concat(new[] { lastStyle });
		}

		return GetCombinationsWithoutRepetition(anchorStyles);
	}
}
