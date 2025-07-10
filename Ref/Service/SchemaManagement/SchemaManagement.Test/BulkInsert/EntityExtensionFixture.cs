using System;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	[TestFixture]
	class EntityExtensionFixture
	{
		[Test]
		public void ToDataTable_WithICollectionProperty()
		{
			var dataTable = LanguageTypes.ToDataTable();
			Assert.AreEqual(nameof(RefLanguageType), dataTable.TableName);
			Assert.AreEqual(3, dataTable.Columns.Count);
			Assert.AreEqual(3, dataTable.Rows.Count);
			for (var i = 0; i < dataTable.Rows.Count; i++)
			{
				Assert.AreEqual(LanguageTypes[i].ZX6_PK, dataTable.Rows[i][nameof(RefLanguageType.ZX6_PK)]);
				Assert.AreEqual(LanguageTypes[i].ZX6_Language, dataTable.Rows[i][nameof(RefLanguageType.ZX6_Language)]);
				Assert.AreEqual(LanguageTypes[i].ZX6_Description, dataTable.Rows[i][nameof(RefLanguageType.ZX6_Description)]);
			}
		}

		[Test]
		public void ToDataTable_WithVirtualProperty()
		{
			var dataTable = RefCusMaps.ToDataTable();
			Assert.AreEqual(nameof(RefCusMap), dataTable.TableName);
			Assert.AreEqual(7, dataTable.Columns.Count);
			Assert.AreEqual(2, dataTable.Rows.Count);
			for (var i = 0; i < dataTable.Rows.Count; i++)
			{
				Assert.AreEqual(RefCusMaps[i].ZZM_PK, dataTable.Rows[i][nameof(RefCusMap.ZZM_PK)]);
				Assert.AreEqual(RefCusMaps[i].ZZM_CustomsValue, dataTable.Rows[i][nameof(RefCusMap.ZZM_CustomsValue)]);
				Assert.AreEqual(RefCusMaps[i].ZZM_ZZP_NKMapType, dataTable.Rows[i][nameof(RefCusMap.ZZM_ZZP_NKMapType)]);
				Assert.AreEqual(RefCusMaps[i].ZZM_ZZZ_NKDataGrouping, dataTable.Rows[i][nameof(RefCusMap.ZZM_ZZZ_NKDataGrouping)]);
				Assert.AreEqual(RefCusMaps[i].ZZM_CW1orCommercialValue, dataTable.Rows[i][nameof(RefCusMap.ZZM_CW1orCommercialValue)]);
				Assert.AreEqual(RefCusMaps[i].ZZM_StartDate, dataTable.Rows[i][nameof(RefCusMap.ZZM_StartDate)]);
				Assert.AreEqual(RefCusMaps[i].ZZM_EndDate, dataTable.Rows[i][nameof(RefCusMap.ZZM_EndDate)]);
			}
		}

		[Test]
		public void ToDataTable_WithNullableProperty()
		{
			var dataTable = RefShippingLines.ToDataTable();
			Assert.AreEqual(nameof(RefShippingLine), dataTable.TableName);
			Assert.AreEqual(20, dataTable.Columns.Count);
			Assert.True(dataTable.Columns.Contains(nameof(RefShippingLine.RSL_IsActive)));
		}

		[Test]
		public void ToDataTable_WithGeometryProperty()
		{
			var dataTable = PortPolygons.ToDataTable();
			var byteWriterGeography = new SqlServerBytesWriter() { IsGeography = true };
			Assert.AreEqual(nameof(RefPortPolygon), dataTable.TableName);
			Assert.AreEqual(3, dataTable.Columns.Count);
			Assert.AreEqual(2, dataTable.Rows.Count);
			Assert.AreEqual(typeof(object), dataTable.Columns[nameof(RefPortPolygon.RPP_SerializedPolygon)].DataType);
			for (var i = 0; i < dataTable.Rows.Count; i++)
			{
				Assert.AreEqual(PortPolygons[i].RPP_PK, dataTable.Rows[i][nameof(RefPortPolygon.RPP_PK)]);
				Assert.AreEqual(PortPolygons[i].RPP_PortId, dataTable.Rows[i][nameof(RefPortPolygon.RPP_PortId)]);
				Assert.AreEqual(byteWriterGeography.Write(PortPolygons[i].RPP_SerializedPolygon), dataTable.Rows[i][nameof(RefPortPolygon.RPP_SerializedPolygon)]);
			}
		}

		RefLanguageType[] LanguageTypes = new RefLanguageType[]
		{
			new RefLanguageType
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "Eng",
				ZX6_Description = "English"
			},
			new RefLanguageType
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "Fan",
				ZX6_Description = "Fanch"
			},
			new RefLanguageType
			{
				ZX6_PK = Guid.NewGuid(),
				ZX6_Language = "CH",
				ZX6_Description = "Chinese"
			}
		};

		RefCusMap[] RefCusMaps = new RefCusMap[]
		{
			new RefCusMap
			{
				ZZM_PK = Guid.NewGuid(),
				ZZM_CustomsValue = "Value1",
				ZZM_ZZP_NKMapType = "Type1",
				ZZM_ZZZ_NKDataGrouping = "AU",
				ZZM_CW1orCommercialValue = "Cw1Value1",
				ZZM_StartDate = new DateTime(2020,1,1),
				ZZM_EndDate = new DateTime(2021,1,1)
			},
			new RefCusMap
			{
				ZZM_PK = Guid.NewGuid(),
				ZZM_CustomsValue = "Value2",
				ZZM_ZZP_NKMapType = "Type2",
				ZZM_ZZZ_NKDataGrouping = "US",
				ZZM_CW1orCommercialValue = "Cw1Value2",
				ZZM_StartDate = new DateTime(2021,1,1),
				ZZM_EndDate = new DateTime(2022,1,1)
			}
		};

		RefShippingLine[] RefShippingLines = new RefShippingLine[]
		{
			new RefShippingLine
			{
				RSL_PK = Guid.NewGuid(),
				RSL_CargoWiseOneCode = "CW1",
				RSL_CarrierName = "ADD",
				RSL_StandardCarrierAlphaCode = "ALPA",
				RSL_CargoSphereRatesAvailable = false,
				RSL_ContainerAutomationAvailable = false,
				RSL_IsActive = true,
				RSL_OceanCarrierMessagingAvailable = true,
				RSL_EHubIds = "ids",
				RSL_GlobalSailingScheduleAvailable = false,
				RSL_InvoiceAvailable = false,
				RSL_IsCW1User = true,
				RSL_IsNVO = true
			}
		};

		RefPortPolygon[] PortPolygons = new RefPortPolygon[] {
			new RefPortPolygon
			{
				RPP_PK = Guid.NewGuid(),
				RPP_PortId = 1,
				RPP_SerializedPolygon = new Point(12.333056, 47.609722) { SRID = 4326 }
			},
			new RefPortPolygon
			{
				RPP_PK = Guid.NewGuid(),
				RPP_PortId = 2,
				RPP_SerializedPolygon = new Point(-27.333056, 25.609722) { SRID = 4326 }
			}
		};
	}
}
