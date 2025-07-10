using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core.Writing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	internal class RateOneOffContainerDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestContainerDataExporting()
		{
			AssertContainerData(2, "20GP");
			AssertContainerData(3, "40GP");

			void AssertContainerData(int containerCount, string containerTypeCode)
			{
				var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerTypeCode);
				var container = Factory.New<RateOneOffContainers>();
				container.TC_ContainerCount = (ZShort)containerCount;
				container.TC_RC = containerType.PK;

				var containerData = new RateOneOffContainerDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.CLI, container))).GetDataObject(container);
				AssertNotNull("containerData", containerData);

				CombineAssertions("Only ContainerCount and ContainerType should be written to XML.", () =>
				{
					AssertEquals("ContainerCount", containerCount, containerData.ContainerCount);
					AssertEquals("ContainerType.Code", containerType.RC_Code, containerData.ContainerType.Code);
					AssertEquals("ContainerType.Description", containerType.RC_Description, containerData.ContainerType.Description);
				});
			}
		}
	}
}
