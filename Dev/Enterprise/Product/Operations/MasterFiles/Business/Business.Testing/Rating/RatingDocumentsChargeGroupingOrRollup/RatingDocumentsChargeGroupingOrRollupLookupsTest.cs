using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Rating;
using FluentAssertions;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RatingDocumentsChargeGroupingOrRollupLookupsTest : BusinessObjectLookupsTestCase
	{
		RatingDocumentsChargeGroupingOrRollup parent;
		RatingDocumentsChargeGroupingOrRollupLookups lookups;

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<RatingDocumentsChargeGroupingOrRollup>();
			lookups = parent.Lookups;
		}

		public void TestModuleList()
		{
			lookups
				.ModuleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[] {
					new { Code = "ALL", Description = "All" },
					new { Code = "QTN", Description = "Quotations" },
				});

			Assert(true);
		}

		public void TestDisplayList()
		{
			lookups
				.DisplayList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[] {
					new { Code = "DEF", Description = "Use Defaults from Registry - refer to registry setting for details" },
					new { Code = "ALP", Description = "Alphabetical" },
					new { Code = "ROL", Description = "Roll Up Charges" },
					new { Code = "RSQ", Description = "Roll Up Charges and Sequence" },
					new { Code = "SEQ", Description = "Sequence" },
					new { Code = "SSQ", Description = "Sub Total Charges and Sequence" },
					new { Code = "SUB", Description = "Sub Total Charges" },
				});

			Assert(true);
		}
		public void TestJobTypeList()
		{
			parent.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			lookups
				.JobTypeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "CFS", Description = "CFS" },
						new { Code = "CUS", Description = "Customs" },
						new { Code = "FOR", Description = "Forwarding" },
						new { Code = "LIA", Description = "Liner & Agency" },
						new { Code = "TRA", Description = "Transport" },
						new { Code = "WAR", Description = "Warehouse" }
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsAll()
		{
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
			lookups
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
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			lookups
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "SEA", Description = "Sea Freight" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "RAI", Description = "Rail Freight" },
						new { Code = "BBK", Description = "Break Bulk" },
						new { Code = "BLK", Description = "Bulk" },
						new { Code = "ROR", Description = "Roll On/Off" },
						new { Code = "BCN", Description = "Buyer's Consol" }
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsLinerAndAgency()
		{
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.LinerAndAgency;
			lookups
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
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.CFS;
			lookups
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "SEA", Description = "Sea Freight" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "RAI", Description = "Rail Freight" }
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsWarehouse()
		{
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Warehouse;
			lookups
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "SEA", Description = "Sea Freight" },
						new { Code = "ROA", Description = "Road Freight" }
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsTransport()
		{
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Transport;
			lookups
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "RAI", Description = "Rail Freight" }
					});

			Assert(true);
		}

		public void TestTransportModeList_WhenJobTypeIsCustoms()
		{
			parent.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Customs;
			lookups
				.TransportModeList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "ALL", Description = "All" },
						new { Code = "AIR", Description = "Air Freight" },
						new { Code = "SEA", Description = "Sea Freight" },
						new { Code = "ROA", Description = "Road Freight" },
						new { Code = "RAI", Description = "Rail Freight" },
						new { Code = "BBK", Description = "Break Bulk" },
						new { Code = "BLK", Description = "Bulk" },
						new { Code = "ROR", Description = "Roll On/Off" },
						new { Code = "BCN", Description = "Buyer's Consol" }
					});

			Assert(true);
		}

		public void TestStyleList_WhenDisplayIsDefault()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.Default;
			AssertIfStyleListContainsAllElements();
		}

		public void TestStyleList_WhenDisplayIsAlphabetical()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			lookups
				.StyleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "NOG", Description = "No grouping of charges" },
					});

			Assert(true);
		}

		public void TestStyleList_WhenDisplayIsRollUpCharges()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.RollUpCharges;
			AssertIfStyleListContainsAllElements();
		}

		public void TestStyleList_WhenDisplayIsRollUpChargesANDSequence()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.RollUpChargesAndSequence;
			AssertIfStyleListContainsAllElements();
		}

		public void TestStyleList_WhenDisplayIsSequence()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.Sequence;
			lookups
				.StyleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "NOG", Description = "No grouping of charges" },
					});

			Assert(true);
		}

		public void TestStyleList_WhenDisplayIsSubTotalChargesANDSequence()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.SubTotalChargesAndSequence;
			AssertIfStyleListContainsAllElements();
		}

		public void TestStyleList_WhenDisplayIsSubTotalCharges()
		{
			parent.RCG_Display = DocRollupOrSortDisplayList.Codes.SubTotalCharges;
			AssertIfStyleListContainsAllElements();
		}

		void AssertIfStyleListContainsAllElements()
		{
			lookups
				.StyleList
				.ToArray()
				.Select(l => new { l.Code, l.Description })
				.Should()
				.BeEquivalentTo(new[]
					{
						new { Code = "DEF", Description = "Use Defaults from Registry - refer to registry setting for details" },
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
					});

			Assert(true);
		}
	}
}
