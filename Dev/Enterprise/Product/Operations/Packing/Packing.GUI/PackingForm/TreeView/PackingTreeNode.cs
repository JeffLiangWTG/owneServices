using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public partial class PackingTreeNode : ZBusinessObjectTreeNode, IDisposable
	{
		#region Construction

		public PackingTreeNode(PkgPackage package)
			: base(package, "", 0, 1, 2, 3)
		{
			IsPackage = true;
			HookEvents();
		}

		public PackingTreeNode(PkgPackageJob packageJob)
			: base(packageJob, "", 0, 1, 2, 3)
		{
			IsPackageJob = true;
			HookEvents();
		}

		public PackingTreeNode(PkgPackageItemDivotsWrapper packedItem)
			: base(packedItem, "", 0, 1, 2, 3)
		{
			IsPackedItem = true;
			HookEvents();
		}

		#endregion

		#region Events

		#region Package Changing from Container that had user data

		void PackageTypeChangingFromContainerWithData(object sender, PkgPackage.PackTypeChangingFromContainerWithDataEventArgs e)
		{
			string caption = Res.GetString("1abf59cc-889a-4ba4-9ea2-8fce5abb2182", "Package Type Change");
			string msg = Res.GetString("9d44884d-9143-4fb8-90ca-7fa6b6fa44de",
				"Changing the Package Type will clear any Container-Specific details. Are you sure you wish to continue?");

			e.Continue = Globals.Message.Show(msg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No) == DialogResult.Yes;
		}

		#endregion

		#region Pack Type Changing to / from Container

		void PackTypeChangedToContainer(object sender, EventArgs e) // changED
		{
			HookContainerEvents();
		}

		void PackageTypeChangingFromContainer(object sender, EventArgs e) // changING
		{
			UnHookContainerEvents();
		}

		#endregion

		#region Repainting on Package Change

		void PackageQtyOrTypeChanged(object sender, EventArgs e)
		{
			PaintIncludingParent();

			if (IsSelected)
			{
				TreeView.OnSelectedPackageNodeQtyOrTypeChanged();
			}
		}

		void BizOValueChanged(object sender, EventArgs e)
		{
			PaintSummaryIncludingParent(); // do not call Paint() as it is expensive in tight loops, and we only need to paint the summary
		}

		void BizOValueChanged_ChangeDoesNotRequireParentUpdate(object sender, EventArgs e)
		{
			PaintSummary(); // do not paint parent summary as this change only affects the current node (fast in tight loops eg. Generate IDs)
		}

		#endregion

		#region Repainting on Packed Item Change

		void PackedItem_DescriptionChanged(object sender, EventArgs e)
		{
			Paint();
		}

		void PackedItem_QuantityChanged(object sender, EventArgs e)
		{
			Paint();
		}

		#endregion

		#region ParentJobNumberChanged

		void ParentJobNumberChanged(object sender, EventArgs e)
		{
			Paint();
		}

		#endregion

		#region Removing on Delete

		void BizO_HasChangesChanged(object sender, EventArgs e)
		{
			if (BizO.IsDeleted)
			{
				this.Remove();
				PaintParentSummary();
			}
		}

		public new bool IsSelected => TreeView.NodeSelector.IsSelected(this);

		#endregion

		#region Removing on Changing Package Job

		void KP_KJ_ParentPackageJobInfo_ValueChanged(object sender, EventArgs e)
		{
			Remove();
		}

		#endregion

		#region Packed Item Divots Changed

		void Package_PackedItemDivotsCountChanged(object sender, EventArgs e)
		{
			// this event would only be fired if the packed item divots were changed through some automatic process like data refresh bus and thus we need to rebuild the package's child nodes
			TreeView.PopulatePackageChildNodes(this);
		}

		#endregion

		#region Hook / Unhook Events

		void HookEvents()
		{
			HookPackageEvents();
			HookContainerEvents();
			HookPackedItemEvents();
			HookDeleteEvent();
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		void UnhookPackingEvents()
		{
			UnhookPackageEvents();
			UnHookContainerEvents();
			UnhookPackedItemEvents();
			UnhookDeleteEvent();
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		void HookPackageEvents()
		{
			if (IsPackage)
			{
				var package = Package;

				// package type
				package.PackTypeChangingFromContainerWithData += PackageTypeChangingFromContainerWithData;
				package.PackTypeChangingFromContainer += PackageTypeChangingFromContainer;
				package.PackTypeChangedToContainer += PackTypeChangedToContainer;

				// package
				package.KP_PackageQtyInfo.ValueChanged += PackageQtyOrTypeChanged;
				package.KP_F3_NKPackTypeInfo.ValueChanged += PackageQtyOrTypeChanged;

				// weight and volume
				package.KP_WeightInfo.ValueChanged += BizOValueChanged;
				package.KP_WeightUQInfo.ValueChanged += BizOValueChanged;
				package.KP_VolumeInfo.ValueChanged += BizOValueChanged;
				package.KP_VolumeUQInfo.ValueChanged += BizOValueChanged;

				// dims
				package.KP_LengthInfo.ValueChanged += BizOValueChanged;
				package.KP_WidthInfo.ValueChanged += BizOValueChanged;
				package.KP_HeightInfo.ValueChanged += BizOValueChanged;
				package.KP_DimensionUQInfo.ValueChanged += BizOValueChanged;

				// IDs
				package.KP_PackageIDInfo.ValueChanged += BizOValueChanged_ChangeDoesNotRequireParentUpdate;
				package.KP_TransportRefInfo.ValueChanged += BizOValueChanged_ChangeDoesNotRequireParentUpdate;

				// Closed
				package.KP_ClosedTimeUtcInfo.ValueChanged += BizOValueChanged;
				package.KP_IsReleasedViaJobInfo.ValueChanged += BizOValueChanged;
				package.KP_ReleasedTimeUtcInfo.ValueChanged += BizOValueChanged;

				// temperature
				package.KP_RequiresTemperatureControlInfo.ValueChanged += BizOValueChanged;
				package.KP_RequiredTemperatureMinimumInfo.ValueChanged += BizOValueChanged;
				package.KP_RequiredTemperatureMaximumInfo.ValueChanged += BizOValueChanged;
				package.KP_RequiredTemperatureUnitInfo.ValueChanged += BizOValueChanged;

				// commodity
				package.KP_RH_NKCommodityCodeInfo.ValueChanged += BizOValueChanged;

				// package job
				package.KP_KJ_ParentPackageJobInfo.ValueChanged += KP_KJ_ParentPackageJobInfo_ValueChanged;

				// packed items
				package.PackedItemDivotsCountChanged += Package_PackedItemDivotsCountChanged;
			}
			else if (IsPackageJob)
			{
				var packageJob = PackageJob;
				packageJob.ParentJobNumberChanged += ParentJobNumberChanged;
			}
		}

		void UnhookPackageEvents()
		{
			if (IsPackage)
			{
				var package = Package;

				// package type
				package.PackTypeChangingFromContainerWithData -= PackageTypeChangingFromContainerWithData;
				package.PackTypeChangingFromContainer -= PackageTypeChangingFromContainer;
				package.PackTypeChangedToContainer -= PackTypeChangedToContainer;

				// package
				package.KP_F3_NKPackTypeInfo.ValueChanged -= PackageQtyOrTypeChanged;
				package.KP_PackageQtyInfo.ValueChanged -= PackageQtyOrTypeChanged;

				// weight and volume
				package.KP_WeightInfo.ValueChanged -= BizOValueChanged;
				package.KP_WeightUQInfo.ValueChanged -= BizOValueChanged;
				package.KP_VolumeInfo.ValueChanged -= BizOValueChanged;
				package.KP_VolumeUQInfo.ValueChanged -= BizOValueChanged;

				// dims
				package.KP_LengthInfo.ValueChanged -= BizOValueChanged;
				package.KP_WidthInfo.ValueChanged -= BizOValueChanged;
				package.KP_HeightInfo.ValueChanged -= BizOValueChanged;
				package.KP_DimensionUQInfo.ValueChanged -= BizOValueChanged;

				// IDs
				package.KP_PackageIDInfo.ValueChanged -= BizOValueChanged_ChangeDoesNotRequireParentUpdate;
				package.KP_TransportRefInfo.ValueChanged -= BizOValueChanged_ChangeDoesNotRequireParentUpdate;

				// Closed
				package.KP_ClosedTimeUtcInfo.ValueChanged -= BizOValueChanged;
				package.KP_IsReleasedViaJobInfo.ValueChanged -= BizOValueChanged;
				package.KP_ReleasedTimeUtcInfo.ValueChanged -= BizOValueChanged;

				// temperature
				package.KP_RequiresTemperatureControlInfo.ValueChanged -= BizOValueChanged;
				package.KP_RequiredTemperatureMinimumInfo.ValueChanged -= BizOValueChanged;
				package.KP_RequiredTemperatureMaximumInfo.ValueChanged -= BizOValueChanged;
				package.KP_RequiredTemperatureUnitInfo.ValueChanged -= BizOValueChanged;

				// commodity
				package.KP_RH_NKCommodityCodeInfo.ValueChanged -= BizOValueChanged;

				// package job
				package.KP_KJ_ParentPackageJobInfo.ValueChanged -= KP_KJ_ParentPackageJobInfo_ValueChanged;

				// packed items
				package.PackedItemDivotsCountChanged -= Package_PackedItemDivotsCountChanged;
			}
			else if (IsPackageJob)
			{
				var packageJob = PackageJob;
				packageJob.ParentJobNumberChanged -= ParentJobNumberChanged;
			}
		}

		void HookContainerEvents()
		{
			if (IsPackage)
			{
				Container = Package.Container; // set state for later unhook
				if (Container != null)
				{
					Container.K0_IsControlledAtmosphereInfo.ValueChanged += BizOValueChanged;
					Container.K0_SetPointTempInfo.ValueChanged += BizOValueChanged;
					Container.K0_SetPointTempUnitInfo.ValueChanged += BizOValueChanged;
				}
			}
		}

		void UnHookContainerEvents()
		{
			if (IsPackage && Container != null)
			{
				Container.K0_IsControlledAtmosphereInfo.ValueChanged -= BizOValueChanged;
				Container.K0_SetPointTempInfo.ValueChanged -= BizOValueChanged;
				Container.K0_SetPointTempUnitInfo.ValueChanged -= BizOValueChanged;
				Container = null; // remove state
			}
		}

		void HookPackedItemEvents()
		{
			if (IsPackedItem)
			{
				PackedItem.DescriptionChanged += PackedItem_DescriptionChanged;
				PackedItem.QuantityChanged += PackedItem_QuantityChanged;
			}
		}

		void UnhookPackedItemEvents()
		{
			if (IsPackedItem)
			{
				PackedItem.DescriptionChanged -= PackedItem_DescriptionChanged;
				PackedItem.QuantityChanged -= PackedItem_QuantityChanged;
			}
		}

		void HookDeleteEvent()
		{
			BizO.HasChangesChanged += BizO_HasChangesChanged;
		}

		void UnhookDeleteEvent()
		{
			BizO.HasChangesChanged -= BizO_HasChangesChanged;
		}

		// When deleting a Container node the Package is deleted before the Container (because Container references Package).
		// Accessing Package.Container to unhook Container events will result in accessing the deleted Package PK in order to
		// load the Container so we instead cache the Container here. An alternative solution would be to add and hook Package.Deleting.
		PkgPackageContainer Container;

		#endregion

		#endregion

		#region Flags

		public readonly bool IsPackageJob;
		public readonly bool IsPackage;
		public readonly bool IsPackedItem;

		#region IsChildOf(node)

		public bool IsChildOf(PackingTreeNode node)
		{
			var parent = Parent;

			while (parent != null)
			{
				if (parent == node)
				{
					return true;
				}
				parent = parent.Parent;
			}

			return false;
		}

		#endregion

		#endregion

		#region BizO

		public PkgPackageJob PackageJob => BizO as PkgPackageJob;

		public PkgPackage Package => BizO as PkgPackage;

		public PkgPackageItemDivotsWrapper PackedItem => BizO as PkgPackageItemDivotsWrapper;

		public PkgPackageCollection Packages => IsPackage ? Package.Packages : IsPackageJob ? PackageJob.Packages : null;

		#endregion

		#region Parent Node and Parent Tree

		public new PackingTreeNode Parent => (PackingTreeNode)base.Parent;

		public new PackingTreeView TreeView => (PackingTreeView)base.TreeView;

		#endregion

		#region Paint

		/// <summary>
		/// Repaints the entire node which includes the [Dotted Line], [Text] and [Summary].
		/// WARNING -- This method is expensive and should be avoided in tight loops.
		/// </summary>
		public void Paint()
		{
			TreeView.NodePainter.PaintText(this);
		}

		public void PaintIncludingParent()
		{
			Paint();

			if (Parent != null)
			{
				Parent.Paint();
			}
		}

		void PaintSummaryIncludingParent()
		{
			PaintSummary();
			PaintParentSummary();
		}

		void PaintSummary()
		{
			TreeView.NodePainter.PaintSummary(this);
		}

		void PaintParentSummary()
		{
			if (Parent != null)
			{
				TreeView.NodePainter.PaintSummary(Parent);
			}
		}

		#endregion

		#region Move

		public void MoveNodesTo(PackingTreeNode[] nodesToMove)
		{
			foreach (var node in nodesToMove)
			{
				node.RemoveOnly();
			}

			Nodes.AddRange(nodesToMove);

			foreach (var node in nodesToMove)
			{
				TreeView.NodePainter.PaintText(node); // done after adding otherwise text is sometimes chopped off
			}
		}

		void RemoveOnly() => base.Remove();

		#endregion

		#region Remove

		/// <summary>
		/// It is necessary to unhook the node from the underlying BizO once the node is removed from it's parent tree.
		/// Unfortunately TreeNode and TreeNodeCollection offer no clean way to intercept the remove, so the next best
		/// thing is this public new.
		/// </summary>
		public new void Remove()
		{
			RemoveWithoutDispose();
			Dispose();
		}

		void RemoveWithoutDispose()
		{
			TreeView.NodeSelector.DeselectNode(this);
			base.Remove();
		}

		#endregion

		#region Dispose

		public void Dispose()
		{
			if (TreeView != null && !TreeView.IsDisposed)
			{
				RemoveWithoutDispose();
			}

			Array.ForEach(Nodes.Cast<PackingTreeNode>().ToArray(), node => node.Dispose());
			UnhookPackingEvents();
			UnhookEvents();

			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
