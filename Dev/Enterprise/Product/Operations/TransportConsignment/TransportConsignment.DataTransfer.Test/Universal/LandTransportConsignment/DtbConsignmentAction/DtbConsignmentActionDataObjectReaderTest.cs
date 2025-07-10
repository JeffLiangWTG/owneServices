using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentActionDataObjectReaderTest : OrganizationAddressTestHelper
	{
		#region TestBasicFieldMappings
		public void TestBasicFieldMappings()
		{
			var now = new ZDateTime(ZDateTime.Now.Year, 1, 2);
			var nowDTO = new ZDateTimeOffset(now);
			var actionDataObject = new Confirmation();

			actionDataObject.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			actionDataObject.ActualDate = now;
			actionDataObject.EstimatedDate = now.AddDays(1);
			actionDataObject.RequiredFromDate = now.AddDays(2);
			actionDataObject.RequiredToDate = now.AddDays(3);
			actionDataObject.SlotDate = now.AddDays(4);
			actionDataObject.ReceivedBy = "Test";
			actionDataObject.Reference = "CONFREF";
			actionDataObject.DateDescription = ActionTypes.Codes.PickUp;
			actionDataObject.SetPackingLinkCollection(() => new List<PackingLink>()
			{
				new PackingLink() { PackingLineLink = 1, PackedQuantity = 2 },
				new PackingLink() { PackingLineLink = 2, PackedQuantity = 4 },
				new PackingLink() { PackingLineLink = 1, PackedQuantity = 3, IsContainer = true },
			});
			AssertAction(actionDataObject, nowDTO);
			AssertAction(actionDataObject, nowDTO);
		}

		void AssertAction(Confirmation actionDataObject, ZDateTimeOffset now)
		{
			var reader = GetNewReader(actionDataObject, Logger, Factory);
			var action = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("action.LTA_Actual", now, action.LTA_ActualTime);
				AssertEquals("action.LTA_Estimated", now.AddDays(1), action.LTA_EstimatedTime);
				AssertEquals("action.LTA_ReceivedBy", "Test", action.LTA_SignedBy);
				AssertEquals("action.LTA_ReferenceNum", "CONFREF", action.LTA_ReferenceNumber);
				AssertEquals("action.LTA_ActionType", "PIC", action.LTA_ActionType);
				AssertEquals("action.LTA_RequiredFrom", now.AddDays(2), action.LTA_RequiredFrom);
				AssertEquals("action.LTA_RequiredTo", now.AddDays(3), action.LTA_RequiredTo);
				AssertEquals("action.LTA_SlotDateTime", now.AddDays(4), action.LTA_Slot);

				AssertEquals("Action package divot count", 3, action.PackageDivots.Count);
				var package1Divot = action.PackageDivots.FirstOrDefault(divot => divot.LTP_KP_Package == package1.PK);
				var package2Divot = action.PackageDivots.FirstOrDefault(divot => divot.LTP_KP_Package == package2.PK);
				var package3Divot = action.PackageDivots.FirstOrDefault(divot => divot.LTP_KP_Package == package4.PK);
				AssertNotNull(package1Divot);
				AssertEquals("Action package divot quantity", 2, package1Divot.LTP_PackageQuantity);
				AssertEquals("Action package divot quantity", 4, package2Divot.LTP_PackageQuantity);
				AssertEquals("Action package divot quantity", 3, package3Divot.LTP_PackageQuantity);
				AssertNotNull(package2Divot);
				AssertNotNull(package3Divot);
			});
		}

		#endregion
		#region TestRequiredByToFallbackToEstimatedDateIfEmpty
		public void TestRequiredByToFallbackToEstimatedDateIfEmpty()
		{
			var year = ZDateTime.Now.Year;
			var actionDataObject = new Confirmation();
			actionDataObject.EstimatedDate = new ZDateTime(year, 1, 2);
			var reader = GetNewReader(actionDataObject, Logger, Factory);
			var action = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("action.LTA_EstimatedTime", new ZDateTimeOffset(new ZDateTime(year, 1, 2)), action.LTA_EstimatedTime);
				AssertEquals("action.LTA_RequiredFrom", new ZDateTimeOffset(new ZDateTime(year, 1, 2)), action.LTA_RequiredFrom);
			});
		}

		#endregion
		#region Helper

		DtbConsignmentActionDataObjectReader GetNewReader(Confirmation actionDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new DtbConsignmentActionDataObjectReader(actionDataObject, logger, factory, packageLinksDictionary, containerLinksDictionary);
		}

		protected override void SetUp()
		{
			package1 = Factory.BOFactory.New<Packing.Business.PkgPackage>();
			package2 = Factory.BOFactory.New<Packing.Business.PkgPackage>();
			package3 = Factory.BOFactory.New<Packing.Business.PkgPackage>();
			package4 = Factory.BOFactory.New<Packing.Business.PkgPackage>();

			packageLinksDictionary = new Dictionary<ZInt, Packing.Business.PkgPackage>()
			{
				{ 1, package1 },
				{ 2, package2 },
				{ 3, package3 },
			};

			containerLinksDictionary = new Dictionary<ZInt, Packing.Business.PkgPackage>()
			{
				{ 1, package4 },
			};

			base.SetUp();
		}

		Packing.Business.PkgPackage package1;
		Packing.Business.PkgPackage package2;
		Packing.Business.PkgPackage package3;
		Packing.Business.PkgPackage package4;
		Dictionary<ZInt, Packing.Business.PkgPackage> packageLinksDictionary;
		Dictionary<ZInt, Packing.Business.PkgPackage> containerLinksDictionary;
		#endregion
	}
}
