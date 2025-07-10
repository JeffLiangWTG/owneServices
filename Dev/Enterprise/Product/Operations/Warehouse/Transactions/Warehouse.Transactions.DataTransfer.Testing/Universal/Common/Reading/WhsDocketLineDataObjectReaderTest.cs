using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	abstract class WhsDocketLineDataObjectReaderTest<TDocket, TDocketLine, TDocketLineReader> : WhsUniversalTestCase
		where TDocket : WhsDocket
		where TDocketLine : WhsDocketLine
		where TDocketLineReader : WhsDocketLineDataObjectReader<TDocket, TDocketLine>
	{
		#region Product

		#region TestExceptionIsThrownWhenProductCodeNotSupplied

		public void TestExceptionIsThrownWhenProductCodeNotSupplied()
		{
			var whs = Data.GetOrCreateWarehouseInDB();
			var whsDocket = GetNewDocketLineParent(Factory, whs.PK);

			Factory.SaveForTesting();

			var docketLineDataObject = new OrderLine();
			docketLineDataObject.Link = 3;
			var reader1 = GetNewReader(docketLineDataObject, Logger, whsDocket);
			AssertExceptionThrown("Cannot import DocketLine without valid Product Code.", typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0} Line 3\r\nNo Product was provided.", GetDocketType()), () => reader1.ReadIntoBusinessObject());

			docketLineDataObject.Product = new Product { Code = "BOWLHAT" };
			var reader2 = GetNewReader(docketLineDataObject, Logger, whsDocket);
			AssertNoExceptionThrown("DocketLine reads in fine.", () => reader2.ReadIntoBusinessObject());

			var docketLine = whsDocket.Lines.AddNew();
			docketLine.WE_LineNo = new ZShort(3);
			docketLine.WE_SubLineNo = new ZShort(1);
			docketLine.WE_TransactionQuantity = 1m;
			docketLine.WE_OP = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "BOWLHAT")).PK;
			docketLine.WE_AdjustmentArrivalDate = docketLine.WE_DocketLineType == "ORD" || docketLine.WE_DocketLineType == "WOR" || docketLine.WE_DocketLineType == "DWO" ? ZDateTimeOffset.Empty : ZDateTimeOffset.Today;
			if (NeedLocation)
			{
				docketLine.WE_WL = whs.DefaultLocation.PK;
			}

			Factory.SaveForTesting();

			docketLineDataObject.Product = null;
			docketLineDataObject.LineNumber = new ZShort(3);
			docketLineDataObject.SubLineNumber = new ZShort(1);
			var reader3 = GetNewReader(docketLineDataObject, Logger, whsDocket);
			AssertExceptionThrown("No Product is provided, should reject even if line no was matched.", typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0} Line 3\r\nNo Product was provided.", GetDocketType()), () => reader3.ReadIntoBusinessObject());

			docketLineDataObject.Product = new Product { Code = "INVALID" };
			var reader4 = GetNewReader(docketLineDataObject, Logger, whsDocket);
			AssertExceptionThrown("Cannot import DocketLine without valid Product Code.", typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0} Line 3\r\nUnable to match Product: INVALID for Client CRAHOLSYD.", GetDocketType()), () => reader4.ReadIntoBusinessObject());
		}

		#endregion

		#region TestExceptionIsThrownWhenMultipleProductCodeIsMatched

		public void TestExceptionIsThrownWhenMultipleProductCodeIsMatched()
		{
			using (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var whsDocket = GetNewDocketLineParent(Factory);
				var supplier1 = Factory.New<OrgHeader>();
				supplier1.OH_Code = "SUP1@#$";
				whsDocket.SupplierDocAddress.E2_OA_Address = supplier1.MainAddress.PK;
				Factory.SaveForTesting();

				var docketLineDataObject = new OrderLine();
				docketLineDataObject.Link = 3;
				docketLineDataObject.Product = new Product { Code = "BOWLHAT" };
				var reader = GetNewReader(docketLineDataObject, Logger, whsDocket);
				AssertNoExceptionThrown("DocketLine reads in fine.", () => reader.ReadIntoBusinessObject());

				var part = Factory.New<OrgSupplierPart>();
				part.OP_PartNum = "BOWLHAT";
				part.OP_Desc = "Bowler Hat";
				part.OP_StockKeepingUnit = "UNT";
				part.OP_RH_NKCommodityCode = "CMM";

				var supplier2 = Factory.New<OrgHeader>();
				supplier2.OH_Code = "SUP2@#$";
				part.RelatedOrganisations.AddOwner(whsDocket.Client);
				part.RelatedOrganisations.AddSupplier(supplier2);

				using (DuplicateProductTriggerSuspenderForTest.Suspend())
				{
					Factory.SaveForTesting();
				}

				reader = GetNewReader(docketLineDataObject, Logger, whsDocket);
				AssertExceptionThrown("Cannot import DocketLine with mulitple product match.", typeof(DataObjectReadFailureException),
					string.Format(@"Cannot Import {0} Line 3.
Multiple Product matched: BOWLHAT for Client {1}.", GetDocketType(), whsDocket.Client.OH_Code, supplier1.OH_Code), () => reader.ReadIntoBusinessObject());
			}
		}

		#endregion

		#region TestMatchProduct

		public void TestMatchProduct()
		{
			var whsDocket = Factory.NewWithValidTestData<TDocket>();
			whsDocket.WD_OH_Client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), Factory).GetMatchedOrNewForTesting().Header.PK;

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "BOWLHAT";
			part.OP_Desc = "Bowler Hat";

			var buyer = part.RelatedOrganisations.AddNew();
			buyer.OU_OH = whsDocket.WD_OH_Client;
			buyer.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			Factory.SaveForTesting();

			var docketLineDataObject = new OrderLine();
			docketLineDataObject.Link = 3;
			docketLineDataObject.Product = new Product { Code = "BOWLHAT", Description = "Bowler Hat" };
			var reader = GetNewReader(docketLineDataObject, Logger, whsDocket);
			AssertNoExceptionThrown("DocketLine reads in fine.", () => reader.ReadIntoBusinessObject());

			buyer.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.SaveForTesting();

			reader = GetNewReader(docketLineDataObject, Logger, whsDocket);
			AssertExceptionThrown("DocketLine requires valid product.", typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0} Line 3\r\nUnable to match Product: BOWLHAT - Bowler Hat for Client CRAHOLSYD.", GetDocketType()), () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestProductsWithSameCodeShouldNotImported

		public void TestProductsWithSameCodeShouldNotImported()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var supplier1 = Helper.CreateClient("Supplier1");
			var supplier2 = Helper.CreateClient("Supplier2");
			var docket = GetNewDocket(client, Data.GetOrCreateWarehouseInDB());
			var product1 = Helper.CreateProduct(client, "DupCode");
			product1.OP_IsActive = false;
			var product2 = Helper.CreateProduct(client, "DupCode");
			product1.RelatedOrganisations.AddSupplier(supplier1);
			product2.RelatedOrganisations.AddSupplier(supplier2);
			docket.SupplierDocAddress.E2_OA_Address = supplier1.MainAddress.PK;
			Factory.SaveForTesting();

			var docketLineDataObject = new OrderLine { Link = 3, Product = new Product { Code = "DupCode" } };
			var reader = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);

			AssertNoExceptionThrown("Should be able to find product based on supplier", () => reader.ReadIntoBusinessObject());

			product2.OP_IsActive = false;
			AssertExceptionThrown(typeof(DataObjectReadFailureException),
				string.Format("Cannot Import {0} Line 3\r\nUnable to match Product: DupCode for Client CRAHOLSYD.", GetDocketType()), () => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region TestExceptionIsThrownWhenProductCodeSuppliedIsInvalidWithRegistryOff

		public void TestExceptionIsThrownWhenProductCodeSuppliedIsInvalidWithRegistryOff()
		{
			if (SupportsProductCreation)
			{
				using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var docket = GetNewDocketLineParent(Factory);

					Factory.SaveForTesting();

					var docketLineDataObject = new OrderLine();
					docketLineDataObject.Link = 3;
					docketLineDataObject.Product = new Product { Code = "INVALID", Description = "LALA" };

					var reader = GetNewReader(docketLineDataObject, Logger, docket);
					AssertExceptionThrown("Cannot import Order Line without valid Product Code.", typeof(DataObjectReadFailureException),
						string.Format(@"Cannot Import {0} Line 3
Unable to match Product: INVALID - LALA for Client CRAHOLSYD.", GetDocketType()), () => reader.ReadIntoBusinessObject());
				}
			}
			else
			{
				Assert("This Docket Line Reader does not support Product Creation.", true);
			}
		}

		#endregion

		#region TestNewProductIsCreatedWhenProductCodeSuppliedIsInvalidWithRegistryOn

		public void TestNewProductIsCreatedWhenProductCodeSuppliedIsInvalidWithRegistryOn()
		{
			if (SupportsProductCreation)
			{
				using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var docket = GetNewDocketLineParent(Factory);
					Helper.SetClientAttributeType(docket.Client, AttributeNumber.One, true);
					Helper.SetClientAttributeType(docket.Client, AttributeNumber.Three, true);
					Helper.SetClientAttributeType(docket.Client, AttributeNumber.PackingDate, true);
					Helper.SetClientAttributeType(docket.Client, AttributeNumber.ExpiryDate, true);
					Helper.SetClientAttributeType(docket.Client, AttributeNumber.Serial, true);
					Factory.SaveForTesting();

					var docketLineDataObject1 = new OrderLine();
					docketLineDataObject1.Link = 3;
					docketLineDataObject1.Product = new Product { Code = "TEST", Description = "LALA" };
					docketLineDataObject1.OrderedQtyUnit = new CodeDescriptionPair { Code = "PLT" };
					docketLineDataObject1.PartAttribute1 = "Red";
					docketLineDataObject1.PartAttribute2 = "Medium";
					docketLineDataObject1.SerialNumber = "SER4";
					AssertProductCreatedOnImport(docket, docketLineDataObject1, "TEST", "LALA", "PLT", "Red", "Medium", "", "SER4", ZDate.Empty, ZDate.Empty,
						"Unable to match Product: TEST - LALA. New Product created.");

					Helper.SetClientAttributeType(docket.Client, AttributeNumber.Two, true);
					var docketLineDataObject2 = new OrderLine();
					docketLineDataObject2.Link = 4;
					docketLineDataObject2.Product = new Product { Code = "NEW PROD", Description = "" };
					docketLineDataObject2.PartAttribute2 = "Medium";
					docketLineDataObject2.PartAttribute3 = "B1234";
					docketLineDataObject2.PackingDate = new ZDate(2013, 1, 1);
					docketLineDataObject2.ExpiryDate = new ZDate(2013, 1, 2);
					AssertProductCreatedOnImport(docket, docketLineDataObject2, "NEW PROD", "NEW PROD", "UNT", "", "Medium", "B1234", "", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 1, 2),
						"Unable to match Product: NEW PROD. New Product created.");
				}
			}
			else
			{
				Assert("This Docket Line Reader does not support Product Creation.", true);
			}
		}

		void AssertProductCreatedOnImport(TDocket docket, OrderLine docketLineDataObject, ZString expectedCode, ZString expectedDescription, ZString expectedSKU, ZString expectedAttrib1,
			ZString expectedAttrib2, ZString expectedAttrib3, ZString expectedSerialnum, ZDateTime expectedPackingDate, ZDateTime expectedExpiryDate, ZString expectedLog)
		{
			Logger.ClearLogs();
			var reader = GetNewReader(docketLineDataObject, Logger, docket);
			var docketLine = reader.ReadIntoBusinessObject();
			var product = docketLine.SupplierPart;
			AssertNotNull(product);
			AssertEquals(expectedCode, product.OP_PartNum);
			AssertEquals(expectedDescription, product.OP_Desc);
			AssertEquals("When SKU not provided, new product should default to UNT.", expectedSKU, product.OP_StockKeepingUnit);

			var relationShip = product.RelatedOrganisations.FindByOrganisationAndRelationship(docket.Client, OrgPartRelation.RelationshipTypes.Owner);
			AssertNotNull(relationShip);

			var lineAttributes = GetLineAttributes(docketLine);
			AssertEquals(expectedAttrib1, lineAttributes.PartAttrib1);
			AssertEquals(expectedAttrib2, lineAttributes.PartAttrib2);
			AssertEquals(expectedAttrib3, lineAttributes.PartAttrib3);
			AssertEquals(expectedSerialnum, lineAttributes.SerialNumber);
			AssertEquals(expectedPackingDate, lineAttributes.PackingDate);
			AssertEquals(expectedExpiryDate, lineAttributes.ExpiryDate);
			AssertEquals("Part Attrib 1 flag was not set properly on Relationship", !expectedAttrib1.IsEmpty, relationShip.OU_UsePartAttrib1);
			AssertEquals("Part Attrib 2 flag was not set properly on Relationship", !expectedAttrib2.IsEmpty, relationShip.OU_UsePartAttrib2);
			AssertEquals("Part Attrib 3 flag was not set properly on Relationship", !expectedAttrib3.IsEmpty, relationShip.OU_UsePartAttrib3);
			AssertEquals("Serial number flag was not set properly on Relationship", !expectedSerialnum.IsEmpty, relationShip.OU_UseSerialNumber);
			AssertEquals("Packing Date flag was not set properly on Relationship", !expectedPackingDate.IsEmpty, relationShip.OU_UsePackingDate);
			AssertEquals("Expiry Date flag was not set properly on Relationship", !expectedExpiryDate.IsEmpty, relationShip.OU_UseExpiryDate);
		}

		protected virtual ILineAttributes GetLineAttributes(TDocketLine docketLine)
		{
			return docketLine;
		}

		#endregion

		#region TestShouldBeAbleToLoadAndValidateDocketLine

		public void TestShouldBeAbleToLoadAndValidateDocketLine()
		{
			var org = Data.CreateClientOrgCRAHOLSYDInDB();
			var docket = GetNewDocket(org, Data.GetOrCreateWarehouseInDB());

			var mainProduct = Helper.CreateProduct(org, "BOWLHAT");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(org, "BOM1");
			Helper.CreateProductBOM(mainProduct, bomComponentProduct, 1m, "UNT");

			Factory.SaveForTesting();

			Factory.BOFactory.ResumeValidation();
			var docketLineDataObject = new OrderLine { Link = 3, Product = new Product { Code = "BOWLHAT" } };
			var reader = GetNewReader(docketLineDataObject, Logger, docket, useCleanFactory: false);

			AssertNoExceptionThrown("Should be able load docketline.", () => reader.ReadIntoBusinessObject());
		}

		#endregion

		protected abstract bool SupportsProductCreation { get; }

		#endregion

		#region TestOrganisationLevelCustomFieldsAreImported

		public void TestOrganisationLevelCustomFieldsAreImported()
		{
			if (SupportsCustomFieldsImport)
			{
				var docket = GetNewDocketLineParent(Factory);
				var client = docket.Client;
				Data.SetupCustomLabels(client);
				Factory.SaveForTesting();

				var docketLineDataObject = new OrderLine();
				docketLineDataObject.Product = new Product { Code = "BOWLHAT" };
				Data.AddCustomFieldsToDataObject(docketLineDataObject, addTextBlobToCollection: true);

				var reader = GetNewReader(docketLineDataObject, Logger, docket);
				var docketLine = reader.ReadIntoBusinessObject();

				AssertNotNull(docketLine);
				CombineAssertions(delegate
				{
					AssertCustomFieldsImported(new WhsDocketLine.CustomLabelsProvider(docket), docketLine);
				});
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool SupportsCustomFieldsImport => true;

		#endregion

		#region AssertReadOnlyFields

		protected void AssertReadOnlyFieldIsUnchanged(IZType originalValue, IZType expectedNewValue, ZPropertyInfo info, bool isCustoms = false)
		{
			if (isCustoms)
			{
				AssertEquals($"Field {info.Name} must be modified for all customs transactions.", expectedNewValue, info.Value);
			}
			else if (info.ReadOnly && !info.Value.IsEmpty)
			{
				AssertEquals($"Field {info.Name} is read only but its value was modified.", originalValue, info.Value);
			}
			else if (!info.ReadOnly)
			{
				AssertEquals($"Field {info.Name} must be modified since it's not a Read Only field.", expectedNewValue, info.Value);
			}
		}

		#endregion

		#region Implementation

		protected abstract string GetDocketType();
		protected abstract TDocket GetNewDocket(OrgHeader client, WhsWarehouse warehouse);
		protected abstract TDocketLineReader GetNewReader(OrderLine docketLineDataObject, IXmlImportLogger logger, TDocket whsDocket, bool useCleanFactory = true);

		protected virtual bool NeedLocation => true;

		internal static TDocket GetNewDocketLineParent(UniversalObjectFactory factory, ZGuid? whsPK = null)
		{
			var docket = factory.NewWithValidTestData<TDocket>();
			docket.WD_OH_Client = new OrganisationDataObjectReader(GetNewAddressData_CRAHOLSYD(nameof(DocAddressType.ConsignorDocumentaryAddress)),
				new TestErrorLogger(), factory).GetMatchedOrNewForTesting().Header.PK;
			docket.WD_WW_Whs = whsPK ?? docket.WD_WW_Whs;

			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "BOWLHAT";
			part.OP_Desc = "Bowler Hat";
			part.OP_StockKeepingUnit = "UNT";
			part.OP_RH_NKCommodityCode = "CMM";

			if (docket is WhsOrder order)
			{
				docket.WD_DocketSubType = "CUS"; // For testing customs data
				order.ConsigneeAddressPK = factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			}
			else if (docket is WhsWorkOrder)
			{
				docket.WD_DocketSubType = "ASS";
				// We have no special logic for customs in WhsWorkOrder import/export yet

				var component = factory.New<OrgSupplierPart>();
				component.OP_PartNum = "HATRIB";
				component.OP_Desc = "Bowler Hat Ribbon";
				component.OP_StockKeepingUnit = "UNT";
				component.OP_RH_NKCommodityCode = "CMM";

				var bomComponent = factory.New<OrgPartBOM>();
				bomComponent.OE_OP_MainProduct = part.PK;
				bomComponent.OE_OP_Component = component.PK;
				bomComponent.OE_F3_NKPackType = "UNT";
				bomComponent.OE_ComponentQty = 1m;
			}
			else if (docket is WhsDynamicWorkOrder)
			{
				var component = factory.New<OrgSupplierPart>();
				component.OP_PartNum = "HATCOMP";
				component.OP_Desc = "Bowler Hat Component";
				component.OP_StockKeepingUnit = "UNT";
				component.OP_RH_NKCommodityCode = "CMM";

				var componentOwner = component.RelatedOrganisations.AddNew();
				componentOwner.OU_OH = docket.WD_OH_Client;
				componentOwner.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			}

			var buyer = part.RelatedOrganisations.AddNew();
			buyer.OU_OH = docket.WD_OH_Client;
			buyer.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			return docket;
		}

		#endregion
	}
}
