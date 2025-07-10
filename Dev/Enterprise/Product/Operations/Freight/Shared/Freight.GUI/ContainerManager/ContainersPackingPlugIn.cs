using System;
using System.Collections;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.GUI
{
	public abstract class ContainersPackingPlugIn : ZPlugIn
	{
		public ContainersPackingPlugIn(CommonConsol hostEntity)
			: base(hostEntity)
		{
			this.consol = hostEntity;
			if (this.consol != null)
			{
				Enabled = true;
			}
		}

		readonly CommonConsol consol;

		#region IZPlugIn Members

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			if (this.consol.Containers != null)
			{
				this.consol.Containers.QueryReJoinPackLines -= new CommonContainerCollection.QueryReJoinPackLinesEventHandler(Containers_QueryReJoinPackLines);
			}

			if (IsEligibleForUpdatingExportReferenceNumber)
			{
				this.consol.JK_RL_NKLoadPortInfo.ValueChanged -= HandleExportRefNumberHeaderUpdate;
				this.consol.JK_TransportModeInfo.ValueChanged -= HandleExportRefNumberHeaderUpdate;
			}
		}

		public override bool CanDelete
		{
			get { return true; }
		}

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override Control GetNewUserControl()
		{
			PackingDetailsUserControl = new PackingDetailsUserControl();
			PackingDetailsUserControl.AfterFirstBinding += (s, e) => OnUserControlAfterFirstBinding();

			if (IsEligibleForUpdatingExportReferenceNumber)
			{
				PackingDetailsUserControl.Load += (s, e) => UpdateExportReferenceNumberHeaderText();
			}

			SetUpContextMenus();
			OnUserControlCreated();
			return PackingDetailsUserControl;
		}

		protected virtual void OnUserControlCreated()
		{
		}

		protected virtual void OnUserControlAfterFirstBinding()
		{
			if (IsEligibleForUpdatingExportReferenceNumber)
			{
				UpdateExportReferenceNumberHeaderText();
			}
		}

		public override string Name
		{
			get { return Res.GetString("6c802db9-5d1e-4557-9fec-93e771d7ac12", "Containers Packing"); }
		}

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			if (this.consol.Containers != null)
			{
				this.consol.Containers.QueryReJoinPackLines += new CommonContainerCollection.QueryReJoinPackLinesEventHandler(Containers_QueryReJoinPackLines);
			}

			if (IsEligibleForUpdatingExportReferenceNumber)
			{
				this.consol.JK_RL_NKLoadPortInfo.ValueChanged += HandleExportRefNumberHeaderUpdate;
				this.consol.JK_TransportModeInfo.ValueChanged += HandleExportRefNumberHeaderUpdate;
			}
		}

		protected override ZBool AllowPlugInDisplayWithNoLicence
		{
			get { return Globals.IsTest ? ZBool.True : base.AllowPlugInDisplayWithNoLicence; }
		}

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			return consol;
		}

		#endregion

		#region ExportReferenceNumber HeaderText

		void HandleExportRefNumberHeaderUpdate(object sender, EventArgs e)
		{
			UpdateExportReferenceNumberHeaderText();
		}

		void UpdateExportReferenceNumberHeaderText()
		{
			var exportRefNumberCol = PackingDetailsUserControl?.ContainerDetailsGrid.Columns[PackLine.Schema.JL_ExportRefNumber];
			if (exportRefNumberCol != null)
			{
				var needToUpdate = this.consol.JK_TransportMode == Constants.TransportModes.Sea
					&& countryCodesForUpdatingExportReferenceNumber.Contains(this.consol.JK_RL_NKLoadPort.Left(2).ToUpper());
				exportRefNumberCol.ColumnStyle.HeaderText = needToUpdate
						? Res.GetString("3c41f0a7-568b-434a-9a27-2c188e97dd1f", "Shipping Order/Shi Lian Dan")
						: Res.GetString("0cbea1fe-a532-4a6e-9886-fe53bb9e3182", "Export Reference Number");
			}
		}

		protected virtual bool IsEligibleForUpdatingExportReferenceNumber
			=> countryCodesForUpdatingExportReferenceNumber.Contains(GlbCompany.CurrentCompany.Country.Code);

		readonly ZString[] countryCodesForUpdatingExportReferenceNumber = new ZString[]
		{
			Core.Constants.CountryCodes.China,
			Core.Constants.CountryCodes.Taiwan,
			Core.Constants.CountryCodes.HongKong
		};

		#endregion

		#region Implementation

		protected BusinessObject[] UnAllocatedPackLines
		{
			get
			{
				BusinessObject[] elements = null;
				if (PackingDetailsUserControl != null && PackingDetailsUserControl.UnAllocatedPackLinesGrid.List != null)
				{
					elements = ((BusinessObjectCollection)PackingDetailsUserControl.UnAllocatedPackLinesGrid.List).ToArray();
				}

				return elements;
			}
		}

		protected internal PackingDetailsUserControl PackingDetailsUserControl
		{
			get; private set;
		}

		protected ZModuleButtonGrid ContainersModuleButtonGrid
		{
			get
			{
				return PackingDetailsUserControl == null ? null : PackingDetailsUserControl.ContainersModuleButtonGrid;
			}
		}

		public CommonContainer[] GetSelectedContainers()
		{
			var result = new ArrayList();
			if (BusinessEntity != null)
			{
				if (PackingDetailsUserControl != null)
				{
					result.AddRange(PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.SelectedElements);
				}

				if (result.Count == 0)
				{
					if (this.consol != null && this.consol.Containers.Count == 1)
					{
						result.Add(this.consol.Containers[0]);
					}
				}
			}
			return (CommonContainer[])result.ToArray(typeof(CommonContainer));
		}

		public PackLine[] GetSelectedUnAllocatedPackLines()
		{
			return BusinessEntity != null
				? PackingDetailsUserControl.UnAllocatedPackLinesGrid.SelectedElements.Cast<PackLine>().ToArray()
				: Array.Empty<PackLine>();
		}

		#region Menu Items

		protected internal MenuItem packMenuItem;
		protected internal MenuItem unpackMenuItem;
		protected MenuItem packAllMenuItem;
		protected MenuItem unpackAllMenuItem;
		protected MenuItem splitMenuItem;
		protected MenuItem packLinedivider;
		protected MenuItem containerdivider;

		#endregion

		#region SetUpContextMenus

		protected virtual void SetUpContextMenus()
		{
			#region Load Lists Grid

			packLineMenuIndex = 0;

			packMenuItem = new ZMenuItem(ResString.GetMultilingualString("LoadList|Pack", "Pack"));
			packMenuItem.Click += Pack_Click;
			AddPackLineMenuItem(packMenuItem);

			packAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("LoadList|PackAll", "Pack All"));
			packAllMenuItem.Click += PackAll_Click;
			AddPackLineMenuItem(packAllMenuItem);

			splitMenuItem = new ZMenuItem(ResString.GetMultilingualString("LoadList|Split", "Split"));
			splitMenuItem.Click += Split_Click;
			AddPackLineMenuItem(splitMenuItem);

			packLinedivider = new ZMenuItem(ResString.GetMultilingualString("LoadListForm|Unpack", "-"));
			AddPackLineMenuItem(packLinedivider);

			#endregion

			#region Container Details Grid

			containerMenuIndex = 0;

			unpackMenuItem = new ZMenuItem(ResString.GetMultilingualString("478cee49-b679-4f34-a664-1d291791c975", "Unpack"));
			unpackMenuItem.Click += Unpack_Click;
			AddContainerMenuItem(unpackMenuItem);

			unpackAllMenuItem = new ZMenuItem(ResString.GetMultilingualString("LoadListForm|UnpackAll", "Unpack All"));
			unpackAllMenuItem.Click += UnpackAll_Click;
			AddContainerMenuItem(unpackAllMenuItem);

			#endregion
		}

		protected void AddPackLineMenuItem(MenuItem menuItem)
		{
			PackingDetailsUserControl.UnAllocatedPackLinesGrid.ContextMenu.MenuItems.Add(packLineMenuIndex, menuItem);
			packLineMenuIndex++;
		}

		protected void AddContainerMenuItem(MenuItem menuItem)
		{
			PackingDetailsUserControl.ContainerDetailsGrid.ContextMenu.MenuItems.Add(containerMenuIndex, menuItem);
			containerMenuIndex++;
		}

		int packLineMenuIndex;
		int containerMenuIndex;

		#endregion

		#region Packing

		#region Pack and PackAll

		void Pack_Click(object sender, EventArgs e)
		{
			var currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				var packlinesByShipment = Array.ConvertAll(PackingDetailsUserControl.UnAllocatedPackLinesGrid.SelectedElements, line => (PackLine)line).GroupBy(line => line.Shipment);

				foreach (var packline in packlinesByShipment)
				{
					currentContainer.AdjustOverriddenGrossWeightWithUserConfirmation(packline.First());
					currentContainer.AddPackLines(packline.ToArray());

					PackingDetailsUserControl.UnAllocatedPackLinesGrid.Refresh();
				}
			}
			else
			{
				NoContainerSelected();
			}
		}

		void PackAll_Click(object sender, EventArgs e)
		{
			var currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				var packlinesByShipment = this.consol.UnAllocatedPackLines.OfType<PackLine>().GroupBy(line => line.Shipment);

				foreach (var packline in packlinesByShipment)
				{
					currentContainer.AdjustOverriddenGrossWeightWithUserConfirmation(packline.First());
					currentContainer.AddPackLines(packline.ToArray());

					PackingDetailsUserControl.UnAllocatedPackLinesGrid.Refresh();
				}
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		#region Unpack_Click

		void Unpack_Click(object sender, EventArgs e)
		{
			var currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				WarnOnUnpack(currentContainer);
				currentContainer.RemovePackLines(PackingDetailsUserControl.ContainerDetailsGrid.SelectedElements);
				PackingDetailsUserControl.ContainerDetailsGrid.Refresh();
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		#region UnpackAll_Click

		void UnpackAll_Click(object sender, EventArgs e)
		{
			var currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				WarnOnUnpack(currentContainer);
				currentContainer.RemovePackLines(currentContainer.PackLines.ToArray());
				PackingDetailsUserControl.ContainerDetailsGrid.Refresh();
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		void WarnOnUnpack(CommonContainer container)
		{
			if (container != null && container.Services.Count > 0)
			{
				Globals.Message.ShowWarning(Res.GetString("243e0a26-8519-4825-a078-0c76ddb15b22", "The services on the selected shipments will be reset."), Res.GetString("6b274818-98f3-4233-9924-0b5bafc2ce40", "Unpack shipment from container"));
			}
		}

		#region Split_Click

		void Split_Click(object sender, EventArgs e)
		{
			var currentContainer = GetSelectedContainer();
			if (currentContainer != null)
			{
				if (PackingDetailsUserControl.UnAllocatedPackLinesGrid.SelectedElements.Length == 1)
				{
					var packLine = (PackLine)PackingDetailsUserControl.UnAllocatedPackLinesGrid.SelectedElements[0];
					var continueWithSplit = DialogResult.Yes;

					if (packLine.PackLocations.Count > 0)
					{
						ZString splitMessage = Res.GetString("d302015f-2f14-48ca-816a-8201b145c410", "This Pack Line has warehouse locations which splitting will unbalance.\r\nThese can be adjusted in the Shipments screen.\r\nAre you sure you want to split it?");
						continueWithSplit = Globals.Message.Show(splitMessage,
							Res.GetString("cd404d45-1cab-4070-be32-389cfb24bd0d", "Warehouse Locations"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					}

					if (continueWithSplit == DialogResult.Yes)
					{
						var splitter = new PackLineSplitter(packLine, currentContainer);
						var splitForm = new SplitForm(splitter);
						ZFormModaliser.Show(splitForm, this.Form);
					}
				}
				else
				{
					NoPackLineSelected();
				}
			}
			else
			{
				NoContainerSelected();
			}
		}

		#endregion

		protected CommonContainer GetSelectedContainer()
		{
			CommonContainer result;
			var selectedContainers = PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.SelectedElements;
			var currentRowIndex = PackingDetailsUserControl.ContainersModuleButtonGrid.InnerGrid.CurrentRowIndex;

			if (selectedContainers.Length > 0 && currentRowIndex >= 0 && currentRowIndex < consol.Containers.Count)
			{
				result = consol.Containers[currentRowIndex];
			}
			else if (this.consol.Containers.Count == 1)
			{
				result = this.consol.Containers[0];
			}
			else
			{
				result = null;
			}
			return result;
		}

		#endregion

		#region Containers_QueryReJoinPackLines

		void Containers_QueryReJoinPackLines(object sender, CommonContainerCollection.QueryReJoinPackLinesEventArgs e)
		{
			string message = e.Line != null && !e.Line.JL_Calc_JS_UniqueConsignRef.IsEmpty
				? Res.GetString("60e59c9a-a784-4dd2-8055-8d9bd1534a56", "Rejoin packlines from shipment {0} that were previously split?", e.Line.JL_Calc_JS_UniqueConsignRef)
				: Res.GetString("bbd6c37e-05fa-49d7-88f5-05bf3599cf19", "Rejoin packlines that were previously split?");
			var result = Globals.Message.Show(message, Res.GetString("c034db0c-440a-48f2-80b6-e54513bcaee8", "Rejoin split packlines"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		#endregion

		#region Error Messages

		protected void NoContainerSelected()
		{
			Globals.Message.Show(Res.GetString("35b351b3-b503-43b8-801d-915782a8b31d", "Please select an existing container or create a new container."), Res.GetString("a16a9f59-979d-4aba-aac4-7a0c96802580", "Select A Container"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		protected void NoPackLineSelected()
		{
			Globals.Message.Show(Res.GetString("e1c0ff92-9b05-49c5-b0a0-0edf4707c287", "Please select a packline."), Res.GetString("48596695-23cf-4857-8116-a635b6ddbec7", "Select A Packline"), MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		#endregion

		#endregion
	}
}
