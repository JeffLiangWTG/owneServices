using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[TransactionedTestCase]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
class RefCusCodeListUserViewFixture
{
	[Test]
	public void Triggers()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connectionString = TestConnectionString.GetAdmin(dbName);
		using (var entities = new StagingRepository(connectionString))
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
			entities.Add(codeListView);
			entities.SaveChanges();
			entities.Add(attributeView);
			entities.SaveChanges();
			var code = entities.Get<RefCusCodeList>().FirstOrDefault();
			Assert.AreEqual("PE", code.ZZD_Code);
			var transport = entities.Get<RefCusCodeOrAttributeTransportMode>().FirstOrDefault(x => x.ZZU_ZZD_CodeList == code.ZZD_PK);
			Assert.AreEqual("AIR", transport.ZZU_TransportMode);
			var attr = entities.Get<RefCusCodeListAttribute>().FirstOrDefault();
			Assert.AreEqual("BB", attr.ZZE_Value);
			transport = entities.Get<RefCusCodeOrAttributeTransportMode>().FirstOrDefault(x => x.ZZU_ZZE_Attribute == attr.ZZE_PK);
			Assert.AreEqual("SEA", transport.ZZU_TransportMode);

			// Update
			codeListView.ZZD_IsAir = false;
			codeListView.ZZD_IsSea = true;
			codeListView.ZZD_IsPublished = false;
			attributeView.ZZE_IsAir = true;
			attributeView.ZZE_IsSea = false;
			entities.Update(codeListView);
			entities.Update(attributeView);
			entities.SaveChanges();
		}
		using (var entities = new StagingRepository(connectionString))
		{
			var code = entities.Get<RefCusCodeList>().FirstOrDefault();
			Assert.AreEqual("PE", code.ZZD_Code);
			var transport = entities.Get<RefCusCodeOrAttributeTransportMode>().FirstOrDefault(x => x.ZZU_ZZD_CodeList == code.ZZD_PK);
			Assert.AreEqual("SEA", transport.ZZU_TransportMode);
			var dpi = entities.Get<DataProcessingInformation>().FirstOrDefault(x => x.DPI_ParentPk == code.ZZD_PK);
			Assert.AreEqual("Delete", dpi.DPI_Message);
			var attr = entities.Get<RefCusCodeListAttribute>().FirstOrDefault();
			Assert.AreEqual("BB", attr.ZZE_Value);
			transport = entities.Get<RefCusCodeOrAttributeTransportMode>().FirstOrDefault(x => x.ZZU_ZZE_Attribute == attr.ZZE_PK);
			Assert.AreEqual("AIR", transport.ZZU_TransportMode);
		}

		// Delete
		using (var entities = new StagingRepository(connectionString))
		{
			var codeView = entities.Get<RefCusCodeListUserView>().FirstOrDefault();
			var attrView = entities.Get<RefCusCodeListAttributeUserView>().FirstOrDefault();
			entities.Remove(attrView);
			entities.Remove(codeView);
			entities.SaveChanges();
			Assert.Null(entities.Get<RefCusCodeList>().FirstOrDefault());
			Assert.Null(entities.Get<RefCusCodeListAttribute>().FirstOrDefault());
		}
	}
}
