using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(DocUSOrganisation))]
	sealed class DocUSOrganisationTest : GenericWrapperWithNotesTest
	{
		public void TestNew()
		{
			AssertNull("New", DocUSOrganisation.New(null, Factory));
			helper.USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("New", DocUSOrganisation.New(helper.USOrganisation, Factory));
			helper.USOrganisation.ZO_OH_Organisation = helper.Supplier.PK;
			AssertNotNull("New", DocUSOrganisation.New(helper.USOrganisation, Factory));
		}

		public void TestNewWhenUSOrganisationIsDeleted()
		{
			helper.USOrganisation.ZO_OH_Organisation = helper.Supplier.PK;
			helper.USOrganisation.Delete();
			AssertNull("Null", DocUSOrganisation.New(helper.USOrganisation, Factory));
		}

		public void TestNewWhenUSOrganisationDocAddressIsDeleted()
		{
			helper.USOrganisation.ZO_OH_Organisation = helper.Supplier.PK;
			helper.USOrganisation.USOrganisationDocAddress.Delete();
			AssertNull("Null", DocUSOrganisation.New(helper.USOrganisation, Factory));
		}

		public void TestLocationAddress()
		{
			helper.USOrganisation.ZO_OA_Address = helper.SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("LocationAddress's type", typeof(Enterprise.DocumentWrappers.DocDocAddress), orgWrapper.LocationAddress.GetType());
			helper.USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
		}

		#region Implementation

		DocUSOrganisation orgWrapper;
		USOrganisationWrapperTestHelper helper;

		public override void TestWrapperMappingsEmpty()
		{
			//If the source is null, this wrapper is null, so let's ignore this test
			Assert(true);
		}

		protected override ZString ExpectedDefaultFormatting => @"
EXFreightBillTo : DUMMY SUPPLIER COMPANY
IMFreightBillTo : DUMMY SUPPLIER COMPANY
Registry : (No Default Field Value Available on Registry)
";
		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return DocUSOrganisation.New(helper.USOrganisation, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new USOrganisationWrapperTestHelper(Factory);
			orgWrapper = DocUSOrganisation.New(helper.USOrganisation, Factory);
			AssertNotNull("OrgWrapper created not null", orgWrapper);
		}

		protected override string ExpectedFieldMap => @"
DocUSOrganisation                                (Default Field: Name)
======================================================================
Name                                    Type
----------------------------------------------------------------------
EXFreightBillTo                         DocOrganisation
IMFreightBillTo                         DocOrganisation
ABN                                     String
AdditionalAddressInformation            String
Address1                                String
Address2                                String
ApprovedExporterCode                    String
ARCreditManagementNote                  String
BusinessRegNo                           String
BusinessRegType                         String
CarrierAirlinePrefix                    String
CarrierMasterBillPrefix                 String
CartageInstructions                     String
CBR                                     String
CCC                                     String
CCD                                     String
CID                                     String
City                                    String
Code                                    String
Context                                 String
CSC                                     String
CurrentDate                             DateTime
CustomAttrib1                           String
CustomAttrib2                           String
CustomAttrib3                           String
CustomDate1                             DateTime
CustomDate2                             DateTime
CustomDate3                             DateTime
CustomDecimal1                          Decimal
CustomDecimal2                          Decimal
CustomDecimal3                          Decimal
CustomFlag1                             Bool
CustomFlag2                             Bool
CustomFlag3                             Bool
DGContactName                           String
DGContactPhone                          String
DisbursmentTerms                        String
ECRCode                                 String
EIN                                     String
Email                                   String
Fax                                     String
GST                                     String
HandlingInstructions                    String
HasPartAttrib1                          Bool
HasPartAttrib2                          Bool
HasPartAttrib3                          Bool
HasSerialNumber                         Bool
IsActive                                Bool
IsAirCTO                                Bool
IsAirLine                               Bool
IsAirWholesaler                         Bool
IsBroker                                Bool
IsCompetitor                            Bool
IsConsignee                             Bool
IsConsignor                             Bool
IsContainerPark                         Bool
IsCreditor                              Bool
IsDebtor                                Bool
IsForwarder                             Bool
IsInlandWaterwayProvider                Bool
IsLineHaulProvider                      Bool
IsLocalTransport                        Bool
IsMiscFreightServices                   Bool
IsPackDepot                             Bool
IsRailProvider                          Bool
IsSalesLead                             Bool
IsSeaCTO                                Bool
IsSeaWholesaler                         Bool
IsShippingLine                          Bool
IsShippingProvider                      Bool
IsTempAccount                           Bool
IsTransportClient                       Bool
IsUnpackDepot                           Bool
IsUserFlag1                             Bool
IsUserFlag10                            Bool
IsUserFlag11                            Bool
IsUserFlag12                            Bool
IsUserFlag13                            Bool
IsUserFlag14                            Bool
IsUserFlag15                            Bool
IsUserFlag16                            Bool
IsUserFlag17                            Bool
IsUserFlag18                            Bool
IsUserFlag19                            Bool
IsUserFlag2                             Bool
IsUserFlag20                            Bool
IsUserFlag21                            Bool
IsUserFlag22                            Bool
IsUserFlag23                            Bool
IsUserFlag24                            Bool
IsUserFlag25                            Bool
IsUserFlag26                            Bool
IsUserFlag27                            Bool
IsUserFlag28                            Bool
IsUserFlag29                            Bool
IsUserFlag3                             Bool
IsUserFlag30                            Bool
IsUserFlag31                            Bool
IsUserFlag32                            Bool
IsUserFlag4                             Bool
IsUserFlag5                             Bool
IsUserFlag6                             Bool
IsUserFlag7                             Bool
IsUserFlag8                             Bool
IsUserFlag9                             Bool
IsWarehouseClient                       Bool
Kennitala                               String
LocalBusinessRegNo                      String
LocalCustomsCarrierCode                 String
LocalCustomsClientCode                  String
LocalCustomsSupplierCode                String
LocalRebateUserCode                     String
LocalVATCode                            String
LSC                                     String
Mobile                                  String
Name                                    String
OrgType                                 String
PAN                                     String
PartAttrib1Name                         String
PartAttrib2Name                         String
PartAttrib3Name                         String
Phone                                   String
PostalAddress                           String
PostalAddress1                          String
PostalAddress2                          String
PostalAddressCity                       String
PostalAddressExcludeCountryIfSame       String
PostalAddressExcludeName                String
PostalAddressInEnglish                  String
PostalAddressPostCode                   String
PostCode                                String
SCAC                                    String
ShortDisbursementTerms                  String
ShortInvoiceTerms                       String
SplitFullName1                          String
SplitFullName2                          String
SSN                                     String
SSNorEIN                                String
State                                   String
Web                                     String
Notes                                   Note Collection
NotesIncludingRelated                   Note Collection
";

		#endregion

		protected override Enterprise.DocumentWrappers.DocBaseWrapper GetNewDocumentWrapper()
		{
			if (helper == null)
			{
				helper = new USOrganisationWrapperTestHelper(Factory);
			}

			return DocUSOrganisation.New(helper.USOrganisation, Factory);
		}

		public override void TestWrapperNotes()
		{
			helper.USOrganisation.Organisation.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "These notes exist against the organisation and should be algamated");
			helper.USOrganisation.Organisation.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "This is the second line that gets added to the first line using the indexer");
			helper.USOrganisation.Organisation.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "Some agent notes that should not be joined together as there are no others");

			AssertEquals("Notes Count", 3, orgWrapper.Notes.Count);

			var noteWrapper = orgWrapper.Notes[PredefinedNoteTypes.Instance.HandlingInstructions.Description];
			AssertNotNull("The handling instructions is found using the indexer", noteWrapper);
			AssertEquals("The Text is algamated", "These notes exist against the organisation and should be algamated\r\nThis is the second line that gets added to the first line using the indexer", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Goods Handling Instructions", noteWrapper.Description);

			noteWrapper = orgWrapper.Notes[PredefinedNoteTypes.Instance.AgentNotes.Description];
			AssertNotNull("Int indexer gets the agent Notes", noteWrapper);
			AssertEquals("noteWrapper.Text", "Some agent notes that should not be joined together as there are no others", noteWrapper.Text);
			AssertEquals("noteWrapper.Description", "Agent Notes", noteWrapper.Description);
		}
	}
}
