using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsDetailsChangeOfOwnershipTest : TestCaseWithFactory
	{
		public void TestGetDetails()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var logger = new DummyLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>(new[]
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "WENDY THE DESTROYER" },
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "JACK THE PEACEMAKER", AddressType = nameof(DocAddressType.ImporterDocumentaryAddress) }
				}));
			shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[]
				{
					new EntryInstruction()
					{
						OrganizationAddressCollection = new List<OrganizationAddress>(new []
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "JOE THE JOYFUL" },
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { CompanyName = "BOB THE BUILDER", AddressType = AddressTypes.Owner }
						})
					}
				}));

			IWarehouseCustomsDetailsChangeOfOwnership provider = new WarehouseCustomsDetailsChangeOfOwnership(shipment);
			AssertEquals("OldOwner.CompanyName", "JACK THE PEACEMAKER", provider.OldOwner.CompanyName);
			AssertEquals("NewOwner.CompanyName", "BOB THE BUILDER", provider.NewOwner.CompanyName);

			shipment.EntryInstructionCollection.Add(new EntryInstruction());
			provider = new WarehouseCustomsDetailsChangeOfOwnership(shipment);
			AssertEquals("OldOwner.CompanyName", "JACK THE PEACEMAKER", provider.OldOwner.CompanyName);
			AssertNull("NewOwner.CompanyName", provider.NewOwner);
		}
	}
}
