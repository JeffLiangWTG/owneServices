using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Web.WebService.WhsPackageAuditManagerForWebServices;

namespace Enterprise.Warehouse.Web.WebService
{
	public class WhsPackageAuditManagerTest : WhsTestCaseWithFactory
	{
		#region TestVerifyPackageForAuditForReference

		#region TestVerifyPackageForAuditForReference_Unique

		public void TestVerifyPackageForAuditForReference_Unique()
		{
			AssertTestVerifyPackageForAudit_Unique(true);
		}

		#endregion

		#region TestVerifyPackageForAuditForReference_Multiple

		public void TestVerifyPackageForAuditForReference_Multiple()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "P1";

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "2", data.Part1, 5m);
			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			var innerPackage = package2.Packages.AddNew("PLT", "P1");
			Factory.Save();

			var result = new PackageWebServiceResponse();
			WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, package1.KP_PackageID);
			AssertContainsExactElementsInAnyOrder("Since there are several of them even if some don't comply the requisites should be returned to the user", new[] { package1.PK, innerPackage.PK }, result.PackageChoices.Select(p => p.PK));
		}

		#endregion

		#region TestVerifyPackageForAuditForReference_NotCompletelyPicked

		public void TestVerifyPackageForAuditForReference_NotCompletelyPicked()
		{
			AssertTestVerifyPackageForAudit_NotCompletelyPicked(true);
		}

		#endregion

		#region TestVerifyPackageForAuditForReference_NotFound

		public void TestVerifyPackageForAuditForReference_NotFound()
		{
			var result = new PackageWebServiceResponse();
			WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, "P1");
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
			AssertEquals("The Package P1 has not been found.", result.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
		}

		#endregion

		#region TestVerifyPackageForAuditForReference_DontBelongToOrder

		public void TestVerifyPackageForAuditForReference_DontBelongToOrder()
		{
			AssertTestVerifyPackageForAudit_DontBelongToOrder(true);
		}

		#endregion

		#region TestVerifyPackageForAuditForReference_AnOuterPackageOrFirstLevelPackage

		public void TestVerifyPackageForAuditForReference_AnOuterPackageOrFirstLevelPackage()
		{
			AssertTestVerifyPackageForAudit_AnOuterPackageOrFirstLevelPackage(true);
		}

		#endregion

		#region TestVerifyPackageForAuditForReference_NotAnOuterPackageOrFirstLevelPackage

		public void TestVerifyPackageForAuditForReference_NotAnOuterPackageOrFirstLevelPackage()
		{
			AssertTestVerifyPackageForAudit_NotAnOuterPackageOrFirstLevelPackage(false);
		}

		#endregion

		#endregion

		#region TestVerifyPackageForAuditForPK

		#region TestVerifyPackageForAuditForPK_Unique

		public void TestVerifyPackageForAuditForPK_Unique()
		{
			AssertTestVerifyPackageForAudit_Unique(false);
		}

		#endregion

		#region TestVerifyPackageForAuditForPK_NotCompletelyPicked

		public void TestVerifyPackageForAuditForPK_NotCompletelyPicked()
		{
			AssertTestVerifyPackageForAudit_NotCompletelyPicked(false);
		}

		#endregion

		#region TestVerifyPackageForAuditForPK_NotFound

		public void TestVerifyPackageForAuditForPK_NotFound()
		{
			var result = new PackageWebServiceResponse();
			WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, new ZGuid());
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
			AssertEquals("The Package has not been found.", result.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
		}

		#endregion

		#region TestVerifyPackageForAuditForPK_AnOuterPackageOrFirstLevelPackage

		public void TestVerifyPackageForAuditForPK_AnOuterPackageOrFirstLevelPackage()
		{
			AssertTestVerifyPackageForAudit_AnOuterPackageOrFirstLevelPackage(false);
		}

		#endregion

		#region TestVerifyPackageForAuditForPK_NotAnOuterPackageOrFirstLevelPackage

		public void TestVerifyPackageForAuditForPK_NotAnOuterPackageOrFirstLevelPackage()
		{
			AssertTestVerifyPackageForAudit_NotAnOuterPackageOrFirstLevelPackage(false);
		}

		#endregion

		#region TestVerifyPackageForAuditForPK_DontBelongToOrder

		public void TestVerifyPackageForAuditForPK_DontBelongToOrder()
		{
			AssertTestVerifyPackageForAudit_DontBelongToOrder(false);
		}

		#endregion

		#endregion

		#region TestVerifyPackageForPreviousAudit

		public void TestVerifyPackageForPreviousAudit_IsPassed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG", "PLT");
			var audit = Helper.CreateWhsPackageAudit(order);
			audit.WPA_PackageID = "PKG";
			Factory.Save();

			VerifyPackageForPreviousAudit(Factory, result, package.PK.ToGuid());
			AssertEquals("Must set error type to enquire the user.", ErrorTypes.YesNoEnquiry, result.Error);
			AssertEquals("Must set pre-audited package message.", "This package has already been audited and passed. Do you want to re-audit?", result.ErrorMessage);
		}

		public void TestVerifyPackageForPreviousAudit_WithFailPassFailData()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG", "PLT");
			var audit01 = Helper.CreateWhsPackageAuditWithLineFailure(order, "PKG", data.Part1, 5m, 10m);
			Factory.Save();

			VerifyPackageForPreviousAudit(Factory, result, package.PK.ToGuid());
			AssertEquals("Must set error type to enquire the user.", ErrorTypes.YesNoEnquiry, result.Error);
			AssertEquals("Must set pre-audited package message.", "This package has already been audited and failed. Do you want to re-audit?", result.ErrorMessage);

			var znow = ZDateTimeOffset.Now;
			var audit02 = Helper.CreateWhsPackageAudit(order, "PKG", znow.AddMinutes(1));
			Factory.Save();

			VerifyPackageForPreviousAudit(Factory, result, package.PK.ToGuid());
			AssertEquals("Must set error type to enquire the user.", ErrorTypes.YesNoEnquiry, result.Error);
			AssertEquals("Must set pre-audited package message.", "This package has already been audited and passed. Do you want to re-audit?", result.ErrorMessage);

			var audit03 = Helper.CreateWhsPackageAuditWithLineFailure(order, "PKG", data.Part1, 5m, 20m, znow.AddMinutes(2));
			Factory.Save();

			VerifyPackageForPreviousAudit(Factory, result, package.PK.ToGuid());
			AssertEquals("Must set error type to enquire the user.", ErrorTypes.YesNoEnquiry, result.Error);
			AssertEquals("Must set pre-audited package message.", "This package has already been audited and failed. Do you want to re-audit?", result.ErrorMessage);
		}

		public void TestVerifyPackageForAudit_WithApproximateSameTime()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG", "PLT");
			var znow = ZDateTimeOffset.Now;
			var audit1 = Helper.CreateWhsPackageAudit(order, "PKG", znow);
			audit1.WPA_PackageID = "PKG";
			Factory.Save();

			var audit02 = Helper.CreateWhsPackageAudit(order, "PKG", znow.AddSeconds(1));
			Factory.Save();

			AssertEquals(ErrorTypes.None, result.Error);
			AssertEquals(null, result.ErrorMessage);
		}

		public void TestVerifyPackageForPreviousAudit_NotAuditedBefore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG", "PLT");
			Factory.Save();

			VerifyPackageForPreviousAudit(Factory, result, package.PK.ToGuid());
			AssertEquals("Must allow the user to continue to audit the package.", ErrorTypes.None, result.Error);
			AssertNullOrEmpty("Error message must be empty.", result.ErrorMessage);
		}

		public void TestVerifyPackageForPreviousAudit_InvalidPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG", "PLT");
			package.Delete();
			Factory.Save();

			AssertEquals("Precondition", true, package.IsDeleted);

			VerifyPackageForPreviousAudit(Factory, result, package.PK.ToGuid());

			AssertEquals("Error must be produced when verifying for reaudit.", ErrorTypes.BusinessValidationError, result.Error);
			AssertEquals("Should show error message to user.", PackageAuditManagerStrings.Error_PackageOrderNotFound, result.ErrorMessage);
		}

		#endregion

		#region VerifyPackageForAuditAuxiliarFunctions

		#region AssertTestVerifyPackageForAudit_Unique

		public void AssertTestVerifyPackageForAudit_Unique(bool reference)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			var result = new PackageWebServiceResponse();

			if (reference)
			{
				package.KP_PackageID = "P1";
				Factory.Save();
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, package.KP_PackageID);
			}
			else
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, package.PK);
			}
			AssertContainsExactElementsInAnyOrder("Since there is just one package and it complies with all the requisites, it should be the only returned", new[] { package.PK }, result.PackageChoices.Select(p => p.PK));
		}

		#endregion

		#region AssertTestVerifyPackageForAudit_NotCompletelyPicked

		public void AssertTestVerifyPackageForAudit_NotCompletelyPicked(bool reference)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save(); // to create stock

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(pkgJob, "P1", 1, "BOX");
			var pickLine1 = orderLine1.PickLines.Single();
			var packageDivot1 = PackingHelper.CreatePackageDivot(package, pickLine1);

			WhsPickLine pickLine2;
			var releaseLine = orderLine1.ReleaseLines.AddNew("RED", "", "", "RED", ZDate.Empty, ZDate.Empty);
			pickLine2 = pickLine1.Split(3);
			pickLine1.WZ_ReleaseCapturedPartAttrib1 = "RED";
			pickLine2.WZ_ReleaseCapturedPartAttrib1 = "RED";

			var packageDivot2 = PackingHelper.CreatePackageDivot(package, pickLine2);
			packageDivot2.KI_PackedQty = 2;

			Factory.Save();

			var result = new PackageWebServiceResponse();
			if (reference)
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, package.KP_PackageID);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
				AssertEquals("The Package P1 has not been completely picked.", result.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);

				result.Error = ErrorTypes.None;
				result.ErrorMessage = "";
				result.Error = ErrorTypes.None;
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, package.KP_PackageID);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
				AssertEquals("The Package P1 has not been completely picked.", result.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);

				result.Error = ErrorTypes.None;
				result.ErrorMessage = "";
				result.Error = ErrorTypes.None;
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: Testing PickedDateTime, NOT OriginalPickedInventory.", ZGuid.Empty, pickLine2.WZ_WE_OriginalPickedInventoryLine);
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, package.KP_PackageID);
				AssertEquals(ErrorTypes.None, result.Error);
				AssertEquals("", result.ErrorMessage);
				AssertEquals(ErrorTypes.None, result.Error);

				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				AssertNotNull("Precondition: Made In-Transit Transfer.", orderLine1.PickLines.Single(pl => pl.WZ_PickedDateTime.IsEmpty && !pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty));
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, package.KP_PackageID);
				AssertEquals(ErrorTypes.None, result.Error);
				AssertEquals("", result.ErrorMessage);
				AssertEquals(ErrorTypes.None, result.Error);
			}
			else
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, package.PK);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
				AssertEquals("The Package P1 has not been completely picked.", result.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);

				result.Error = ErrorTypes.None;
				result.ErrorMessage = "";
				result.Error = ErrorTypes.None;
				pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, package.PK);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
				AssertEquals("The Package P1 has not been completely picked.", result.ErrorMessage);
				AssertEquals(ErrorTypes.BusinessValidationError, result.Error);

				result.Error = ErrorTypes.None;
				result.ErrorMessage = "";
				result.Error = ErrorTypes.None;
				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
				AssertEquals("Precondition: Testing PickedDateTime, NOT OriginalPickedInventory.", ZGuid.Empty, pickLine2.WZ_WE_OriginalPickedInventoryLine);
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, package.PK);
				AssertEquals(ErrorTypes.None, result.Error);
				AssertEquals("", result.ErrorMessage);

				pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Empty;
				Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
				AssertNotNull("Precondition: Made In-Transit Transfer.", orderLine1.PickLines.Single(pl => pl.WZ_PickedDateTime.IsEmpty && !pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty));
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, package.PK);
				AssertEquals(ErrorTypes.None, result.Error);
				AssertEquals("", result.ErrorMessage);
			}
			AssertContainsExactElementsInAnyOrder("Now the package should be good for audit", new[] { package.PK }, result.PackageChoices.Select(p => p.PK));
		}

		#endregion

		#region AssertTestVerifyPackageForAudit_DontBelongToOrder

		public void AssertTestVerifyPackageForAudit_DontBelongToOrder(bool reference)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "1", data.Part1, 10m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);

			var consol = Factory.New<IDtbBookingConsolidation>();
			consol.KB_ParentID = order.PK;
			consol.KB_ParentTableCode = WhsDocketSchema.Constants.Prefix;

			var booking = Factory.New<IDtbBooking>();
			booking.KM_KB_Booking = consol.PK;
			booking.KM_JobID = "Booking";

			var packageJob = booking.PackageJob as PkgPackageJob;
			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "P1";
			Factory.Save();

			var result = new PackageWebServiceResponse();
			if (reference)
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, "P1");
			}
			else
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, package.PK);
			}
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
			AssertEquals("The Package P1 does not belong to an order.", result.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
		}

		#endregion

		#region AssertTestVerifyPackageForAudit_AnOuterPackageOrFirstLevelPackage

		public void AssertTestVerifyPackageForAudit_AnOuterPackageOrFirstLevelPackage(bool reference)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var container = order.PackageJob.Packages.AddNew("CNT");
			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.Container.K0_RC_ContainerType = containerType.PK;
			var packageInsideContainer = container.Packages.AddNew();
			packageInsideContainer.KP_PackageID = "P1";

			var result = new PackageWebServiceResponse();
			if (reference)
			{
				Factory.Save();
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, "P1");
			}
			else
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, packageInsideContainer.PK);
			}
			AssertContainsExactElementsInAnyOrder("Since there is just one and it's indeed a first level package in a container, it should be returned", new[] { packageInsideContainer.PK }, result.PackageChoices.Select(p => p.PK));
		}

		#endregion

		#region AssertTestVerifyPackageForAudit_NotAnOuterPackageOrFirstLevelPackage

		public void AssertTestVerifyPackageForAudit_NotAnOuterPackageOrFirstLevelPackage(bool reference)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			var innerPackage = package.Packages.AddNew("PLT", "P1");

			var result = new PackageWebServiceResponse();
			if (reference)
			{
				Factory.Save();
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForReference(Factory, result, "P1");
			}
			else
			{
				WhsPackageAuditManagerForWebServices.VerifyPackageForAuditForPK(Factory, result, innerPackage.PK);
			}
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
			AssertEquals("The Package P1 is not an outer or a first level Package so it can't be audited.", result.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, result.Error);
		}

		#endregion

		#endregion

		#region TestPerformPackageContentsAudit

		#region TestFullMatch

		public void TestPerformPackageContentsAudit_FullMatch()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new[]
			{
				AddPackageAuditInfo(data.Part1, 5m),
				AddPackageAuditInfo(data.Part2, 10m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have no variance.", 0, audit.PackageAuditFailureLines.Count);
			AssertEquals("No variances, should not hold package.", false, package.KP_IsHeld);
		}

		#endregion

		#region TestEmptyAudit

		public void TestPerformPackageContentsAudit_EmptyAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = System.Array.Empty<WhsPackageProductInfo>();
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have variance for both products in order.", 2, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part1, 5m, 0m);
			AssertVarianceExist(audit, data.Part2, 10m, 0m);
			AssertEquals("With variances, it should hold package.", true, package.KP_IsHeld);
		}

		#endregion

		#region TestProductMissingInAudit

		public void TestPerformPackageContentsAudit_ProductMissingInAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part2, 10m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have 1 variance line.", 1, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part1, 5m, 0m);
			AssertEquals("When having variance, package must be held.", true, package.KP_IsHeld);
		}

		#endregion

		#region TestProductMissingInOrder

		public void TestPerformPackageContentsAudit_ProductMissingInOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 5m),
				AddPackageAuditInfo(data.Part2, 10m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have 1 variance line.", 1, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part2, 0m, 10m);
			AssertEquals("When having variance, package must be held.", true, package.KP_IsHeld);
		}

		#endregion

		#region TestSingleQuantityVariance

		public void TestPerformPackageContentsAudit_SingleQuantityVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 5m),
				AddPackageAuditInfo(data.Part2, 12m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have 1 variance line.", 1, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part2, 10m, 12m);
			AssertEquals("When having variance, package must be held.", true, package.KP_IsHeld);
		}

		#endregion

		#region TestMultipleQuantityVariance

		public void TestPerformPackageContentsAudit_MultipleQuantityVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 3m),
				AddPackageAuditInfo(data.Part2, 12m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have 2 variance lines.", 2, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part1, 5m, 3m);
			AssertVarianceExist(audit, data.Part2, 10m, 12m);
			AssertEquals("When having variance, package must be held.", true, package.KP_IsHeld);
		}

		#endregion

		#region TestMultipleLinesInOrderForSameProduct

		public void TestPerformPackageContentsAudit_MultipleLinesInOrderForSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 20m),
				AddPackageAuditInfo(data.Part2, 10m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have no variance lines.", 0, audit.PackageAuditFailureLines.Count);
			AssertEquals("No variances, should not hold package.", false, package.KP_IsHeld);
		}

		#endregion

		#region TestMultipleLinesInOrderForSameProductWithVariance

		public void TestPerformPackageContentsAudit_MultipleLinesInOrderForSameProductWithVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("JSM", "JohnSmith");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "P1", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 15m),
				AddPackageAuditInfo(data.Part2, 10m)
			};
			WhsPackageAuditManagerForWebServices.PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have 1 variance line.", 1, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part1, 20m, 15m);
			AssertEquals("When having variance, package must be held.", true, package.KP_IsHeld);
		}

		#endregion

		#endregion

		#region TestPerformPackageContentsAudit_SupportMethods

		#region AddOrderWithLinesAndInventory

		WhsOrder AddOrderWithLinesAndInventory(BusinessObjectFactory factory, OrgHeader client, WhsWarehouse whs, OrgSupplierPart part1, decimal units1, OrgSupplierPart part2, decimal units2, decimal inventoryUnits)
		{
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part1, inventoryUnits);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", part2, inventoryUnits);

			var order = Helper.CreateWhsOrder(client, whs);
			Helper.CreateWhsOrderLine(order, part1, units1);
			Helper.CreateWhsOrderLine(order, part2, units2);

			factory.Save();
			return order;
		}

		#endregion

		#region AddPackageToOrder

		PkgPackage AddPackageToOrder(WhsOrder order, ZString packageId, ZString packageType)
		{
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, packageId, 1, packageType);
			foreach (var orderLine in order.Lines)
			{
				var pickLine = orderLine.PickLines.Single();
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Today;

				PackingHelper.CreatePackageDivot(package, pickLine);
			}
			return package;
		}

		#endregion

		#region AddPackageAuditInfo

		WhsPackageProductInfo AddPackageAuditInfo(OrgSupplierPart product, decimal actualQty)
		{
			var auditInfo = new WhsPackageProductInfo(product.PK.ToGuid(), string.Empty, string.Empty, actualQty, 0m, 0m, string.Empty);
			return auditInfo;
		}

		#endregion

		#endregion

		#region AuditPackageContents_Assertions

		WhsPackageAudit AssertWhsPackageAuditIsRegistered(BusinessObjectFactory factory, WhsOrder order, PkgPackage package, GlbStaff staff)
		{
			var query = new ZQuery(WhsPackageAuditSchema.WPA_WD_Order, order.PK);
			query.AddToFilter(WhsPackageAuditSchema.WPA_PackageID, package.KP_PackageID);

			var auditLog = factory.Load<WhsPackageAudit>(query).OrderByDescending(c => c.WPA_AuditCompleteTime).FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertNotNull(auditLog);
				AssertEquals(order.PK, auditLog.Order.PK);
				AssertEquals(package.KP_PackageID, auditLog.WPA_PackageID);
				AssertEquals(staff.GS_Code, auditLog.WPA_GS_NKAuditor);
				AssertNotEquals(ZDateTime.Empty, auditLog.WPA_AuditCompleteTime);
			});

			return auditLog;
		}

		void AssertVarianceExist(WhsPackageAudit audit, OrgSupplierPart product, decimal expectedUnits, decimal auditedUnits)
		{
			var failureLine = audit.PackageAuditFailureLines.Single(l => l.WPF_OP == product.PK);
			CombineAssertions(() =>
			{
				AssertEquals("WPF_ExpectedQty", expectedUnits, failureLine.WPF_ExpectedQty);
				AssertEquals("WPF_AuditedQty", auditedUnits, failureLine.WPF_AuditedQty);
			});
		}

		#endregion

		#region TestPackageAuditEvents

		#region TestPackageAuditEvents_PackageHeld_SingleProductAuditFailure

		public void TestPackageAuditEvents_PackageHeld_SingleProductAuditFailure()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReference = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Audit Failed", package.KP_PackageID, data.Part1.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var auditLines = new WhsPackageProductInfo[]
				{
				AddPackageAuditInfo(data.Part1, 4m),
				AddPackageAuditInfo(data.Part2, 10m)
				};
				PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);
				AssertEquals("Precondition: The package must be held.", true, package.KP_IsHeld);

				var eventLog = GetPackageHoldEventLogs(order.PK).Single();
				AssertEventLog(eventLog, AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			}
		}

		#endregion

		#region TestPackageAuditEvents_PackageHeld_ManyProductAuditFailures

		public void TestPackageAuditEvents_PackageHeld_ManyProductAuditFailures()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReference = ZString.Format("Package: {0}, Product: MULTIPLE|DEP={1}|FAC={2}|RES=Audit Failed", package.KP_PackageID, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var auditLines = new WhsPackageProductInfo[]
				{
				AddPackageAuditInfo(data.Part1, 4m),
				AddPackageAuditInfo(data.Part2, 9m)
				};
				PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);
				AssertEquals("Precondition: The package must be held.", true, package.KP_IsHeld);

				var eventLog = GetPackageHoldEventLogs(order.PK).Single();
				AssertEventLog(eventLog, AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			}
		}

		#endregion

		#region TestPackageAuditEvents_PackageHeld_PassedFirstAudit

		public void TestPackageAuditEvents_PackageHeld_PassedFirstAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			Factory.Save();

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 5m),
				AddPackageAuditInfo(data.Part2, 10m)
			};
			PerformPackageContentsAudit(Factory, new WebServiceResponse(), auditLines, package.PK.ToGuid(), staff);
			AssertEquals("Precondition: The package must NOT be held.", false, package.KP_IsHeld);
			Assert("The package should not have any held (SHL) or cleared hold (SCH) events.", !GetPackageHoldEventLogs(order.PK).Any(x => x.SL_SE_NKEvent == AutoEvents.Held.Code && x.SL_SE_NKEvent == AutoEvents.ClearedHold.Code));
		}

		#endregion

		#region TestPackageAuditEvents_PackageHeld_FailedReAudit

		public void TestPackageAuditEvents_PackageHeld_FailedReAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReference1 = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Audit Failed", package.KP_PackageID, data.Part2.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			var expectedReference2 = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Audit Failed", package.KP_PackageID, data.Part1.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				LogPackageHoldEventForTesting(package, order, data.Part1.OP_PartNum, "Audit Failed", ZDateTime.Now);
				Factory.Save();

				var auditLines = new WhsPackageProductInfo[]
				{
				AddPackageAuditInfo(data.Part1, 5m),
				AddPackageAuditInfo(data.Part2, 8m)
				};
				PerformPackageContentsAudit(Factory, new WebServiceResponse(), auditLines, package.PK.ToGuid(), staff);

				var eventLogs = GetPackageHoldEventLogs(order.PK);
				AssertEquals("The package must be held.", true, package.KP_IsHeld);
				AssertEquals("There must be 2 event logs for SHL", 2, eventLogs.Length);
				AssertEventLog(eventLogs[0], AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference1, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEventLog(eventLogs[1], AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference2, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			}
		}

		#endregion

		#region TestPackageAuditEvents

		#region TestPackageAuditEvents_PackageHoldCleared_PassedReAuditWithSHLHold

		public void TestPackageAuditEvents_PackageHoldCleared_PassedReAuditWithSHLHold()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReferenceSHL = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Audit Failed", package.KP_PackageID, data.Part1.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			var expectedReferenceSCH = ZString.Format("Package: {0}|DEP={2}|FAC={3}|RES=Audit Passed", package.KP_PackageID, data.Part1.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				LogPackageHoldEventForTesting(package, order, data.Part1.OP_PartNum, "Audit Failed", ZDateTime.Now.AddMinutes(-10));
				Factory.Save();

				var auditLines = new WhsPackageProductInfo[]
				{
					AddPackageAuditInfo(data.Part1, 5m),
					AddPackageAuditInfo(data.Part2, 10m)
				};
				AssertEquals("Precondition: The package must be held.", true, package.KP_IsHeld);
				PerformPackageContentsAudit(Factory, new WebServiceResponse(), auditLines, package.PK.ToGuid(), staff);

				var eventLogs = GetPackageHoldEventLogs(order.PK);
				AssertEquals("There must be 3 event logs, 1 for SHL and 2 for SCH since PkgPackage adds 1 SCH automatically.", 3, eventLogs.Length);
				// these last two logs added almost at same time, so query can return at these any order
				AssertHasOneEventLog(eventLogs, AutoEvents.ClearedHold.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReferenceSCH, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertHasOneEventLog(eventLogs, AutoEvents.ClearedHold.Code, WhsDocketSchema.Constants.TableName, order.PK, "Package is no longer held|RFN=PKG001", staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEventLog(eventLogs[2], AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReferenceSHL, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEquals("The hold on package must be cleared since re-audit doesn't have any variance.", false, package.KP_IsHeld);
			}
		}

		#endregion

		#region TestPackageAuditEvents_SuccessfulAudit

		public void TestPackageAuditEvents_SuccessfulAudit()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 20m, data.Part2, 30m, 30m);
			Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			Factory.Save();

			var expectedReference = ZString.Format("Package: {0}|DEP={2}|FAC={3}|RES=Audit Passed", package.KP_PackageID, data.Part1.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 20m),
				AddPackageAuditInfo(data.Part2, 30m)
			};
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				PerformPackageContentsAudit(Factory, new WebServiceResponse(), auditLines, package.PK.ToGuid(), staff);

				var eventLogs = GetPackageHoldEventLogs(order.PK);
				AssertEquals("There must be 1 event log for the successful audit package.", 1, eventLogs.Length);
				AssertHasOneEventLog(eventLogs, AutoEvents.RecordAudited.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
			}
		}

		#endregion

		void AssertHasOneEventLog(StmALog[] eventLogs, ZString eventCode, ZString expectedTable, ZGuid expectedParentPK, ZString expectedReference, ZString expectedUser, ZString expectedBranch, ZString expectedDepartment)
		{
			AssertNotNull($"Expect to have log SL_Table = {expectedTable} and SL_Parent = {expectedParentPK} and SL_Reference = {expectedReference} and SL_GS_NKUser = {expectedUser} && SL_GB_NKBranch = {expectedBranch} and SL_GE_NKDepartment = {expectedDepartment}.",
				eventLogs.SingleOrDefault(eventLog => eventLog.SL_Table == expectedTable && eventLog.SL_Parent == expectedParentPK && eventLog.SL_Reference == expectedReference && eventLog.SL_GS_NKUser == expectedUser && eventLog.SL_GB_NKBranch == expectedBranch && eventLog.SL_SE_NKEvent == eventCode && eventLog.SL_GE_NKDepartment == expectedDepartment));
		}

		#endregion

		#region TestPackageAuditEvents_PackageHoldCleared_PassedReAuditWithNoSHLHold

		public void TestPackageAuditEvents_PackageHoldCleared_PassedReAuditWithNoSHLHold()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReference = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Audit Failed", package.KP_PackageID, data.Part1.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			var otherReference = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Any other reason", package.KP_PackageID, data.Part2.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				LogPackageHoldEventForTesting(package, order, data.Part1.OP_PartNum, "Audit Failed", ZDateTime.Now.AddMinutes(-10));
				LogPackageHoldEventForTesting(package, order, data.Part2.OP_PartNum, "Any other reason", ZDateTime.Now.AddMinutes(-1));
				Factory.Save();

				var auditLines = new WhsPackageProductInfo[]
				{
					AddPackageAuditInfo(data.Part1, 5m),
					AddPackageAuditInfo(data.Part2, 10m)
				};
				AssertEquals("Precondition: The package must be held.", true, package.KP_IsHeld);
				PerformPackageContentsAudit(Factory, new WebServiceResponse(), auditLines, package.PK.ToGuid(), staff);

				var eventLogs = GetPackageHoldEventLogs(order.PK).Where(x => x.SL_SE_NKEvent == AutoEvents.Held.Code || x.SL_SE_NKEvent == AutoEvents.ClearedHold.Code).ToArray();
				AssertEquals("There must be 2 event logs for SHL/SCH since we can't clear a hold on package for reason different than Audit Failed.", 2, eventLogs.Length);
				AssertEquals("The most recent SHL event doesn't match the last added.", otherReference, eventLogs[0].SL_Reference);
				AssertEventLog(eventLogs[1], AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEquals("The hold on package must remain since last SHL event wasn't logged by an audit fail.", true, package.KP_IsHeld);
			}
		}

		#endregion

		#region TestPackageAuditEvents_AuditReleasedPackage

		public void TestPackageAuditEvents_AuditReleasedPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var result = new WebServiceResponse();
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			Factory.Save();

			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			var auditLines = new WhsPackageProductInfo[]
			{
				AddPackageAuditInfo(data.Part1, 5m),
				AddPackageAuditInfo(data.Part2, 12m)
			};
			PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);
			AssertEquals("The webservice must return an error.", ErrorTypes.BusinessValidationError, result.Error);
			AssertEquals("The webservice must show an error message related to package hold.", PackageAuditManagerStrings.Error_HoldPackage, result.ErrorMessage);

			var audit = AssertWhsPackageAuditIsRegistered(Factory, order, package, staff);
			AssertEquals("Should have 1 variance line.", 1, audit.PackageAuditFailureLines.Count);
			AssertVarianceExist(audit, data.Part2, 10m, 12m);

			var eventLogs = GetPackageHoldEventLogs(order.PK);
			AssertEquals("No events must be logged since the hold on package was not updated.", 0, eventLogs.Length);
			AssertEquals("Since the package is released, the hold status must not be changed.", false, package.KP_IsHeld);
		}

		#endregion

		#region TestPackageAuditEvents_ReAuditAnUnheldPackageWithExistingSHLEvent

		public void TestPackageAuditEvents_ReAuditAnUnheldPackageWithExistingSHLEvent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReference = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Any Reason", package.KP_PackageID, data.Part2.OP_PartNum, "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			var result = new WebServiceResponse();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				LogPackageHoldEventForTesting(package, order, data.Part2.OP_PartNum, "Any Reason", ZDateTime.Now);
				package.KP_IsHeld = false;
				Factory.Save();

				var auditLines = new WhsPackageProductInfo[]
				{
					AddPackageAuditInfo(data.Part1, 5m),
					AddPackageAuditInfo(data.Part2, 10m)
				};
				AssertEquals("Precondition: The package must NOT be held.", false, package.KP_IsHeld);

				PerformPackageContentsAudit(Factory, result, auditLines, package.PK.ToGuid(), staff);
				AssertEquals("Precondition: The audit must not contain any errors but found error: " + result.ErrorMessage, ErrorTypes.None, result.Error);

				var eventLogs = GetPackageHoldEventLogs(order.PK).Where(x => x.SL_SE_NKEvent == AutoEvents.Held.Code || x.SL_SE_NKEvent == AutoEvents.ClearedHold.Code).ToArray();
				AssertEquals("There must be 2 event logs, 1 for SHL and 1 for SCH which is logged by PkgPackage.", 2, eventLogs.Length);
				AssertEventLog(eventLogs[0], AutoEvents.ClearedHold.Code, WhsDocketSchema.Constants.TableName, order.PK, "Package is no longer held|RFN=PKG001", staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEventLog(eventLogs[1], AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEquals("The package must remain unheld.", false, package.KP_IsHeld);
			}
		}

		#endregion

		#region TestPackageAuditEvents_AuditPackageWithNoProducts

		public void TestPackageAuditEvents_AuditPackageWithNoProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("ADT", "Auditor");
			var order = AddOrderWithLinesAndInventory(Factory, data.Org1, data.Whs1, data.Part1, 5m, data.Part2, 10m, 100m);
			var pick = Helper.CreatePickNew(order);
			var package = AddPackageToOrder(order, "PKG001", "PLT");
			var expectedReference = ZString.Format("Package: {0}, Product: {1}|DEP={2}|FAC={3}|RES=Audit Failed", package.KP_PackageID, "MULTIPLE", "Warehouse", data.Whs1.WW_WarehouseNameMultilingual);
			package.PackedItems.RemoveAll();
			package.PackedItemDivots.DeleteAll();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				Factory.Save();

				var auditLines = new WhsPackageProductInfo[]
				{
					AddPackageAuditInfo(data.Part1, 5m),
					AddPackageAuditInfo(data.Part2, 10m)
				};
				AssertEquals("Precondition: The package must NOT be held.", false, package.KP_IsHeld);

				PerformPackageContentsAudit(Factory, new WebServiceResponse(), auditLines, package.PK.ToGuid(), staff);

				var eventLogs = GetPackageHoldEventLogs(order.PK);
				AssertEquals("There must be 1 event log for SHL since the audit failed.", 1, eventLogs.Length);
				AssertEventLog(eventLogs.Single(), AutoEvents.Held.Code, WhsDocketSchema.Constants.TableName, order.PK, expectedReference, staff.GS_Code, Env.CurrentBranch.Code, Env.CurrentDepartment.Code);
				AssertEquals("The package must be held.", true, package.KP_IsHeld);
			}
		}

		#endregion

		#endregion

		#region TestPackageAuditEvents_SupportMethods

		void AssertEventLog(StmALog eventLog, ZString eventCode, ZString expectedTable, ZGuid expectedParentPK, ZString expectedReference, ZString expectedUser, ZString expectedBranch, ZString expectedDepartment)
		{
			CombineAssertions("Errors found when verifying Event Log.", () =>
			{
				AssertEquals("The log's table is incorrect.", expectedTable, eventLog.SL_Table);
				AssertEquals("The log's parent PK is incorrect.", expectedParentPK, eventLog.SL_Parent);
				AssertEquals("The log's reference is incorrect.", expectedReference, eventLog.SL_Reference);
				AssertEquals("The log's user code is incorrect.", expectedUser, eventLog.SL_GS_NKUser);
				AssertEquals("The log's branch is incorrect.", expectedBranch, eventLog.SL_GB_NKBranch);
				AssertEquals("The log's department is incorrect.", expectedDepartment, eventLog.SL_GE_NKDepartment);
			});
		}

		StmALog[] GetPackageHoldEventLogs(ZGuid expectedParentPK)
		{
			var eventCode = new string[] { AutoEvents.Held.Code, AutoEvents.ClearedHold.Code, AutoEvents.RecordAudited.Code };
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, expectedParentPK);
			query.OrderBy = StmALogSchema.Constants.SL_EventTime + OrderByClause.Descending;

			return Factory.Load<StmALog>(query);
		}

		void LogPackageHoldEventForTesting(PkgPackage package, WhsDocket order, ZString productCode, ZString reason, ZDateTime eventTime)
		{
			order.Logs.AddNew(AutoEvents.Held, ZString.Format("Package: {0}, Product: {1}", package.KP_PackageID, productCode), eventTime.ToOffset(), new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>("DEP", "Warehouse"),
				new KeyValuePair<string, string>("RES", reason),
				new KeyValuePair<string, string>("FAC", order.Warehouse.WW_WarehouseNameMultilingual)
			});
			package.KP_IsHeld = true;
		}

		#endregion

		#region Implementations

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion
	}
}
