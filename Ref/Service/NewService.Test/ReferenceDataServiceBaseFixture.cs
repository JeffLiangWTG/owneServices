using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class ReferenceDataServiceBaseFixture
	{
		[Test]
		public void GetDataSetId()
		{
			var repoMock = new Mock<IReadOnlyReferenceDataRepository>();
			var dataSetList = GetDataSetInformations();
			repoMock.Setup(x => x.Get<RefDataSetInformation>()).Returns(dataSetList.AsQueryable());

			var service = new Mock<ReferenceDataServiceBase<It.IsAnyType>>(repoMock.Object);
			var dataSetName = "RefCusRateType";
			Assert.AreEqual(4, service.Object.GetDataSetId(dataSetName));
			dataSetName = "RefCusCodeType0_26_9";
			Assert.AreEqual(3, service.Object.GetDataSetId(dataSetName));
			dataSetName = "New Table";
			Assert.AreEqual(0, service.Object.GetDataSetId(dataSetName));
		}

		List<RefDataSetInformation> GetDataSetInformations()
		{
			var dataSetList = new List<RefDataSetInformation>();
			dataSetList.Add(new RefDataSetInformation
			{
				RDS_PK = Guid.NewGuid(),
				RDS_DataSetId = 3,
				RDS_TableName = "RefCusCodeType",
				RDS_DataSetName = "RefCusCodeType"
			});

			dataSetList.Add(new RefDataSetInformation
			{
				RDS_PK = Guid.NewGuid(),
				RDS_DataSetId = 4,
				RDS_TableName = "RefCusRateType",
				RDS_DataSetName = "RefCusRateType"
			});
			return dataSetList;
		}
	}
}
