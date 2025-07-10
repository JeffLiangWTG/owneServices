using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.ZA.Business.Testing
{
	public static class ExternalWarehouseInterchangeTestHelper
	{
		public static void SetUpTwoRealInterchangesForTest(BusinessObjectFactory factory, out EDIInterchange interchange1, out EDIInterchange interchange2)
		{
			var interchangeWithoutFutherElements1 = @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin>ZA</CountryOrigin>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

			var interchangeWithFurtherElements2 = @"<ExternalWarehouseBatch>
					<Batch>Batch2</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-25</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>12</Quantity>
							<TotalValue>1300.23</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin>ZA</CountryOrigin>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>10</Quantity>
							<TotalValue>24500.23</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin>ZA</CountryOrigin>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>3</BatchLineno>
							<TransactionType>REC</TransactionType>
							<ExportType/>
							<TransactionDate>2022-11-25</TransactionDate>
							<OwnerReference>11123</OwnerReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>12</Quantity>
							<TotalValue>345300.56</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin>ZA</CountryOrigin>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

			interchange2 = factory.New<EDIInterchange>();
			interchange1 = factory.New<EDIInterchange>();
			interchange1.EI_ApplicationCode = interchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange1.EI_InterchangeType = interchange2.EI_ApplicationCode = GenericMessageDeliveryInterchangeTypeList.Codes.ExternalWarehouse;
			interchange2.EI_BodyText = interchangeWithFurtherElements2;
			interchange1.EI_BodyText = interchangeWithoutFutherElements1;
			interchange2.EI_InterchangeNum = "X";
			interchange1.EI_InterchangeNum = "Y";
			interchange2.EI_ReceiveTransmit = interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange1.EI_Status = interchange2.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			factory.Save();
		}

		public static (OrgSupplierPart, OrgSupplierPart, OrgHeader) SetupWarehouseAndPart(BusinessObjectFactory factory)
		{
			var warehouse = factory.NewWithValidTestData<OrgHeader>();
			warehouse.OH_Code = "FORDWHS";
			var warehouseAddress = factory.NewWithValidTestData<OrgAddress>();
			warehouseAddress.OA_OH = warehouse.PK;
			warehouseAddress.OA_Code = "3 NEW RD";

			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "AB3921971ABSMR3";
			var ownRelation = part.RelatedOrganisations.AddNew();
			var owner = factory.New<OrgHeader>();
			owner.OH_Code = "FORDORG";
			ownRelation.OU_OH = owner.PK;
			ownRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var inactivePart = factory.New<OrgSupplierPart>();
			inactivePart.OP_PartNum = "AB3921971ABSMR4";
			inactivePart.OP_IsActive = false;
			inactivePart.RelatedOrganisations.AddOwner(owner);

			var part2 = factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PART2";
			var ownRelation2 = part2.RelatedOrganisations.AddNew();
			ownRelation2.OU_OH = owner.PK;
			ownRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			factory.Save();

			return (part, part2, owner);
		}

		public static ZString GetTestMessageMissingBatch() => @"<ExternalWarehouseBatch>
					<Batch></Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageMissingAndZeroBatchLineNo() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno></BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>0</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef2</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>ABC</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageDuplicatedBatchLineNo() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef2</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>REC</TransactionType>
							<ExportType></ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef3</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageMissingOwnerReference() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference></OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageInvalidProductOwner() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>INVALIDPRODUCT</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef2</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>INVALIDOWNER</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageInactiveProduct() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR4</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageInvalidWarehouse() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>INVALIDWHS</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef2</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageValid() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef2</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>3</BatchLineno>
							<TransactionType>REC</TransactionType>
							<ExportType></ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef3</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin></CountryOrigin>
							<IsFinal>False</IsFinal>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageValid2() => @"<ExternalWarehouseBatch>
					<Batch>Batch2</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111088</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>PART2</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>5</Quantity>
							<TotalValue>200.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";

		public static ZString GetTestMessageValidWithIsFinalReceipt() => @"<ExternalWarehouseBatch>
					<Batch>Batch1</Batch>
					<OrganizationAddress>
						<AddressType>Warehouse1</AddressType>
						<AddressShortCode>3 NEW RD</AddressShortCode>
						<OrganizationCode>FORDWHS</OrganizationCode>
					</OrganizationAddress>
					<ExternalWarehouseTransactionCollection>
						<ExternalWarehouseTransaction>
							<BatchLineno>1</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>EXP</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111089</OwnerReference>
							<LineReference>LineRef1</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>22</Quantity>
							<TotalValue>1800.2389</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
							<IsFinal>False</IsFinal>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>2</BatchLineno>
							<TransactionType>ORD</TransactionType>
							<ExportType>BLN</ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111090</OwnerReference>
							<LineReference>LineRef2</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin/>
							<IsFinal>False</IsFinal>
						</ExternalWarehouseTransaction>
						<ExternalWarehouseTransaction>
							<BatchLineno>3</BatchLineno>
							<TransactionType>REC</TransactionType>
							<ExportType></ExportType>
							<TransactionDate>2022-11-23</TransactionDate>
							<OwnerReference>111091</OwnerReference>
							<LineReference>LineRef3</LineReference>
							<ProductCode>AB3921971ABSMR3</ProductCode>
							<Owner>FORDORG</Owner>
							<Quantity>20</Quantity>
							<TotalValue>35234.00</TotalValue>
							<Currency>ZAR</Currency>
							<CountryOrigin></CountryOrigin>
							<IsFinal>True</IsFinal>
						</ExternalWarehouseTransaction>
					</ExternalWarehouseTransactionCollection>
				</ExternalWarehouseBatch>";
	}
}
