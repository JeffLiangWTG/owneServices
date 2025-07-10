using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class UniversalTestHelper
	{
		#region Address
		public static OrgAddress CreateAddressInDB(OrganizationAddress addressDataObject, UniversalObjectFactory factory)
		{
			var orgAddress = new OrganisationDataObjectReader(addressDataObject, new TestErrorLogger(), factory).GetMatchedOrNewForTesting();
			factory.SaveForTesting();
			return orgAddress;
		}

		#endregion
		#region Instruction
		public static Instruction CreateInstructionDataObject(ZString instructionType, OrganizationAddress orgAddressDataObject = null)
		{
			return new Instruction(DefaultDataObjectWriterStrategy.TestInstance)
			{ Address = orgAddressDataObject, Type = new CodeDescriptionPair { Code = instructionType } };
		}

		#endregion
		#region Confirmation
		public static Confirmation CreateConfirmationDataObject(ZString confirmationType)
		{
			return new Confirmation { DateDescription = confirmationType };
		}

		public static Confirmation AddDivotConfirmation(Instruction instruction, ZString confirmationType)
		{
			var confirmation = CreateConfirmationDataObject(confirmationType);
			var instructionPackingLink = new InstructionPackingLineLink { ConfirmationCollection = new List<Confirmation> { confirmation } };
			if (instruction.InstructionPackingLineLinkCollection == null)
			{
				instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());
			}

			instruction.InstructionPackingLineLinkCollection.Add(instructionPackingLink);
			return confirmation;
		}

		public static Confirmation AddContainerDivotConfirmation(Instruction instruction, ZString confirmationType)
		{
			var confirmation = CreateConfirmationDataObject(confirmationType);
			var instructionContainerLink = new InstructionContainerLink { ConfirmationCollection = new List<Confirmation> { confirmation } };

			if (instruction.InstructionContainerLinkCollection == null)
			{
				instruction.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink>());
			}
			instruction.InstructionContainerLinkCollection.Add(instructionContainerLink);

			return confirmation;
		}

		public static Confirmation AddInstructionConfirmation(Instruction instruction, ZString confirmationType)
		{
			Confirmation confirmation;
			if (instruction.InstructionPackingLineLinkCollection == null || instruction.InstructionPackingLineLinkCollection.Count == 0)
			{
				confirmation = AddDivotConfirmation(instruction, confirmationType);
			}
			else
			{
				confirmation = CreateConfirmationDataObject(confirmationType);
				foreach (var instructionPackingLink in instruction.InstructionPackingLineLinkCollection)
				{
					if (instructionPackingLink.ConfirmationCollection == null)
					{
						instructionPackingLink.ConfirmationCollection = new List<Confirmation>();
					}

					instructionPackingLink.ConfirmationCollection.Add(confirmation);
				}
			}

			return confirmation;
		}

		#endregion
		#region Container
		public static Container CreateContainerDataObject(ZString containerID, ZInt? containerLink = null)
		{
			return new Container { ContainerNumber = containerID, ContainerType = new ContainerType { Code = "20GP" }, Link = containerLink };
		}

		public static void AddContainerLink(Instruction instruction, ZInt containerLink)
		{
			if (instruction.InstructionContainerLinkCollection == null)
			{
				instruction.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink>());
			}

			instruction.InstructionContainerLinkCollection.Add(new InstructionContainerLink { ContainerLink = containerLink });
		}

		#endregion
		#region Package
		public static PackingLine CreatePackageDataObject(ZString packageID, ZString packType, ZInt? packageLink = null)
		{
			return new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ReferenceNumber = packageID, PackType = new PackageType { Code = packType }, Link = packageLink, PackQty = 1 };
		}

		public static PackingLine CreatePackageDataObject(ZLong quantity, ZString packageID, ZString packType, ZInt? packageLink = null)
		{
			return new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ReferenceNumber = packageID, PackType = new PackageType { Code = packType }, Link = packageLink, PackQty = quantity };
		}

		public static PackingLine CreatePackageDataObject(ZInt quantity, ZString packageID, ZString packType, ZDecimal volume, ZDecimal weight, ZInt? packageLink = null)
		{
			return new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{ ReferenceNumber = packageID, PackType = new PackageType { Code = packType }, Link = packageLink, PackQty = new ZLong(quantity), Volume = volume, Weight = weight };
		}

		public static void AddPackageLink(Instruction instruction, ZInt packageLink, ZInt? quantity = null)
		{
			if (instruction.InstructionPackingLineLinkCollection == null)
			{
				instruction.SetInstructionPackingLineLinkCollection(() => new List<InstructionPackingLineLink>());
			}

			if (instruction.InstructionPackingLineLinkCollection.Any(l => l.PackingLineLink == packageLink))
			{
				throw new ArgumentException("This PackageLink already exists.");
			}

			instruction.InstructionPackingLineLinkCollection.Add(new InstructionPackingLineLink { PackingLineLink = packageLink, Quantity = quantity });
		}

		public static void AddContainerLink(Instruction instruction, ZInt containerLink, ZInt? quantity = null)
		{
			if (instruction.InstructionContainerLinkCollection == null)
			{
				instruction.SetInstructionContainerLinkCollection(() => new List<InstructionContainerLink>());
			}

			if (instruction.InstructionContainerLinkCollection.Any(l => l.ContainerLink == containerLink))
			{
				throw new ArgumentException("This ContainerLink already exists.");
			}

			instruction.InstructionContainerLinkCollection.Add(new InstructionContainerLink { ContainerLink = containerLink, Quantity = quantity });
		}
		#endregion
	}
}
