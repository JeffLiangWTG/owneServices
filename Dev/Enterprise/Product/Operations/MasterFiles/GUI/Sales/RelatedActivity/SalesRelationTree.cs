using System.Collections.Generic;
using System.Linq;
using Aga.Controls.Tree;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class SalesRelationTree : ZTreeViewAdv, ICaptionedComponents
	{
		public SalesRelationTree()
		{
			ColumnReordered += SalesRelationTree_ColumnReordered;

			ElementType = typeof(IRelatableActivity);
		}

		#region Column Reorder

		internal static string RelationColumnHeader
		{
			get { return Res.GetString("ec8b3a6f-a42e-44d9-9c1e-247cea68b2cb", "Relation"); }
		}

		internal static string SummaryColumnHeader
		{
			get { return Res.GetString("48a1ade4-cbd5-4f71-8f75-1f088df1f544", "Summary"); }
		}

		internal static string CreatedTimeColumn
		{
			get { return Res.GetString("1d80a08e-50b6-4fbd-8604-1a96e5db88fb", "Created Time"); }
		}

		internal static string LastEditTimeColumn
		{
			get { return Res.GetString("6f09a0c0-88db-4d53-abc8-f034c3f88c05", "Last Edit Time"); }
		}

		public static string OrganizationNameColumn
		{
			get { return Res.GetString("161F68B8-FB0F-498C-B002-60A12324C6DC", "Organization Name"); }
		}

		public static string OrganizationCodeColumn
		{
			get { return Res.GetString("52DA05C7-9092-441D-BCE3-AC7B086D3986", "Org. Code"); }
		}

		public static string ContactNameColumn
		{
			get { return Res.GetString("DC308FCA-4866-450A-A45D-12D31C8723E7", "Contact Name"); }
		}

		void SalesRelationTree_ColumnReordered(object sender, TreeColumnEventArgs e)
		{
			if (Columns[0].Header != RelationColumnHeader)
			{
				var relationColumn = Columns.FirstOrDefault(column => column.Header == RelationColumnHeader);
				if (relationColumn != null)
				{
					Columns.Remove(relationColumn);
					Columns.Insert(0, relationColumn);
				}
			}
		}

		#endregion

		#region Restructure Tree

		public static ResourceStringData NameOfATreeElement
		{
			get { return Res.GetData("e0bdcf6a-9125-4f4d-b8a1-f1a0ad65448c", "Relatable Activity"); }
		}

		public static ResourceStringData NameOfTreeElementsPlural
		{
			get { return Res.GetData("d5f8070a-7663-4373-9277-ccb79a2c0f65", "Relatable Activities"); }
		}

		protected override void OnNodeDragDrop_Inside<T>(List<ZNode<T>> nodes, ZNode<T> dropNode)
		{
			var validNodes = new List<ZNode<T>>();
			var invalidActivities = new List<IRelatableActivity>();
			foreach (var node in nodes)
			{
				if (IsNodeEditable(node, invalidActivities))
				{
					validNodes.Add(node);
				}
			}
			base.OnNodeDragDrop_Inside(validNodes, dropNode);
			ShowInvalidMessagesForDragIfNotEmpty(invalidActivities);
		}

		protected override void OnNodeDragDrop_Before<T>(ZNode<T> node, ZNode<T> dropNode)
		{
			var invalidActivities = new List<IRelatableActivity>();
			if (!IsNodeEditable(node, invalidActivities))
			{
				ShowInvalidMessagesForDragIfNotEmpty(invalidActivities);
				return;
			}

			var dropNodeParent = dropNode.ParentNode;
			if (((IRelatableActivity)dropNode.BizObj).ActivityType == RelatableActivityTypeList.Codes.Communication)
			{
				var dropNodeChild = dropNode.ChildNodes.FirstOrDefault();
				if (dropNodeChild != null && dropNodeChild != node)
				{
					if (
						!UnsetParentNode(dropNode) ||
						!UnsetParentNode(dropNodeChild) ||
						!dropNode.SetParentNode(dropNodeChild, true) ||
						!dropNodeChild.SetParentNode(dropNodeParent, true))
					{
						dropNode.SetParentNode(dropNodeParent, false);
						dropNodeChild.SetParentNode(dropNode, false);
						return;
					}
					else
					{
						dropNode = dropNodeChild;
					}
				}
			}

			base.OnNodeDragDrop_Before(node, dropNode);
		}

		bool IsNodeEditable<T>(ZNode<T> node, List<IRelatableActivity> invalidActivities) where T : class, IBusiness
		{
			if (node.ParentNode != null)
			{
				var activity = node.BizObj as IRelatableActivity;
				var parentActivity = node.ParentNode.BizObj as IRelatableActivity;
				var pivot = parentActivity.RelatedChildActivityPivotCollection.FindPivot(activity);
				if (pivot != null && !pivot.IsEditable)
				{
					invalidActivities.Add(activity);
					return false;
				}
			}
			return true;
		}

		void ShowInvalidMessagesForDragIfNotEmpty(List<IRelatableActivity> invalidActivities)
		{
			if (invalidActivities.Any())
			{
				var caption = ResString.GetMultilingualString("0AE5DA43-5F44-4028-9B29-B5A11CC7C0D5", "Drag {0}", NameOfATreeElement.Caption);
				var message = new ZStringBuilder(ResString.GetMultilingualString("ED0EE89F-7B37-4ED8-90D2-2FC632FA3988", "Unable to move the following {0}:", NameOfTreeElementsPlural.Caption));
				foreach (var activity in invalidActivities)
				{
					message.AppendLine();
					message.Append($"• {activity.Summary}");
				}
				Globals.Message.ShowInformation(message.ToString(), caption);
			}
		}

		protected override bool SetParentNodeIfValid<T>(ZNode<T> child, ZNode<T> parent)
		{
			if (!base.SetParentNodeIfValid(child, parent))
			{
				return false;
			}

			if (parent == null)
			{
				return true;
			}
			else
			{
				var parentActivity = (IRelatableActivity)parent.BizObj;
				var childActivity = (IRelatableActivity)child.BizObj;
				var caption = SalesRelationControl.GetAttachingCaption(childActivity, parentActivity);
				var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((KForm)this.FindForm(), caption);
				return DoImportChildRelatedActivityInfoOnAttachActions(parentActivity, childActivity, deciderFactory);
			}
		}

		protected override bool UnsetParentNode<T>(ZNode<T> node)
		{
			var originalParent = node.ParentNode;
			if (!base.UnsetParentNode(node))
			{
				return false;
			}
			if (originalParent != null)
			{
				var parentActivity = (IRelatableActivity)originalParent.BizObj;
				var childActivity = (IRelatableActivity)node.BizObj;
				var caption = SalesRelationControl.GetDetachingCaption(childActivity, parentActivity);
				var deciderFactory = new ImportRelatedActivityPromptUserDeciderFactory((KForm)this.FindForm(), caption);
				return DoImportChildRelatedActivityInfoOnDetachActions(parentActivity, childActivity, deciderFactory);
			}
			return true;
		}

		#endregion

		#region DoImportRelatedActivityInfoActions

		public static bool DoImportChildRelatedActivityInfoOnAttachActions(IRelatableActivity parentActivity, IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			var result = true;
			var importChildInfoOnAttach = parentActivity as IImportChildRelatedActivityInfoOnAttach;
			if (importChildInfoOnAttach != null)
			{
				result &= importChildInfoOnAttach.ImportChildInfo(childActivity, deciderFactory);
			}

			return result;
		}

		public static bool DoImportChildRelatedActivityInfoOnNewSavedActions(IRelatableActivity parentActivity, IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			var result = true;
			var importChildInfoOnNewSaved = parentActivity as IImportChildRelatedActivityInfoOnNewSaved;
			if (importChildInfoOnNewSaved != null)
			{
				result &= importChildInfoOnNewSaved.ImportChildInfo(childActivity, deciderFactory);
			}

			return result;
		}

		public static bool DoImportChildRelatedActivityInfoOnDetachActions(IRelatableActivity parentActivity, IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			var result = true;
			var importChildInfoOnDetach = parentActivity as IImportChildRelatedActivityInfoOnDetach;
			if (importChildInfoOnDetach != null)
			{
				result &= importChildInfoOnDetach.ImportChildInfo(childActivity, deciderFactory);
			}

			return result;
		}

		public static bool DoImportParentRelatedActivityInfoOnNewActions(IRelatableActivity parentActivity, IRelatableActivity childActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			var result = true;
			var importParentInfoOnNew = childActivity as IImportParentRelatedActivityInfoOnNew;
			if (importParentInfoOnNew != null)
			{
				result &= importParentInfoOnNew.ImportParentInfo(parentActivity, deciderFactory);
			}

			return result;
		}

		#endregion
	}
}
