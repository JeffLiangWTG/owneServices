using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZAReferenceData.Properties;

namespace ZAReferenceData.Models
{
	public class VesselAgents
	{
		public List<VesselAgent> VesselAgentsList { get; set; }

		public DateTime StartDate { get; set; }

		public IEnumerable<RefCusCodeList> ToCodeLists()
		{
			foreach (var vesselAgent in VesselAgentsList)
			{
				var codeList = new RefCusCodeList
				{
					ZZD_ZZK_NKCodeType = Settings.Default.ZZD_ZZK_NKCodeType,
					ZZD_Code = vesselAgent.Code.Trim(),
					ZZD_Description = vesselAgent.Description.Trim(),
					ZZD_StartDate = StartDate,
					ZZD_EndDate = Settings.Default.ZZD_EndDate,
					ZZD_ZZZ_NKDataGrouping = Settings.Default.ZZD_ZZZ_NKDataGrouping
				};
				yield return codeList;
			}
		}
	}
}

namespace ZAReferenceData.Models.Test
{
	[TestFixture]
	public class VesselAgentsTest
	{
		[Test]
		public void ToCodeListShouldReturnRightCodeList()
		{
			var vesselAgents = new VesselAgents();
			var testVesselAgentsList = new List<VesselAgent>();

			for (var i = 0; i < 5; i++)
			{
				var vesselAgent = new VesselAgent
				{
					Code = $"AB{i}",
					Description = $"Test Vessel Agent {i}"
				};

				testVesselAgentsList.Add(vesselAgent);
			}

			vesselAgents.VesselAgentsList = testVesselAgentsList;
			vesselAgents.StartDate = DateTime.Today;

			var codeList = vesselAgents.ToCodeLists().ToList();

			Assert.AreEqual(5, codeList.Count);

			Assert.AreEqual("AB3", codeList[3].ZZD_Code);
			Assert.AreEqual("Test Vessel Agent 3", codeList[3].ZZD_Description);
			Assert.AreEqual(DateTime.Today, codeList[0].ZZD_StartDate);
			Assert.AreEqual(DateTime.Parse("2079-06-06 23:59"), codeList[0].ZZD_EndDate);
			Assert.AreEqual("ZA", codeList[0].ZZD_ZZZ_NKDataGrouping);
			Assert.AreEqual("VESAG", codeList[0].ZZD_ZZK_NKCodeType);
		}

		[Test]
		public void ToCodeListShouldReturnTrimCodesAndDescription()
		{
			var vesselAgents = new VesselAgents();
			var testVesselAgentsList = new List<VesselAgent>();

			for (var i = 0; i < 5; i++)
			{
				var vesselAgent = new VesselAgent
				{
					Code = $"AB{i}",
					Description = $"Test Vessel Agent {i}"
				};

				testVesselAgentsList.Add(vesselAgent);
			}

			vesselAgents.VesselAgentsList = testVesselAgentsList;
			vesselAgents.StartDate = DateTime.Today;

			var codeList = vesselAgents.ToCodeLists().ToList();

			Assert.AreEqual(5, codeList.Count);

			Assert.AreEqual("AB3", codeList[3].ZZD_Code);
			Assert.AreEqual("Test Vessel Agent 3", codeList[3].ZZD_Description);
			Assert.AreEqual(DateTime.Today, codeList[0].ZZD_StartDate);
			Assert.AreEqual(DateTime.Parse("2079-06-06 23:59"), codeList[0].ZZD_EndDate);
			Assert.AreEqual("ZA", codeList[0].ZZD_ZZZ_NKDataGrouping);
			Assert.AreEqual("VESAG", codeList[0].ZZD_ZZK_NKCodeType);
		}
	}
}
