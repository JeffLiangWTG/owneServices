using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public class MasterBookingPackingUserControl : PackingUserControl
	{
		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);

			if (Visible && TreeUserControl.Tree.Nodes.Count > 0)
			{
				RebuildNodes();
			}
		}

		protected override void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			base.Factory_Saved(factory, savedSuccessfully);

			if (Visible && TreeUserControl.Tree.Nodes.Count > 0)
			{
				RebuildNodes();
			}
		}

		void RebuildNodes()
		{
			var masterPackageJobNode = TreeUserControl.Tree.Nodes[0];
			var masterPackageJobNodeBizO = ((ZBusinessObjectTreeNode)masterPackageJobNode).BizO;
			var masterPackageJob = masterPackageJobNodeBizO as DtbBookingConsolidationPkgPackageJob;
			var unsortedPackageNodes = masterPackageJobNode.Nodes.Cast<PackingTreeNode>().ToArray().Where(n => n.BizO is PkgPackage);
			RemovePackageNodesNotBelongingToPkgPackageJob(masterPackageJob, unsortedPackageNodes);

			if (masterPackageJob != null)
			{
				var subPackageJobsList = new List<PkgPackageJob>();

				foreach (var package in masterPackageJob.Packages)
				{
					if (!subPackageJobsList.Select(p => p.PK).Contains(package.KP_KJ_ParentPackageJob))
					{
						var subPackageJob = masterPackageJob.Factory.Load<PkgPackageJob>(package.KP_KJ_ParentPackageJob);
						subPackageJobsList.Add(subPackageJob);
					}
				}

				var packageJobNodes = new List<PackingTreeNode>();
				var existingSubPackageJobNodes = masterPackageJobNode.Nodes.Cast<PackingTreeNode>().ToArray().Where(n => n.BizO is PkgPackageJob);
				packageJobNodes.AddRange(existingSubPackageJobNodes);

				var existingSubPackageJobNodePackageJobPKs = existingSubPackageJobNodes.Select(n => n.BizO.PK);

				foreach (var subPackageJob in subPackageJobsList)
				{
					if (!existingSubPackageJobNodePackageJobPKs.Contains(subPackageJob.PK))
					{
						var newPackageJobNode = new PackingTreeNode(subPackageJob);
						packageJobNodes.Add(newPackageJobNode);
						TreeUserControl.Tree.AddNode(newPackageJobNode, masterPackageJobNode.Nodes);
					}
				}

				foreach (var packageNode in unsortedPackageNodes)
				{
					masterPackageJobNode.Nodes.Remove(packageNode);

					var packageBizO = (PkgPackage)packageNode.BizO;
					var packageJobNodeForPackageNode = packageJobNodes.FirstOrDefault(n => n.BizO.PK == packageBizO.KP_KJ_ParentPackageJob);
					if (packageJobNodeForPackageNode != null)
					{
						packageJobNodeForPackageNode.Nodes.Add(packageNode);
					}
				}

				foreach (var packageJobNode in existingSubPackageJobNodes)
				{
					if (!subPackageJobsList.Select(p => p.PK).Contains(packageJobNode.BizO.PK))
					{
						masterPackageJobNode.Nodes.Remove(packageJobNode);
						packageJobNode.Dispose();
					}
				}

				foreach (ZBusinessObjectTreeNode node in masterPackageJobNode.Nodes)
				{
					node.Expand();
				}
			}
		}

		void RemovePackageNodesNotBelongingToPkgPackageJob(DtbBookingConsolidationPkgPackageJob masterPackageJob, IEnumerable<PackingTreeNode> packingTreeNodes)
		{
			var nodesToProcess = new List<PackingTreeNode>();
			var parentNodes = packingTreeNodes;
			while (parentNodes.Any())
			{
				var childNodes = parentNodes.SelectMany(n => n.Nodes.Cast<PackingTreeNode>());
				nodesToProcess.AddRange(parentNodes);
				parentNodes = childNodes;
			}

			foreach (var parentNode in nodesToProcess)
			{
				foreach (var childNode in parentNode.Nodes.Cast<PackingTreeNode>().ToArray())
				{
					if (childNode.BizO is PkgPackage pkgPackage)
					{
						if (!masterPackageJob.ContainsPackage(pkgPackage))
						{
							parentNode.Nodes.Remove(childNode);
							childNode.Dispose();
						}
					}
				}
			}
		}
	}
}
