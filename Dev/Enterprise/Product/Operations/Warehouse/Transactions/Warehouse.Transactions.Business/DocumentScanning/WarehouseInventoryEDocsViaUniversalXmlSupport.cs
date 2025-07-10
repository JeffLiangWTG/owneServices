using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WarehouseInventoryEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public ZString ExpectedCodeFormat => CodeParts.ExpectedCodeFormat;

		public ZString ExampleCodeFormat => "ABC|PALEALE|MEL|LOC1|ABCDEF|19-Sep-24|1|2|3|SN";

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			WhsInventoryView result = null;
			var codeParts = CodeParts.GetParsedCodeParts(code);
			if (codeParts != null)
			{
				var clientPK = ZGuid.Empty;
				var productPK = ZGuid.Empty;
				var locationPK = ZGuid.Empty;
				var warehousePK = GetWarehousePK(factory, codeParts.WarehouseCode);
				if (!warehousePK.IsEmpty)
				{
					clientPK = GetClientPK(factory, codeParts.ClientCode);
					if(!clientPK.IsEmpty)
					{
						productPK = GetProductPK(factory, codeParts.ProductCode, clientPK);
						if (!productPK.IsEmpty)
						{
							locationPK = GetLocationPK(factory, warehousePK, codeParts.LocationString);
						}
					}
				}

				if (!locationPK.IsEmpty)
				{
					var query = new ZQuery()
						.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m)
						.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, clientPK)
						.AddToFilter(new ZQuery(WhsInventoryViewSchema.WI_OP, SQLComparisonOperator.Equal, productPK))
						.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, warehousePK)
						.AddToFilter(WhsInventoryViewSchema.WI_WL, locationPK)
						.AddToFilter(WhsInventoryViewSchema.WI_ArrivalDate, SQLComparisonOperator.EqualToDatePartOnly, codeParts.ArrivalDate);

					if (!codeParts.PalletID.IsEmpty)
					{
						query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, codeParts.PalletID);
					}
					if (!codeParts.PartAttrib1.IsEmpty)
					{
						query.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib1, codeParts.PartAttrib1);
					}
					if (!codeParts.PartAttrib2.IsEmpty)
					{
						query.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib2, codeParts.PartAttrib2);
					}
					if (!codeParts.PartAttrib3.IsEmpty)
					{
						query.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib3, codeParts.PartAttrib3);
					}
					if (!codeParts.SerialNumber.IsEmpty)
					{
						query.AddToFilter(WhsInventoryViewSchema.WI_SerialNumber, codeParts.SerialNumber);
					}
					var matchedResults = factory.Load<WhsInventoryView>(query);

					if (matchedResults.Length == 1)
					{
						result = matchedResults[0];
					}
					else if (matchedResults.Length > 1)
					{
						throw new NonUniqueAllocationCodeException(Res.GetString("1139f2d3-4612-4daa-bcc2-eff889d899e4", "The unique code {0} you have supplied for Warehouse Inventory exists in multiple records.", code));
					}
				}
			}

			return result;
		}

		ZGuid GetClientPK(BusinessObjectFactory factory, ZString orgCode)
		{
			var orgHeader = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode);
			return orgHeader != null ? orgHeader.PK : ZGuid.Empty;
		}

		ZGuid GetWarehousePK(BusinessObjectFactory factory, ZString warehouseCode)
		{
			var warehouse = factory.LoadFromNaturalKey<WhsWarehouse>(WhsWarehouseSchema.WW_WarehouseCode, warehouseCode);
			return warehouse != null ? warehouse.PK : ZGuid.Empty;
		}

		ZGuid GetProductPK(BusinessObjectFactory factory, ZString productCode, ZGuid clientPK)
		{
			var productQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, productCode);
			productQuery.AddToFilter(OrgSupplierPartSchema.OP_IsActive, ZBool.True);
			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.Equal, clientPK);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both });
			productQuery.AddSubQuery(relationSubQuery, JoinCondition.And);
			var products = factory.Load<OrgSupplierPart>(productQuery);
			return products.Length == 1 ? products[0].PK : ZGuid.Empty;
		}

		ZGuid GetLocationPK(BusinessObjectFactory factory, ZGuid warehousePK, ZString locationString)
		{
			var location = WhsLocation.FindLocation(factory, locationString, warehousePK);
			return location != null ? location.PK : ZGuid.Empty;
		}

		class CodeParts
		{
			CodeParts(ZString clientCode, ZString productCode, ZString warehouseCode, ZString locationString, ZString palletID, ZDateTime arrivalDate,
				ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber)
			{
				ClientCode = clientCode;
				ProductCode = productCode;
				WarehouseCode = warehouseCode;
				LocationString = locationString;
				PalletID = palletID;
				ArrivalDate = arrivalDate;
				PartAttrib1 = partAttrib1;
				PartAttrib2 = partAttrib2;
				PartAttrib3 = partAttrib3;
				SerialNumber = serialNumber;
			}

			public static CodeParts GetParsedCodeParts(ZString code)
			{
				var parts = code.Split(Separator);
				return parts.Length == 10 ? PopulateValues(parts) : null;
			}

			public ZString ClientCode { get; }
			public ZString ProductCode { get; }
			public ZString WarehouseCode { get; }
			public ZString LocationString { get; }
			public ZString PalletID { get; }
			public ZDateTime ArrivalDate  { get; }
			public ZString PartAttrib1 { get; }
			public ZString PartAttrib2 { get; }
			public ZString PartAttrib3 { get; }
			public ZString SerialNumber { get; }

			public const string Separator = "|";

			public static ZString ExpectedCodeFormat
			{
				get
				{
					var properties = new[]
					{
						nameof(ClientCode),
						nameof(ProductCode),
						nameof(WarehouseCode),
						nameof(LocationString),
						nameof(PalletID),
						nameof(ArrivalDate),
						nameof(PartAttrib1),
						nameof(PartAttrib2),
						nameof(PartAttrib3),
						nameof(SerialNumber),
					};
					return string.Join(Separator, properties);
				}
			}

			static CodeParts PopulateValues(ZString[] parts)
			{
				var index = 0;
				var codeParts = new CodeParts(parts[index++], parts[index++], parts[index++], parts[index++], parts[index++], ParseDate(parts[index++]), parts[index++], parts[index++], parts[index++], parts[index++]);
				return Validate(codeParts) ? codeParts : null;
			}

			static bool Validate(CodeParts codeParts)
			{
				var propertiesToValidate = new[]
				{
					new { Name = nameof(ClientCode), Value = (IZType)codeParts.ClientCode },
					new { Name = nameof(ProductCode), Value = (IZType)codeParts.ProductCode },
					new { Name = nameof(WarehouseCode), Value = (IZType)codeParts.WarehouseCode },
					new { Name = nameof(LocationString), Value = (IZType)codeParts.LocationString },
					new { Name = nameof(ArrivalDate), Value = (IZType)codeParts.ArrivalDate },
				};
				var emptyProperties = propertiesToValidate.Where(v => v.Value.IsEmpty).Select(v => v.Name).ToArray();

				return emptyProperties.Length == 0 && codeParts.ArrivalDate.IsValid;
			}

			static ZDateTime ParseDate(ZString value)
			{
				ZDateTime.TryParseExact(value, out var result, ZDateTime.ShortDateFormat);
				return result;
			}
		}
	}
}
