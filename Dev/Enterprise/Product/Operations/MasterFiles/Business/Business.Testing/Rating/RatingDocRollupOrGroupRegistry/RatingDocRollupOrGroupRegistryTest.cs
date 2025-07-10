using System;
using System.Linq;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using FluentAssertions;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RatingDocRollupOrGroupRegistry))]
	public class RatingDocRollupOrGroupRegistryTest : RegistryBusinessObjectTemplateTestCase<RatingDocRollupOrGroupRegistry>
	{
		protected override RatingDocRollupOrGroupRegistry GetBusinessObjectToClone()
			=> new RatingDocRollupOrGroupRegistry(new FallbackLevel(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

		protected override RatingDocRollupOrGroupRegistry GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		RatingDocRollupOrGroupRegistry ratingDocumentsChargeGroupingAndRollUp;

		protected override void SetUp()
		{
			base.SetUp();
			var collection = new RatingDocRollupOrGroupRegistryCollection();
			ratingDocumentsChargeGroupingAndRollUp = collection.AddNew();
		}

		// The list order has been precisely picked by products, and that any changes to it should be confirmed by them
		public void TestModuleList()
		{
			ratingDocumentsChargeGroupingAndRollUp
				.ModuleList
				.ToArray()
				.Select(ml => new { ml.Code, ml.Description })
				.Should()
				.BeEquivalentTo(
					new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "QTN", Description = "Quotations" },
					},
					options => options.WithStrictOrdering()
				);

			Assert(true);
		}

		// The list order has been precisely picked by products, and that any changes to it should be confirmed by them
		public void TestJobTypeList()
		{
			ratingDocumentsChargeGroupingAndRollUp
				.JobTypeList
				.ToArray()
				.Select(ml => new { ml.Code, ml.Description })
				.Should()
				.BeEquivalentTo
				(
					new[] {
						new { Code = "ALL", Description = "All" },
						new { Code = "CFS", Description = "CFS" },
						new { Code = "CUS", Description = "Customs" },
						new { Code = "FOR", Description = "Forwarding" },
						new { Code = "LIA", Description = "Liner & Agency" },
						new { Code = "TRA", Description = "Transport" },
						new { Code = "WAR", Description = "Warehouse" },
					},
					options => options.WithStrictOrdering()
				);

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsAll()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.All;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" }
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsForwarding()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "BBK", Description = "Break Bulk" },
						new { Code = "BLK", Description = "Bulk" },
						new { Code = "BCN", Description = "Buyer's Consol" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "RAI", Description = "Rail Freight" },
						new { Code = "ROR", Description = "Roll On/Off" },
						new { Code = "SEA", Description = "Sea Freight" },
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsLinerAndAgency()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.LinerAndAgency;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "FCL", Description = "Containerized" },
						new { Code = "LCL", Description = "Non-Containerized" },
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsCFS()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.CFS;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "SEA", Description = "Sea Freight" },
						new { Code = "RAI", Description = "Rail Freight" },
						new { Code = "ROA", Description = "Road Freight" },
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsWarehouse()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.Warehouse;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "SEA", Description = "Sea Freight" },
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsTransport()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.Transport;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "RAI", Description = "Rail Freight" },
						new { Code = "ROA", Description = "Road Freight" },
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsCustoms()
		{
			ratingDocumentsChargeGroupingAndRollUp.JobType = DocRollupOrSortJobTypeList.Codes.Customs;
			ratingDocumentsChargeGroupingAndRollUp
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "BBK", Description = "Break Bulk" },
						new { Code = "BLK", Description = "Bulk" },
						new { Code = "BCN", Description = "Buyer's Consol" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "RAI", Description = "Rail Freight" },
						new { Code = "ROR", Description = "Roll On/Off" },
						new { Code = "SEA", Description = "Sea Freight" },
					});

			Assert(true);
		}

		// The list order has been precisely picked by products, and that any changes to it should be confirmed by them
		public void TestDisplayList()
		{
			ratingDocumentsChargeGroupingAndRollUp
				.DisplayList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.OrderBy(c => c.Code)
				.Should()
				.BeEquivalentTo
				(
					new[]
					{
						new { Code = "ALP", Description = "Alphabetical" },
						new { Code = "ROL", Description = "Roll Up Charges" },
						new { Code = "RSQ", Description = "Roll Up Charges and Sequence" },
						new { Code = "SEQ", Description = "Sequence" },
						new { Code = "SSQ", Description = "Sub Total Charges and Sequence" },
						new { Code = "SUB", Description = "Sub Total Charges" },
					},
					options => options.WithStrictOrdering()
				);

			Assert(true);
		}

		public void TestStyleList_WhenDisplayIsAlphabetical()
		{
			ratingDocumentsChargeGroupingAndRollUp.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			ratingDocumentsChargeGroupingAndRollUp
				.StyleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "NOG", Description = "No grouping of charges" },
					}.OrderBy(c => c.Code));

			Assert(true);
		}

		public void TestStyleList_WhenDisplayIsRollUpCharges()
		{
			ratingDocumentsChargeGroupingAndRollUp.Display = DocRollupOrSortDisplayList.Codes.RollUpCharges;
			AssertIfStyleListContainsAllElementsExceptDefault();
		}

		public void TestStyleList_WhenDisplayIsRollUpChargesANDSequence()
		{
			ratingDocumentsChargeGroupingAndRollUp.Display = DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence;
			AssertIfStyleListContainsAllElementsExceptDefault();
		}

		public void TestStyleList_WhenDisplayIsSequence()
		{
			ratingDocumentsChargeGroupingAndRollUp.Display = DocRollupOrSortDisplayList.Codes.Sequence;
			ratingDocumentsChargeGroupingAndRollUp
				.StyleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "NOG", Description = "No grouping of charges" },
					}.OrderBy(c => c.Code));

			Assert(true);
		}

		public void TestStyleList_WhenDisplayIsSubTotalCharges()
		{
			ratingDocumentsChargeGroupingAndRollUp.Display = DocRollupOrSortDisplayList.Codes.SubTotalCharges;
			AssertIfStyleListContainsAllElementsExceptDefault();
		}

		public void TestStyleList_WhenDisplayIsSubTotalChargesANDSequence()
		{
			ratingDocumentsChargeGroupingAndRollUp.Display = DocRollupOrSortDisplayList.Codes.SubTotalChargesAndSequence;
			AssertIfStyleListContainsAllElementsExceptDefault();
		}

		// The list order has been precisely picked by products, and that any changes to it should be confirmed by them
		void AssertIfStyleListContainsAllElementsExceptDefault()
		{
			ratingDocumentsChargeGroupingAndRollUp
				.StyleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo
				(
					new[]
					{
						new { Code = "NOG", Description = "No grouping of charges" },
						new { Code = "ALL", Description = "All charges except Customs Duty and Tax as one line" },
						new { Code = "AEC", Description = "Origin, Loading, Freight, Insurance, Unload and Destination as one line" },
						new { Code = "O&F", Description = "Origin, Loading, Freight and Insurance as one line" },
						new { Code = "ORF", Description = "Origin and Freight as one line" },
						new { Code = "FRT", Description = "Freight charges as one line" },
						new { Code = "F&D", Description = "Freight, Insurance, Unload and Destination charges as one line" },
						new { Code = "OFD", Description = "Origin and Loading as one line, Freight and Insurance as one line, Unload and Destination as one line" },
						new { Code = "OFO", Description = "Origin and Loading as one line, Freight and Insurance as one line" },
						new { Code = "OFF", Description = "Origin and Loading as one line, Freight as one line, Insurance as one line, Unloading and Destination as one line" },
						new { Code = "OFI", Description = "Origin and Loading as one line, Freight as one line, Insurance as one line" },
						new { Code = "CCD", Description = "Charge Code" },
						new { Code = "CCG", Description = "Charge Code Group" },
					},
					options => options.WithStrictOrdering()
				);

			Assert(true);
		}

		public void TestValidateDefaultRecord()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();
			var registryItem = collection.AddNew();

			registryItem.Module = DocRollupOrSortModuleList.Codes.Quotations;
			registryItem.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			registryItem.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			registryItem.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			registryItem.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			registryItem.RunPreSaveValidation();
			Assert(registryItem.ModuleInfo.HasError("You must always have a row with Module = All, Job Type = All, Mode = All."));

			registryItem.Module = DocRollupOrSortModuleList.Codes.All;
			registryItem.JobType = DocRollupOrSortJobTypeList.Codes.All;
			registryItem.RunPreSaveValidation();
			AssertNoErrors(registryItem);
		}

		public void TestValidateDuplicateLines()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();
			var defaultRecord = collection.AddNew();

			defaultRecord.Module = DocRollupOrSortModuleList.Codes.All;
			defaultRecord.JobType = DocRollupOrSortJobTypeList.Codes.All;
			defaultRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			defaultRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			defaultRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			defaultRecord.RunPreSaveValidation();
			AssertNoErrors(defaultRecord);

			var secondRecord = collection.AddNew(); // New record with same Module, JobType and TransportMode
			secondRecord.Module = DocRollupOrSortModuleList.Codes.All;
			secondRecord.JobType = DocRollupOrSortJobTypeList.Codes.All;
			secondRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			secondRecord.Display = DocRollupOrSortDisplayList.Codes.Sequence; // New Display
			secondRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			secondRecord.RunPreSaveValidation();

			Assert(secondRecord.ModuleInfo.HasError("Can NOT have more than one Line with the same Module, Job Type, and Mode."));

			secondRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			secondRecord.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			secondRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			secondRecord.Display = DocRollupOrSortDisplayList.Codes.Sequence; // New Display
			secondRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			secondRecord.RunPreSaveValidation();

			AssertNoErrors(secondRecord);

			var thirdRecord = collection.AddNew();
			thirdRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			thirdRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			thirdRecord.Display = DocRollupOrSortDisplayList.Codes.Sequence;
			thirdRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RunPreSaveValidation();

			Assert(thirdRecord.ModuleInfo.HasError("Can NOT have more than one Line with the same Module, Job Type, and Mode."));
		}

		public void TestValidateModule()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();
			var defaultRecord = collection.AddNew();

			defaultRecord.Module = DocRollupOrSortModuleList.Codes.All;
			defaultRecord.JobType = DocRollupOrSortJobTypeList.Codes.All;
			defaultRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			defaultRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			defaultRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			defaultRecord.RunPreSaveValidation();
			AssertNoErrors(defaultRecord.ModuleInfo);

			var newRecord = collection.AddNew();

			newRecord.JobType = DocRollupOrSortJobTypeList.Codes.All;
			newRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			newRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			newRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			newRecord.RunPreSaveValidation();
			Assert("Module: Mandatory Validation", newRecord.ModuleInfo.HasError("Please enter a value."));

			newRecord.Module = "XYZ";
			newRecord.RunPreSaveValidation();
			Assert("Module: List Validation", newRecord.ModuleInfo.HasError("Enter a valid selection."));

			newRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			newRecord.RunPreSaveValidation();
			AssertNoErrors(newRecord.ModuleInfo);
		}

		public void TestValidateJobType()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();
			var defaultRecord = collection.AddNew();

			defaultRecord.Module = DocRollupOrSortModuleList.Codes.All;
			defaultRecord.JobType = DocRollupOrSortJobTypeList.Codes.All;
			defaultRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			defaultRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			defaultRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			defaultRecord.RunPreSaveValidation();
			AssertNoErrors(defaultRecord.ModuleInfo);

			var newRecord = collection.AddNew();
			newRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			newRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			newRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			newRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			newRecord.RunPreSaveValidation();
			Assert("Module: Mandatory Validation", newRecord.JobTypeInfo.HasError("Please enter a value."));

			newRecord.JobType = "XYZ";
			newRecord.RunPreSaveValidation();
			Assert("Module: List Validation", newRecord.JobTypeInfo.HasError("Enter a valid selection."));

			newRecord.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			newRecord.RunPreSaveValidation();
			AssertNoErrors(newRecord.JobTypeInfo);
		}

		public void TestValidateTransportMode()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();
			var defaultRecord = collection.AddNew();

			defaultRecord.Module = DocRollupOrSortModuleList.Codes.All;
			defaultRecord.JobType = DocRollupOrSortJobTypeList.Codes.All;
			defaultRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			defaultRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			defaultRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			defaultRecord.RunPreSaveValidation();
			AssertNoErrors(defaultRecord.ModuleInfo);

			var newRecord = collection.AddNew();
			newRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			newRecord.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			newRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			newRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			newRecord.RunPreSaveValidation();
			Assert("Module: Mandatory Validation", newRecord.TransportModeInfo.HasError("Please enter a value."));

			newRecord.TransportMode = "XYZ";
			newRecord.RunPreSaveValidation();
			Assert("Module: List Validation", newRecord.TransportModeInfo.HasError("Enter a valid selection."));

			newRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.AirFreight;
			newRecord.RunPreSaveValidation();
			AssertNoErrors(newRecord.TransportModeInfo);
		}

		public void TestValidateDisplay()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();

			var newRecord = collection.AddNew();
			newRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			newRecord.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			newRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			newRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			newRecord.RunPreSaveValidation();
			Assert("Module: Mandatory Validation", newRecord.DisplayInfo.HasError("Please enter a value."));

			newRecord.Display = "XYZ";
			newRecord.RunPreSaveValidation();
			Assert("Module: List Validation", newRecord.DisplayInfo.HasError("Enter a valid selection."));

			newRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			newRecord.RunPreSaveValidation();
			AssertNoErrors(newRecord.DisplayInfo);
		}

		public void TestValidateStyle()
		{
			var collection = new RatingDocRollupOrGroupRegistryCollection();

			var newRecord = collection.AddNew();
			newRecord.Module = DocRollupOrSortModuleList.Codes.Quotations;
			newRecord.JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			newRecord.TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			newRecord.Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			newRecord.RunPreSaveValidation();
			Assert("Module: Mandatory Validation", newRecord.StyleInfo.HasError("Please enter a value."));

			newRecord.Style = "XYZ";
			newRecord.RunPreSaveValidation();
			Assert("Module: List Validation", newRecord.StyleInfo.HasError("Enter a valid selection."));

			newRecord.Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			newRecord.RunPreSaveValidation();
			AssertNoErrors(newRecord.StyleInfo);
		}
	}
}
