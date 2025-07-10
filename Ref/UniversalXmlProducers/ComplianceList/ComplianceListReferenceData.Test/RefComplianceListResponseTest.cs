using System;
using CargoWise.RefDbRepo.ComplianceListReferenceData.Business;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.Test
{
	[TestFixture]
	public class RefComplianceListResponseTest
	{
		[Test]
		public void TestSerializeAndDeserialize()
		{
			var refComplianceList = new RefComplianceListResponse
			{
				RCL_ListCode = "TST2",
				RCL_ListName = "Test List 2",
				RCL_ListDescription = "List Desc 2",
				RCL_IsActive = true,
				RCL_ListPublisher = "Test Publisher 2",
				RCL_PublisherJurisdiction = "Australia",
				RCL_PublisherDescription = "Test Publisher Desc 2",
				RCL_MainSourceURL = "http://www.example.com",
				RCL_SecondarySourceURL = "http://www.test.com",
				RCL_ListType = "Sanction",
				RCL_LastUpdatedDate = DateTime.UtcNow,
				RCL_IntegrationDate = DateTime.UtcNow
			};
			var serializedStr = JsonConvert.SerializeObject(refComplianceList);
			var deserializedRefComplianceList = JsonConvert.DeserializeObject<RefComplianceListResponse>(serializedStr);
			Assert.Multiple(() =>
			{
				Assert.AreEqual(refComplianceList.RCL_ListCode, deserializedRefComplianceList.RCL_ListCode);
				Assert.AreEqual(refComplianceList.RCL_ListDescription, deserializedRefComplianceList.RCL_ListDescription);
				Assert.AreEqual(refComplianceList.RCL_ListName, deserializedRefComplianceList.RCL_ListName);
				Assert.AreEqual(refComplianceList.RCL_ListPublisher, deserializedRefComplianceList.RCL_ListPublisher);
				Assert.AreEqual(refComplianceList.RCL_PublisherDescription, deserializedRefComplianceList.RCL_PublisherDescription);
				Assert.AreEqual(refComplianceList.RCL_PublisherJurisdiction, deserializedRefComplianceList.RCL_PublisherJurisdiction);
				Assert.AreEqual(refComplianceList.RCL_ListType, deserializedRefComplianceList.RCL_ListType);
				Assert.AreEqual(refComplianceList.RCL_MainSourceURL, deserializedRefComplianceList.RCL_MainSourceURL);
				Assert.AreEqual(refComplianceList.RCL_SecondarySourceURL, deserializedRefComplianceList.RCL_SecondarySourceURL);
				Assert.AreEqual(refComplianceList.RCL_IsActive, deserializedRefComplianceList.RCL_IsActive);
				Assert.AreEqual(refComplianceList.RCL_IntegrationDate, deserializedRefComplianceList.RCL_IntegrationDate);
				Assert.AreEqual(refComplianceList.RCL_LastUpdatedDate, deserializedRefComplianceList.RCL_LastUpdatedDate);
			});
		}
	}
}
