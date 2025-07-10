using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSLoadListConsol))]
	public class CFSLoadListConsolBusinessObjectTest : CFSBusinessObjectTestCase
	{
		#region Metadata

		protected override Type ExpectedMetadataType
		{
			get
			{
				return typeof(Metadata.Business.CommonConsol);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var consol = factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_IsLinked = true;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_JX = CreateNewImportSailing(factory).PK;

			AssertEquals("CONSOL - SEA", Constants.TransportModes.Sea, consol.JK_TransportMode);

			return base.GetNewBusinessObjectForDeleteTest(factory);
		}

		public virtual void TestGetNewValidation()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			AssertEquals("Type of Validation", typeof(CFSLoadListConsolValidation), consol.Validation.GetType());
		}

		#endregion

		public void TestPopulateJK_UniqueConsignRefIfNeeded()
		{
			CommonConsol plainConsol = Factory.New<CommonConsol>();
			plainConsol.JK_IsForwarding = true;
			plainConsol.JK_IsCFS = false;
			Factory.Save();
			Assert("should start with forwarding number", plainConsol.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainPrefix));

			plainConsol.JK_IsCFS = true;
			Factory.Save();
			Assert("should start with forwarding number", plainConsol.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainPrefix));

			CFSLoadListConsol cFSOnlyConsol = Factory.New<CFSLoadListConsol>();
			AssertEquals("should be CFS only", false, cFSOnlyConsol.JK_IsForwarding);
			AssertEquals("should be CFS only", true, cFSOnlyConsol.JK_IsCFS);
			Factory.Save();
			Assert("should start with CFS number", cFSOnlyConsol.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainCFSPrefix));

			CFSLoadListConsol bothConsol = Factory.New<CFSLoadListConsol>();
			bothConsol.JK_IsForwarding = true;
			AssertEquals("should be both forwarding and CFS", true, bothConsol.JK_IsForwarding);
			AssertEquals("should be both forwarding and CFS", true, bothConsol.JK_IsCFS);
			Factory.Save();
			Assert("should start with forwarding number", bothConsol.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainPrefix));

			CFSLoadListConsol consolThatChanges = Factory.New<CFSLoadListConsol>();
			AssertEquals("should be CFS only", false, consolThatChanges.JK_IsForwarding);
			AssertEquals("should be CFS only", true, consolThatChanges.JK_IsCFS);
			Factory.Save();
			string oldID = consolThatChanges.JK_UniqueConsignRef;
			Assert("should start with CFS number", consolThatChanges.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainCFSPrefix));
			AssertEquals("precondition for following event creation assert", 0,
				consolThatChanges.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Changed job number from")).Length);
			consolThatChanges.JK_IsForwarding = true;
			AssertEquals("should be both", true, consolThatChanges.JK_IsForwarding);
			AssertEquals("should be both", true, consolThatChanges.JK_IsCFS);
			Factory.Save();
			string newID = consolThatChanges.JK_UniqueConsignRef;
			Assert("should start with forwarding number", consolThatChanges.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainPrefix));
			AssertEquals("expecting event to have been created", 1,
				consolThatChanges.Logs.Find(new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Equal, "Changed job number from " + oldID + " to " + newID)).Length);

			CFSLoadListConsol consolThatCannotChange = Factory.New<CFSLoadListConsol>();
			AssertEquals("should be CFS only", false, consolThatCannotChange.JK_IsForwarding);
			AssertEquals("should be CFS only", true, consolThatCannotChange.JK_IsCFS);
			JobHeader attachedJobHeader = new JobHeader.Loader(consolThatCannotChange).TryLoadOrCreateWithMutex();
			attachedJobHeader.Dispose();
			attachedJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			attachedJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			Assert("should start with CFS number", consolThatCannotChange.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainCFSPrefix));
			consolThatChanges.JK_IsForwarding = true;
			AssertEquals("should be both", false, consolThatCannotChange.JK_IsForwarding);
			AssertEquals("should be both", true, consolThatCannotChange.JK_IsCFS);
			Factory.Save();
			Assert("should start with CFS number", consolThatCannotChange.JK_UniqueConsignRef.StartsWith(NumberFountains.JobConsolFountainCFSPrefix));
		}

		#region Test Effect of Packing and Unpacking on Services

		public void TestPackingPackLinesDoesNotDuplicateServices()
		{
			var consol = Factory.New<CFSLoadListConsol>();

			var container = consol.Containers.AddNew();
			var containerFumigation = container.Services.AddNew();
			containerFumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			var shipment = consol.Shipments.AddNew();
			var shipmentFumigation = shipment.DocsAndCartage.Services.AddNew();
			shipmentFumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;
			packLine.JL_Description = "Stuff to ship";
			AssertEquals("Precondition - number of fumigation rows on shipment should be 1", 1, GetNumberOfFumigations(shipment.DocsAndCartage.Services));

			container.AddPackLines(new BusinessObject[] { packLine });
			AssertEquals("Number of fumigation rows on shipment should be 2", 2, GetNumberOfFumigations(shipment.DocsAndCartage.Services));
			Factory.Save();
			shipment.DocsAndCartage.Services.Load();
			AssertEquals("Number of fumigation rows on shipment should be 2", 2, GetNumberOfFumigations(shipment.DocsAndCartage.Services));
		}

		public void TestUnpackingPackLinesRemovesServicesFromShipment()
		{
			var consol = Factory.New<CFSLoadListConsol>();

			var container = consol.Containers.AddNew();
			var containerFumigation = container.Services.AddNew();
			containerFumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;

			var shipment = consol.Shipments.AddNew();
			var shipmentFumigation = shipment.DocsAndCartage.Services.AddNew();
			shipmentFumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 12;

			container.AddPackLines(new BusinessObject[] { packLine });
			Factory.Save();
			shipment.DocsAndCartage.Services.Load();
			AssertEquals("Precondition - number of fumigation rows on shipment should be 2", 2, GetNumberOfFumigations(shipment.DocsAndCartage.Services));

			container.RemovePackLines(new BusinessObject[] { packLine });
			Factory.Save();
			shipment.DocsAndCartage.Services.Load();
			AssertEquals("There should be a fumigation row on the shipment", 1, GetNumberOfFumigations(shipment.DocsAndCartage.Services));
		}

		int GetNumberOfFumigations(JobServiceDependentCollection services)
		{
			int result = 0;
			foreach (JobService service in services)
			{
				if (service.ES_ServiceCode == Constants.FreightServiceType.Codes.Fumigation)
				{
					result++;
				}
			}

			return result;
		}

		#endregion
	}
}
