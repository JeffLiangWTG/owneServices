using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DuplicateProductDetectorNoBizOTest : TestCaseWithFactory
	{
		public void TestWhenNoRelatedOrgExists()
		{
			var duplicateDetector = new DuplicateProductDetectorNoBizO(ZGuid.NewZGuid(), "NOTADUPLICATE", true, new List<RelatedPartyWithCode>(), new List<PartAndFriends>());
			duplicateDetector.Validate();

			CombineAssertions(delegate
			{
				AssertEquals(".HasError", false, duplicateDetector.HasError);
				AssertEquals(".HasWarning", false, duplicateDetector.HasWarning);
			});
		}

		public void TestOwner()
		{
			AddOwner(Part1);
			AddOwner(Part2);

			var duplicateDetector = new DuplicateProductDetectorNoBizO(Part1.PK, Part1.OP_PartNum, Part1.OP_IsActive, DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(Part1), DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(Part1));
			duplicateDetector.Validate();

			CombineAssertions(delegate
			{
				AssertEquals(".HasError", true, duplicateDetector.HasError);
				AssertEquals(".HasWarning", false, duplicateDetector.HasWarning);
				AssertEquals(".OwnerCodeFromLastErrorOrWarning", OwnerCode, duplicateDetector.OwnerCodeFromLastErrorOrWarning.Trim());
				AssertEquals(".SupplierCodeFromLastErrorOrWarning", string.Empty, duplicateDetector.SupplierCodeFromLastErrorOrWarning.Trim());
			});
		}

		public void TestClassificationOrganization()
		{
			var classificationOrganization = OrgHeader.New(Factory);
			classificationOrganization.OH_Code = "CLS";
			var relation1 = Part1.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.ClassificationOrganization;
			relation1.OU_OH = classificationOrganization.PK;

			var relation2 = Part2.RelatedOrganisations.AddNew();
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.ClassificationOrganization;
			relation2.OU_OH = classificationOrganization.PK;

			var duplicateDetector = new DuplicateProductDetectorNoBizO(Part1.PK, Part1.OP_PartNum, Part1.OP_IsActive, DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(Part1), DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(Part1));
			duplicateDetector.Validate();

			CombineAssertions(delegate
			{
				AssertEquals(".HasError", true, duplicateDetector.HasError);
				AssertEquals(".HasWarning", false, duplicateDetector.HasWarning);
				AssertEquals(".ClassificationOrganizationCodeFromLastErrorOrWarning", "CLS", duplicateDetector.ClassificationOrganizationCodeFromLastErrorOrWarning.Trim());
			});
		}

		public void TestDeactivePartMessage()
		{
			AddSupplier(Part1);
			AddSupplier(Part2);
			AddOwner(Part1);
			AddOwner(Part2);
			var part3 = OrgSupplierPart.New(Factory);
			part3.OP_PartNum = Part1.OP_PartNum;
			var differentSupplier = OrgHeader.New(Factory);
			differentSupplier.OH_Code = "DIFFSUPPLIER";
			part3.RelatedOrganisations.AddSupplier(differentSupplier);
			part3.OP_IsActive = ZBool.False;
			var part4 = OrgSupplierPart.New(Factory);
			part4.OP_PartNum = Part1.OP_PartNum;
			part4.RelatedOrganisations.AddSupplier(differentSupplier);

			Part2.Validation.ValidateOP_PartNum();
			AssertHasErrorContaining(Part2.OP_PartNumInfo, "Duplicate Product detected:");
			AssertEquals(false, Part2.OP_PartNumInfo.HasWarnings());
			part4.Validation.ValidateOP_PartNum();
			AssertEquals(false, part4.OP_PartNumInfo.HasErrors());
			AssertHasWarningContaining(part4.OP_PartNumInfo, "Deactivated Duplicate Product detected");
			AssertHasWarningContaining(part4.OP_PartNumInfo, "Other Duplicates of this Product detected");

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}
			part4.HasChanges = true;
			part4.Validation.ValidateOP_PartNum();
			AssertEquals(false, part4.OP_PartNumInfo.HasErrors());
			AssertHasWarningContaining(part4.OP_PartNumInfo, "Deactivated Duplicate Product detected");
			AssertHasWarningContaining(part4.OP_PartNumInfo, "Other Duplicates of this Product detected");

			part3.Delete();
			Factory.Save();
			part4.HasChanges = true;
			part4.Validation.ValidateOP_PartNum();
			AssertEquals(false, part4.OP_PartNumInfo.HasErrors());
			AssertNoWarningContaining(part4.OP_PartNumInfo, "Deactivated Duplicate Product detected");
			AssertHasWarningContaining(part4.OP_PartNumInfo, "Other Duplicates of this Product detected");
		}

		public void TestCheckInactiveProducts()
		{
			AddOwner(Part1);
			AddOwner(Part2);

			Part2.OP_IsActive = false;
			Factory.Save();

			var duplicateDetectorForPart1 = new DuplicateProductDetectorNoBizO(
				Part1.PK,
				Part1.OP_PartNum,
				Part1.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(Part1),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(Part1)
			);
			var duplicateDetectorForPart2 = new DuplicateProductDetectorNoBizO(
				Part2.PK,
				Part2.OP_PartNum,
				Part2.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(Part2),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(Part2)
			);

			AssertEquals("default falue", true, duplicateDetectorForPart1.CheckInactiveProducts);
			AssertEquals("default falue", true, duplicateDetectorForPart2.CheckInactiveProducts);

			duplicateDetectorForPart1.Validate();
			duplicateDetectorForPart2.Validate();

			CombineAssertions(delegate
			{
				AssertEquals("Part1.HasError", false, duplicateDetectorForPart1.HasError);
				AssertEquals("Part1.HasWarning", true, duplicateDetectorForPart1.HasWarning);
				AssertEquals("Part1.WarningMessage", "Deactivated Duplicate Product detected : Owner = OWNER", duplicateDetectorForPart1.WarningMessage);
				AssertEquals("Part2.HasError", false, duplicateDetectorForPart2.HasError);
				AssertEquals("Part2.HasWarning", false, duplicateDetectorForPart2.HasWarning);
			});

			duplicateDetectorForPart1.CheckInactiveProducts = false;
			duplicateDetectorForPart2.CheckInactiveProducts = false;

			duplicateDetectorForPart1.Validate();
			duplicateDetectorForPart2.Validate();

			CombineAssertions(delegate
			{
				AssertEquals("Part1.HasError", false, duplicateDetectorForPart1.HasError);
				AssertEquals("Part1.HasWarning", false, duplicateDetectorForPart1.HasWarning);
				AssertEquals("Part2.HasError", false, duplicateDetectorForPart2.HasError);
				AssertEquals("Part2.HasWarning", false, duplicateDetectorForPart2.HasWarning);
			});
		}

		public void TestCanDeactivateDuplicateWithoutErrors()
		{
			AddOwner(Part1);
			AddOwner(Part2);

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			Part1.OP_IsActive = false;
			var duplicateDetector = new DuplicateProductDetectorNoBizO(
				Part1.PK,
				Part1.OP_PartNum,
				Part1.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(Part1),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(Part1)
			);
			duplicateDetector.CheckInactiveProducts = true;
			duplicateDetector.Validate();
			AssertEquals("deactivation of existing duplicate should not raise error", false, duplicateDetector.HasError);
			Factory.Save();
		}

		public void TestWarningOnValidNewWithOthersDuplicated()
		{
			AddSupplier(Part1);
			AddSupplier(Part2);
			AddOwner(Part1);
			AddOwner(Part2);
			AssertEquals(true, Part2.OP_PartNumInfo.HasErrors());
			OrgSupplierPart part3 = OrgSupplierPart.New(Factory);
			part3.OP_PartNum = Part1.OP_PartNum;
			OrgPartRelation relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			OrgHeader differentSupplier = OrgHeader.New(Factory);
			differentSupplier.OH_Code = "DIFFSUPPLIER";
			relation.OU_OH = differentSupplier.PK;
			AssertEquals(false, part3.OP_PartNumInfo.HasErrors());
			AssertEquals(true, part3.OP_PartNumInfo.HasWarnings());
		}

		public void TestWarningOnValidNewWithOthersDuplicatedWithBoth()
		{
			AddSupplier(Part1);
			AddSupplier(Part2);
			AddOwner(Part1);
			AddOwner(Part2);
			AssertEquals(true, Part2.OP_PartNumInfo.HasErrors());
			OrgSupplierPart part3 = OrgSupplierPart.New(Factory);
			part3.OP_PartNum = Part1.OP_PartNum;
			OrgPartRelation relation = part3.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			OrgHeader differentSupplier = OrgHeader.New(Factory);
			differentSupplier.OH_Code = "DIFFSUPPLIER";
			relation.OU_OH = differentSupplier.PK;
			AssertEquals(false, part3.OP_PartNumInfo.HasErrors());
			AssertEquals(true, part3.OP_PartNumInfo.HasWarnings());
		}

		public void TestOwnerCases()
		{
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			orgA.OH_FullName = "A";
			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			orgB.OH_FullName = "B";
			var orgC = Factory.NewWithValidTestData<OrgHeader>();
			orgB.OH_FullName = "C";

			CombineAssertions(() =>
			{
				AssertAreDuplicates(false, new[] { ("OWN", orgA) }, null);
				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, null);
				AssertAreDuplicates(false, new[] { ("BTH", orgA) }, null);
				AssertAreDuplicates(false, new[] { ("WCN", orgA) }, null);

				AssertAreDuplicates(true, new[] { ("OWN", orgA) }, new[] { ("OWN", orgA) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA) }, new[] { ("SUP", orgA) });
				AssertAreDuplicates(true, new[] { ("OWN", orgA) }, new[] { ("BTH", orgA) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA) }, new[] { ("WCN", orgA) });

				AssertAreDuplicates(true, new[] { ("BTH", orgA) }, new[] { ("OWN", orgA) });
				AssertAreDuplicates(true, new[] { ("BTH", orgA) }, new[] { ("SUP", orgA) });
				AssertAreDuplicates(true, new[] { ("BTH", orgA) }, new[] { ("BTH", orgA) });
				AssertAreDuplicates(false, new[] { ("BTH", orgA) }, new[] { ("WCN", orgA) });

				AssertAreDuplicates(false, new[] { ("OWN", orgA) }, new[] { ("OWN", orgB) });
				AssertAreDuplicates(false, new[] { ("BTH", orgA) }, new[] { ("BTH", orgB) });

				AssertAreDuplicates(true, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("OWN", orgA) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("SUP", orgA) });
				AssertAreDuplicates(true, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("BTH", orgA) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("WCN", orgA) });

				AssertAreDuplicates(false, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("OWN", orgB) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("SUP", orgB) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("BTH", orgB) });
				AssertAreDuplicates(false, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("WCN", orgB) });

				AssertAreDuplicates(true, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("OWN", orgA), ("SUP", orgB) });
				AssertAreDuplicates(true, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("BTH", orgA), ("SUP", orgB) });

				AssertAreDuplicates(true, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("OWN", orgA), ("SUP", orgC) });
				AssertAreDuplicates(true, new[] { ("OWN", orgA), ("SUP", orgB) }, new[] { ("BTH", orgA), ("SUP", orgC) });
			});
		}

		public void TestStandaloneSupplierCases()
		{
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			orgA.OH_FullName = "A";
			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			orgB.OH_FullName = "B";

			CombineAssertions(() =>
			{
				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("OWN", orgA) });
				AssertAreDuplicates(true, new[] { ("SUP", orgA) }, new[] { ("SUP", orgA) });
				AssertAreDuplicates(true, new[] { ("SUP", orgA) }, new[] { ("BTH", orgA) });

				AssertAreDuplicates(true, new[] { ("BTH", orgA) }, new[] { ("OWN", orgA) });
				AssertAreDuplicates(true, new[] { ("BTH", orgA) }, new[] { ("SUP", orgA) });
				AssertAreDuplicates(true, new[] { ("BTH", orgA) }, new[] { ("BTH", orgA) });

				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("SUP", orgB) });

				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("SUP", orgA), ("OWN", orgB) });
				AssertAreDuplicates(true, new[] { ("SUP", orgA) }, new[] { ("SUP", orgA), ("SUP", orgB) });
				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("SUP", orgA), ("BTH", orgB) });

				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("OWN", orgA), ("OWN", orgB) });
				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("OWN", orgA), ("SUP", orgB) });
				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("OWN", orgA), ("BTH", orgB) });

				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("BTH", orgA), ("OWN", orgB) });
				AssertAreDuplicates(true, new[] { ("SUP", orgA) }, new[] { ("BTH", orgA), ("SUP", orgB) });
				AssertAreDuplicates(false, new[] { ("SUP", orgA) }, new[] { ("BTH", orgA), ("BTH", orgB) });

				AssertAreDuplicates(true, new[] { ("SUP", orgA), ("SUP", orgB) }, new[] { ("SUP", orgA), ("SUP", orgB) });
				AssertAreDuplicates(true, new[] { ("SUP", orgA), ("SUP", orgB) }, new[] { ("SUP", orgA), ("BTH", orgB) });

				AssertAreDuplicates(true, new[] { ("SUP", orgA), ("BTH", orgB) }, new[] { ("SUP", orgA), ("SUP", orgB) });
				AssertAreDuplicates(true, new[] { ("SUP", orgA), ("BTH", orgB) }, new[] { ("SUP", orgA), ("BTH", orgB) });
			});
		}

		void AssertAreDuplicates(
			bool expectedToBeDuplicate,
			(string rel, OrgHeader org)[] part1Relations,
			(string rel, OrgHeader org)[] part2Relations
		)
		{
			ZString partNum = "DUPPART" + nextPartNum++;

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = partNum;

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = partNum;

			part1Relations = part1Relations ?? System.Array.Empty<(string rel, OrgHeader org)>();
			part2Relations = part2Relations ?? System.Array.Empty<(string rel, OrgHeader org)>();

			foreach (var relParams in part1Relations)
			{
				var rel = part1.RelatedOrganisations.AddNew();
				rel.OU_OH = relParams.org.PK;
				rel.OU_Relationship = relParams.rel;
			}

			foreach (var relParams in part2Relations)
			{
				var rel = part2.RelatedOrganisations.AddNew();
				rel.OU_OH = relParams.org.PK;
				rel.OU_Relationship = relParams.rel;
			}

			string part1RelationsAsString = string.Join(", ", part1Relations.Select(r => $"{r.rel} {r.org.OH_FullName}"));
			string part2RelationsAsString = string.Join(", ", part2Relations.Select(r => $"{r.rel} {r.org.OH_FullName}"));
			string testDescription = $"Part1({part1RelationsAsString}), Part2({part2RelationsAsString})";

			var duplicateDetectorForPart1BeforeSave = new DuplicateProductDetectorNoBizO(
				part1.PK,
				part1.OP_PartNum,
				part1.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(part1),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(part1)
			);
			var duplicateDetectorForPart2BeforeSave = new DuplicateProductDetectorNoBizO(
				part2.PK,
				part2.OP_PartNum,
				part2.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(part2),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(part2)
			);

			duplicateDetectorForPart1BeforeSave.Validate();
			duplicateDetectorForPart2BeforeSave.Validate();

			AssertEquals($"{testDescription}, Part1 is duplicate (before save)", expectedToBeDuplicate, duplicateDetectorForPart1BeforeSave.HasError);
			AssertEquals($"{testDescription}, Part2 is duplicate (before save)", expectedToBeDuplicate, duplicateDetectorForPart2BeforeSave.HasError);

			using (DuplicateProductTriggerSuspenderForTest.Suspend())
			{
				Factory.Save();
			}

			var duplicateDetectorForPart1AfterSave = new DuplicateProductDetectorNoBizO(
				part1.PK,
				part1.OP_PartNum,
				part1.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(part1),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(part1)
			);
			var duplicateDetectorForPart2AfterSave = new DuplicateProductDetectorNoBizO(
				part2.PK,
				part2.OP_PartNum,
				part2.OP_IsActive,
				DuplicateProductDetectorNoBizOHelper.GetPartiesOnPart(part2),
				DuplicateProductDetectorNoBizOHelper.GetOtherPartsInFactoryWithSameCode(part2)
			);

			duplicateDetectorForPart1AfterSave.Validate();
			duplicateDetectorForPart2AfterSave.Validate();

			AssertEquals($"{testDescription}, Part1 is duplicate (after save)", expectedToBeDuplicate, duplicateDetectorForPart1AfterSave.HasError);
			AssertEquals($"{testDescription}, Part2 is duplicate (after save)", expectedToBeDuplicate, duplicateDetectorForPart2AfterSave.HasError);
		}

		OrgSupplierPart Part1;
		OrgSupplierPart Part2;
		OrgHeader Supplier;
		OrgHeader Owner;

		const string OwnerCode = "OWNER";
		const string SupplierCode = "SUPPLIER";
		const string PartNumber = "ERTERTERT";

		int nextPartNum = 100;

		protected override void SetUp()
		{
			base.SetUp();

			Part1 = OrgSupplierPart.New(Factory);
			Part1.OP_PartNum = PartNumber;

			Part2 = OrgSupplierPart.New(Factory);
			Part2.OP_PartNum = PartNumber;

			Supplier = OrgHeader.New(Factory);
			Supplier.OH_Code = SupplierCode;

			Owner = OrgHeader.New(Factory);
			Owner.OH_Code = OwnerCode;
		}

		void AddOwner(OrgSupplierPart part)
		{
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation.OU_OH = Owner.PK;
		}

		void AddSupplier(OrgSupplierPart part)
		{
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation.OU_OH = Supplier.PK;
		}
	}
}
