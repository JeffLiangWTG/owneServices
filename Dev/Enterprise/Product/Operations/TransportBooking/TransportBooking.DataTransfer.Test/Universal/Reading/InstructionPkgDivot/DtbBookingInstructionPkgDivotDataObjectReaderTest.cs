using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	abstract class DtbBookingInstructionPkgDivotDataObjectReaderTest<T> : OrganizationAddressTestHelper
		where T : IConfirmationParentDivot, new()
	{
		public void TestBasicFieldMappings()
		{
			var divotDataObject = new T();
			divotDataObject.Quantity = 10;
			SetPackageLink(divotDataObject, 3);

			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew("KEG");
			var packageLinks = new Dictionary<ZInt, PkgPackage>();
			packageLinks.Add(3, package);

			var reader = GetNewReader(divotDataObject, packageLinks, null);
			var instructionPkgDivot = reader.ReadIntoBusinessObject();
			AssertEquals("instructionPkgDivot.KD_KP_Package", package.PK, instructionPkgDivot.KD_KP_Package);
			AssertEquals("instructionPkgDivot.KD_Quantity", 10, instructionPkgDivot.KD_Quantity);
		}

		public void TestReadingInDivotWithNoPackageLinkThrowsException()
		{
			var divotDataObject = new T();
			var reader = GetNewReader(divotDataObject, new Dictionary<ZInt, PkgPackage>(), new List<Confirmation>());
			AssertExceptionThrown(typeof(InvalidOperationException), "Should never try to read in a Divot with no Package Link.", () => reader.ReadIntoBusinessObject());
		}

		public void TestPackageLinksShouldNotBeNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GetNewReader(new T(), null, null));
		}

		public void TestConfirmations()
		{
			var divotDataObject = new T();
			var confirmationToExclude = new Confirmation() { Reference = "EREF" };
			divotDataObject.ConfirmationCollection = new List<Confirmation>();
			divotDataObject.ConfirmationCollection.Add(new Confirmation { Reference = "REF1" });
			divotDataObject.ConfirmationCollection.Add(new Confirmation { Reference = "REF2" });
			divotDataObject.ConfirmationCollection.Add(confirmationToExclude);
			SetPackageLink(divotDataObject, 1);

			var consolidation = Helper.CreateConsolidation();
			var packageJob = Helper.CreatePackageJob(consolidation);
			var package = packageJob.Packages.AddNew("KEG");
			var packageLinks = new Dictionary<ZInt, PkgPackage>();
			packageLinks.Add(1, package);

			var confirmationsToExclude = new List<Confirmation>();
			confirmationsToExclude.Add(confirmationToExclude);

			var reader = GetNewReader(divotDataObject, packageLinks, confirmationsToExclude);
			var instructionPkgDivot = reader.ReadIntoBusinessObject();
			AssertEquals(2, instructionPkgDivot.Confirmations.Count);
			AssertEquals(2, instructionPkgDivot.ConfirmationsDivotOnly.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "REF1", "REF2" }, instructionPkgDivot.ConfirmationsDivotOnly.Select(c => c.KK_ReferenceNum));
		}

		protected abstract void SetPackageLink(T divotDataObject, ZInt link);

		protected abstract DtbBookingInstructionPkgDivotDataObjectReader<T> GetNewReader(T divotDataObject,
			Dictionary<ZInt, PkgPackage> packageLinks, IEnumerable<Confirmation> confirmationDataObjectsToExclude);

		TransportBookingTestHelper helper;
		protected TransportBookingTestHelper Helper => helper ?? (helper = new TransportBookingTestHelper(Factory.BOFactory));
	}
}
