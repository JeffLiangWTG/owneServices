using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using CodeDescriptionPair = Enterprise.ZArchitecture.Core.CodeDescriptionPair;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportInstructionTest<T> : DtbTransportBusinessObjectTestCase
			where T : DtbTransportInstruction
	{
		#region TestDefaultValues

		public void TestDefaultValues()
		{
			var transport = Factory.New<T>();
			AssertEquals(TransportStatuses.Codes.Available, transport.KN_Status);
		}

		#endregion

		#region TestDefaultPackages

		/// <summary>
		/// [Category]		[Add]
		/// Containers		Containers
		/// Both			Containers and their child Loose + Top Level Loose (custom requires top level loose, because they don't ALWAYS pack containers, sometimes they do though!)
		/// Outer			Containers and Top Level Loose			
		/// Loose			Loose inside containers and Top Level Loose
		/// </summary>
		public void TestDefaultPackages()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var instructionToGetPkgFrom = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container2InnerPackage = Helper.CreatePackage("CONT2_IN", 1);
			var container2InnerInnerPackage = Helper.CreatePackage("CONT2_IN_IN", 1);
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);

			// Package tree
			transport.PackageJob.Packages.Add(container);
			transport.PackageJob.Packages.Add(container2);
			transport.PackageJob.Packages.Add(container3);
			transport.PackageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			container2.Packages.Add(container2InnerPackage);
			container2InnerPackage.Packages.Add(container2InnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			AssertPackages(instruction, "", new[] { container, topLevelPackage }, Array.Empty<PkgPackage>());
			AssertPackages(instruction, PackageCategories.Codes.Containers, new[] { container }, new[] { container });
			AssertPackages(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage }, new[] { container });
			AssertPackages(instruction, PackageCategories.Codes.Containers, new[] { topLevelPackage }, new[] { container, container2, container3 }); // none, fall back
			AssertPackages(instruction, PackageCategories.Codes.Outers, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Outers, new[] { container, topLevelPackage }, new[] { container, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Outers, new[] { container }, new[] { container });
			AssertPackages(instruction, PackageCategories.Codes.Both, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Both, new[] { container, topLevelPackage }, new[] { container, containerInnerPackage, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Both, new[] { container }, new[] { container, containerInnerPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { topLevelPackage }, new[] { topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { container, topLevelPackage }, new[] { containerInnerPackage, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { container }, new[] { containerInnerPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, new[] { container3 }, new[] { topLevelPackage, containerInnerPackage, container2InnerPackage }); // none, fall back

			// fall back to package job when none
			AssertPackages(instruction, "", Array.Empty<PkgPackage>(), Array.Empty<PkgPackage>());
			AssertPackages(instruction, PackageCategories.Codes.Containers, Array.Empty<PkgPackage>(), new[] { container, container2, container3 });
			AssertPackages(instruction, PackageCategories.Codes.Outers, Array.Empty<PkgPackage>(), new[] { container, container2, container3, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Both, Array.Empty<PkgPackage>(), new[] { container, container2, container3, containerInnerPackage, container2InnerPackage, topLevelPackage });
			AssertPackages(instruction, PackageCategories.Codes.Loose, Array.Empty<PkgPackage>(), new[] { topLevelPackage, containerInnerPackage, container2InnerPackage });
		}

		#region TestParentContainerLinksAndAddresses_ForPickupInstruction

		public void TestParentContainerLinksAndAddresses_WhenFirstContainerMatchesInstructionAddress_ForPickupInstruction()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: "XXXX", addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container1 has correct address code and type", "CONT1", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddressButOnlySecondContainerIsPassedIn_ForPickupInstruction()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container2 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container2 was passed in as a possible package", "CONT2", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddress_ForPickupInstruction()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertEquals("container1 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList<DtbTransportInstructionPkgDivot>().Select(p => p.PackageID).Contains("CONT1"));
			AssertEquals("container2 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList<DtbTransportInstructionPkgDivot>().Select(p => p.PackageID).Contains("CONT2"));
		}

		#endregion

		#region TestParentContainerLinksAndAddresses_ForDeliveryInstruction

		public void TestParentContainerLinksAndAddresses_WhenFirstContainerMatchesInstructionAddress_ForDeliveryInstruction()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: "XXXX", addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container1 has correct address code and type", "CONT1", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddressButOnlySecondContainerIsPassedIn_ForDeliveryInstruction()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container2 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container2 was passed in as a possible package", "CONT2", instruction.PackageDivots[0].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenBothContainersMatchInstructionAddress_ForDeliveryInstruction()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertEquals("container1 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList<DtbTransportInstructionPkgDivot>().Select(p => p.PackageID).Contains("CONT1"));
			AssertEquals("container2 was passed in as a possible package with matching address details", true, instruction.PackageDivots.ToList<DtbTransportInstructionPkgDivot>().Select(p => p.PackageID).Contains("CONT2"));
		}

		#endregion

		#region TestParentContainerLinksAndAddresses_ShouldFallbackToShipmentContainerYard

		public void TestParentContainerLinksAndAddresses_WhenShouldFallbackToShipmentContainerYardIsTrue_AndCorrectAddressTypeExists()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)) }, shouldFallbackToShipmentContainerYard: true ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: true )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertEquals("container1 has a matching shipment address and shouldFallbackToShipmentContainerYard is true", "CONT1", instruction.PackageDivots[0].PackageID);
			AssertEquals("container2 has a matching address", "CONT2", instruction.PackageDivots[1].PackageID);
		}

		public void TestParentContainerLinksAndAddresses_WhenShouldFallbackToShipmentContainerYardIsTrue_AndCorrectAddressTypeDoesNotExist()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: true ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: true )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container2 has a matching address and no fallback to shipment", "CONT2", instruction.PackageDivots[0].PackageID);
		}

		#endregion

		#region TestParentContainerLinksAndAddresses_ShouldMatchBlankAddress

		public void TestParentContainerLinksAndAddresses_ShouldMatchBlankAddress()
		{
			var transport = GetNewTransport();
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var container1 = Helper.CreatePackage("CONT1", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: null, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: "XXXX", addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: true ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: null, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};

			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;
			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>() {
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 1, instruction.PackageDivots.Count);
			AssertEquals("Only container1 matches the instruction's address code and type", "CONT1", instruction.PackageDivots[0].PackageID);
		}

		#endregion

		#region TestPackageContainerLinks_WhenMultipleContainersExistWithSameID

		public void TestPackageContainerLinks_WhenMultipleContainersExistWithSameID()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, org.Addresses.MainAddress);
			var container1 = Helper.CreatePackage("CONT", 1, "CNT");
			var container2 = Helper.CreatePackage("CONT", 1, "CNT");
			var container3 = Helper.CreatePackage("CONT", 1, "CNT");
			instruction.OrganisationType = OrganisationTypesList.Codes.CYD;
			instruction.PackageCategory = PackageCategories.Codes.Containers;
			var parentContainerLinksAndAddresses = new List<(ZInt? link, List<(ZString? addressCode, ZString? addressType)>, bool shouldFallbackToShipmentContainerYard)>
			{
				(1, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyPickupAddress) }, shouldFallbackToShipmentContainerYard: false ),
				(2, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: (ZString?)nameof(DocAddressType.CustomsContainerYardAddress)) }, shouldFallbackToShipmentContainerYard: true ),
				(3, new List<(ZString? addressCode, ZString? addressType)> { (addressCode: instruction.Address.Address.AddressCode, addressType: AddressTypes.ContainerYardEmptyReturnAddress) }, shouldFallbackToShipmentContainerYard: false )
			};
			transport.ParentContainerLinksAndAddresses = parentContainerLinksAndAddresses;

			transport.PackageContainerLinks = new Dictionary<ZInt, PkgPackage>
			{
				{ 1, container1 },
				{ 2, container2 },
				{ 3, container3 }
			};

			instruction.DefaultPackages(new[] { container1, container2, container3 });
			AssertEquals("Expected packages attached to instruction", 2, instruction.PackageDivots.Count);
			AssertContainsExactElementsInAnyOrder("container3 has the wrong address type, so should not be attached.", new List<string> { container1.PK.ToString(), container2.PK.ToString() }, instruction.PackageDivots.ToList<DtbTransportInstructionPkgDivot>().Select(d => d.Package.PK.ToString()));
		}

		#endregion

		void AssertPackages(DtbTransportInstruction instruction, string category, PkgPackage[] possiblePackages, PkgPackage[] expected)
		{
			Array.ForEach(instruction.DivotsWithPackages.Packages.ToArray(), p => instruction.DivotsWithPackages.RemovePackage(p));
			instruction.PackageCategory = category;
			instruction.DefaultPackages(possiblePackages);
			AssertContainsExactElementsInAnyOrder(instruction.DivotsWithPackages.Packages, expected);
		}

		public void TestDefaultPackages_WhenSuspendSettingPackages()
		{
			var transport = GetNewTransport();
			var org = Helper.CreateOrganisation("AAA");
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var instructionToGetPkgFrom = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, org.Addresses.MainAddress);
			var container = Helper.CreatePackage("CONT", 1, "CNT");
			var containerInnerPackage = Helper.CreatePackage("CONT_IN", 1);
			var containerInnerInnerPackage = Helper.CreatePackage("CONT_IN_IN", 1);
			var container2 = Helper.CreatePackage("CONT2", 1, "CNT");
			var container2InnerPackage = Helper.CreatePackage("CONT2_IN", 1);
			var container2InnerInnerPackage = Helper.CreatePackage("CONT2_IN_IN", 1);
			var container3 = Helper.CreatePackage("CONT3", 1, "CNT");
			var topLevelPackage = Helper.CreatePackage("TOP", 1);
			var topInnerPackage = Helper.CreatePackage("TOP_IN", 1);

			// Package tree
			transport.PackageJob.Packages.Add(container);
			transport.PackageJob.Packages.Add(container2);
			transport.PackageJob.Packages.Add(container3);
			transport.PackageJob.Packages.Add(topLevelPackage);
			container.Packages.Add(containerInnerPackage);
			containerInnerPackage.Packages.Add(containerInnerInnerPackage);
			container2.Packages.Add(container2InnerPackage);
			container2InnerPackage.Packages.Add(container2InnerInnerPackage);
			topLevelPackage.Packages.Add(topInnerPackage);

			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Containers, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Outers, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Outers, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Outers, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Both, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Both, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Both, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { container, topLevelPackage });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { container });
			AssertPackages_WhenSuspendSettingPackages(instruction, PackageCategories.Codes.Loose, new[] { container3 });
		}

		void AssertPackages_WhenSuspendSettingPackages(DtbTransportInstruction instruction, string category, PkgPackage[] possiblePackages)
		{
			instruction.PackageCategory = category;
			Array.ForEach(instruction.DivotsWithPackages.Packages.ToArray(), p => instruction.DivotsWithPackages.RemovePackage(p));
			AssertEquals(0, instruction.DivotsWithPackages.Count);
			using (instruction.Booking.SuspendSettingPackages())
			{
				instruction.DefaultPackages(possiblePackages);
				AssertEquals(0, instruction.DivotsWithPackages.Count);
			}
		}

		#endregion

		#region Related Entities

		#region Depot

		public void TestDepotForAddress()
		{
			var transportJob = GetNewTransport();

			// instruction
			var instruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			var zoneA = Helper.CreateZone("Zone A");
			instruction.KN_TZ_DomesticZone = zoneA.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			address.FillWithValidTestData();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			// PortHubSelection
			var portHubSelection1 = Factory.NewWithValidTestData<PortHubSelection>();
			portHubSelection1.TY_OA_DepotAddress = address.PK;
			portHubSelection1.TY_Direction = "PIC";
			portHubSelection1.TY_RatingFreightMode = "ALL";

			var portHubZonePivot1 = Factory.New<PortHubZonePivot>();
			portHubZonePivot1.TX_TY_Hub = portHubSelection1.PK;
			portHubZonePivot1.TX_TZ_Zone = zoneA.PK;

			Factory.Save();
			AssertNull(instruction.DepotForAddress);

			branch.GB_OH_OrgProxy = orgHeader.PK;
			orgHeader.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			Factory.Save();
			AssertEquals("Checks for branch with org proxy first", branch, instruction.DepotForAddress);

			branch.GB_OH_OrgProxy = ZGuid.Empty;
			orgHeader.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			AssertEquals("Then checks for controlling branch on depot org", branch, instruction.DepotForAddress);

			branch2.GB_OH_OrgProxy = orgHeader.PK;
			orgHeader.CompanyData.OB_GB_ControllingBranch = branch.PK;
			Factory.Save();
			AssertEquals("Priority is correct", branch2, instruction.DepotForAddress);
		}

		#endregion

		#region TestFindDepotAddressAndCutOffTime

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestFindDepotAddressAndCutOffTime()
		{
			var organisation = Helper.CreateOrganisation("ORGA");
			var pickupOrganisationAddress = organisation.MainAddress;
			organisation.MainAddress.OA_PostCode = "2001";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = (Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"))).RL_Code;

			var zone = Helper.CreateZone("ZONE");
			Helper.AddPostCodesToZone(zone, "2001", "3000");

			var depotOrganisation = Helper.CreateOrganisation("DEP");
			var portHubSellection = Helper.CreatePortHub(depotOrganisation.MainAddress);
			Helper.SetPortHubDirection(portHubSellection, "PIC");
			var pivot = Helper.CreatePortHubZonePivot(portHubSellection, zone);
			pivot.TX_PickupCutOffTimeVariance = 60;

			Factory.Save();

			var transportJob = GetNewTransport();
			var instruction = Helper.CreateInstruction(transportJob, InstructionTypes.Codes.PickUp);
			instruction.Address.OrganisationPK = organisation.PK;

			AssertEquals("12:00", instruction.PortHubZonePivotPickupCutOffTime.ToString("HH:mm"));
			AssertEquals(depotOrganisation.MainAddress.PK, instruction.FindDepotAddress().PK);
		}

		#endregion

		#region TestAddress

		public void TestAddress()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			AssertNotNull(instruction.Address);
			AssertEquals(DocAddressType.None, instruction.Address.DocAddressType);

			foreach (CodeDescriptionPair pair in OrganisationTypesList.Instance)
			{
				AssertAddressMatchedOrganisationType(instruction, pair.Code);
			}

			var organisation = Helper.CreateOrganisation("QWESYD");
			var pickupAddress = Helper.AddAddressToOrganisation(organisation, "Pickup Addy", OrgAddressType.Pickup);
			var officeAddress = Helper.AddAddressToOrganisation(organisation, "Office Addy", OrgAddressType.Office);
			var deliveryAddress = Helper.AddAddressToOrganisation(organisation, "Delivery Addy", OrgAddressType.Delivery);

			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.Address.OrganisationPK = organisation.PK;
			AssertEquals("Should default to pickupAddress", pickupAddress.PK, instruction.Address.E2_OA_Address);

			using (transport.GetValidationSuspender()) // not valid to have no Pickup in consignments
			{
				transport.KM_Direction = Constants.CartageDirection.Origin;
				instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
				instruction.Address.OrganisationPK = ZGuid.Empty;
				instruction.Address.OrganisationPK = organisation.PK;
				AssertEquals("Should default to pickupAddress", pickupAddress.PK, instruction.Address.E2_OA_Address);

				instruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
				instruction.Address.OrganisationPK = ZGuid.Empty;
				instruction.Address.OrganisationPK = organisation.PK;
				AssertEquals("Should default to deliveryAddress", deliveryAddress.PK, instruction.Address.E2_OA_Address);

				transport.KM_Direction = Constants.CartageDirection.Destination;
				instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
				instruction.Address.OrganisationPK = ZGuid.Empty;
				instruction.Address.OrganisationPK = organisation.PK;
				AssertEquals("Should default to deliveryAddress", deliveryAddress.PK, instruction.Address.E2_OA_Address);
			}
		}

		void AssertAddressMatchedOrganisationType(DtbTransportInstruction instruction, ZString orgType)
		{
			var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(orgType);
			AssertNotEquals("only a valid orgType should be passed in", DocAddressType.None, docAddressType);

			instruction.OrganisationType = orgType;
			AssertEquals(docAddressType, instruction.Address.DocAddressType);
		}

		#endregion

		#region TestBooking

		public void TestBooking()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			instruction.KN_KM_BookingMovement = transport.PK;

			AssertEquals(transport, instruction.Booking);
			AssertEquals(ExpectedTransportType, instruction.Booking.GetType());
		}

		protected abstract Type ExpectedTransportType { get; }

		#endregion

		#region TestConfirmations

		#region TestUpdateInstructionInfoOnlyUpdateConfirmationsWithSameType

		public void TestUpdateInstructionInfoOnlyUpdateConfirmationsWithSameType()
		{
			var transport = GetNewTransport();
			var instruction = (T)transport.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;

			var pickUpConfirmation = instruction.Confirmations.Cast<DtbTransportConfirmation>().ToArray().FirstOrDefault(c => c.IsPickUp);
			var deliveryConfirmation = instruction.Confirmations.Cast<DtbTransportConfirmation>().ToArray().FirstOrDefault(c => c.IsDelivery);
			pickUpConfirmation = pickUpConfirmation ?? (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			deliveryConfirmation = deliveryConfirmation ?? (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			var slotConfirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();

			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;

			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.ReqFrom);

			pickUpConfirmation.KK_RequiredFrom = ZDateTime.Now;
			deliveryConfirmation.KK_RequiredFrom = ZDateTime.Now.AddDays(1);

			AssertEquals("Should take latest date.", deliveryConfirmation.KK_RequiredFrom, instruction.ReqFrom);

			instruction.ReqFrom = ZDateTime.Now.AddDays(2);

			AssertNotEquals("Pickup confirmation should not be updated.", pickUpConfirmation.KK_RequiredFrom, instruction.ReqFrom);
			AssertEquals("Delivery confirmation should be updated.", deliveryConfirmation.KK_RequiredFrom, instruction.ReqFrom);
		}

		#endregion

		public void TestConfirmations()
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			instruction.Confirmations.AddNew();
			AssertEquals(ExpectedConfirmationCollectionType, instruction.Confirmations.GetType());
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.Confirmations));
		}

		protected abstract Type ExpectedConfirmationCollectionType { get; }

		#endregion

		#region TestDivotsWithPackages

		public void TestDivotsWithPackages()
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			AssertEquals(ExpectedDivotsWithPackagesCollectionType, instruction.DivotsWithPackages.GetType());

			var package = Factory.New<PkgPackage>();
			var divot = (DtbTransportInstructionPkgDivot)instruction.PackageDivots.AddNew();
			divot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new[] { package }, instruction.DivotsWithPackages.Packages);
		}

		protected abstract Type ExpectedDivotsWithPackagesCollectionType { get; }

		#endregion

		#region TestPackageDivots

		public void TestPackageDivots()
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			AssertNotNull(instruction.PackageDivots);
			AssertEquals(ExpectedInstructionPkgDivotCollectionType, instruction.PackageDivots.GetType());

			instruction.PackageDivots.AddNew();
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.PackageDivots));
		}

		protected abstract Type ExpectedInstructionPkgDivotCollectionType { get; }

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var transport = GetNewTransport();
			var pickupInstruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var deliveryInstruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			AssertEquals("??? @ ??? - ???", deliveryInstruction.Description);

			pickupInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			deliveryInstruction.Address.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE; // Consignments will default org type based on instruction, which delivery is CNR
			AssertEquals("DLV @ CNE - ABCSYD", deliveryInstruction.Description);

			deliveryInstruction.Address.E2_AddressOverride = true;
			AssertEquals("DLV @ CNE - ???", deliveryInstruction.Description);

			deliveryInstruction.Address.E2_CompanyName = "Hello";
			AssertEquals("DLV @ CNE - Hello", deliveryInstruction.Description);

			deliveryInstruction.KN_InstructionType = "";
			AssertEquals("??? @ CNE - Hello", deliveryInstruction.Description);
		}

		#endregion

		#region TestZone

		public void TestZone()
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			AssertNull(instruction.Zone);

			var zone = Helper.CreateZone("Zone1");
			instruction.KN_TZ_DomesticZone = zone.PK;
			AssertEquals(zone, instruction.Zone);
		}

		#endregion

		#endregion

		#region TestOnDocAddressChanged_ZoneTypes

		public void TestOnDocAddressChanged_ZoneTypes()
		{
			var zoneRating = Helper.CreateZone("ZoneRating", RatingConstants.RatingZoneTypes.Rating);
			Helper.AddPostCodesToZone(zoneRating, "1000", "1100");

			var zoneOperations = Helper.CreateZone("ZoneOperations", RatingConstants.RatingZoneTypes.Operations);
			Helper.AddPostCodesToZone(zoneOperations, "1200", "1300");

			var zoneAll = Helper.CreateZone("ZoneAll", RatingConstants.RatingZoneTypes.All);
			Helper.AddPostCodesToZone(zoneAll, "2000", "2100");

			var ratingCityAddress = Helper.CreateOrganisation("City123").MainAddress;
			ratingCityAddress.OA_Address1 = "123 Rating Street";
			ratingCityAddress.OA_City = "RatingCity";
			ratingCityAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			ratingCityAddress.OA_PostCode = "1001";

			var operationCityAddress = Helper.CreateOrganisation("City456").MainAddress;
			operationCityAddress.OA_Address1 = "456 Operation Street";
			operationCityAddress.OA_City = "OperationCity";
			operationCityAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			operationCityAddress.OA_PostCode = "1201";

			var allCityAddress = Helper.CreateOrganisation("City2000").MainAddress;
			allCityAddress.OA_Address1 = "2000 All Street";
			allCityAddress.OA_City = "AllCity";
			allCityAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			allCityAddress.OA_PostCode = "2001";

			Factory.Save();

			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp);
			AssertNull("Precondition", instruction.Zone);

			instruction.Address.E2_OA_Address = ratingCityAddress.PK;
			AssertNull("Should still no zone, cause exclude Rating zone.", instruction.Zone);

			instruction.Address.E2_OA_Address = operationCityAddress.PK;
			AssertEquals("Should match to zoneOperations as it contains the operationCityAddress.", zoneOperations, instruction.Zone);

			instruction.Address.E2_OA_Address = allCityAddress.PK;
			AssertNull("Cannot match to All Zone as a operations zone exists for the same location and will always be prefered", instruction.Zone);

			instruction.Address.E2_OA_Address = ratingCityAddress.PK;
			AssertNull("Should still no zone, cause exclude Rating zone.", instruction.Zone);
		}

		#endregion

		#region Properties

		// persistent

		#region TestKN_KM_BookingMovement_UpdatesBookingStatus

		public void TestKN_KM_BookingMovement_UpdatesBookingStatus()
		{
			var transport = GetNewTransport();
			AssertEquals("Precondition", TransportStatuses.Codes.Available, transport.KM_Status);

			var pickUpInstruction = Factory.New<T>();
			pickUpInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			pickUpInstruction.KN_Status = TransportStatuses.Codes.PickedUp;

			transport.Instructions.Add(pickUpInstruction);
			AssertEquals(TransportStatuses.Codes.PickedUp, transport.KM_Status);

			// ensure instruction delete updates the booking status
			pickUpInstruction.Delete();
			AssertEquals(TransportStatuses.Codes.Available, transport.KM_Status);
		}

		#endregion

		#region TestKN_DropMode

		public void TestKN_DropMode()
		{
			var instruction = Factory.New<T>();
			AssertEquals("", instruction.KN_DropMode);

			instruction.KN_DropMode = "HSL";
			AssertEquals("HSL", instruction.KN_DropMode);

			instruction.KN_DropMode = "ABC";
			AssertEquals("ABC", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			AssertEquals("", instruction.KN_DropMode);
		}

		#endregion

		#region TestKN_Sequence

		public void TestKN_Sequence()
		{
			AssertEquals(true, ((DtbTransportInstruction)GetNewBusinessObject()).KN_SequenceInfo.ReadOnly);
		}

		#endregion

		#region TestKN_Status

		public void TestKN_Status()
		{
			var transport = GetNewTransport();
			var pickUpInstruction = ((DtbTransportInstructionCollection<T>)transport.Instructions).AddNew(InstructionTypes.Codes.PickUp);
			var deliveryInstruction = ((DtbTransportInstructionCollection<T>)transport.Instructions).AddNew(InstructionTypes.Codes.Delivery);

			// status should always be readonly
			AssertEquals(TransportStatuses.Codes.Available, pickUpInstruction.KN_Status);
			AssertEquals(true, pickUpInstruction.KN_StatusInfo.ReadOnly);

			// changing the instruction status should recalculate the booking status
			pickUpInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			AssertEquals(TransportStatuses.Codes.PickedUp, transport.KM_Status);

			deliveryInstruction.KN_Status = TransportStatuses.Codes.Delivered;
			AssertEquals(TransportStatuses.Codes.Delivered, transport.KM_Status);
		}

		#endregion

		// calculated

		#region AuthorisedToLeave

		#region TestAuthorisedToLeave_Log

		public void TestAuthorisedToLeave_Log()
		{
			var transport = GetNewTransport();
			var pickupOrganisation = Helper.CreateOrganisation("PICSYD");
			var pickupAddress = Helper.AddAddressToOrganisation(pickupOrganisation, "Pickup Addy", OrgAddressType.Pickup);

			var deliveryOrganisation = Helper.CreateOrganisation("DELSYD");
			var deliveryAddress = Helper.AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery);

			var pickupInstruction = Helper.CreateInstruction(transport);
			pickupInstruction.Address.OrganisationPK = pickupOrganisation.PK;
			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			AssertEquals("Should default to pickup address", pickupAddress.PK, pickupInstruction.Address.E2_OA_Address);

			var deliveryInstruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.Delivery);
			deliveryInstruction.Address.OrganisationPK = deliveryOrganisation.PK;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			AssertEquals("Should default to delivery address", deliveryAddress.PK, deliveryInstruction.Address.E2_OA_Address);
			AssertEquals("ATL default value is false", false, deliveryInstruction.KN_IsAuthorisedToLeave);

			Factory.Save();
			AssertEquals("Should be no logs, as false is default.", false, deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.Authorised.Code));

			deliveryInstruction.KN_IsAuthorisedToLeave = true;
			Factory.Save();

			var message = "SE_Code Log Events in the collection: " + string.Join(",", deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Select(l => l.Event.SE_Code));
			AssertEquals(message, 1, deliveryInstruction.Logs.GetAllLogs().Count);
			var authorisedLog = deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Single();
			AssertEquals(true, deliveryInstruction.KN_IsAuthorisedToLeave);
			AssertEquals("The log recently added should be ATH", Events.Authorised.Code, authorisedLog.Event.SE_Code);
			AssertEquals("Free Text Reference.", "", authorisedLog.ReferenceFreeText);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", authorisedLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);

			deliveryInstruction.KN_IsAuthorisedToLeave = false;
			Factory.Save();

			message = "SE_Code Log Events in the collection: " + string.Join(",", deliveryInstruction.Logs.GetAllLogs().Cast<StmALog>().Select(l => l.Event.SE_Code));
			AssertEquals(message, 2, deliveryInstruction.Logs.GetAllLogs().Count);
			var authorisationWithdrawnLog = deliveryInstruction.Logs.GetAllLogs().Where(l => l != authorisedLog).Cast<StmALog>().Single();
			AssertEquals(false, deliveryInstruction.KN_IsAuthorisedToLeave);
			AssertEquals("The log recently added should be ATW", Events.AuthorisationWithdrawn.Code, authorisationWithdrawnLog.Event.SE_Code);
			AssertEquals("Free Text Reference.", "", authorisationWithdrawnLog.ReferenceFreeText);
			AssertEquals("Event Reference Parameter Type", "Authorised To Leave", authorisationWithdrawnLog.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type]);
		}

		#endregion

		#region TestAuthorisedToLeave_Default

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, true);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientYES()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, false);
		}

		public void TestDefaultAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientNO()
		{
			CheckDefaultAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, false);
		}

		void CheckDefaultAuthorityToLeave(string consignorATL, string consigneeATL, string clientATL, bool expectedDefault)
		{
			//Setup data
			var data = new ATLTestData(Helper);
			data.SetUpOrganizationTestData();
			data.PickupAddress1.OA_AuthorityToLeave = consignorATL;
			data.DeliveryAddress1.OA_AuthorityToLeave = consigneeATL;
			data.ClientAddress1.OA_AuthorityToLeave = clientATL;
			data.SetUpTransportTestData(GetNewTransport(), GetNewTransport());

			//Run test
			AssertEquals("Precondition: Check expectedDefault is correct", expectedDefault, data.DeliveryInstruction1.DefaultAuthorisedToLeave);
			AssertEquals("Check that ATL has been defaulted to the correct value", expectedDefault, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);
			AssertEquals("Pickup instruction should never be true for ATL", false, data.PickupInstruction1.KN_IsAuthorisedToLeave);
		}

		public void TestAuthorisedToLeave_ResetAfterOrgTypeChanged()
		{
			var transport = GetNewTransport();
			var pickInstruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var deliveryInstruction = (DtbTransportInstruction)transport.Instructions.AddNew();

			var pickOrganisation = Helper.CreateOrganisation("PICSYD");
			var pickupAddress = Helper.AddAddressToOrganisation(pickOrganisation, "Pickup Addy", OrgAddressType.Pickup);

			var deliveryOrganisation = Helper.CreateOrganisation("DELSYD");
			var deliveryAddress = Helper.AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery);

			pickInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			pickInstruction.Address.OrganisationPK = pickOrganisation.PK;
			pickInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;
			AssertEquals("Should default to pickup address", pickupAddress.PK, pickInstruction.Address.E2_OA_Address);

			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			deliveryInstruction.Address.OrganisationPK = deliveryOrganisation.PK;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;
			AssertEquals("Should default to delivery address", deliveryAddress.PK, deliveryInstruction.Address.E2_OA_Address);

			deliveryInstruction.KN_IsAuthorisedToLeave = true;

			Factory.Save();

			AssertEquals("ATL should be Ture", true, deliveryInstruction.KN_IsAuthorisedToLeave);

			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.WHS;

			AssertEquals("Should be set back to default(false) since wrong organization type.", false, deliveryInstruction.KN_IsAuthorisedToLeave);
		}

		#endregion

		public class ATLTestData
		{
			public ATLTestData(TransportCommonTestHelper helper)
			{
				this.helper = helper;
			}

			readonly TransportCommonTestHelper helper;

			internal void SetUpOrganizationTestData()
			{
				PickupOrganisation1 = helper.CreateOrganisation("PIC1SYD");
				PickupOrganisation1.OrganisationTypes = OrganisationTypes.Consignor;
				PickupAddress1 = helper.AddAddressToOrganisation(PickupOrganisation1, "Pickup1 Addy", OrgAddressType.Pickup);

				PickupOrganisation2 = helper.CreateOrganisation("PIC2SYD");
				PickupOrganisation2.OrganisationTypes = OrganisationTypes.Consignor;
				PickupAddress2 = helper.AddAddressToOrganisation(PickupOrganisation2, "Pickup2 Addy", OrgAddressType.Pickup);

				DeliveryOrganisation1 = helper.CreateOrganisation("DEL1SYD");
				DeliveryOrganisation1.OrganisationTypes = OrganisationTypes.Consignee;
				DeliveryAddress1 = helper.AddAddressToOrganisation(DeliveryOrganisation1, "Delivery1 Addy", OrgAddressType.Delivery);

				DeliveryOrganisation2 = helper.CreateOrganisation("DEL2SYD");
				DeliveryOrganisation2.OrganisationTypes = OrganisationTypes.Consignee;
				DeliveryAddress2 = helper.AddAddressToOrganisation(DeliveryOrganisation2, "Delivery2 Addy", OrgAddressType.Delivery);

				PickupOrganizationLink1 = PickupOrganisation1.BuyerLinks.AddNew(DeliveryOrganisation1);
				PickupOrganizationLink2 = PickupOrganisation2.BuyerLinks.AddNew(DeliveryOrganisation2);
				DeliveryOrganisation1.SupplierLinks.Add(PickupOrganizationLink1);
				DeliveryOrganisation2.SupplierLinks.Add(PickupOrganizationLink2);

				ClientOrganisation1 = helper.CreateOrganisation("CLI1SYD");
				ClientOrganisation1.OrganisationTypes = OrganisationTypes.TransportClient;
				ClientAddress1 = helper.AddAddressToOrganisation(ClientOrganisation1, "Client1 Addy", OrgAddressType.Miscellaneous);

				ClientOrganisation2 = helper.CreateOrganisation("CLI2SYD");
				ClientOrganisation2.OrganisationTypes = OrganisationTypes.TransportClient;
				ClientAddress2 = helper.AddAddressToOrganisation(ClientOrganisation2, "Client2 Addy", OrgAddressType.Miscellaneous);
			}

			internal void SetUpTransportTestData(DtbTransport transport1, DtbTransport transport2)
			{
				Transport1 = transport1;
				Transport2 = transport2;
				PickupInstruction1 = (DtbTransportInstruction)Transport1.Instructions.AddNew();
				DeliveryInstruction1 = (DtbTransportInstruction)Transport1.Instructions.AddNew();
				PickupInstruction2 = (DtbTransportInstruction)Transport2.Instructions.AddNew();
				DeliveryInstruction2 = (DtbTransportInstruction)Transport2.Instructions.AddNew();

				PickupInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
				PickupInstruction1.Address.OrganisationPK = PickupOrganisation1.PK;
				PickupInstruction1.OrganisationType = OrganisationTypesList.Codes.CNR;
				AssertEquals("Should default to pickup address1", PickupAddress1.PK, PickupInstruction1.Address.E2_OA_Address);

				PickupInstruction2.KN_InstructionType = InstructionTypes.Codes.PickUp;
				PickupInstruction2.Address.OrganisationPK = PickupOrganisation2.PK;
				PickupInstruction2.OrganisationType = OrganisationTypesList.Codes.CNR;
				AssertEquals("Should default to pickup address2", PickupAddress2.PK, PickupInstruction2.Address.E2_OA_Address);

				DeliveryInstruction1.KN_InstructionType = InstructionTypes.Codes.Delivery;
				DeliveryInstruction1.Address.OrganisationPK = DeliveryOrganisation1.PK;
				DeliveryInstruction1.OrganisationType = OrganisationTypesList.Codes.CNE;
				AssertEquals("Should default to delivery address1", DeliveryAddress1.PK, DeliveryInstruction1.Address.E2_OA_Address);

				DeliveryInstruction2.KN_InstructionType = InstructionTypes.Codes.Delivery;
				DeliveryInstruction2.Address.OrganisationPK = DeliveryOrganisation2.PK;
				DeliveryInstruction2.OrganisationType = OrganisationTypesList.Codes.CNE;
				AssertEquals("Should default to delivery address2", DeliveryAddress2.PK, DeliveryInstruction2.Address.E2_OA_Address);

				Job1 = new JobHeader.Loader(Transport1).TryCreate();
				Job1.JH_OA_LocalChargesAddr = ClientAddress1.PK;
				ClientAddress1 = DeliveryInstruction1.Booking.BillingPartyOrLocalClientAddress;

				Job2 = new JobHeader.Loader(Transport2).TryCreate();
				Job2.JH_OA_LocalChargesAddr = ClientAddress2.PK;
				ClientAddress2 = DeliveryInstruction2.Booking.BillingPartyOrLocalClientAddress;
			}

			internal DtbTransport Transport1;
			internal DtbTransport Transport2;
			internal DtbTransportInstruction PickupInstruction1;
			internal DtbTransportInstruction PickupInstruction2;
			internal DtbTransportInstruction DeliveryInstruction1;
			internal DtbTransportInstruction DeliveryInstruction2;
			internal OrgHeader PickupOrganisation1;
			internal OrgHeader PickupOrganisation2;
			internal OrgHeader DeliveryOrganisation1;
			internal OrgHeader DeliveryOrganisation2;
			internal OrgHeader ClientOrganisation1;
			internal OrgHeader ClientOrganisation2;
			internal OrgSupplierBuyerLink PickupOrganizationLink1;
			internal OrgSupplierBuyerLink PickupOrganizationLink2;
			internal OrgAddress PickupAddress1;
			internal OrgAddress PickupAddress2;
			internal OrgAddress DeliveryAddress1;
			internal OrgAddress DeliveryAddress2;
			internal OrgAddress ClientAddress1;
			internal OrgAddress ClientAddress2;
			internal JobHeader Job1;
			internal JobHeader Job2;
		}

		#endregion

		#endregion

		#region TestPackageCategory

		public void TestPackageCategory()
		{
			var transport = GetNewTransport();
			var container = GetPackageJob(transport).Packages.AddNew("CNT");
			var box = container.Packages.AddNew("BOX");
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			instruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			instruction.PackageCategory = "CNT";
			AssertContainsExactElementsInAnyOrder(new[] { container }, instruction.DivotsWithPackages.Packages);

			instruction.PackageCategory = "LSE";
			AssertContainsExactElementsInAnyOrder(new[] { box }, instruction.DivotsWithPackages.Packages);

			instruction.PackageCategory = "BTH";
			AssertContainsExactElementsInAnyOrder(new[] { container, box }, instruction.DivotsWithPackages.Packages);
		}

		protected abstract PkgPackageJob GetPackageJob(DtbTransport transport);

		#endregion

		#region TestOrganisationType

		public void TestOrganisationType()
		{
			var transport = GetNewTransport();
			var instruction = (T)transport.Instructions.AddNew();

			AssertOrganisationTypeSaves(instruction, "");

			foreach (CodeDescriptionPair pair in OrganisationTypesList.Instance)
			{
				AssertOrganisationTypeSaves(instruction, pair.Code);
			}
		}

		void AssertOrganisationTypeSaves(T instruction, ZString orgType)
		{
			instruction.OrganisationType = orgType;
			AssertEquals("OrganisationType was just set, so should be the same", orgType, instruction.OrganisationType);

			Factory.Save();

			var freshFactory = new BusinessObjectFactory();
			var instruction_InFreshFactory = freshFactory.Load<T>(instruction.PK);
			AssertEquals("Should have loaded with the same value", orgType, instruction_InFreshFactory.OrganisationType);
		}

		#endregion

		#region TestReqFrom

		public void TestReqFrom()
		{
			var transport = GetNewTransport();
			var instruction = (T)transport.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", ZDateTime.Empty, instruction.ReqFrom);

			var pickUpConfirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			var deliveryConfirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.ReqFrom);

			pickUpConfirmation.KK_RequiredFrom = ZDateTime.Now;
			deliveryConfirmation.KK_RequiredFrom = ZDateTime.Now.AddDays(1);
			AssertEquals("Should take latest date.", deliveryConfirmation.KK_RequiredFrom, instruction.ReqFrom);
		}

		#endregion

		#region TestReqTo

		public void TestReqTo()
		{
			var transport = GetNewTransport();
			var instruction = (T)transport.Instructions.AddNew();
			AssertEquals("No confirmations, no date.", ZDateTime.Empty, instruction.ReqTo);

			var pickUpConfirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			var deliveryConfirmation = (DtbTransportConfirmation)instruction.Confirmations.AddNew();
			pickUpConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			AssertEquals("Confirmations with no date.", ZDateTime.Empty, instruction.ReqTo);

			pickUpConfirmation.KK_RequiredTo = ZDateTime.Now;
			deliveryConfirmation.KK_RequiredTo = ZDateTime.Now.AddDays(1);
			AssertEquals("Should take latest date.", deliveryConfirmation.KK_RequiredTo, instruction.ReqTo);
		}

		#endregion

		#region Flags

		#region TestHasAContainer

		public void TestHasAContainer()
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			AssertEquals("Since there are no packages on the instruction, the instruction cannot have a container", false, instruction.HasAContainer);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = "CNT";
			instruction.DivotsWithPackages.AddPackage(container);
			AssertEquals("A container has been added as a package to the instruction, therefore the instruction should have a container", true, instruction.HasAContainer);

			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "BOX";
			instruction.DivotsWithPackages.AddPackage(package);
			AssertEquals("Only one package on the instruction needs to be a container for 'HasAContainer' to return true", true, instruction.HasAContainer);
		}

		#endregion

		#region TestIsDelivery

		public void TestIsDelivery()
		{
			AssertFlag("IsDelivery", InstructionTypes.Codes.Delivery, InstructionTypes.Codes.PickUp);
		}

		#endregion

		#region TestIsLoose

		public void TestIsLoose()
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			AssertEquals(false, instruction.IsLoose);

			var container = Factory.New<PkgPackage>();
			container.KP_F3_NKPackType = "CNT";

			instruction.DivotsWithPackages.AddPackage(container);
			AssertEquals(false, instruction.IsLoose);

			var package = Factory.New<PkgPackage>();
			instruction.DivotsWithPackages.AddPackage(package);
			AssertEquals("All packs need to be loose", false, instruction.IsLoose);

			var containerDivot = instruction.PackageDivots.Cast<DtbTransportInstructionPkgDivot>().First(d => d.KD_KP_Package == container.PK);
			containerDivot.Delete();
			AssertEquals("All packs are now loose", true, instruction.IsLoose);
		}

		#endregion

		#region TestIsPickUp

		public void TestIsPickUp()
		{
			AssertFlag("IsPickUp", InstructionTypes.Codes.PickUp, InstructionTypes.Codes.Delivery);
		}

		#endregion

		#region TestIsMulti

		public void TestIsMulti()
		{
			AssertFlag("IsMulti", InstructionTypes.Codes.Multi, InstructionTypes.Codes.Delivery);
		}

		#endregion

		#region TestIsOrgFlags

		public void TestIsOrgFlags()
		{
			AssertIsOrgFlags("CFS", "CNR", (i) => i.IsDepot);
			AssertIsOrgFlags("CTO", "CNR", (i) => i.IsCTO);
			AssertIsOrgFlags("CYD", "CNR", (i) => i.IsCYD);
		}

		void AssertIsOrgFlags(ZString org, ZString not, Func<DtbTransportInstruction, ZBool> isOrg)
		{
			var transport = GetNewTransport();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			instruction.OrganisationType = not;
			AssertEquals(false, isOrg(instruction));

			instruction.OrganisationType = org;
			AssertEquals(true, isOrg(instruction));

			instruction.OrganisationType = not;
			AssertEquals(false, isOrg(instruction));
		}

		#endregion

		#region TestIsOwnDepot

		public void TestIsOwnDepot()
		{
			var transport = GetNewTransport();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			instruction.OrganisationType = "CNR";
			AssertEquals(false, instruction.IsOwnDepot);

			instruction.OrganisationType = "CFS";
			AssertEquals(false, instruction.IsOwnDepot);

			instruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			AssertEquals(true, instruction.IsOwnDepot);

			instruction.OrganisationType = "CNE";
			AssertEquals(false, instruction.IsOwnDepot);
		}

		#endregion

		protected void AssertFlag(ZString flagPropertyName, ZString validCode, ZString invalidCode)
		{
			var instruction = (DtbTransportInstruction)GetNewBusinessObject();

			AssertEquals(false, instruction[flagPropertyName]);

			instruction.KN_InstructionType = validCode;
			AssertEquals(true, instruction[flagPropertyName]);

			instruction.KN_InstructionType = invalidCode;
			AssertEquals(false, instruction[flagPropertyName]);
		}

		#endregion

		#region DefaultDropMode

		public void TestSetDropModeFromInstructionAddress()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_AIREquipmentNeeded = "DM1";
			address.OA_LCLEquipmentNeeded = "DM2";
			address.OA_FCLEquipmentNeeded = "DM3";

			var containerPackage = Helper.CreatePackage("123", 1, Constants.PkgUnit.Container);
			var loosePackage = Helper.CreatePackage("456", 1, Constants.PkgUnit.Box);

			var transport = GetNewTransport();
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, address);
			instruction.KN_DropMode = "TST";
			instruction.DefaultDropMode();
			AssertEquals("If instruction have a Drop Mode preset, then it should NOT be overridden from Address.", "TST", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			instruction.DefaultDropMode();
			AssertEquals("If instruction doesn't have a Drop Mode and have no packages attached, then it should NOT be set from Address.", "", instruction.KN_DropMode);

			instruction.DivotsWithPackages.AddPackage(loosePackage); // adds Divot
			AssertEquals("If instruction doesn't have a Drop Mode and have only loose packages attached, then it should default LCL drop mode from Address.", "DM2", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			instruction.DivotsWithPackages.AddPackage(containerPackage);
			AssertEquals("If instruction doesn't have a Drop Mode and has loose and container packages attached, then it should default FCL drop mode from Address.", "DM3", instruction.KN_DropMode);

			instruction.KN_DropMode = "";
			instruction.PackageDivots.Cast<DtbTransportInstructionPkgDivot>().Single(d => d.KD_KP_Package == loosePackage.PK).Delete();
			AssertEquals("If instruction doesn't have a Drop Mode and has container packages attached, then it should default FCL drop mode from Address.", "DM3", instruction.KN_DropMode);

			instruction.KN_DropMode = "TST";
			instruction.DefaultDropMode();
			AssertEquals("If instruction have a Drop Mode set and packages attached, then it should not override preset drop mode from an Address.", "TST", instruction.KN_DropMode);
		}

		#endregion

		#region TestSuspendDefaultingDropMode

		public void TestSuspendDefaultingDropMode()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_AIREquipmentNeeded = "DM1";
			address.OA_LCLEquipmentNeeded = "DM2";
			address.OA_FCLEquipmentNeeded = "DM3";

			var transport = GetNewTransport();
			var instruction = Helper.CreateInstruction(transport, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CNE, address);
			instruction.KN_DropMode = "";

			using (instruction.SuspendDefaultingDropMode())
			{
				var loosePackage = Helper.CreatePackage("456", 1, Constants.PkgUnit.Box);
				instruction.DivotsWithPackages.AddPackage(loosePackage);
				AssertEquals("Drop mode defaulting is suspended, should remain empty.", "", instruction.KN_DropMode);

				using (instruction.SuspendDefaultingDropMode())
				{
					instruction.DefaultDropMode();
					AssertEquals("Drop mode defaulting is suspended, should remain empty.", "", instruction.KN_DropMode);
				}

				instruction.DefaultDropMode();
				AssertEquals("Drop mode defaulting is suspended, should remain empty.", "", instruction.KN_DropMode);
			}

			instruction.DefaultDropMode();
			AssertEquals("If instruction doesn't have a Drop Mode and have only loose packages attached, then it should default LCL drop mode from Address.", "DM2", instruction.KN_DropMode);
		}

		#endregion

		#region Delete

		#region TestDelete_DeletesConfirmations

		public void TestDelete_DeletesConfirmations()
		{
			var transport = GetNewTransport();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("Precondition", false, confirmation.IsDeleted);

			instruction.Delete();
			AssertEquals(true, instruction.IsDeleted);
			AssertEquals(true, confirmation.IsDeleted);
		}

		#endregion

		#region TestDelete_DeletesDocAddress

		public void TestDelete_DeletesDocAddress()
		{
			var transport = GetNewTransport();
			var instruction = (DtbTransportInstruction)transport.Instructions.AddNew();
			var jobDocAddress = instruction.Address;
			AssertEquals("Precondition", false, jobDocAddress.IsDeleted);

			instruction.Delete();
			AssertEquals(true, instruction.IsDeleted);
			AssertEquals(true, jobDocAddress.IsDeleted);
		}

		#endregion

		#region Delete

		public void TestDelete_Sequence()
		{
			var transport = GetNewTransport();
			var instruction1 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instruction2 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instruction3 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instruction4 = (DtbTransportInstruction)transport.Instructions.AddNew();
			var instruction5 = (DtbTransportInstruction)transport.Instructions.AddNew();

			AssertEquals("Precondition", 1, instruction1.KN_Sequence);
			AssertEquals("Precondition", 2, instruction2.KN_Sequence);
			AssertEquals("Precondition", 3, instruction3.KN_Sequence);
			AssertEquals("Precondition", 4, instruction4.KN_Sequence);
			AssertEquals("Precondition", 5, instruction5.KN_Sequence);

			instruction2.Delete();
			AssertEquals(1, instruction1.KN_Sequence);
			AssertEquals(2, instruction3.KN_Sequence);
			AssertEquals(3, instruction4.KN_Sequence);
			AssertEquals(4, instruction5.KN_Sequence);

			using (transport.Instructions.SuspendListChanged())
			{
				instruction4.Delete();
				AssertEquals(1, instruction1.KN_Sequence);
				AssertEquals(2, instruction3.KN_Sequence);
				AssertEquals(3, instruction5.KN_Sequence);

				instruction3.Delete();
				AssertEquals(1, instruction1.KN_Sequence);
				AssertEquals(2, instruction5.KN_Sequence);
			}
		}

		#endregion

		#endregion

		#region TestSuspendOnDocAddressChanged

		public void TestSuspendOnDocAddressChanged()
		{
			var data = new ATLTestData(Helper);
			data.SetUpOrganizationTestData();

			data.PickupAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.DeliveryAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.ClientAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.SetUpTransportTestData(GetNewTransport(), GetNewTransport());

			data.PickupInstruction1.Address.E2_OA_Address = data.PickupAddress1.PK;
			data.DeliveryInstruction1.Address.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Without the Delivery Address set, Is Authorised to leave should not be set.", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);

			data.DeliveryInstruction1.Address.E2_OA_Address = data.DeliveryAddress1.PK;
			AssertEquals("Precondition: Setting Delivery Address should update Is Authorised to leave.", true, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);

			data.DeliveryInstruction1.Address.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: Setting Delivery Address should update Is Authorised to leave.", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);

			using (data.DeliveryInstruction1.SuspendOnDocAddressChanged())
			{
				data.DeliveryInstruction1.Address.E2_OA_Address = data.DeliveryAddress1.PK;
				AssertEquals("When DocAddressChanged is suspended, Is Authorised to leave should *not* be updated.", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);
			}
		}

		#endregion

		#region TestUniversalCopyAttributes

		public void TestUniversalCopyAttributes()
		{
			var instruction = Factory.New<T>();

			var componentType = instruction.GetType();
			var organisationTypeInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "OrganisationType");
			AssertEquals("OrganisationType should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, organisationTypeInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));

			var docAddressesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "DocAddresses");
			AssertEquals("DocAddresses collection should have UniversalCopyCollectionEntityAttribute.", true, docAddressesInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), true).First() != null);
		}

		public void TestPropertyWithUniversalCopyAttribute()
		{
			var instruction = Factory.New<T>();
			var componentType = instruction.GetType();

			var propertyInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "KN_InstructionType");
			AssertEquals("KN_InstructionType should have UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning attribute.", true, propertyInfo.GetCustomAttributes(typeof(UniversalCopyAlwaysCopyPropertyAttribute), true)
				.Cast<UniversalCopyAlwaysCopyPropertyAttribute>().Any(attr => attr.Mode == UniversalCopyAlwaysCopyPropertyAttribute.CopyMode.AtTheBeginning));

			var propertyrelatedInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "Address");
			AssertEquals("Address should have UniversalCopyRelatedEntityAttribute.", true, propertyrelatedInfo.GetCustomAttributes(typeof(UniversalCopyRelatedEntityAttribute), true).Cast<UniversalCopyRelatedEntityAttribute>().Any(attr => attr.DisableCopyMethodLink && attr.CommaSeparatedSkipPropertiesNames == "E2_ParentID"));
		}

		#endregion

		#region TestUpdateStatus

		public void TestUpdateStatusWithDeletedDtbTransportInstruction()
		{
			var transport = GetNewTransport();
			var instruction = ((DtbTransportInstructionCollection<T>)transport.Instructions).AddNew(InstructionTypes.Codes.PickUp);

			instruction.Delete();

			Assert(instruction.IsDeleted);
			AssertNoExceptionThrown(() => instruction.UpdateStatus());
		}

		public void TestUpdateStatus()
		{
			var transport = GetNewTransport();
			var instruction = ((DtbTransportInstructionCollection<T>)transport.Instructions).AddNew(InstructionTypes.Codes.PickUp);

			instruction.KN_Status = "";
			AssertEquals("Precondition", "", instruction.KN_Status);

			instruction.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Available, instruction.KN_Status);

			TestUpdateStatusCore();
		}

		protected abstract void TestUpdateStatusCore();

		#endregion

		#region IDocAddresses Members

		#region TestIDocAddresses_DocAddresses

		public void TestIDocAddresses_DocAddresses()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			instruction.Address.E2_OA_Address = Helper.CreateOrganisation("ABCSYD").MainAddress.PK;
			AssertContainsExactElementsInAnyOrder(new JobDocAddress[] { instruction.Address }, instruction.DocAddresses);
			AssertEquals(true, instruction.IsRegisteredEditableChildObject(instruction.DocAddresses));
		}

		#endregion

		#region TestIDocAddresses_SupportedAddressTypes

		public void TestIDocAddresses_SupportedAddressTypes()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			var expected = new List<DocAddressType>();
			expected.Add(CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(""));

			foreach (CodeDescriptionPair pair in OrganisationTypesList.Instance)
			{
				expected.Add(CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(pair.Code));
			}

			AssertContainsExactElementsInAnyOrder(expected, iInstruction.SupportedAddressTypes);
		}

		#endregion

		#region TestIDocAddresses_CanDeleteAddress

		public void TestIDocAddresses_CanDeleteAddress()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			AssertEquals("Want to always have an Instruction Address.", false, iInstruction.CanDeleteAddress(instruction.Address));
		}

		#endregion

		#region TestIDocAddresses_GetCanOverrideCheckpoint

		public void TestIDocAddresses_GetCanOverrideCheckpoint()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;
			AssertEquals(Env.Security.TransportJobMISCDetails, iInstruction.GetCanOverrideCheckpoint(null));

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageCFS;
			AssertEquals(Env.Security.TransportJobMISCDetailsCFS, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageCTO;
			AssertEquals(Env.Security.TransportJobMISCDetailsCTO, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageExporter;
			AssertEquals(Env.Security.TransportJobMISCDetailsConsignor, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageImporter;
			AssertEquals(Env.Security.TransportJobMISCDetailsConsignee, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageYard;
			AssertEquals(Env.Security.TransportJobMISCDetailsContYard, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageService;
			AssertEquals(Env.Security.TransportJobMISCDetails, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));

			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.LocalCartageMSC;
			AssertEquals(Env.Security.TransportJobMISCDetails, iInstruction.GetCanOverrideCheckpoint(jobDocAddress));
		}

		#endregion

		#region TestIDocAddresses_GetDocAddressRequirement

		public void TestIDocAddresses_GetDocAddressRequirement()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			foreach (CodeDescriptionPair docAddressTypePair in OrganisationTypesList.Instance)
			{
				var docAddressType = CartageOrgTypeAddressTypeConverter.GetDocAddressTypeFromOrgType(docAddressTypePair.Code);

				foreach (CodeDescriptionPair instructionTypePair in new InstructionTypes().List)
				{
					instruction.KN_InstructionType = instructionTypePair.Code;

					AddressType reqAddressTypeWhenPickup;
					AddressType reqAddressTypeWhenDelivery;
					AddressType reqAddressTypeWhenLocal;

					switch (instructionTypePair.Code)
					{
						case InstructionTypes.Codes.PickUp:
							reqAddressTypeWhenPickup = AddressType.PIC;
							reqAddressTypeWhenDelivery = AddressType.PIC;
							reqAddressTypeWhenLocal = AddressType.PIC;
							break;

						case InstructionTypes.Codes.Delivery:
							reqAddressTypeWhenPickup = AddressType.DLV;
							reqAddressTypeWhenDelivery = AddressType.DLV;
							reqAddressTypeWhenLocal = AddressType.DLV;
							break;

						case InstructionTypes.Codes.Multi:
							reqAddressTypeWhenPickup = AddressType.PIC;
							reqAddressTypeWhenDelivery = AddressType.DLV;
							reqAddressTypeWhenLocal = AddressType.NoDefault;
							break;

						default:
							reqAddressTypeWhenPickup = AddressType.NoDefault;
							reqAddressTypeWhenDelivery = AddressType.NoDefault;
							reqAddressTypeWhenLocal = AddressType.NoDefault;
							break;
					}

					transport.KM_Direction = Constants.CartageDirection.Origin;
					AssertRequirement(docAddressType, reqAddressTypeWhenPickup, iInstruction);

					transport.KM_Direction = Constants.CartageDirection.Export;
					AssertRequirement(docAddressType, reqAddressTypeWhenPickup, iInstruction);

					transport.KM_Direction = Constants.CartageDirection.Destination;
					AssertRequirement(docAddressType, reqAddressTypeWhenDelivery, iInstruction);

					transport.KM_Direction = Constants.CartageDirection.Import;
					AssertRequirement(docAddressType, reqAddressTypeWhenDelivery, iInstruction);

					transport.KM_Direction = Constants.CartageDirection.Local;
					AssertRequirement(docAddressType, reqAddressTypeWhenLocal, iInstruction);
				}
			}
		}

		void AssertRequirement(DocAddressType docAddressType, AddressType reqAddressType, IDocAddresses iInstruction)
		{
			var requirement = iInstruction.GetDocAddressRequirement(docAddressType);

			AssertEquals(docAddressType, requirement.DefaultDocAddressType);
			AssertEquals(ContactType.LocalTransport, requirement.DefaultContactType);
			AssertEquals(reqAddressType, requirement.DefaultAddressType);
			AssertEquals(true, requirement.SaveEvenIfBlank);
		}

		#endregion

		#region TestIDocAddresses_PiggyBackedDocAddressValidation

		public void TestIDocAddresses_PiggyBackedDocAddressValidation()
		{
			TestIDocAddresses_PiggyBackedDocAddressValidationCore();
		}

		protected virtual void TestIDocAddresses_PiggyBackedDocAddressValidationCore()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;
			AssertNull(iInstruction.PiggyBackedDocAddressValidation(null));
		}

		#endregion

		#region TestIDocAddresses_GetOrgHeaderList

		public void TestIDocAddresses_GetOrgHeaderList()
		{
			var transport = (DtbTransport)Factory.New(ExpectedTransportType);
			var instruction = (T)transport.Instructions.AddNew();
			var iInstruction = (IDocAddresses)instruction;

			// implemented doc address types
			AssertEquals(typeof(DepotCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageCFS).GetType());
			AssertEquals(typeof(CTOCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageCTO).GetType());
			AssertEquals(typeof(ContainerYardCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageYard).GetType());
			AssertEquals(typeof(ConsigneeCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageImporter).GetType());
			AssertEquals(typeof(ConsignorCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageExporter).GetType());

			// not implemented doc address types
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageMSC).GetType());
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageWarehouse).GetType());
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.LocalCartageService).GetType());
			AssertEquals(typeof(OrgHeaderCollection), iInstruction.GetOrgHeaderList(DocAddressType.None).GetType());
		}

		#endregion

		#region TestIDocAddresses_OnDocAddressChangedSetsUtcDates

		public void TestIDocAddresses_OnDocAddressChangedSetsUtcDates()
		{
			var year = ZDateTime.Now.Year;

			var date = new ZDateTime(year, 9, 27);
			var auOrg = Helper.CreateOrganisation("AU");
			var usOrg = Helper.CreateOrganisation("US");
			var instruction = Helper.CreateInstruction(GetNewTransport());
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			auOrg.OH_RL_NKClosestPort = "AUSYD";
			usOrg.OH_RL_NKClosestPort = "USSFO";

			instruction.Address.E2_OA_Address = auOrg.MainAddress.PK;
			confirmation.KK_RequiredFrom = date;
			confirmation.KK_RequiredTo = date.AddHours(1);
			confirmation.KK_Estimated = date.AddHours(2);
			AssertEquals("Precondition", date.AddHours(-10), confirmation.KK_RequiredFromUtc);
			AssertEquals("Precondition", date.AddHours(-9), confirmation.KK_RequiredToUtc);
			AssertEquals("Precondition", date.AddHours(-8), confirmation.KK_EstimatedUtc);

			instruction.Address.E2_OA_Address = usOrg.MainAddress.PK;
			AssertEquals("After address change, Req From Utc should be updated.", date.AddHours(7), confirmation.KK_RequiredFromUtc);
			AssertEquals("After address change, Req To Utc should be updated.", date.AddHours(8), confirmation.KK_RequiredToUtc);
			AssertEquals("After address change, Estimated Utc should be updated.", date.AddHours(9), confirmation.KK_EstimatedUtc);
		}

		#endregion

		#endregion

		#region TestIConsignmentAddress_Properties

		public void TestIConsignmentAddress_Properties()
		{
			var transport = GetNewTransport();
			var instruction = ((DtbTransportInstructionCollection<T>)transport.Instructions).AddNew(InstructionTypes.Codes.PickUp);
			var equipment = Factory.New<RefEquipment>();

			instruction.KN_RQ_Equipment = equipment.PK;
			instruction.KN_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			instruction.KN_Status = TransportStatuses.Codes.Available;
			instruction.KN_ServiceInstruction = "Test Notes";

			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, instruction.DropMode);
			AssertEquals(TransportStatuses.Codes.Available, instruction.Status);
			AssertEquals("Test Notes", instruction.ServiceInstruction);
			AssertEquals(equipment, instruction.Equipment);
			AssertEquals(InstructionTypes.Codes.PickUp, instruction.ConsignmentAddressType);
		}

		#endregion

		#region Implementation

		protected abstract DtbTransportConsolidation GetNewConsolidation();
		protected abstract DtbTransport GetNewTransport();

		#endregion
	}
}
