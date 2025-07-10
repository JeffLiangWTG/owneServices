using System.Linq;
using System.Web.Http.Results;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using SupplierBookingLoadMode = Enterprise.Core.Constants.SupplierBookingLoadMode;

namespace Enterprise.Services.ServiceHost.Tests
{
	class OrderManagerContainerControllerTest : TestCaseWithFactory
	{
		public void TestChangeContainerNumber_EmptyContainerId()
		{
			var args = GetChangeContainerNumberArgs("", "ABC100", "ABC101");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertNotEquals("Please provide valid Container details.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_EmptyContainerJobID()
		{
			var args = GetChangeContainerNumberArgs("ABC101");
			args.ContainerJobID = "";

			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Please provide valid Container details.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_EmptyContainerLoadListID_ShouldFallbackToBooking()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var container = CreateContainer("ABC101", "JOB01", supplierBooking.PK);

			var args = GetChangeContainerNumberArgs("ABC101", "", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<OkResult>(result);

			container.Reload();
			AssertEquals("ABC101", container.JC_ContainerNum);
			AssertEquals(1, container.JC_ContainerCount.ToZInt());
		}

		public void TestChangeContainerNumber_EmptyContainerLoadListID_ShouldErrorIfBookingUnrelated()
		{
			CreateContainer("ABC101", "JOB01", null);

			var args = GetChangeContainerNumberArgs("ABC101", "", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("The Container provided is not related to a Supplier Booking.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_EmptyContainerLoadListID_ShouldErrorIfInvalidStatus()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateContainer("ABC101", "JOB01", supplierBooking.PK);

			Factory.Save();

			foreach (ICodeDescription status in new SupplierBookingStatusList())
			{
				supplierBooking.JSB_Status = status.Code;
				Factory.Save();

				var args = GetChangeContainerNumberArgs("ABC102", "", "JOB01");
				var result = controller.ChangeContainerNumber(args);

				if (status.Code == SupplierBookingStatusList.Codes.PLN || status.Code == SupplierBookingStatusList.Codes.CNV)
				{
					AssertType<OkResult>(result);
				}
				else
				{
					AssertType<BadRequestErrorMessageResult>(result);
					AssertEquals("The Supplier Booking related to the Container is in an un-editable state, you cannot perform this action at this time.", (result as BadRequestErrorMessageResult).Message);
				}
			}
		}

		public void TestChangeContainerNumber_NonExistentContainerLoadList()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateContainer("", "JOB01", supplierBooking.PK);
			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("A Container Load List with the details provided could not be found.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_NonExistentContainer()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);
			CreateConsol("CON01");

			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("A Container with the details provided could not be found.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_SingleContainerCount_ShouldAssignContainerNumber()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container);

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<OkResult>(result);

			container.Reload();
			AssertEquals("ABC101", container.JC_ContainerNum);
			AssertEquals(1, container.JC_ContainerCount.ToZInt());
		}

		public void TestChangeContainerNumber_SingleContainerCount_ShouldAssignEmptyContainerNumber()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container);

			Factory.Save();

			var result = controller.ChangeContainerNumber(GetChangeContainerNumberArgs("", "CLH01", "JOB01"));

			AssertType<OkResult>(result);

			container.Reload();
			AssertEquals("", container.JC_ContainerNum);
			AssertEquals(1, container.JC_ContainerCount.ToZInt());
		}

		public void TestChangeContainerNumber_SingleContainerCount_ShouldRaiseValidationErrorsIfTheyExist()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container1 = CreateContainer("ABC101", supplierBookingPK: supplierBooking.PK);
			var container2 = CreateContainer("", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container1);
			consol.Containers.Add(container2);

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("This container number is already in use and cannot be entered here.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_MultipleContainerCount_ShouldCloneAndSplit()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			container.JC_RC = refContainer.PK;
			consol.Containers.Add(container);

			Factory.Save();

			var containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Pre-condition", 1, containers.Length);

			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<OkResult>(result);

			container.Reload();

			CombineAssertions("It should apply the container number to the original container and set it's count to 1", () =>
			{
				AssertEquals("ABC101", container.JC_ContainerNum);
				AssertEquals(1, container.JC_ContainerCount.ToZInt());
			});

			containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals(2, containers.Length);

			var clonedContainer = containers.First(x => x.JC_ContainerCount == 4);

			CombineAssertions("Should have cloned the original container and deducted the count by 1", () =>
			{
				AssertNotNull(clonedContainer);
				AssertEquals("", clonedContainer.JC_ContainerNum);
				AssertEquals(4, clonedContainer.JC_ContainerCount.ToZInt());
				AssertEquals(supplierBooking.PK, clonedContainer.JC_JSB_SupplierBooking);
				AssertEquals(refContainer.PK, clonedContainer.JC_RC);
			});
		}

		public void TestChangeContainerNumber_MultipleContainerCount_ShouldRaiseValidationErrorsIfTheyExist()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container1 = CreateContainer("ABC101", supplierBookingPK: supplierBooking.PK);
			var container2 = CreateContainer("", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container1);
			consol.Containers.Add(container2);

			container2.JC_ContainerCount = 5;

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("This container number is already in use and cannot be entered here.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_MultipleContainerCount_ShouldIgnoreAlreadyAllocatedAndApprovedSupplierBookingsErrorMessage()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var supplierBooking = CreateJobSupplierBooking("JSB01");
				var consol = CreateConsol("CON01");
				CreateCYContainerLoadList("CLH01", supplierBooking.PK);

				var container = CreateContainer("", "JOB01", supplierBooking.PK);
				container.JC_ContainerCount = 5;
				consol.Containers.Add(container);

				Factory.Save();

				var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
				var result = controller.ChangeContainerNumber(args);

				AssertType<OkResult>(result);
			});
		}

		public void TestChangeContainerNumber_MultipleContainerCount_ShouldRaiseValidationErrorsWithAlreadyAllocatedMessage()
		{
			AdvOrmFeatureHelper.RunTestWith(true, action: () =>
			{
				var supplierBooking = CreateJobSupplierBooking("JSB01");
				var consol = CreateConsol("CON01");
				CreateCYContainerLoadList("CLH01", supplierBooking.PK);

				var container = CreateContainer("", "JOB01", supplierBooking.PK);
				container.JC_ContainerCount = 5;
				consol.Containers.Add(container);

				Factory.Save();

				var args = GetChangeContainerNumberArgs("ABC1011234567", "CLH01", "JOB01");
				var result = controller.ChangeContainerNumber(args);

				AssertType<BadRequestErrorMessageResult>(result);
				AssertEquals("The container number can be no more than 12 characters long", (result as BadRequestErrorMessageResult).Message);
			});
		}

		public void TestChangeContainerNumber_MultipleContainerCount_ShouldAttachContainerLoadPlan_IfOriginalContainerIsAssignedToContainerLoadListPlan()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var consol = CreateConsol("CON01");
			var containerLoadList = CreateCFSContainerLoadList("CLH01");

			var container = CreateContainer("", "JOB01", null);
			container.JC_ContainerCount = 5;
			container.JC_RC = refContainer.PK;
			container.JC_CLH_LoadListPlan = containerLoadList.PK;
			consol.Containers.Add(container);

			Factory.Save();

			var containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Pre-condition", 1, containers.Length);

			var args = GetChangeContainerNumberArgs("ABC101", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<OkResult>(result);

			container.Reload();

			CombineAssertions("It should apply the container number to the original container and set it's count to 1", () =>
			{
				AssertEquals("ABC101", container.JC_ContainerNum);
				AssertEquals(1, container.JC_ContainerCount.ToZInt());
			});

			containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals(2, containers.Length);

			var clonedContainer = containers.First(x => x.JC_ContainerCount == 4);

			AssertEquals("Original container was attached to a Container Load Plan, should attach the new container too", containerLoadList.PK, clonedContainer.JC_CLH_LoadListPlan);
		}

		public void TestChangeContainerNumber_ShouldErrorIfContainerNotRelatedToLoadListOrSupplierBooking()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container1 = CreateContainer("ABC101", "JOB01");
			consol.Containers.Add(container1);

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC102", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("The Container provided is not related to the relevant Supplier Booking or Container Load List.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_CFS_ShouldErrorIfContainerNotRelatedToLoadPlan()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCFSContainerLoadList("CLH01");

			var container1 = CreateContainer("ABC101", "JOB01");
			consol.Containers.Add(container1);

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC102", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("The Container provided is not related to the relevant Container Load Plan.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_IfContactUser_ShouldErrorIfNoTVFPermissions()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var container1 = CreateContainer("ABC101", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container1);

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC102", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("You do not have permission to perform this action. Please contact your system administrator.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestChangeContainerNumber_IfContactUser_ShouldNotErrorIfHasTVFPermissions()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			var consol = CreateConsol("CON01");
			var containerLoadList = CreateCYContainerLoadList("CLH01", supplierBooking.PK);
			containerLoadList.CLH_OH_LoadListParty = orgHeader.PK;

			var container1 = CreateContainer("ABC101", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container1);

			Factory.Save();

			var args = GetChangeContainerNumberArgs("ABC102", "CLH01", "JOB01");
			var result = controller.ChangeContainerNumber(args);

			AssertType<OkResult>(result);
		}

		public void TestChangeContainerNumber_ContainerLoadPlanWithStatuses()
		{
			var consol = CreateConsol("CON01");
			var containerLoadPlan = CreateCFSContainerLoadList("CLH01");
			var container = CreateContainer("ABC101", "JOB01", null);
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			consol.Containers.Add(container);

			Factory.Save();

			foreach (ICodeDescription status in new CommonContainerLoadListStatusList())
			{
				containerLoadPlan.CLH_Status = status.Code;
				Factory.Save();

				var args = GetChangeContainerNumberArgs("ABC102", "CLH01", "JOB01");
				var result = controller.ChangeContainerNumber(args);

				if (status.Code == CommonContainerLoadListStatusList.Codes.CAN || status.Code == CommonContainerLoadListStatusList.Codes.CNV)
				{
					AssertType<BadRequestErrorMessageResult>(result);
					AssertEquals("The Container Load List/Plan is in an un-editable state, you cannot perform this action at this time.", (result as BadRequestErrorMessageResult).Message);
				}
				else
				{
					AssertType<OkResult>(result);
				}
			}
		}

		public void TestChangeContainerNumber_ContainerLoadListWithStatuses()
		{
			var consol = CreateConsol("CON01");
			var supplierBooking = CreateJobSupplierBooking("SBK01");
			var containerLoadList = CreateCYContainerLoadList("CLH01", supplierBooking.PK);
			var container = CreateContainer("ABC101", "JOB01", supplierBooking.PK);
			consol.Containers.Add(container);

			Factory.Save();

			foreach (ICodeDescription status in new CommonContainerLoadListStatusList())
			{
				containerLoadList.CLH_Status = status.Code;
				Factory.Save();

				var args = GetChangeContainerNumberArgs("ABC102", "CLH01", "JOB01");
				var result = controller.ChangeContainerNumber(args);

				if (status.Code == CommonContainerLoadListStatusList.Codes.INC || status.Code == CommonContainerLoadListStatusList.Codes.REJ)
				{
					AssertType<OkResult>(result);
				}
				else
				{
					AssertType<BadRequestErrorMessageResult>(result);
					AssertEquals("The Container Load List/Plan is in an un-editable state, you cannot perform this action at this time.", (result as BadRequestErrorMessageResult).Message);
				}
			}
		}

		public void TestSplitContainer_EmptyContainerJobID()
		{
			var args = GetSplitContainerArgs(0);
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Please provide valid Container details.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_NegativeContainerCount_ShouldError()
		{
			var args = GetSplitContainerArgs(-15, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Unable to split a container with zero or a negative value.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_NonExistentContainer_ShouldError()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);
			CreateConsol("CON01");

			var args = GetSplitContainerArgs(1, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("A Container with the details provided could not be found.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_Allocation_ShouldErrorIfZeroMatchingUnallocatedContainers()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var allocatedContainer = CreateContainer("", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 5;
			consol.Containers.Add(allocatedContainer);

			Factory.Save();

			var args = GetSplitContainerArgs(10, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("There are zero unallocated containers available on the selected consol.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_Allocation_ShouldErrorIfNotEnoughMatchingUnallocatedContainers()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");

			var allocatedContainer = CreateContainer("", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 5;
			allocatedContainer.JC_RC = refContainer.PK;

			var unallocatedContainer1 = CreateContainer("", "JOB02");
			unallocatedContainer1.JC_ContainerCount = 3;
			unallocatedContainer1.JC_RC = refContainer.PK;

			var unallocatedContainer2 = CreateContainer("", "JOB03");
			unallocatedContainer2.JC_ContainerCount = 2;
			unallocatedContainer2.JC_RC = refContainer.PK;

			var containerWithNumberAlreadyAssigned = CreateContainer("CONTAINER", "JOB04");
			containerWithNumberAlreadyAssigned.JC_ContainerCount = 1;
			containerWithNumberAlreadyAssigned.JC_RC = refContainer.PK;

			consol.Containers.Add(allocatedContainer);
			consol.Containers.Add(unallocatedContainer1);
			consol.Containers.Add(unallocatedContainer2);
			consol.Containers.Add(containerWithNumberAlreadyAssigned);

			Factory.Save();

			var args = GetSplitContainerArgs(20, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals(
				"Should not include container with number already assigned",
				"There are only 5 unallocated containers available on the selected consol. Please enter an allocation between zero and 10.",
				(result as BadRequestErrorMessageResult).Message
			);
		}

		public void TestSplitContainer_Allocation_ShouldFullyConsumeContainerCount_IfUnallocatedContainersAvailable()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");

			var allocatedContainer = CreateContainer("", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 5;
			allocatedContainer.JC_RC = refContainer.PK;

			var unallocatedContainer1 = CreateContainer("", "JOB02");
			unallocatedContainer1.JC_ContainerCount = 2;
			unallocatedContainer1.JC_RC = refContainer.PK;

			var unallocatedContainer2 = CreateContainer("", "JOB03");
			unallocatedContainer2.JC_ContainerCount = 3;
			unallocatedContainer2.JC_RC = refContainer.PK;

			consol.Containers.Add(allocatedContainer);
			consol.Containers.Add(unallocatedContainer1);
			consol.Containers.Add(unallocatedContainer2);

			Factory.Save();

			var containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Pre-condition", 3, containers.Length);

			var args = GetSplitContainerArgs(10, "JOB01");
			var result = controller.Split(args);

			AssertType<OkResult>(result);

			containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			allocatedContainer.Reload();

			AssertEquals("Should have deleted unallocated containers", 1, containers.Length);
			AssertEquals("Should have consumed container count from both unallocated containers", 10, allocatedContainer.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_Allocation_ShouldPartiallyConsumeContainerCount_IfUnallocatedContainersHaveExcessContainerCount()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");

			var allocatedContainer = CreateContainer("", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 5;
			allocatedContainer.JC_RC = refContainer.PK;

			var unallocatedContainer1 = CreateContainer("", "JOB02");
			unallocatedContainer1.JC_ContainerCount = 10;
			unallocatedContainer1.JC_RC = refContainer.PK;

			var unallocatedContainer2 = CreateContainer("", "JOB03");
			unallocatedContainer2.JC_ContainerCount = 10;
			unallocatedContainer2.JC_RC = refContainer.PK;

			var unallocatedContainer3 = CreateContainer("", "JOB04");
			unallocatedContainer3.JC_ContainerCount = 10;
			unallocatedContainer3.JC_RC = refContainer.PK;

			consol.Containers.Add(allocatedContainer);
			consol.Containers.Add(unallocatedContainer1);
			consol.Containers.Add(unallocatedContainer2);
			consol.Containers.Add(unallocatedContainer3);

			Factory.Save();

			var containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Pre-condition", 4, containers.Length);

			var args = GetSplitContainerArgs(20, "JOB01");
			var result = controller.Split(args);

			AssertType<OkResult>(result);

			containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Should have deleted one of the unallocated containers as it's container count was fully exhausted", 3, containers.Length);

			var allocatedContainerReloaded = Factory.Load<ForwardingContainer>(allocatedContainer.PK);
			AssertEquals("Should have consumed container count from unallocated containers", 20, allocatedContainerReloaded.JC_ContainerCount.ToZInt());

			AssertContainsExactElementsInAnyOrder(
				"There should be two unallocated containers left and only the required container count to allocate should have been consumed",
				new ZShort[] { 10, 5 },
				containers.Where(c => c.JC_JSB_SupplierBooking == ZGuid.Empty).Select(x => x.JC_ContainerCount)
			);
		}

		public void TestSplitContainer_Allocation_ShouldErrorIfContainerNumAlreadyAssigned()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var allocatedContainer = CreateContainer("CONTAINER1", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 1;
			allocatedContainer.JC_RC = refContainer.PK;

			Factory.Save();

			var args = GetSplitContainerArgs(5, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("The Container provided already has a Container Number assigned.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_Deallocation_ShouldNotChangeContainerCount()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);

			Factory.Save();

			AssertEquals(supplierBooking.PK, container.JC_JSB_SupplierBooking);

			var args = GetSplitContainerArgs(0, "JOB01");
			var result = controller.Split(args);

			container.Reload();

			AssertType<OkResult>(result);
			AssertEquals(ZGuid.Empty, container.JC_JSB_SupplierBooking);
			AssertEquals(5, container.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_Deallocation_ShouldDetachContainerIfNoMatchedExistingContainerForSupplierBooking()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);

			Factory.Save();

			AssertEquals(supplierBooking.PK, container.JC_JSB_SupplierBooking);

			var result = controller.Split(GetSplitContainerArgs(0, "JOB01"));

			container.Reload();

			AssertType<OkResult>(result);
			AssertEquals(ZGuid.Empty, container.JC_JSB_SupplierBooking);
			AssertEquals(5, container.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_Deallocation_ShouldDetachContainerWithContainerNumberIfNoMatchedExistingContainerForSupplierBooking()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("CON01", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 1;
			consol.Containers.Add(container);

			Factory.Save();

			AssertEquals(supplierBooking.PK, container.JC_JSB_SupplierBooking);

			var result = controller.Split(GetSplitContainerArgs(0, "JOB01"));

			container.Reload();

			AssertType<OkResult>(result);
			AssertEquals(ZGuid.Empty, container.JC_JSB_SupplierBooking);
			AssertEquals(1, container.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_Deallocation_ShouldDetachContainerIfNoMatchedExistingContainerForContainerLoadPlan()
		{
			var containerLoadPlan = CreateCFSContainerLoadList("CLH01");

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", ZGuid.Empty);
			container.JC_ContainerCount = 5;
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			consol.Containers.Add(container);

			Factory.Save();

			AssertEquals(containerLoadPlan.PK, container.JC_CLH_LoadListPlan);

			var result = controller.Split(GetSplitContainerArgs(0, "JOB01", containerLoadPlan.CLH_LoadListId));

			container.Reload();

			AssertType<OkResult>(result);
			AssertEquals(ZGuid.Empty, container.JC_CLH_LoadListPlan);
			AssertEquals(5, container.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_Deallocation_ShouldDetachContainerWithContainerNumberIfNoMatchedExistingContainerForContainerLoadPlan()
		{
			var containerLoadPlan = CreateCFSContainerLoadList("CLH01");

			var consol = CreateConsol("CON01");
			var container = CreateContainer("CON01", "JOB01", ZGuid.Empty);
			container.JC_ContainerCount = 1;
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			consol.Containers.Add(container);

			Factory.Save();

			AssertEquals(containerLoadPlan.PK, container.JC_CLH_LoadListPlan);

			var result = controller.Split(GetSplitContainerArgs(0, "JOB01", containerLoadPlan.CLH_LoadListId));

			container.Reload();

			AssertType<OkResult>(result);
			AssertEquals(ZGuid.Empty, container.JC_CLH_LoadListPlan);
			AssertEquals(1, container.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_Deallocation_ShouldBeConsumedByExistingContainerIfItExists()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");

			var allocatedContainer = CreateContainer("", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 5;
			allocatedContainer.JC_RC = refContainer.PK;

			var unallocatedContainer = CreateContainer("", "JOB02");
			unallocatedContainer.JC_ContainerCount = 10;
			unallocatedContainer.JC_RC = refContainer.PK;

			consol.Containers.Add(allocatedContainer);
			consol.Containers.Add(unallocatedContainer);

			Factory.Save();

			var containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Pre-condition", 2, containers.Length);

			var args = GetSplitContainerArgs(0, "JOB01");
			var result = controller.Split(args);

			AssertType<OkResult>(result);

			containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Should have deleted the allocated container", 1, containers.Length);

			unallocatedContainer.Reload();

			AssertEquals(ZGuid.Empty, unallocatedContainer.JC_JSB_SupplierBooking);
			AssertEquals(
				"Unallocated container should have consumed the container count from the now-deleted container",
				15,
				unallocatedContainer.JC_ContainerCount.ToZInt()
			);
		}

		public void TestSplitContainer_Deallocation_ShouldNotBeConsumedByContainerWithNumberAlreadyAssigned()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			container.JC_RC = refContainer.PK;

			var containerWithNumberAlreadyAssigned = CreateContainer("CONTAINER", "JOB04");
			containerWithNumberAlreadyAssigned.JC_ContainerCount = 1;
			containerWithNumberAlreadyAssigned.JC_RC = refContainer.PK;

			consol.Containers.Add(container);
			consol.Containers.Add(containerWithNumberAlreadyAssigned);

			Factory.Save();

			AssertEquals(supplierBooking.PK, container.JC_JSB_SupplierBooking);

			var args = GetSplitContainerArgs(0, "JOB01");
			var result = controller.Split(args);

			container.Reload();
			containerWithNumberAlreadyAssigned.Reload();

			CombineAssertions("Should remove the container from the supplier booking and not re-allocate count to container with container number", () =>
			{
				AssertType<OkResult>(result);
				AssertEquals(ZGuid.Empty, container.JC_JSB_SupplierBooking);
				AssertEquals(5, container.JC_ContainerCount.ToZInt());
				AssertEquals(1, containerWithNumberAlreadyAssigned.JC_ContainerCount.ToZInt());
			});
		}

		public void TestSplitContainer_Deallocation_ShouldDeallocateLoadPlanIfCFS()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			supplierBooking.JSB_LoadMode = SupplierBookingLoadMode.ContainerFreightStation;

			var containerLoadPlan = CreateCFSContainerLoadList("CLH01");
			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			container.JC_CLH_LoadListPlan = containerLoadPlan.PK;
			consol.Containers.Add(container);

			Factory.Save();

			CombineAssertions("Pre-conditions", () =>
			{
				AssertEquals(supplierBooking.PK, container.JC_JSB_SupplierBooking);
				AssertEquals(containerLoadPlan.PK, container.JC_CLH_LoadListPlan);
			}); 

			var args = GetSplitContainerArgs(0, "JOB01", "CLH01");
			var result = controller.Split(args);

			container.Reload();

			AssertType<OkResult>(result);
			CombineAssertions("Should set JC_CLH_LoadListPlan to null rather than JC_JSB_SupplierBooking, since we are deallocating fromm CFS", () =>
			{
				AssertEquals(supplierBooking.PK, container.JC_JSB_SupplierBooking);
				AssertEquals(ZGuid.Empty, container.JC_CLH_LoadListPlan);
			});
		}

		public void TestSplitContainer_WithExistingContainerNum_ShouldError()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01"); 
			var container = CreateContainer("CNT01", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);

			Factory.Save();

			var args = GetSplitContainerArgs(3, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Container already has a container number assigned, a container count of one or not enough containers to split.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_ContainerLoadPlanWithStatuses()
		{
			var containerLoadList = CreateCFSContainerLoadList("CLH01");
			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01");
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);

			Factory.Save();

			foreach (ICodeDescription status in new CommonContainerLoadListStatusList())
			{
				container.JC_ContainerCount = 5;
				containerLoadList.CLH_Status = status.Code;
				Factory.Save();

				var args = GetSplitContainerArgs(3, "JOB01", containerLoadList.CLH_LoadListId);
				var result = controller.Split(args);

				if (status.Code == CommonContainerLoadListStatusList.Codes.INC || status.Code == CommonContainerLoadListStatusList.Codes.REJ)
				{
					AssertType<OkResult>(result);
				}
				else
				{
					AssertType<BadRequestErrorMessageResult>(result);
					AssertEquals("Reducing count or splitting container is only allowed if there are no lines linked to the container.", (result as BadRequestErrorMessageResult).Message);
				}
			}
		}

		public void TestSplitContainer_ContainerLoadPlanNotFound()
		{
			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01");
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);
			container.JC_ContainerCount = 5;
			Factory.Save();

			var args = GetSplitContainerArgs(3, "JOB01", "XXX");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("A Container Load List with the details provided could not be found.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_WithSingleContainer_ShouldError()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 1;
			consol.Containers.Add(container);

			Factory.Save();

			var args = GetSplitContainerArgs(1, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Container already has a container number assigned, a container count of one or not enough containers to split.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_IfAllocationIsWithinZeroToCurrentCount_ShouldCloneAndSplit()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			container.JC_RC = refContainer.PK;
			consol.Containers.Add(container);

			Factory.Save();

			var containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals("Pre-condition", 1, containers.Length);

			var args = GetSplitContainerArgs(2, "JOB01");
			var result = controller.Split(args);

			AssertType<OkResult>(result);

			container.Reload();

			CombineAssertions("It should deduct the split amount from the container count on the original container", () =>
			{
				AssertEquals(2, container.JC_ContainerCount.ToZInt());
			});

			containers = Factory.Load<ForwardingContainer>(new ZQuery(JobContainerSchema.JC_RC, refContainer.PK));
			AssertEquals(2, containers.Length);

			var clonedContainer = containers.First(x => x.PK != container.PK);

			CombineAssertions("Should have cloned the original container, split the container count and de-allocated it from the supplier booking", () =>
			{
				AssertNotNull(clonedContainer);
				AssertEquals(3, clonedContainer.JC_ContainerCount.ToZInt());
				AssertEquals(refContainer.PK, clonedContainer.JC_RC);
				AssertEquals(ZGuid.Empty, clonedContainer.JC_JSB_SupplierBooking);
			});
		}

		public void TestSplitContainer_IfAllocationIsWithinZeroToCurrentCount_ShouldSplitContainerCountIntoUnallocatedContainerIfItExists()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var allocatedContainer = CreateContainer("", "JOB01", supplierBooking.PK);
			allocatedContainer.JC_ContainerCount = 5;
			allocatedContainer.JC_RC = refContainer.PK;

			var unallocatedContainer = CreateContainer("", "JOB02");
			unallocatedContainer.JC_ContainerCount = 10;
			unallocatedContainer.JC_RC = refContainer.PK;

			var containerWithNumberAlreadyAssigned = CreateContainer("CONTAINER", "JOB04");
			containerWithNumberAlreadyAssigned.JC_ContainerCount = 1;
			containerWithNumberAlreadyAssigned.JC_RC = refContainer.PK;

			consol.Containers.Add(allocatedContainer);
			consol.Containers.Add(unallocatedContainer);
			consol.Containers.Add(containerWithNumberAlreadyAssigned);

			Factory.Save();

			var args = GetSplitContainerArgs(2, "JOB01");
			var result = controller.Split(args);

			AssertType<OkResult>(result);

			allocatedContainer.Reload();
			unallocatedContainer.Reload();

			AssertEquals("It should deduct the split amount from the container count on the original container", 2, allocatedContainer.JC_ContainerCount.ToZInt());
			AssertEquals("It should add the unallocated count to existing container", 13, unallocatedContainer.JC_ContainerCount.ToZInt());
			AssertEquals("It should not have allocated anything to container with existing container number", 1, containerWithNumberAlreadyAssigned.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_IfAllocationIsWithinZeroToCurrentCount_ShouldSplitIntoNewContainerIfNoMatchedExistingContainerForContainerLoadPlan()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			var containerLoadPlan = CreateCFSContainerLoadList("CLH01");

			var consol = CreateConsol("CON01");
			var allocatedContainer1 = CreateContainer("", "JOB01");
			allocatedContainer1.JC_ContainerCount = 5;
			allocatedContainer1.JC_RC = refContainer.PK;
			allocatedContainer1.JC_CLH_LoadListPlan = containerLoadPlan.PK;

			var allocatedContainer2 = CreateContainer("", "JOB02");
			allocatedContainer2.JC_ContainerCount = 10;
			allocatedContainer2.JC_RC = refContainer.PK;
			allocatedContainer2.JC_CLH_LoadListPlan = containerLoadPlan.PK;

			consol.Containers.Add(allocatedContainer1);
			consol.Containers.Add(allocatedContainer2);

			Factory.Save();

			var result = controller.Split(GetSplitContainerArgs(2, "JOB01", containerLoadPlan.CLH_LoadListId));

			AssertType<OkResult>(result);

			consol.Reload();
			allocatedContainer1.Reload();
			allocatedContainer2.Reload();

			var newUnallocatedContainer = consol.Containers.OfType<ForwardingContainer>().FirstOrDefault(c => c.PK != allocatedContainer1.PK && c.PK != allocatedContainer2.PK);

			AssertEquals("It should deduct the split amount from the container count on the original container", 2, allocatedContainer1.JC_ContainerCount.ToZInt());
			AssertEquals("It should not change as it has been attached to container load list", 10, allocatedContainer2.JC_ContainerCount.ToZInt());
			AssertEquals("It should allocated anything to new container with existing container type", 3, newUnallocatedContainer.JC_ContainerCount.ToZInt());
		}

		public void TestSplitContainer_IfAllocationIsEqualToCurrentCount_ShouldError()
		{
			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);

			Factory.Save();

			var args = GetSplitContainerArgs(5, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Container already has a container number assigned, a container count of one or not enough containers to split.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestSplitContainer_IfContactUser_ShouldError()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC01";

			var contact = Factory.New<OrgContact>();
			contact.OC_OH = orgHeader.PK;

			GlowTicketTestHelper.SetUpContactPrincipal(controller, contact);

			var supplierBooking = CreateJobSupplierBooking("JSB01");
			CreateCYContainerLoadList("CLH01", supplierBooking.PK);

			var consol = CreateConsol("CON01");
			var container = CreateContainer("", "JOB01", supplierBooking.PK);
			container.JC_ContainerCount = 5;
			consol.Containers.Add(container);

			Factory.Save();

			var args = GetSplitContainerArgs(2, "JOB01");
			var result = controller.Split(args);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("You do not have permission to perform this action. Please contact your system administrator.", (result as BadRequestErrorMessageResult).Message);
		}

		#region TestForGetDensityFactor

		readonly string defaultLoadedWeightUnit = "KG";
		readonly string defaultLoadedVolumeUnit = "M3";
		readonly decimal defaultLoadedWeight = 1000m;
		readonly decimal defaultLoadedVolume = 6m;

		public void TestGetDensityFactor_EmptyContainerId()
		{
			var result = controller.GetDensityFactor("", defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("Please provide valid Container details.", (result as BadRequestErrorMessageResult).Message);
		}
		public void TestGetDensityFactor_NotFoundContainer()
		{
			var result = controller.GetDensityFactor("test", defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			AssertType<BadRequestErrorMessageResult>(result);
			AssertEquals("A Container with the details provided could not be found.", (result as BadRequestErrorMessageResult).Message);
		}

		public void TestGetDensityFactor_InValidLoadedWeight()
		{
			var container = CreateContainerForDensity();
			var result = controller.GetDensityFactor(container.JC_ContainerJobID, defaultLoadedWeightUnit, defaultLoadedVolumeUnit, 0m, defaultLoadedVolume);

			var getDensityFactorResponse = result as JsonResult<DensityFactorResponse>;
			AssertEquals(0m, getDensityFactorResponse.Content.dense);
			AssertEquals(-1, getDensityFactorResponse.Content.denseIndex);
		}

		public void TestGetDensityFactor_InValidLoadedVolume()
		{
			var container = CreateContainerForDensity();
			var result = controller.GetDensityFactor(container.JC_ContainerJobID, defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, 0m);

			if (result is JsonResult<DensityFactorResponse> getDensityFactorResponse)
			{
				AssertEquals(0m, getDensityFactorResponse.Content.dense);
				AssertEquals(-1, getDensityFactorResponse.Content.denseIndex);
			}
			else
			{
				Fail("expected response");
			}
		}

		public void TestGetDensityFactor_InValidLoadedWeightUnit()
		{
			var container = CreateContainerForDensity();
			var result = controller.GetDensityFactor(container.JC_ContainerJobID, "aa", defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			if (result is JsonResult<DensityFactorResponse> getDensityFactorResponse)
			{
				AssertEquals(0m, getDensityFactorResponse.Content.dense);
				AssertEquals(-1, getDensityFactorResponse.Content.denseIndex);
			}
			else
			{
				Fail("expected response");
			}
		}

		public void TestGetDensityFactor_InValidLoadedVolumeUnit()
		{
			var container = CreateContainerForDensity();
			var result = controller.GetDensityFactor(container.JC_ContainerJobID, defaultLoadedWeightUnit, "bb", defaultLoadedWeight, defaultLoadedVolume);

			if (result is JsonResult<DensityFactorResponse> getDensityFactorResponse)
			{
				AssertEquals(0m, getDensityFactorResponse.Content.dense);
				AssertEquals(-1, getDensityFactorResponse.Content.denseIndex);
			}
			else
			{
				Fail("expected response");
			}
		}

		public void TestGetDensityFactor_AIR()
		{
			var container = CreateContainerForDensity();
			var result = controller.GetDensityFactor(container.JC_ContainerJobID, defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			if (result is JsonResult<DensityFactorResponse> getDensityFactorResponse)
			{
				AssertEquals(1m, getDensityFactorResponse.Content.dense);
				AssertEquals(5, getDensityFactorResponse.Content.denseIndex);
			}
			else
			{
				Fail("expected response");
			}
		}

		public void TestGetDensityFactor_SEA()
		{
			var container = CreateContainerForDensity("SEA");
			var result = controller.GetDensityFactor(container.JC_ContainerJobID, defaultLoadedWeightUnit, defaultLoadedVolumeUnit, defaultLoadedWeight, defaultLoadedVolume);

			if (result is JsonResult<DensityFactorResponse> getDensityFactorResponse)
			{
				AssertEquals(6m, getDensityFactorResponse.Content.dense);
				AssertEquals(11, getDensityFactorResponse.Content.denseIndex);
			}
			else
			{
				Fail("expected response");
			}
		}

		#endregion

		#region Implementation

		ChangeContainerNumberArgs GetChangeContainerNumberArgs(
			string containerNum,
			string containerLoadListID = "",
			string containerJobID = "")
		{
			return new ChangeContainerNumberArgs()
			{
				ContainerID = containerNum,
				ContainerLoadListID = containerLoadListID,
				ContainerJobID = containerJobID
			};
		}

		SplitContainerArgs GetSplitContainerArgs(int containerCount, string containerJobID = "", string containerLoadListID = "")
		{
			return new SplitContainerArgs()
			{
				ContainerCount = containerCount,
				ContainerJobID = containerJobID,
				ContainerLoadListID = containerLoadListID,
			};
		}

		ForwardingConsol CreateConsol(string consolID)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolID;

			Factory.Save();

			return consol;
		}

		JobSupplierBooking CreateJobSupplierBooking(string supplierBookingID)
		{
			var jobSupplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			jobSupplierBooking.JSB_BookingId = supplierBookingID;

			Factory.Save();

			return jobSupplierBooking;
		}

		CommonContainerLoadList CreateCYContainerLoadList(string containerLoadListID, ZGuid? supplierBookingPK = null)
		{
			return CreateContainerLoadList(containerLoadListID, supplierBookingPK, SupplierBookingLoadMode.ContainerYard);
		}

		CommonContainerLoadList CreateCFSContainerLoadList(string containerLoadListID)
		{
			return CreateContainerLoadList(containerLoadListID, null, SupplierBookingLoadMode.ContainerFreightStation);
		}

		CommonContainerLoadList CreateContainerLoadList(string containerLoadListID, ZGuid? supplierBookingPK, string loadMode)
		{
			var containerLoadListHeader = Factory.NewWithValidTestData<CommonContainerLoadList>();
			containerLoadListHeader.CLH_LoadListId = containerLoadListID;
			containerLoadListHeader.CLH_Status = Core.Constants.ContainerLoadListHeaderStatus.Incomplete;
			containerLoadListHeader.CLH_LoadMode = loadMode;

			if (supplierBookingPK != null)
			{
				containerLoadListHeader.CLH_JSB_Booking = (ZGuid)supplierBookingPK;
			}

			Factory.Save();

			return containerLoadListHeader;
		}

		ForwardingContainer CreateContainer(string containerNum, string containerJobID = "", ZGuid? supplierBookingPK = null)
		{
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			container.JC_ContainerNum = containerNum;
			container.JC_ContainerJobID = containerJobID;

			if (supplierBookingPK != null)
			{
				container.JC_JSB_SupplierBooking = (ZGuid)supplierBookingPK;
			}

			Factory.Save();

			return container;
		}

		ForwardingContainer CreateContainerForDensity(string transportMode = "AIR")
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = Factory.NewWithValidTestData<ForwardingContainer>();
			consol.JK_TransportMode = transportMode;
			consol.Containers.Add(container);

			Factory.Save();

			return container;
		}

		protected override void SetUp()
		{
			controller = new OrderManagerContainerController();
			GlowTicketTestHelper.SetUpStaffPrincipal(controller, Factory.NewWithValidTestData<GlbStaff>());
		}

		OrderManagerContainerController controller;

		#endregion
	}
}
