using System.Linq;
using Aga.Controls.Tree;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class GlbStaffManagementTree : ZTreeViewAdv, ICaptionedComponents
	{
		public GlbStaffManagementTree()
		{
			ColumnReordered += OnColumnReordered;
		}

		public override bool AllowDrop
		{
			get { return false; }
			set { }
		}

		void OnColumnReordered(object sender, TreeColumnEventArgs treeColumnEventArgs)
		{
			if (Columns[0].Header != RoleColumnHeader)
			{
				var relationColumn = Columns.FirstOrDefault(column => column.Header == RoleColumnHeader);
				if (relationColumn != null)
				{
					Columns.Remove(relationColumn);
					Columns.Insert(0, relationColumn);
				}
			}
		}

		#region Column Headers

		internal static string RoleColumnHeader => Res.GetString("633bbcc9-bf02-458c-82d1-4d030f858d70", "Role");

		internal static string JobTitleColumnHeader => Res.GetString("cf7cf3f5-77cd-42f2-bfd0-048e8437d444", "Job Title");

		internal static string EffectiveDateColumnHeader => Res.GetString("8427a3a5-bbec-4607-875b-350c7628d2ab", "Effective");

		internal static string BranchColumnHeader => Res.GetString("6512cd20-019e-4a35-baa4-c8137f8e3644", "Branch");

		#endregion
	}
}
