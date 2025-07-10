using System;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SlotAllocationTest : BaseFreightTest
	{
		public void TestGetSetAspect()
		{
			const string aspect1 = "AS1";
			const string aspect2 = "AS2";
			const string aspect3 = "AS3";

			SlotAllocation allocation = Factory.New<SlotAllocation>();

			AssertAspects("", "", allocation);
			AssertEquals(0m, allocation.GetAspect(aspect1));
			AssertEquals(0m, allocation.GetAspect(aspect2));
			AssertEquals(0m, allocation.GetAspect(aspect3));

			const string expected1 =
				"AS1: 5.500\r\n" +
				"AS2: 50000.000\r\n" +
				"";

			allocation.SetAspect(aspect1, 5.5m);
			allocation.SetAspect(aspect2, 50000m);

			AssertAspects("", expected1, allocation);
			AssertEquals(5.5m, allocation.GetAspect(aspect1));
			AssertEquals(50000m, allocation.GetAspect(aspect2));
			AssertEquals(0m, allocation.GetAspect(aspect3));

			const string expected2 =
				"AS1: 0.000\r\n" +
				"AS2: 50000.000\r\n" +
				"";

			allocation.SetAspect(aspect1, 0m);
			allocation.SetAspect(aspect3, 0m);

			AssertAspects("", expected2, allocation);
			AssertEquals(0m, allocation.GetAspect(aspect1));
			AssertEquals(50000m, allocation.GetAspect(aspect2));
			AssertEquals(0m, allocation.GetAspect(aspect3));
		}

		public void TestHumanReadableName()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "BOB";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			SlotAllocation countryAlloc = sailing.Origin.VoyageCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			AssertEquals("Generic: Country/Region = 'AU'", countryAlloc.HumanReadableName);
			countryAlloc.E0_OH_Principal = principal.PK;
			AssertEquals("BOB: Country/Region = 'AU'", countryAlloc.HumanReadableName);
		}

		public void TestParent()
		{
			SlotAllocation countryAlloc = ExportSailing.Origin.VoyageCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			AssertSame(ExportSailing.Origin.VoyageCountry, countryAlloc.Parent);

			SlotAllocation originAlloc = ExportSailing.Origin.SlotAllocations.GetAllocation(ZGuid.Empty);
			AssertSame(ExportSailing.Origin, originAlloc.Parent);

			SlotAllocation sailingAlloc = ExportSailing.SlotAllocations.GetAllocation(ZGuid.Empty);
			AssertSame(ExportSailing, sailingAlloc.Parent);
		}

		public void TestNullExceptionOnHumanReadableName()
		{
			var principal = Factory.New<OrgHeader>();
			principal.OH_Code = "BOB";

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.Origin.VoyageCountry.J0_AllocationMethod = AllocationMethodList.Codes.Sailing;
			sailing.Origin.VoyageCountry.J0_AllocationsByPrincipal = true;
			var countryAlloc = sailing.Origin.VoyageCountry.SlotAllocations.GetAllocation(ZGuid.Empty);
			countryAlloc.E0_OH_Principal = principal.PK;

			Factory.Save();
			AssertEquals("BOB: Country/Region = 'AU'", countryAlloc.HumanReadableName);

			countryAlloc.E0_ParentID = ZGuid.Empty;
			AssertEquals("BOB: ", countryAlloc.HumanReadableName);
		}

		public void DefaultOverallocationPercent()
		{
			FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m);
			AssertEquals(0m, SlotAllocation.DefaultOverAllocationPercent);

			FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5m);
			AssertEquals(50m, SlotAllocation.DefaultOverAllocationPercent);
		}

		public void TestE0_OverallocationPercent()
		{
			SlotAllocation allocation = Factory.New<SlotAllocation>();
			DataRow row = ((INeedRow)allocation).Row;

			FreightConfigurationRegistry.Instance.DefaultOverAllocationPercent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10m);

			allocation.E0_UseDefaultOverAllocation = true;
			AssertEquals("When not overriden, return the default value", 10m, allocation.E0_OverAllocationPercent);
			AssertEquals("When not overriden, save value as 0", 0m, row[JobSlotAllocationSchema.Constants.E0_OverAllocationPercent]);

			allocation.E0_UseDefaultOverAllocation = false;
			AssertEquals("E0_OverallocationPercent should not change when overriding", SlotAllocation.DefaultOverAllocationPercent, allocation.E0_OverAllocationPercent);
			AssertEquals("E0_OverallocationPercent should save the new value when overriding", SlotAllocation.DefaultOverAllocationPercent, row[JobSlotAllocationSchema.Constants.E0_OverAllocationPercent]);

			allocation.E0_OverAllocationPercent = 50m;
			AssertEquals("E0_OverallocationPercent should reflect it's new value", 50m, allocation.E0_OverAllocationPercent);
			AssertEquals("E0_OverallocationPercent should save it's new value", 50m, row[JobSlotAllocationSchema.Constants.E0_OverAllocationPercent]);

			allocation.E0_UseDefaultOverAllocation = true;
			AssertEquals("E0_OverAllocationPercent should revert to default when setting E0_UseDefaultOverAllocation", SlotAllocation.DefaultOverAllocationPercent, allocation.E0_OverAllocationPercent);
			AssertEquals("E0_OverAllocationPercent should save value as 0", 0m, row[JobSlotAllocationSchema.Constants.E0_OverAllocationPercent]);
		}

		public void TestClone()
		{
			VoyageCountry country = Factory.New<VoyageCountry>();
			SlotAllocation allocation = country.SlotAllocations.GetAllocation(ZGuid.NewZGuid());
			SlotAllocation allocationClone = (SlotAllocation)allocation.Clone();

			AssertEquals("Parent ID", ZGuid.Empty, allocationClone.E0_ParentID);
			AssertEquals("Parent Type", "", allocationClone.E0_ParentTableCode);
			AssertEquals("Principal", allocation.E0_OH_Principal, allocationClone.E0_OH_Principal);
		}

		#region Implementation

		void AssertAspects(string message, string expected, SlotAllocation allocation)
		{
			SlotAllocationAspect[] aspects = allocation.Aspects.ToArray();
			Array.Sort(aspects, (a1, a2) => StringComparer.OrdinalIgnoreCase.Compare(a1.D5_Type, a2.D5_Type));

			StringBuilder builder = new StringBuilder();

			foreach (SlotAllocationAspect aspect in aspects)
			{
				builder.Append(aspect.D5_Type);
				builder.Append(": ");
				builder.AppendLine(aspect.D5_Value.ToString("0.000"));
			}

			AssertMultilineASCIIEquals(message, expected, builder.ToString());
		}

		#endregion
	}
}
