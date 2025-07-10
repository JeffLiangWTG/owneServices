using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	[TestedType(typeof(OutturnDataContextManager))]
	sealed class OutturnDataContextManagerTest : ShipmentDataContextManagerTestCase<OutturnDataContextManager, CusOutturn>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		public void TestEventContextValues()
		{
			var outturn = Factory.New<CusOutturn>();
			outturn.C5_MasterBill = "MB200413";
			outturn.C5_HouseBill = "HB200413";
			outturn.C5_ContainerNumber = "CON000";
			outturn.C5_CustomsStatus = "CLR";

			var dataContextManager = new OutturnDataContextManager();
			((IDataContextManager)(dataContextManager)).Init(outturn);

			var expectedValues = new[] { "MBOLNumber|MB200413", "HBOLNumber|HB200413", "ContainerNumber|CON000" };
			var actualValues = dataContextManager.EventContextValues.Select(c => $"{c.Key}|{c.Value}").ToArray();

			AssertArrayEqualsByElements(expectedValues, actualValues);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.COA };

		protected override void MakeShipmentUsableForThisRole(RecipientRoleType recipientRoleType, Shipment shipmentWithRecipientRole)
		{
			if (recipientRoleType == RecipientRoleType.COA)
			{
				((System.Collections.IList)shipmentWithRecipientRole.DataContext.DataSourceCollection).Clear();
				shipmentWithRecipientRole.DataContext.AddDataSource(DataContextType.TransitReceive, null);
			}
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			var header = Factory.BOFactory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			outturn.C5_MasterBill = "MB200413";
			outturn.C5_HouseBill = "HB200413";
			outturn.C5_ContainerNumber = "CON000";
			Factory.SaveForTesting();
		}

		protected override string ValidPopulatedUniversalShipmentXML => @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>TransitReceive</Type>
		  <Key>RC000001</Key>
		</DataSource>
	  </DataSourceCollection>
	</DataContext>
	<ContainerCollection>
	  <Container>
		<ContainerNumber>CON000</ContainerNumber>
		<ContainerType>
		  <Code>FCL</Code>
		  <Description>Full Container load</Description>
		</ContainerType>
		<IsSealOk>false</IsSealOk>
		<Link>1</Link>
		<Seal></Seal>
	  </Container>
	</ContainerCollection>
	<AdditionalReferenceCollection>
	  <AdditionalReference>
		<Type>
		  <Code>MAB</Code>
		</Type>
		<ReferenceNumber>MB200413</ReferenceNumber>
		<ContextInformation></ContextInformation>
		<IssueDate></IssueDate>
	  </AdditionalReference>
	  <AdditionalReference>
		<Type>
		  <Code>HSB</Code>
		</Type>
		<ReferenceNumber>HB200413</ReferenceNumber>
		<ContextInformation></ContextInformation>
		<IssueDate></IssueDate>
	  </AdditionalReference>
	</AdditionalReferenceCollection>
  </Shipment>
</UniversalShipment>";
	}
}
