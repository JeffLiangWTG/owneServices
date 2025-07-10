#if DEBUG
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	static class PackingGuiTestExtensions
	{
		#region AssertPackingTreeView

		/// <summary>
		/// This build a text version of the package tree and asserts the expected.
		/// Sample Layout for Dummy Parent w/ 1 box w/ tv and 1 pallet w/ 3 tv's:
		///- Dummy 
		///  - 1x Box 2 KG
		///    - 1x TV - Size: 63in
		///  - 1x Pallet 6 KG
		///    - 3x TV - Size: 63in
		/// </summary>
		public static void AssertPackingTreeView(this PackingTreeView packageTreeView, ZString message, ZString expected)
		{
			var builder = new ZStringBuilder();
			BuildPackingTreeView(builder, packageTreeView.Nodes.Cast<PackingTreeNode>());
			Assertion.AssertMultilineASCIIEquals(message, expected.Trim(), builder.ToString().Trim());
		}

		static void BuildPackingTreeView(ZStringBuilder treeBuilder, IEnumerable<PackingTreeNode> nodes, int level = 0)
		{
			var indent = ZString.Replicate(' ', level * 2);

			foreach (var n in nodes)
			{
				var packageBuilder = new ZStringBuilder($"{indent}- {n.Text}");

				if (n.IsPackage)
				{
					var summary = (IPackageSummary)n.BizO;
					var showWeight = !summary.Weight.StartsWith("0 ");
					packageBuilder.AppendIfNotEmpty(showWeight ? $"{summary.Weight}" : "");
				}

				treeBuilder.AppendLine(packageBuilder.ToStringWithDelimiterBetweenAppends(" "));
				BuildPackingTreeView(treeBuilder, n.Nodes.Cast<PackingTreeNode>(), level + 1);
			}
		}

		#endregion
	}
}
#endif
