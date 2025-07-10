using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class InternalCartageManagerTest : TestCaseWithFactory
	{
		public void TestParentNotes()
		{
			AssertCollectionContains("Should have note", note, CartageExporter.ParentNotes.GetAllNotes());
		}

		public void TestParentLogs()
		{
			AssertCollectionContains("Should have log", log, CartageExporter.ParentLogs.GetAllLogs());
		}

		public void TestFactory()
		{
			AssertEquals("Should have same factory", dummyCartageParent.Factory, CartageExporter.ParentFactory);
		}

		public void TestDescription()
		{
			AssertEquals("Should have description", cartageType.Description, CartageExporter.Description);
		}

		public void TestGetCartageForExport_Existing()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			var (cartage, cartageType, _, _) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			var exportManager = (ICartageExporter)new InternalCartageManager(cartageType);
			var exportedCartage = exportManager.GetCartageForExport(buffer);
			AssertEquals("Should have cartage", cartage, exportedCartage);
			Assert("Should have no errors", !buffer.HasErrors);
		}

		public void TestGetCartageForExport_NoSecurity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";
			staff.GS_IsSystemAccount = ZBool.True;
			var createNewLocalCartageCheckPoint = Env.Security.LocalCartageJobTypeNew;
			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = createNewLocalCartageCheckPoint.Code;
			securityRecord.GU_ItemGUID = createNewLocalCartageCheckPoint.ItemGuid;
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
			Factory.Save();
			var orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			cartageType.SetCartageOrganisation(orgProxy);
			var buffer = new NotificationBuffer();
			var cartage = CartageExporter.GetCartageForExport(buffer);
			AssertNotNull("Should bypass security and create a cartage", cartage);
			Assert("Should have no errors.", !buffer.HasErrors);
			Assert("Should not be able to save factory.", !((IBusinessObjectFactoryInternals)cartage.Factory).CanSave);
		}

		public void TestGetCartageForExport_JobTypeEmpty()
		{
			var buffer = new NotificationBuffer();
			var cartageType = dummyCartageParent.CartageTypes.First();
			var dct = (DummyCartageType)cartageType;
			dct.SetCartageJobType("");
			cartageType.LocalTransportProviderAddress.Header.OH_IsLocalTransport = true;
			var internalCartageManager = new InternalCartageManager(cartageType);
			var cartageExporter = (ICartageExporter)internalCartageManager;
			var cartage = cartageExporter.GetCartageForExport(buffer);
			Assert("should have errors.", buffer.HasErrors);
			string expectedString = "Error: Invalid Port Transport Job Type: ''.\r\n";
			AssertEquals("As String", expectedString, buffer.AsString);
			AssertNull("cartage should be null", cartage);
		}

		public void TestJobNumber()
		{
			AssertEquals("Should have Local Transport JobNumber", dummyCartageParent.UniqueConsignmentID, CartageExporter.ParentJobNumber);
		}

		public void TestSendTo()
		{
			AssertEquals("Should have Local Transport Provider", cartageType.LocalTransportProviderAddress.Header, CartageExporter.SendTo);
		}

		public void TestSendToDescription()
		{
			AssertEquals("Should have Local Transport Provider", "Dummy Local Transport Company", CartageExporter.SendToDescription);
		}

		public void TestCannotDeactivateMessage()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			CommonCartage cartage = Factory.New<CommonCartage>();
			JobHeader job = new JobHeader.Loader(cartage).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = cartage.PK;
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			Factory.Save();
			InternalCartageManager cartageManager = new InternalCartageManager(cartageType);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			NotificationBuffer buffer = new NotificationBuffer();
			BusinessObject accHotCheque = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IAccHotCheque)));
			accHotCheque[AccHotChequeSchema.Constants.AQ_JH] = job.PK;
			Factory.Save();
			Assert(!cartageManager.DeactivateExisting(buffer, cartage, newFactory));
			AssertEquals(@"Error: The existing Dummy Port Transport (with parent job number Dum1001) has been found. It cannot be deactivated because Job(s) attached cannot be deactivated. Reasons being: 
This record cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (T00001000) in the company EDI.
", buffer.AsString);
			buffer.Clear();
			AccTransactionHeader invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_JH = job.PK;
			Factory.Save();
			Assert(!cartageManager.DeactivateExisting(buffer, cartage, newFactory));
			AssertEquals(@"Error: The existing Dummy Port Transport (with parent job number Dum1001) has been found. It cannot be deactivated because Job(s) attached cannot be deactivated. Reasons being: 
This record cannot be deactivated.
Accounting Transaction(s) have been saved against this Invoicing Job Header (T00001000) in the company EDI.
", buffer.AsString);
			buffer.Clear();
			AccTransactionLines invoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine.AL_JH = job.PK;
			invoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			Factory.Save();
			Assert(!cartageManager.DeactivateExisting(buffer, cartage, newFactory));
			AssertEquals(@"Error: The existing Dummy Port Transport (with parent job number Dum1001) has been found. It cannot be deactivated because Job(s) attached cannot be deactivated. Reasons being: 
This record cannot be deactivated.
Accounting Transaction Line(s) have been saved against this Invoicing Job Header (T00001000) in the company EDI.
", buffer.AsString);
			buffer.Clear();
			JobCharge charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.JR_JH = job.PK;
			charge1.JR_OSCostAmt = 10m;
			charge1.JR_OSCostExRate = 1m;
			Factory.Save();
			Assert(!cartageManager.DeactivateExisting(buffer, cartage, newFactory));
			AssertEquals(@"Error: The existing Dummy Port Transport (with parent job number Dum1001) has been found. It cannot be deactivated because Job(s) attached cannot be deactivated. Reasons being: 
This record cannot be deactivated.
Job Invoicing Charge(s) have been saved against this Invoicing Job Header (T00001000) in the company EDI.
", buffer.AsString);
		}

		public void TestDeactivateEnsureJobHeaderLoadedAsJob()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			CommonCartage cartage = Factory.New<CommonCartage>();
			JobHeader job = new JobHeader.Loader(cartage).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = cartage.PK;
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			Factory.Save();
			InternalCartageManager cartageManager = new InternalCartageManager(cartageType);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartage cartageInNewFactory = newFactory.Load<CommonCartage>(cartage.PK);
			JobHeader jobInNewFactory = cartageInNewFactory.Job;
			Assert("calling deactivate with user response No - do not deactivate, should return false", !cartageManager.DeactivateExisting(buffer, cartage, newFactory)); //Loads JobHeaders
			AssertNoExceptionThrown("Job Header should be loaded - hence setting IsCancelled is okay", () =>
			{
				cartageInNewFactory.IsCancelled = true;
			});
		}

		public void TestDeactivateAlsoDeactivatesJobHeader()
		{
			var (cartage, cartageType, _, _) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			JobHeader job = new JobHeader.Loader(cartage).TryCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = cartage.PK;
			job.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
			Factory.Save();
			var notify = new TestNotify();
			notify.ResponseToDialogs = true;
			var cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(notify, null, delegate
			{
				MethodToCall();
			});
			AssertEquals("Form should have cartage as the cartage to be deactivated", cartage.PK, cartageManager.cartageForm.CartageToBeDeactivated.PK);
			AssertEquals("Should return yes from fire of save. After this save, old cartage should now be deactivated", ContinueWithSave.Yes, cartageManager.cartageForm.FireSaveButton());
			AssertEquals("Should have deactivated existing cartage", true, cartageManager.cartageForm.CartageToBeDeactivated.IsCancelled);
			AssertEquals("Should also have deactivated the job on existing cartage", true, cartageManager.cartageForm.CartageToBeDeactivated.Job.IsCancelled);
			cartageManager.cartageForm.Close();
		}

		public void TestCreateAndShowCartage()
		{
			OrgHeader orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			foreach (GlbCompany company in companies)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			Factory.Save();
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			cartageType.SetCartageOrganisation(orgProxy);
			NotificationBuffer buffer = new NotificationBuffer();
			InternalCartageManager cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			Assert(!UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(false, OuterPacksTotalsDiffer(cartageManager.Cartage));
			Assert(!buffer.HasErrors);
			AssertNotNull(cartageManager.cartageForm);
			Assert(!wasMethodCalled);
			cartageManager.cartageForm.Close();
			Assert(wasMethodCalled);
		}

		public void TestCreateAndShowCartage_WithRegistrySetting_WithYesSelected()
		{
			var orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			var companies = new GlbCompanyCollection(Factory);
			foreach (var company in companies)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			Factory.Save();
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			cartageType.SetCartageOrganisation(orgProxy);
			var buffer = new NotificationBuffer();
			var cartageManager = new InternalCartageManager(cartageType);
			TransportRegistry.Instance.ShowPortTransportJobOnCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals("Port Transport job has been created would you like to view it?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, OuterPacksTotalsDiffer(cartageManager.Cartage));
			Assert(!buffer.HasErrors);
			AssertNotNull(cartageManager.cartageForm);
			Assert(!wasMethodCalled);
			Assert(!cartageManager.Cartage.IsInDatabase);
			cartageManager.cartageForm.Close();
			Assert(wasMethodCalled);
		}

		public void TestCreateAndShowCartage_WithRegistrySetting_WithNoSelected()
		{
			var orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			var companies = new GlbCompanyCollection(Factory);
			foreach (var company in companies)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			Factory.Save();
			var dummyParent = new DummyCartageParent(Factory);
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			cartageType.SetCartageOrganisation(orgProxy);
			var buffer = new NotificationBuffer();
			var cartageManager = new InternalCartageManager(cartageType);
			TransportRegistry.Instance.ShowPortTransportJobOnCreation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals("Port Transport job has been created would you like to view it?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, OuterPacksTotalsDiffer(cartageManager.Cartage));
			Assert(!buffer.HasErrors);
			AssertNull(cartageManager.cartageForm);
			Assert(wasMethodCalled);
			Assert(cartageManager.Cartage.IsInDatabase);
		}

		public void TestCartageToBeDeactivatedGetsDeactivatedValueInForm()
		{
			var (cartage, cartageType, _, _) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			var notify = new TestNotify();
			notify.ResponseToDialogs = true;
			var cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(notify, null, delegate
			{
				MethodToCall();
			});
			AssertEquals("Should deactivate the existing cartage", cartage.PK, cartageManager.cartageForm.CartageToBeDeactivated.PK);
			cartageManager.cartageForm.Close();
		}

		public void TestCheckCartageAddressRequirements_NoErrors()
		{
			OrgHeader orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			foreach (GlbCompany company in companies)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			Factory.Save();
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			Dictionary<ZString, JobDocAddress> addresses = new Dictionary<ZString, JobDocAddress>();
			JobDocAddress ctoAddress = Factory.New<JobDocAddress>();
			ctoAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			JobDocAddress cneAddress = Factory.New<JobDocAddress>();
			cneAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			addresses.Add(LocalCartageJobOrgTypeList.Codes.CTO, ctoAddress);
			addresses.Add(LocalCartageJobOrgTypeList.Codes.CNE, cneAddress);
			CartageType ct = new DummyCartageType(dummyParent, Constants.CartageJobType.NEW_FCLImportToCNE, addresses);
			dummyParent.SetCartageType(ct);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			AssertEquals("pre", ct, cartageType);
			cartageType.SetCartageOrganisation(orgProxy);
			NotificationBuffer buffer = new NotificationBuffer();
			InternalCartageManager cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			Assert(!buffer.HasErrors);
			cartageManager.cartageForm.Close();
			Assert(wasMethodCalled);
		}

		public void TestCartageCreated()
		{
			OrgHeader orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			foreach (GlbCompany company in companies)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			Factory.Save();
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			Dictionary<ZString, JobDocAddress> addresses = new Dictionary<ZString, JobDocAddress>();
			JobDocAddress ctoAddress = Factory.New<JobDocAddress>();
			ctoAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			JobDocAddress cneAddress = Factory.New<JobDocAddress>();
			cneAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			addresses.Add(LocalCartageJobOrgTypeList.Codes.CTO, ctoAddress);
			addresses.Add(LocalCartageJobOrgTypeList.Codes.CNE, cneAddress);
			CartageType ct = new DummyCartageType(dummyParent, Constants.CartageJobType.NEW_FCLImportToCNE, addresses);
			dummyParent.SetCartageType(ct);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			AssertEquals("pre", ct, cartageType);
			AssertEquals(0, cartageType.CartageAdvisedCount);
			cartageType.SetCartageOrganisation(orgProxy);
			NotificationBuffer buffer = new NotificationBuffer();
			InternalCartageManager cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			AssertEquals(1, cartageType.CartageAdvisedCount);
			AssertEquals(cartageManager.Cartage.Factory, cartageType.LastCartageAdvisedFactory);
			Assert(!buffer.HasErrors);
			Assert(!dummyParent.CartageCreatedCalled);
			cartageManager.Cartage.Factory.Save();
			Assert(dummyParent.CartageCreatedCalled);
			dummyParent.CartageCreatedCalled = false;
			cartageManager.Cartage.Factory.Save();
			Assert("Was already called", !dummyParent.CartageCreatedCalled);
			cartageManager.cartageForm.Close();
		}

		public void TestCheckCartageAddressRequirements_NoCNE()
		{
			OrgHeader orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			orgProxy.OH_IsLocalTransport = true;
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);
			foreach (GlbCompany company in companies)
			{
				company.GC_OH_OrgProxy = orgProxy.PK;
			}

			Factory.Save();
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			Dictionary<ZString, JobDocAddress> addresses = new Dictionary<ZString, JobDocAddress>();
			JobDocAddress ctoAddress = Factory.New<JobDocAddress>();
			ctoAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			JobDocAddress cydAddress = Factory.New<JobDocAddress>();
			cydAddress.E2_OA_Address = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			addresses.Add(LocalCartageJobOrgTypeList.Codes.CTO, ctoAddress);
			addresses.Add(LocalCartageJobOrgTypeList.Codes.CYD, cydAddress);
			CartageType ct = new DummyCartageType(dummyParent, Constants.CartageJobType.NEW_FCLImportToCNE, addresses);
			dummyParent.SetCartageType(ct);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			AssertEquals("pre", ct, cartageType);
			cartageType.SetCartageOrganisation(orgProxy);
			NotificationBuffer buffer = new NotificationBuffer();
			InternalCartageManager cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			Assert(buffer.HasErrors);
			AssertEquals(1, buffer.Events.Count());
			AssertEquals("Error: The Address 'CNE' has not been entered. It is mandatory for a Port Transport of type 'FCL IMPORT TO CNE'.", buffer.Events[0].Message);
		}

		bool OuterPacksTotalsDiffer(CommonCartage cartage)
		{
			return cartage.TotalLooseBookedPackages != cartage.JJ_OuterPacks || ZArchitecture.Core.Utilities.Round(cartage.TotalLooseBookedWeight, 3) != ZArchitecture.Core.Utilities.Round(cartage.JJ_Weight, 3) || ZArchitecture.Core.Utilities.Round(cartage.TotalLooseBookedVolume, 3) != ZArchitecture.Core.Utilities.Round(cartage.JJ_Volume, 3);
		}

		void MethodToCall()
		{
			wasMethodCalled = true;
		}

		bool wasMethodCalled;

		public void TestCreateCartageWithoutNewPermission()
		{
			// setup test user and deny create-new permissions for local cartage
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "TST";
			staff.StaffPlainTextPassword = "TEST";
			var createNewLocalCartageCheckPoint = Env.Security.LocalCartageJobTypeNew;
			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = createNewLocalCartageCheckPoint.Code;
			securityRecord.GU_ItemGUID = createNewLocalCartageCheckPoint.ItemGuid;
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);
			Factory.Save();
			// setup OrgProxy, Cartage parent and cartage type
			var orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompany.OrganisationPK));
			orgProxy.OH_IsLocalTransport = true;
			orgProxy.OH_Code = "O1";
			var dummyParent = new DummyCartageParent(Factory);
			dummyParent.HasChanges = true;
			dummyParent.Notes.AddNew();
			dummyParent.Logs.AddNew();
			var dummyCartageParent = (ICartageParent)dummyParent;
			var cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			cartageType.SetCartageOrganisation(orgProxy);
			// test user with security rights to create new local cartage
			var bufferWithSecurity = new NotificationBuffer();
			var cartageManagerWithSecurity = new InternalCartageManager(cartageType);
			cartageManagerWithSecurity.CreateCartageAndShow(bufferWithSecurity, null, delegate
			{
				MethodToCall();
			});
			AssertEquals("Precondition", true, bufferWithSecurity.HasErrors);
			AssertEquals("Error: Please save this Dummy Cartage Parent before creating a Port Transport Job.", bufferWithSecurity.AsString.Trim());
			AssertNull("Cartage should be not be created.", cartageManagerWithSecurity.Cartage);
			dummyParent.HasChanges = false;
			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				// test user without security rights to create new local cartage
				var buffer = new NotificationBuffer();
				var cartageManager = new InternalCartageManager(cartageType);
				AssertNoExceptionThrown("No exception should be thrown, when there are no permissions to create a new local cartage job.", () => cartageManager.CreateCartageAndShow(buffer, null, delegate
				{
					MethodToCall();
				}));
				AssertEquals("Error: You do not have the appropriate security rights to run this function.\r\n\r\n" + "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:\r\n\r\n" + "Operate -> Port Transport -> Transport Jobs -> New", buffer.AsString.Trim());
				AssertNull("Cartage should be not be created.", cartageManager.Cartage);
			}
		}

		public void TestCreateAndShowWorksEvenIfAlreadyHasCancelledCartage()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			var (cartage, cartageType, cartageParent, orgProxy) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			cartage.IsCancelled = true;
			Factory.Save();
			var cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			});
			cartageManager.Cartage.Factory.Save();
			cartageManager.cartageForm.Close();
			var otherFactory = new BusinessObjectFactory();
			var newCartageQuery = new ZQuery(JobCartageSchema.JJ_ParentID, ((CommonShipment)cartageParent).PK);
			var newCartage = otherFactory.LoadTop1<CommonCartage>(newCartageQuery);
			AssertNotNull("Should have create new cartage", newCartage);
			AssertEquals("CreateCartageAndShow should have called delegate", true, wasMethodCalled);
			Assert("Should have no errors", !buffer.HasErrors);
		}

		public void TestCreateAndShowCartage_ConcurrencyIssues()
		{
			var cartage = Factory.New<CommonCartage>();
			Factory.RefreshEnabled = false;
			var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S1";
			var transportAddress = Factory.NewWithValidTestData<OrgAddress>();
			transportAddress.Header.OH_IsLocalTransport = true;
			transportAddress.Header.OH_Code = "TRANSCOSYD";
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			var cfsAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = transportAddress.PK;
			shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "PSL";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = cfsAddress.PK;
			Factory.Save();
			var cartageParent = (ICartageParent)shipment;
			var cartageType = cartageParent.CartageTypes.ElementAt(1); // delivery
			var cartageManager = new InternalCartageManager(cartageType);
			cartageManager.CreateCartageAndShow(new TestNotificationBuffer(), null, delegate
			{
				MethodToCall();
			});
			cartageManager.Cartage.Factory.Save(); // create cartage for shipment
			cartageManager.cartageForm.Close();
			// first user try to create cartage again
			var buffer = new TestNotificationBuffer();
			var newFactory1 = new BusinessObjectFactory()
			{ RefreshEnabled = false };
			var shipmentInNewFactory1 = newFactory1.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(shipment.PK);
			var cartageManagerInNewFactory1 = new InternalCartageManager(((ICartageParent)shipmentInNewFactory1).CartageTypes.ElementAt(1));
			cartageManagerInNewFactory1.CreateCartageForParentAndSave += delegate
			{
				// second user creates the cartage while first user tries to create cartage for the same cartage parent
				var newFactory2 = new BusinessObjectFactory()
				{ RefreshEnabled = false };
				var shipmentInNewFactory2 = newFactory2.Load<Enterprise.Integration.Forwarding.IForwardingShipment>(shipment.PK);
				var cartageManagerInNewFactory2 = new InternalCartageManager(((ICartageParent)shipmentInNewFactory2).CartageTypes.ElementAt(1));
				cartageManagerInNewFactory2.CreateCartageAndShow(new TestNotificationBuffer(), null, delegate
				{
				});
				cartageManagerInNewFactory2.Cartage.Factory.Save();
				cartageManagerInNewFactory2.cartageForm.Close();
			};
			AssertNoExceptionThrown(() => cartageManagerInNewFactory1.CreateCartageAndShow(buffer, null, delegate
			{
				MethodToCall();
			}));
			AssertEquals("Error: The existing Delivery Port Transport (with parent job number S1) has been overridden by another user. Close and Open this job before attempting to create the Port Transport Job again.\r\n", buffer.AsString);
		}

		public EventHandler CreateCartageForParentAndSave;

		class TestNotificationBuffer : NotificationBuffer
		{
			protected override void QueryUser(IQueryUserEventArgs e)
			{
				base.QueryUser(e);
				var args = (QueryUserYesNoEventArgs)e;
				args.Response = true;
			}
		}

		public void TestViewCartage_LicenceCheckpoint()
		{
			var buffer = new TestNotificationBuffer();
			var cartage = Factory.New<CommonCartage>();
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			Factory.Save();
			using (var cartageForm = (ZForm)manager.ViewCartage(buffer, null))
			{
				Assert("ContainsCheckpoint(Env.Licence.LocalTransport)", cartageForm.LicensedComponentManager.ContainsCheckpoint(Env.Licence.LocalTransport));
			}
		}

		public void TestViewCartage_CartageDeleted()
		{
			var cartage = Factory.New<CommonCartage>();
			var buffer = new TestNotificationBuffer();
			var cartageManager = new InternalCartageManager(cartageType);
			Factory.Save();
			cartage.Delete();
			AssertNoExceptionThrown(() => cartageManager.ViewCartage(buffer, null));
			AssertEquals("Error: There are no Port Transport Jobs with parent job number Dum1001, it has probably been deleted.\r\n", buffer.AsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			note = dummyParent.Notes.AddNew();
			log = dummyParent.Logs.AddNew();
			dummyCartageParent = dummyParent;
			cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			manager = new InternalCartageManager(cartageType);
		}

		InternalCartageManager manager;
		StmNote note;
		StmALog log;
		ICartageParent dummyCartageParent;
		DummyCartageType cartageType;
		ICartageExporter CartageExporter
		{
			get
			{
				return manager;
			}
		}

		class TestNotify : INotifications, INotificationSubscriberQueryUser
		{
			void INotifications.Add(INotification notification)
			{
				Notifications.Add(notification);
			}

			void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
			{
				var queryUserArgs = e as QueryUserYesNoEventArgs;
				if (queryUserArgs != null)
				{
					queryUserArgs.Response = ResponseToDialogs;
				}
			}

			public List<INotification> Notifications = new List<INotification>();
			public bool ResponseToDialogs { get; set; }
		}
	}
}
