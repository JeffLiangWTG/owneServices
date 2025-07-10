using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(ShipmentDeclarationCollection))]
	[HttpContextEnabledTest]
	sealed class ShipmentDeclarationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentDeclarationCollection>
	{
		#region TestLoad

		public void TestLoad()
		{
			var testHelper = new ZWebTestHelper(Factory);
			var testOrg = testHelper.TestOrg;
			testOrg.OH_Code = "XXXYYYZZZ";
			testOrg.OH_IsForwarder = true;
			OrgContact testContact = testHelper.TestContact;
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			DateTime eTA = new DateTime(2009, 01, 01);

			for (int i = 0; i < 5; i++)
			{
				var shipment = Factory.NewWithValidTestData<TrackingShipment>();
				shipment.JS_E_ARV = eTA.AddDays(i);
			}

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_OH_Forwarder = testHelper.TestOrg.PK;
			declaration1.JE_DateAtFinalDestination = eTA.AddDays(0);

			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration2.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration2.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration2.JE_DateAtFinalDestination = eTA.AddDays(1);

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration3.JE_OH_Supplier = testHelper.TestOrg.PK;
			declaration3.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration3.JE_DateAtFinalDestination = eTA.AddDays(2);

			var declaration4 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration4.JE_OH_Importer = testHelper.TestOrg.PK;
			declaration4.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration4.JE_DateAtFinalDestination = eTA.AddDays(3);

			var declaration5 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration5.JE_OH_Importer = testHelper.TestOrg.PK;
			declaration5.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration5.JE_DateAtFinalDestination = eTA.AddDays(4);

			var declaration6 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration6.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration6.JE_DateAtFinalDestination = eTA.AddDays(5);
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_JobNum = "hello";
			job.JH_ParentID = declaration6.PK;
			job.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			declaration6.Job.JH_OA_LocalChargesAddr = testHelper.TestOrg.MainAddress.PK;

			Factory.Save();

			WebEnv.AppInstance.SiteUser.Login(testContact.Header.OH_Code, testContact.OC_Email, testContact.PasswordForTesting);

			var shipmentFilter = new ZQuery() { MaximumRows = 4 };
			var declarationFilter = OrgRestrictionFilterFactory.Instance.GetFilter<TrackingDeclaration>();

			Collection.Load(shipmentFilter, declarationFilter, ShipmentDeclarationSchema.ETA.Name, ListSortDirection.Descending);

			AssertEquals("Expected number of the collection items should not exceed maximum rows count", 4, Collection.Count);

			bool shipmentExists = false;
			bool declarationExists = false;
			foreach (BusinessObject bizO in Collection)
			{
				if (bizO is TrackingShipment)
				{
					shipmentExists = true;
				}

				if (bizO is TrackingDeclaration)
				{
					declarationExists = true;
				}
			}

			Assert("Resulting set should contain both shipments and declarations", shipmentExists && declarationExists);

			BusinessObject[] array = Collection.ToArray();
			IShipmentDeclaration firstShipDec = (IShipmentDeclaration)array[0];
			IShipmentDeclaration lastShipDec = (IShipmentDeclaration)array[array.Length - 1];

			Assert("Sorting should be applied", firstShipDec.ETA > lastShipDec.ETA);

			shipmentFilter.MaximumRows = 20;
			Collection.Load(shipmentFilter, declarationFilter, ShipmentDeclarationSchema.ETA.Name, ListSortDirection.Descending);
			var pks = Collection.OfType<TrackingDeclaration>().Select(trackDec => trackDec.Declaration.PK);

			AssertEquals(3, pks.Count());
			AssertCollectionContains(declaration3.PK, pks);
			AssertCollectionContains(declaration4.PK, pks);
			AssertCollectionContains(declaration6.PK, pks);
		}

		#endregion

		#region Implementation

		protected override ShipmentDeclarationCollection GetCollectionToTest()
		{
			return new ShipmentDeclarationCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TrackingShipment>();
		}

		#endregion
	}
}
