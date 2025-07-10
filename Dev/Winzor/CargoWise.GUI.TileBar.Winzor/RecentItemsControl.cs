using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.Main.Navigation;
using Enterprise.ZArchitecture.Favorites;
using FontFamily = System.Windows.Media.FontFamily;
using MenuItem = CargoWise.Main.Navigation.MenuItem;

namespace CargoWise.GUI.TileBar;

public partial class RecentItemsControl : UserControl, IWinzorSupportedWPFContent
{
	public RecentItemsControl()
	{
		AutomaticallyReRenderOnEventCallbacks = true;
	}

	public override bool UseParentDivForLayout => false;
	public FontFamily FontFamily
	{
		get => new FontFamily(Font.Name);
		set
		{
			if (value != null)
			{
				Font = new Font(value.FamilyName, Font.Size, Font.Style);
			}
			else
			{
				Font = null;
			}
		}
	}

	public object DataContext
	{
		get => dataContext;
		set
		{
			if (dataContext != value && value is MenuSection menuSection)
			{
				menuSection.Items.CollectionChanged += (_, _) => NotifyRenderRequired();
			}

			dataContext = value;
		}
	}
	object dataContext;

	public Color PanelBackgroundColor { get; set; }
	public Color PanelBorderColor { get; set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Referenced in RecentItemsControl.razor")]
	ImmutableList<MenuItem> RecentItemsToRender;

	protected override void OnBeforeRender()
	{
		base.OnBeforeRender();
		RecentItemsToRender = (DataContext as MenuSection)?.Items.OrderBy(i => i.Index).ToImmutableList();
	}

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		RecentItemManager.RecentItemsChanged += RecentItemManager_RecentItemsChanged;
	}

	void RecentItemManager_RecentItemsChanged(object sender, RecentItemsChangedEventArgs e)
	{
		RecentItemsToRender = (DataContext as MenuSection)?.Items.OrderBy(i => i.Index).ToImmutableList();
		NotifyRenderRequired();
	}

	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed)
		{
			RecentItemManager.RecentItemsChanged -= RecentItemManager_RecentItemsChanged;
		}
		base.Dispose(disposing);
	}
}
