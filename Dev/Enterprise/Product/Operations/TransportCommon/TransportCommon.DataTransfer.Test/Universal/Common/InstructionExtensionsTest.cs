using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.Packing.Business.Testing;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	class InstructionExtensionsTest : OrganizationAddressTestHelper
	{
		#region TestGetConfirmations

		public void TestGetConfirmations()
		{
			var instruction = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<Confirmation>(), instruction.GetConfirmations());

			var packingLineLink1 = new InstructionPackingLineLink();
			instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink> { packingLineLink1 });
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<Confirmation>(), instruction.GetConfirmations());

			var confirmation1 = new Confirmation { Reference = "123" };
			packingLineLink1.ConfirmationCollection = new List<Confirmation> { confirmation1 };
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1 }, instruction.GetConfirmations());

			var confirmation2 = new Confirmation { Reference = "456" };
			packingLineLink1.ConfirmationCollection.Add(confirmation2);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1, confirmation2 }, instruction.GetConfirmations());

			var packingLineLink2 = new InstructionPackingLineLink();
			packingLineLink2.ConfirmationCollection = new List<Confirmation> { confirmation1 };
			AssertContainsExactElementsInAnyOrder(new[] { "123", "456" }, instruction.GetConfirmations().Select(c => c.Reference.ToString()));

			var containerLink = new InstructionContainerLink();
			instruction.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink> { containerLink });
			AssertContainsExactElementsInAnyOrder(new[] { "123", "456" }, instruction.GetConfirmations().Select(c => c.Reference.ToString()));

			containerLink.ConfirmationCollection = new List<Confirmation> { confirmation1, new Confirmation { Reference = "789" } };
			AssertContainsExactElementsInAnyOrder(new[] { "123", "456", "789" }, instruction.GetConfirmations().Select(c => c.Reference.ToString()));
		}

		#endregion

		#region TestGetConfirmationsThatApplyToAllPackagesOnly

		public void TestGetConfirmationsThatApplyToAllPackagesOnly()
		{
			int containerLink1 = 1;
			int containerLink2 = 3;
			int nonExistingContainerLink = 10;
			int packageLink1 = 1;
			int packageLink2 = 2;
			int nonExistingPackageLink = 10;

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.SetContainerCollection(() => new DataObjectList<Container>());
			universalShipment.ContainerCollection.Add(new Container { ContainerNumber = "CONT1", ContainerType = new ContainerType { Code = "20GP" }, Link = containerLink1 });
			universalShipment.ContainerCollection.Add(new Container { ContainerNumber = "CONT2", ContainerType = new ContainerType { Code = "20GP" }, Link = containerLink2 });
			universalShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			universalShipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 3, ReferenceNumber = "PACK1", Link = packageLink1 });
			universalShipment.PackingLineCollection.Add(new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 6, ReferenceNumber = "PACK2", Link = packageLink2 });

			// Populate package Links
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItem);
			var packageJobReader = new PkgPackageJobDataObjectReader(universalShipment, Logger, Factory, Factory.New<DummyWithPacking>());

			try
			{
				packageJobReader.ReadIntoBusinessObject();
			}
			finally
			{
				DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
				DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = null;
			}

			var instructionDataObject = new Instruction(DefaultDataObjectWriterStrategy.TestInstance);

			// Add Container Links
			AddConfirmationContainerLink(instructionDataObject, nonExistingContainerLink, "DONOTCREATE1"); // Has no container to link to
			AddConfirmationContainerLink(instructionDataObject, null, "DONOTCREATE2"); // Has no container link

			var containerPackageDivot1 = AddConfirmationContainerLink(instructionDataObject, containerLink1, "CREF1");
			var containerPackageDivot2 = AddConfirmationContainerLink(instructionDataObject, containerLink2, "CREF2-1");
			containerPackageDivot2.ConfirmationCollection.Add(new Confirmation { Reference = "CREF2-2" });

			containerPackageDivot1.ConfirmationCollection.Add(new Confirmation { Reference = "IREF1" });
			containerPackageDivot1.ConfirmationCollection.Add(new Confirmation { Reference = "IREF2" });
			containerPackageDivot2.ConfirmationCollection.Add(new Confirmation { Reference = "IREF1" });
			containerPackageDivot2.ConfirmationCollection.Add(new Confirmation { Reference = "IREF2" });

			// Add Package Links
			AddConfirmationPackageLink(instructionDataObject, nonExistingPackageLink, "DONOTCREATE3"); // Has no package to link to
			AddConfirmationPackageLink(instructionDataObject, null, "DONOTCREATE4"); // Has no package link

			var loosePackingPackageDivot1 = AddConfirmationPackageLink(instructionDataObject, packageLink1, "PREF1");
			var loosePackingPackageDivot2 = AddConfirmationPackageLink(instructionDataObject, packageLink2, "PREF2");

			loosePackingPackageDivot1.ConfirmationCollection.Add(new Confirmation { Reference = "IREF1" });
			loosePackingPackageDivot1.ConfirmationCollection.Add(new Confirmation { Reference = "IREF2" });
			loosePackingPackageDivot2.ConfirmationCollection.Add(new Confirmation { Reference = "IREF1" });
			loosePackingPackageDivot2.ConfirmationCollection.Add(new Confirmation { Reference = "IREF2" });

			// test for common confirmations
			var confirmations = instructionDataObject.GetConfirmationsThatApplyToAllPackagesOnly(packageJobReader);
			AssertEquals("It should find two common confirmations.", 2, confirmations.Count());
			AssertContainsExactElementsInAnyOrder("Common confirmation references should be IREF1, IREF2", new ZString[] { "IREF1", "IREF2" }, confirmations.Select(c => c.Reference));
		}

		InstructionContainerLink AddConfirmationContainerLink(Instruction instructionDataObject, ZInt? containerLink, ZString confirmationReference)
		{
			var instructionContainerLink = new InstructionContainerLink { ContainerLink = containerLink };
			instructionContainerLink.ConfirmationCollection = new List<Confirmation> { new Confirmation { Reference = confirmationReference } };

			if (instructionDataObject.InstructionContainerLinkCollection == null)
			{
				instructionDataObject.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink>());
			}

			instructionDataObject.InstructionContainerLinkCollection.Add(instructionContainerLink);
			return instructionContainerLink;
		}

		InstructionPackingLineLink AddConfirmationPackageLink(Instruction instructionDataObject, ZInt? packingLineLink, ZString confirmationReference)
		{
			var instructionPackageLink = new InstructionPackingLineLink { PackingLineLink = packingLineLink };
			if (instructionPackageLink.ConfirmationCollection == null)
			{
				instructionPackageLink.ConfirmationCollection = new List<Confirmation>();
			}

			var confirmation = new Confirmation { Reference = confirmationReference };
			instructionPackageLink.ConfirmationCollection.Add(confirmation);

			if (instructionDataObject.InstructionPackingLineLinkCollection == null)
			{
				instructionDataObject.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());
			}
			instructionDataObject.InstructionPackingLineLinkCollection.Add(instructionPackageLink);

			return instructionPackageLink;
		}

		#endregion
	}
}
