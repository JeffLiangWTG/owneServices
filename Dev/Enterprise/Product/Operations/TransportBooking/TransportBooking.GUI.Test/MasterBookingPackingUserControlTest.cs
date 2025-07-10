using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.Packing.GUI.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Testing
{
	public class MasterBookingPackingUserControlTest : PackingUserControlTest
	{
		public void TestSubPackagesAreSortedByPackageJobInTree()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation1 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation1.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation1.KB_MasterBookingVersion = 1;
			var subBooking1 = subConsolidation1.Bookings.AddNew();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subConsolidation1Package = subConsolidation1.PackageJob.Packages.AddNew();
			var subConsolidation1InnerPackage = subConsolidation1Package.Packages.AddNew();

			var subConsolidation2 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation2.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation2.KB_MasterBookingVersion = 1;
			var subBooking2 = subConsolidation2.Bookings.AddNew();
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_MasterBookingVersion = subConsolidation2.KB_MasterBookingVersion;
			var subConsolidation2Package = subConsolidation2.PackageJob.Packages.AddNew();

			var masterBookingPackageJob = masterConsolidation.PackageJob;

			LinkSubBookingInstructionToMasterBookingInstruction(masterBooking, subBooking1, subBooking2);

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidation1Package.PK, packageQty: subConsolidation1Package.KP_PackageQty),
				(packagePK: subConsolidation1InnerPackage.PK, packageQty: subConsolidation1InnerPackage.KP_PackageQty),
				(packagePK: subConsolidation2Package.PK, packageQty: subConsolidation2Package.KP_PackageQty),
			};
			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			using (var form = new ZForm(masterConsolidation))
			{
				var masterPackingUserControl = new MasterBookingPackingUserControl();
				form.Controls.Add(masterPackingUserControl);
				form.Show();
				Application.DoEvents();

				masterPackingUserControl.Visible = true;

				var tree = ((PackingTreeViewUserControl)masterPackingUserControl.Controls.Find("TreeUserControl", true).Single()).Tree;

				var masterPackageJobNode = tree.Nodes[0];
				AssertEquals("Precondition: top level node should be for the Master Booking PackageJob.", masterBookingPackageJob, ((ZBusinessObjectTreeNode)masterPackageJobNode).BizO);

				var listOfPackageJobNodes = masterPackageJobNode.Nodes.Cast<TreeNode>().ToArray();
				var jobsInNodes = listOfPackageJobNodes.Select(n => ((ZBusinessObjectTreeNode)n).BizO);
				AssertEquals(2, jobsInNodes.Count());
				AssertContainsExactElementsInAnyOrder("There should be a node for each PackageJob on a sub TB.", new[] { subConsolidation1.PackageJob, subConsolidation2.PackageJob }, jobsInNodes);

				foreach (var packageJobNode in listOfPackageJobNodes)
				{
					var listOfPackageNodes = packageJobNode.Nodes.Cast<TreeNode>().ToArray();
					var packagesInNodes = listOfPackageNodes.Select(n => ((ZBusinessObjectTreeNode)n).BizO);

					foreach (var package in packagesInNodes)
					{
						AssertEquals("Each node in each PackageJob node should be for a Package that belongs to that PackageJob.", ((ZBusinessObjectTreeNode)packageJobNode).BizO.PK, ((PkgPackage)package).KP_KJ_ParentPackageJob);
					}
				}

				AssertNoExceptionThrown("Making the control visible a second time should not cause an exception.", () =>
				{
					masterPackingUserControl.Visible = false;
					masterPackingUserControl.Visible = true;
					Application.DoEvents();
				});

				subBooking2.KM_KM_MasterBooking = ZGuid.Empty;
				subBooking2.KM_MasterBookingVersion = 0;
				masterBooking.DetachSubBookingPackagesAndDivotsFromMasterBookingRestoreOnSubBookings(new List<DtbBooking>() { subBooking2 });

				Factory.Save();

				// Don't use masterPackingUserControl being non-visible and visible again to trigger a change, instead rely upon the factory being saved

				listOfPackageJobNodes = masterPackageJobNode.Nodes.Cast<TreeNode>().ToArray();
				jobsInNodes = listOfPackageJobNodes.Select(n => ((ZBusinessObjectTreeNode)n).BizO);
				AssertEquals("Should only be the node for the first sub TB", 1, jobsInNodes.Count());
				AssertContainsExactElementsInAnyOrder("Should only be the node for the first sub TB", new[] { subConsolidation1.PackageJob }, jobsInNodes);

				masterPackingUserControl.Visible = false;
				masterPackingUserControl.Visible = true;
				Application.DoEvents();

				listOfPackageJobNodes = masterPackageJobNode.Nodes.Cast<TreeNode>().ToArray();
				jobsInNodes = listOfPackageJobNodes.Select(n => ((ZBusinessObjectTreeNode)n).BizO);
				CombineAssertions("Making the control visible another time should not cause nodes to disappear", () =>
				{
					AssertEquals("Should only be the node for the first sub TB", 1, jobsInNodes.Count());
					AssertContainsExactElementsInAnyOrder("Should only be the node for the first sub TB", new[] { subConsolidation1.PackageJob }, jobsInNodes);
				});
			}
		}

		public void TestPackagesNotBelongingToPkgPackageJobAreExcluded()
		{
			var masterConsolidation = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			masterConsolidation.KB_IsMaster = true;
			masterConsolidation.KB_MasterBookingVersion = 1;
			var masterBooking = masterConsolidation.Bookings.AddNew();
			masterBooking.KM_IsMaster = true;
			masterBooking.KM_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;
			var masterInstruction = masterBooking.Instructions.AddNew();
			masterInstruction.KN_IsMaster = true;
			masterInstruction.KN_MasterBookingVersion = masterConsolidation.KB_MasterBookingVersion;

			var subConsolidation1 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation1.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation1.KB_MasterBookingVersion = 1;
			var subBooking1 = subConsolidation1.Bookings.AddNew();
			subBooking1.KM_KM_MasterBooking = masterBooking.PK;
			subBooking1.KM_MasterBookingVersion = subConsolidation1.KB_MasterBookingVersion;
			var subConsolidation1Package = subConsolidation1.PackageJob.Packages.AddNew();
			var subConsolidation1InnerPackage = subConsolidation1Package.Packages.AddNew();

			var subConsolidation2 = Factory.NewWithValidTestData<DtbBookingConsolidation>();
			subConsolidation2.KB_KB_MasterBookingConsolidation = masterConsolidation.PK;
			subConsolidation2.KB_MasterBookingVersion = 1;
			var subBooking2 = subConsolidation2.Bookings.AddNew();
			subBooking2.KM_KM_MasterBooking = masterBooking.PK;
			subBooking2.KM_MasterBookingVersion = subConsolidation2.KB_MasterBookingVersion;
			var subConsolidation2Package = subConsolidation2.PackageJob.Packages.AddNew();
			var subConsolidation2InnerPackage = subConsolidation2Package.Packages.AddNew();

			var masterBookingPackageJob = masterConsolidation.PackageJob;

			var masterInstructionAssignedPackages = new (ZGuid packagePK, ZInt packageQty)[]
			{
				(packagePK: subConsolidation1Package.PK, packageQty: subConsolidation1Package.KP_PackageQty),
				(packagePK: subConsolidation1InnerPackage.PK, packageQty: subConsolidation1InnerPackage.KP_PackageQty),
				(packagePK: subConsolidation2Package.PK, packageQty: subConsolidation2Package.KP_PackageQty),
				// Don't assign subConsolidation2InnerPackage
			};
			AssignDivotsToInstructions(masterInstruction, masterInstructionAssignedPackages);

			Factory.Save();

			AssertEquals("Precondition: masterBookingPackageJob does contain subConsolidation1Package", true, masterBookingPackageJob.ContainsPackage(subConsolidation1Package));
			AssertEquals("Precondition: masterBookingPackageJob does contain subConsolidation1InnerPackage", true, masterBookingPackageJob.ContainsPackage(subConsolidation1InnerPackage));
			AssertEquals("Precondition: masterBookingPackageJob does contain subConsolidation2Package", true, masterBookingPackageJob.ContainsPackage(subConsolidation2Package));
			AssertEquals("Precondition: masterBookingPackageJob does not contain subConsolidation2InnerPackage", false, masterBookingPackageJob.ContainsPackage(subConsolidation2InnerPackage));

			using (var form = new ZForm(masterConsolidation))
			{
				var masterPackingUserControl = new MasterBookingPackingUserControl();
				form.Controls.Add(masterPackingUserControl);
				form.Show();
				Application.DoEvents();

				masterPackingUserControl.Visible = true;

				var tree = ((PackingTreeViewUserControl)masterPackingUserControl.Controls.Find("TreeUserControl", true).Single()).Tree;

				var masterPackageJobNode = tree.Nodes[0];
				AssertEquals("Precondition: top level node should be for the Master Booking PackageJob.", masterBookingPackageJob, ((ZBusinessObjectTreeNode)masterPackageJobNode).BizO);

				var listOfPackageJobNodes = masterPackageJobNode.Nodes.Cast<TreeNode>().ToArray();
				var jobsInNodes = listOfPackageJobNodes.Select(n => ((ZBusinessObjectTreeNode)n).BizO);
				AssertEquals(2, jobsInNodes.Count());
				AssertContainsExactElementsInAnyOrder("There should be a node for each PackageJob on a sub TB.", new[] { subConsolidation1.PackageJob, subConsolidation2.PackageJob }, jobsInNodes);

				foreach (var packageJobNode in listOfPackageJobNodes)
				{
					var listOfPackageNodes = packageJobNode.Nodes.Cast<TreeNode>().ToArray();

					foreach (var packageNode in listOfPackageNodes)
					{
						var package = ((ZBusinessObjectTreeNode)packageNode).BizO;
						if (package == subConsolidation1Package)
						{
							AssertEquals("Should have inner node corresponding to subConsolidation1InnerPackage", 1, packageNode.Nodes.Count);
							var innerPackageNode = packageNode.Nodes[0];
							AssertEquals("Inner package node should have PackageJob of subConsolidation1InnerPackage", subConsolidation1InnerPackage, ((ZBusinessObjectTreeNode)innerPackageNode).BizO);
						}
						else
						{
							AssertEquals("Should not have inner node corresponding to subConsolidation2InnerPackage", 0, packageNode.Nodes.Count);
						}
						foreach (var innerPackageNode in packageNode.Nodes)
						{
							AssertNoExceptionThrown("Should not throw an error about selecting a package that is not part of the package job", () =>
							{
								tree.SelectedNode = (PackingTreeNode)innerPackageNode;
							});
						}
						AssertEquals("Each node in each PackageJob node should be for a Package that belongs to that PackageJob.", ((ZBusinessObjectTreeNode)packageJobNode).BizO.PK, ((PkgPackage)package).KP_KJ_ParentPackageJob);
					}
				}
			}
		}

		void LinkSubBookingInstructionToMasterBookingInstruction(DtbBooking masterBooking, params DtbBooking[] subBookings)
		{
			foreach (var subBooking in subBookings)
			{
				foreach (var masterInstruction in masterBooking.Instructions)
				{
					var subInstruction = subBooking.Instructions.AddNew(masterInstruction.KN_InstructionType);
					subInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
				}
			}
		}

		void AssignDivotsToInstructions(DtbBookingInstruction instruction, (ZGuid packagePK, ZInt packageQty)[] divotDetails)
		{
			instruction.PackageDivots.DeleteAll();
			foreach (var divotDetail in divotDetails)
			{
				var packageDivot = instruction.PackageDivots.AddNew();
				packageDivot.KD_KP_Package = divotDetail.packagePK;
				packageDivot.KD_Quantity = divotDetail.packageQty;
			}
		}
	}
}
