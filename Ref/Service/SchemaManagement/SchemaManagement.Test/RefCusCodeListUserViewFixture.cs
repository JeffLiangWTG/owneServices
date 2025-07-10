using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	[CreateDatabase("F46929C23F0B4F11BCB97246B0703FC4", DbSchema.RefDbRepoSafe, ActionTargets.Test)]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class RefCusCodeListUserViewFixture
	{
		const string DbName = CreateDatabaseAttribute.DbNamePrefix + "F46929C23F0B4F11BCB97246B0703FC4";

		[SetUp]
		public void SetUp()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				context.RefDataGroupings.Add(new RefDataGrouping
				{
					ZZZ_PK = Guid.NewGuid(),
					ZZZ_DataGrouping = "ZA",
					ZZZ_Description = "South Africa"
				});
				context.RefCusCodeTypes.Add(new RefCusCodeType
				{
					ZZK_PK = Guid.NewGuid(),
					ZZK_CodeType = "PKG",
					ZZK_Description = "Package",
					ZZK_MaxLength = 0,
					ZZK_ZZZ_NKDataGrouping = "ZA"
				});
				context.SaveChanges();
			}
		}

		[Test]
		public void View()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				context.RefCusCodeListAttributeNames.Add(new RefCusCodeListAttributeName
				{
					ZXE_PK = Guid.NewGuid(),
					ZXE_Name = "AA",
					ZXE_ZZK_NKCodeType = "PKG",
					ZXE_ZZZ_NKDataGrouping = "ZA",
					ZXE_Description = "AA",
					ZXE_IsMandatory = false,
					ZXE_AllowDuplicates = false,
					ZXE_IsValueMandatory = false,
					ZXE_ValueDataType = string.Empty,
					ZXE_MinLengthOrValue = 0,
					ZXE_MaxLengthOrValue = 0,
					ZXE_DecimalPlaces = 0,
					ZXE_ColumnCaption = ""
				});
				context.SaveChanges();

				// Insert
				var codeListView = new RefCusCodeListUserView
				{
					ZZD_PK = Guid.NewGuid(),
					ZZD_CodeType = "PKG",
					ZZD_Code = "PE",
					ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
					ZZD_StartDate = new DateTime(1900, 01, 01),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZZD_CountryOrGrouping = "ZA",
					ZZD_IsAir = true,
					ZZD_IsPublished = true
				};
				var attributeView = new RefCusCodeListAttributeUserView
				{
					ZZE_PK = Guid.NewGuid(),
					ZZE_ZZD_CodeList = codeListView.ZZD_PK,
					ZZE_ZXE_NKName = "AA",
					ZZE_Value = "BB",
					ZZE_IsSea = true,
					ZZE_CodeType = "PKG",
					ZZE_CountryOrGrouping = "ZA"
				};
				context.RefCusCodeListUserViews.Add(codeListView);
				context.SaveChanges();
				context.RefCusCodeListAttributeUserViews.Add(attributeView);
				context.SaveChanges();

				Assert.That(context.DataSetChangeHistories.Where(x => x.DCH_ParentPK == codeListView.ZZD_PK).ToArray(), Has.Length.EqualTo(2));

				var code = context.RefCusCodeLists.FirstOrDefault();
				Assert.AreEqual("PE", code.ZZD_Code);
				var transport = context.RefCusCodeOrAttributeTransportModes.FirstOrDefault(x => x.ZZU_ZZD_CodeList == code.ZZD_PK);
				Assert.AreEqual("AIR", transport.ZZU_TransportMode);
				var attr = context.RefCusCodeListAttributes.FirstOrDefault();
				Assert.AreEqual("BB", attr.ZZE_Value);
				transport = context.RefCusCodeOrAttributeTransportModes.FirstOrDefault(x => x.ZZU_ZZE_Attribute == attr.ZZE_PK);
				Assert.AreEqual("SEA", transport.ZZU_TransportMode);

				// Update
				codeListView.ZZD_IsAir = false;
				codeListView.ZZD_IsSea = true;
				codeListView.ZZD_IsPublished = false;
				attributeView.ZZE_IsAir = true;
				attributeView.ZZE_IsSea = false;
				context.SaveChanges();
			}
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				var code = context.RefCusCodeLists.FirstOrDefault();
				Assert.AreEqual("PE", code.ZZD_Code);
				var transport = context.RefCusCodeOrAttributeTransportModes.FirstOrDefault(x => x.ZZU_ZZD_CodeList == code.ZZD_PK);
				Assert.AreEqual("SEA", transport.ZZU_TransportMode);
				var versionControl = context.RefDbVersionControls.FirstOrDefault(x => x.RVC_ParentPK == code.ZZD_PK);
				Assert.AreEqual(true, versionControl.RVC_Deleted);
				var attr = context.RefCusCodeListAttributes.FirstOrDefault();
				Assert.AreEqual("BB", attr.ZZE_Value);
				transport = context.RefCusCodeOrAttributeTransportModes.FirstOrDefault(x => x.ZZU_ZZE_Attribute == attr.ZZE_PK);
				Assert.AreEqual("AIR", transport.ZZU_TransportMode);
			}

			// Delete
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				var attrView = context.RefCusCodeListAttributeUserViews.FirstOrDefault();
				context.RefCusCodeListAttributeUserViews.Remove(attrView);
				context.SaveChanges();
				Assert.Null(context.RefCusCodeListAttributes.FirstOrDefault());
			}
		}

		/// <summary>
		/// Update a record with transport mode SEA to SEA & AIR
		/// </summary>
		[Test]
		public void UpdateFrom1To2TransportModes()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				// Insert
				var codeListView = new RefCusCodeListUserView
				{
					ZZD_PK = Guid.NewGuid(),
					ZZD_CodeType = "PKG",
					ZZD_Code = "PE",
					ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
					ZZD_StartDate = new DateTime(1900, 01, 01),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZZD_CountryOrGrouping = "ZA",
					ZZD_IsPublished = true
				};
				context.RefCusCodeListUserViews.Add(codeListView);
				context.SaveChanges();

				codeListView = context.RefCusCodeListUserViews.First(x => x.ZZD_PK == codeListView.ZZD_PK);
				Assert.That(codeListView.ZZD_IsSea, Is.False);

				// Update the first time
				codeListView.ZZD_IsSea = true;
				context.SaveChanges();

				// Update the second time, with the same value for IsSea, should be successful
				codeListView.ZZD_IsSea = true;
				codeListView.ZZD_IsAir = true;
				context.SaveChanges();

				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "SEA" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "AIR" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
			}
		}

		/// <summary>
		/// Insert a record with transport mode SEA & AIR then update the EndDate value, make sure it can still handle
		/// transport modes correctly
		/// </summary>
		[Test]
		public void UpdateNonTransportModeFields()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				// Insert
				var codeListView = new RefCusCodeListUserView
				{
					ZZD_PK = Guid.NewGuid(),
					ZZD_CodeType = "PKG",
					ZZD_Code = "PE",
					ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
					ZZD_StartDate = new DateTime(1900, 01, 01),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZZD_CountryOrGrouping = "ZA",
					ZZD_IsPublished = true
				};
				context.RefCusCodeListUserViews.Add(codeListView);
				context.SaveChanges();

				// Update the first time
				codeListView.ZZD_IsSea = true;
				codeListView.ZZD_IsAir = true;
				context.SaveChanges();

				// Update the second time
				codeListView.ZZD_EndDate = new DateTime(2024, 04, 17);
				context.SaveChanges();

				codeListView = context.RefCusCodeListUserViews.First(x => x.ZZD_PK == codeListView.ZZD_PK);
				Assert.That(codeListView.ZZD_IsSea, Is.True);
				Assert.That(codeListView.ZZD_EndDate, Is.EqualTo(new DateTime(2024, 04, 17)));

				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "SEA" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "AIR" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
			}
		}

		/// <summary>
		/// Update a record with transport mode ROA & SEA to AIR & RAI
		/// </summary>
		[Test]
		public void UpdateTransportModeNoOverlap()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				// Insert
				var codeListView = new RefCusCodeListUserView
				{
					ZZD_PK = Guid.NewGuid(),
					ZZD_CodeType = "PKG",
					ZZD_Code = "PE",
					ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
					ZZD_StartDate = new DateTime(1900, 01, 01),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZZD_CountryOrGrouping = "ZA",
					ZZD_IsPublished = true
				};
				context.RefCusCodeListUserViews.Add(codeListView);
				context.SaveChanges();

				// Update the first time
				codeListView.ZZD_IsSea = true;
				codeListView.ZZD_IsRoa = true;
				context.SaveChanges();

				// Update the second time
				codeListView.ZZD_IsSea = false;
				codeListView.ZZD_IsRoa = false;
				codeListView.ZZD_IsAir = true;
				codeListView.ZZD_IsRai = true;
				context.SaveChanges();

				codeListView = context.RefCusCodeListUserViews.First(x => x.ZZD_PK == codeListView.ZZD_PK);
				Assert.That(codeListView.ZZD_IsSea, Is.False);
				Assert.That(codeListView.ZZD_IsRoa, Is.False);
				Assert.That(codeListView.ZZD_IsAir, Is.True);
				Assert.That(codeListView.ZZD_IsRai, Is.True);

				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "SEA" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(0));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "ROA" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(0));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "AIR" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "RAI" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
			}
		}

		/// <summary>
		/// Update a record with transport mode ROA & SEA to ROA & AIR
		/// </summary>
		[Test]
		public void UpdateTransportModeWithOverlap()
		{
			using (var context = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
			{
				// Insert
				var codeListView = new RefCusCodeListUserView
				{
					ZZD_PK = Guid.NewGuid(),
					ZZD_CodeType = "PKG",
					ZZD_Code = "PE",
					ZZD_Description = "Pallet, modular, collars 80cms * 120cms ",
					ZZD_StartDate = new DateTime(1900, 01, 01),
					ZZD_EndDate = new DateTime(2079, 06, 06, 23, 59, 0),
					ZZD_CountryOrGrouping = "ZA",
					ZZD_IsPublished = true
				};
				context.RefCusCodeListUserViews.Add(codeListView);
				context.SaveChanges();

				// Update the first time
				codeListView.ZZD_IsSea = true;
				codeListView.ZZD_IsRoa = true;
				context.SaveChanges();

				// Update the second time
				codeListView.ZZD_IsSea = false;
				codeListView.ZZD_IsAir = true;
				context.SaveChanges();

				codeListView = context.RefCusCodeListUserViews.First(x => x.ZZD_PK == codeListView.ZZD_PK);
				Assert.That(codeListView.ZZD_IsSea, Is.False);
				Assert.That(codeListView.ZZD_IsRoa, Is.True);
				Assert.That(codeListView.ZZD_IsAir, Is.True);

				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "SEA" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(0));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "ROA" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
				Assert.That(context.RefCusCodeOrAttributeTransportModes.Where(x =>
						x.ZZU_TransportMode == "AIR" && x.ZZU_ZZD_CodeList == codeListView.ZZD_PK).ToList(),
					Has.Count.EqualTo(1));
			}
		}
	}
}
