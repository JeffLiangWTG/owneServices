using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefComplianceListFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[] { new RefComplianceList
				{
					RCL_PK = Guid.NewGuid(),
					RCL_IsActive = true,
					RCL_ListCode = "Code",
					RCL_ListDescription = "Description",
					RCL_ListName = "Name",
					RCL_ListPublisher = "Publisher",
					RCL_ListType = "Type",
					RCL_PublisherDescription = "Publisher Description",
					RCL_PublisherJurisdiction = "Publisher Jurisdiction",
					RCL_MainSourceURL = "MainSourceURL",
					RCL_SecondarySourceURL = "SecondarySourceURL",
					RCL_LastUpdatedDate = DateTime.Now
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			((RefComplianceList)data).RCL_ListDescription = "Updated Description";
			return true;
		}
	}
}
