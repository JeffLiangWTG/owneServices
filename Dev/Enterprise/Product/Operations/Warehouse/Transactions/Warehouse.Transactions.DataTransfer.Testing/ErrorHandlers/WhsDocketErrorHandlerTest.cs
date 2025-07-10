using System;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public abstract class WhsDocketErrorHandlerTest<TBusinessObject> : WhsTestCaseWithFactory
		where TBusinessObject : WhsDocket
	{
		#region Docket

		[TestDate(2007, 6, 1)]
		public void TestSetDocketTypeAndLogDataErrorsAllValidDataDocket()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Incorrect docket type", GetDocketType(), Docket.WD_DocketType);
			AssertEquals("Docket.HasErrors should be false", false, Docket.HasErrors);
		}

		[TestDate(2007, 6, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestDocketInErrorIsFixedOnValidDataImport()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_DocketStatus = DocketStatus.Codes.Error;
			Factory.Save();
			AssertEquals("Docket Status should be ERR", DocketStatus.Codes.Error, Docket.WD_DocketStatus);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);

			AssertEquals("Incorrect docket type", GetDocketType(), Docket.WD_DocketType);
			AssertEquals("Docket.HasErrors should be false", false, Docket.HasErrors);
			AssertEquals("Docket Status should be ENT", DocketStatus.Codes.Entered, Docket.WD_DocketStatus);
		}

		[TestDate(2007, 6, 1)]
		public void TestReferenceInvalidType()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.References[0].WX_RefType = "AAA";

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
		}

		[TestDate(2007, 6, 1)]
		public void TestExternalReferenceInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_ExternalReference = "ExternalRef2";
			AssertNotEquals("Precondition", XsdDocket.Identifier.Reference, Docket.WD_ExternalReference);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (" + Docket.HumanReadableName + " Reference): " + XsdDocket.Identifier.Reference);
		}

		[TestDate(2007, 6, 1)]
		public void TestCustomerReferenceInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_CustomerReference = "CustomerRef2";
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.CustomerReference, Docket.WD_CustomerReference);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Customer Reference): " + XsdDocket.DocketDetail.CustomerReference);
		}

		[TestDate(2007, 6, 1)]
		public void TestTransportReferenceInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_TransportReference = "TransportRef2";
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.TransportReference, Docket.WD_TransportReference);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Transport Reference): " + XsdDocket.DocketDetail.TransportReference);
		}

		[TestDate(2007, 6, 1)]
		public void TestLine_SetDocketTypeAndLogDataErrorsTemporarilyResumesValidation()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			try
			{
				Factory.SuspendValidation();
				Docket.WD_OH_Client = ZGuid.Empty;

				Assert(!Docket.HasErrors);
				AssertNull(Docket.Client);

				DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
				AssertDocketErrorTypeAndLog(Docket, XsdDocket);
				Assert(Docket.HasErrors);
				Assert(Factory.IsValidationSuspended);
			}
			finally
			{
				Factory.ResumeValidation();
			}
		}

		#region Service Level

		#region Carrier Service Level

		#region TestTransportServiceLevel_NotFound

		[TestDate(2007, 6, 1)]
		public void TestTransportServiceLevel_NotFound()
		{
			AssertServiceLevelNotFound(WhsDocketSchema.WD_PL_NKCarrierServiceLevel, "Error (Transport Service Level): ", d => d.TransportServiceLevel);
		}

		#endregion

		#region TestTransportServiceLevel_NotSpecified

		[TestDate(2007, 6, 1)]
		public void TestTransportServiceLevel_NotSpecified()
		{
			AssertServiceLevelNotSpecified(WhsDocketSchema.WD_PL_NKCarrierServiceLevel, d => d.TransportServiceLevel = "");
		}

		#endregion

		#region TestTransportServiceLevel_DefaultUsedIfNotSpecified

		[TestDate(2007, 6, 1)]
		public void TestTransportServiceLevel_DefaultUsedIfNotSpecified()
		{
			AssertDefaulServiceLevelUsedIfNotSpecified(d => d.TransportServiceLevel = ZString.Empty, "Error (Transport Service Level):", d => d.TransportServiceLevel);
		}

		#endregion

		#endregion

		#region Service Level

		#region TestServiceLevel_NotFound

		[TestDate(2007, 6, 1)]
		public void TestServiceLevel_NotFound()
		{
			AssertServiceLevelNotFound(WhsDocketSchema.WD_RS_NKServiceLevel, "Error (Service Level): ", d => d.ServiceLevel);
		}

		#endregion

		#region TestServiceLevel_NotSpecified

		[TestDate(2007, 6, 1)]
		public void TestServiceLevel_NotSpecified()
		{
			AssertServiceLevelNotSpecified(WhsDocketSchema.WD_RS_NKServiceLevel, d => d.ServiceLevel = "");
		}

		#endregion

		#region TestServiceLevel_DefaultUsedIfNotSpecified

		[TestDate(2007, 6, 1)]
		public void TestServiceLevel_DefaultUsedIfNotSpecified()
		{
			AssertDefaulServiceLevelUsedIfNotSpecified(d => d.ServiceLevel = ZString.Empty, "Error (Service Level):", d => d.ServiceLevel);
		}

		#endregion

		#endregion

		void AssertServiceLevelNotSpecified(SchemaStringColumn stringColumn, Action<Xsd.WhsDocketDocketDetail> setServiceLevel)
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			setServiceLevel(XsdDocket.DocketDetail);
			Docket[stringColumn] = "";
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Incorrect docket type", GetDocketType(), Docket.WD_DocketType);
			AssertEquals("Docket.HasErrors should be false", false, Docket.HasErrors);
		}

		void AssertServiceLevelNotFound(SchemaStringColumn stringColumn, string errorMessage, Func<Xsd.WhsDocketDocketDetail, ZString> getTransportServiceLevel)
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket[stringColumn] = "";
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, errorMessage + getTransportServiceLevel(XsdDocket.DocketDetail));
		}

		void AssertDefaulServiceLevelUsedIfNotSpecified(Action<Xsd.WhsDocketDocketDetail> setServiceLevel, string errorMessage, Func<Xsd.WhsDocketDocketDetail, ZString> getTransportServiceLevel)
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			setServiceLevel(XsdDocket.DocketDetail);
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Incorrect docket type", GetDocketType(), Docket.WD_DocketType);
			AssertEquals("Docket.HasErrors should be false", false, Docket.HasErrors);
			AssertDocketLogEventNotCreated(Docket, Events.DataImport.Description, errorMessage + getTransportServiceLevel(XsdDocket.DocketDetail));
		}

		#endregion

		protected void AssertDocketLogEventNotCreated(TBusinessObject docket, ZString eventDescription, ZString eventReference)
		{
			var logs = GetDocketLogEvents(docket, eventDescription, eventReference);
			AssertEquals("Should not find any logs with description '" + eventDescription + "' and reference '" + eventReference + "'", 0, logs.Count);
		}

		[TestDate(2007, 6, 1)]
		public void TestTransportServiceLevelInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var transportCo = Factory.New<OrgHeader>();
			var serviceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "TT2";

			Docket.WD_PL_NKCarrierServiceLevel = "TT2";
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.TransportServiceLevel, Docket.WD_PL_NKCarrierServiceLevel);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Transport Service Level): " + XsdDocket.DocketDetail.TransportServiceLevel);
		}

		#region TestSetDocketTypeAndLogDataErrors_WhenWhsHasClientCodeMapping

		[TestDate(2007, 6, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSetDocketTypeAndLogDataErrors_WhenWhsHasClientCodeMapping()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			// create a new warehouse mapping
			var mappingOrg = Factory.Load<OrgHeader>(Env.CurrentCompany.OrganisationPK);
			var whsOverride = mappingOrg.CreatePatternMatchOverrideForTest();
			whsOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Warehouse;
			whsOverride.OO_LocalGuid = Docket.Warehouse.PK;
			whsOverride.OO_ForeignCode = "ZZ";
			Factory.Save();

			// Set the foreign code to the xml
			XsdDocket.DocketDetail.WarehouseCode = "ZZ";

			AssertNotEquals("Precondition", XsdDocket.DocketDetail.WarehouseCode, Docket.Warehouse.WW_WarehouseCode);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("DocketType incorrect", DocketStatus.Codes.Entered, Docket.WD_DocketStatus);
		}

		#endregion

		#region Transport Company

		[TestDate(2007, 6, 1)]
		public void TestTransportCompanyNotFoundOrCreated()
		{
			if (Docket is IJobWithTransportCompany job)
			{
				PopulateWithValidData(Docket, XsdDocket);
				AssertPrecondition(Docket, XsdDocket);

				job.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
				DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
				AssertDocketErrorTypeAndLog(Docket, XsdDocket);
				GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Transport Company): " + XsdDocket.DocketDetail.TransportCompany.AddressReference.Organisation.OwnerCode);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2007, 6, 1)]
		public void TestTransportCompanyNotSpecified()
		{
			if (Docket is IJobWithTransportCompany job)
			{
				PopulateWithValidData(Docket, XsdDocket);
				AssertPrecondition(Docket, XsdDocket);

				XsdDocket.DocketDetail.TransportCompany = null;
				job.TransportCoDocAddress.OrganisationPK = ZGuid.Empty;
				DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
				AssertEquals("Should have no import error logs for client field", 0, GetDocketLogEvents(Docket, Events.DataExport.Description, "Error (Transport Company): " + XsdDocket.DocketDetail.TransportCompany.AddressReference.Organisation.OwnerCode).Count);
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2007, 6, 1)]
		public void TestTransportCompanyIsSpecifiedAndFoundOrCreated()
		{
			if (Docket is IJobWithTransportCompany job)
			{
				PopulateWithValidData(Docket, XsdDocket);
				AssertPrecondition(Docket, XsdDocket);

				var transportCo = Helper.CreateClient("TC2", "TRANSPORT CO2");
				job.TransportCoDocAddress.OrganisationPK = transportCo.PK;
				AssertNotEquals("Precondition", XsdDocket.DocketDetail.TransportCompany.AddressReference.Organisation.OwnerCode, job.TransportCoDocAddress.Organisation.OH_Code);

				DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
				AssertEquals("Should have no import error logs for client field", 0, GetDocketLogEvents(Docket, Events.DataExport.Description, "Error (Transport Company): " + XsdDocket.DocketDetail.TransportCompany.AddressReference.Organisation.OwnerCode).Count);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region Transport Billed To

		[TestDate(2007, 6, 1)]
		public void TestTransportBilledToNotFoundOrCreated()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.TransportBillToDocAddress.OrganisationPK = ZGuid.Empty;
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Transport Billed To): " + XsdDocket.DocketDetail.TransportBilledTo.AddressReference.Organisation.OwnerCode);
		}

		[TestDate(2007, 6, 1)]
		public void TestTransportBilledToNotSpecified()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			XsdDocket.DocketDetail.TransportBilledTo = null;
			Docket.TransportBillToDocAddress.OrganisationPK = ZGuid.Empty;
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Should have no import error logs for client field", 0, GetDocketLogEvents(Docket, Events.DataExport.Description, "Error (Transport Billed To): " + XsdDocket.DocketDetail.TransportBilledTo.AddressReference.Organisation.OwnerCode).Count);
		}

		[TestDate(2007, 6, 1)]
		public void TestTransportBilledToIsSpecifiedAndFoundOrCreated()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var transportBilledTo = Helper.CreateClient("TB2", "TRANSPORT BILL TO2");
			Docket.TransportBillToDocAddress.OrganisationPK = transportBilledTo.PK;
			AssertNotEquals("Precondition", XsdDocket.DocketDetail.TransportBilledTo.AddressReference.Organisation.OwnerCode, Docket.TransportBillToDocAddress.Organisation.OH_Code);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Should have no import error logs for client field", 0, GetDocketLogEvents(Docket, Events.DataExport.Description, "Error (Transport Billed To): " + XsdDocket.DocketDetail.TransportBilledTo.AddressReference.Organisation.OwnerCode).Count);
		}

		#endregion

		#region Client

		[TestDate(2007, 6, 1)]
		public void TestClientNotFoundOrCreated()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			Docket.WD_OH_Client = ZGuid.Empty;
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Error (Client): " + XsdDocket.Identifier.Client.OwnerCode);
		}

		[TestDate(2007, 6, 1)]
		public void TestErrorsClientNotSpecified()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			XsdDocket.Identifier.Client = null;
			Docket.WD_OH_Client = ZGuid.Empty;
			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Should have no import error logs for client field", 0, GetDocketLogEvents(Docket, Events.DataExport.Description, "Error (Client): " + XsdDocket.Identifier.Client.OwnerCode).Count);
		}

		[TestDate(2007, 6, 1)]
		public void TestClientIsSpecifiedAndFoundOrCreated()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var client = Helper.CreateClient("CT2", "CLIENT2");
			Docket.WD_OH_Client = client.PK;
			AssertNotEquals("Precondition", XsdDocket.Identifier.Client.OwnerCode, Docket.Client.OH_Code);

			DocketErrorHandler.SetDocketTypeAndLogDataErrors(Docket, XsdDocket);
			AssertEquals("Should have no import error logs for client field", 0, GetDocketLogEvents(Docket, Events.DataExport.Description, "Error (Client): " + XsdDocket.Identifier.Client.OwnerCode).Count);
		}

		#endregion

		#endregion

		#region Docket Line

		[TestDate(2007, 6, 1)]
		public void TestSetDocketTypeAndLogDataErrorsFromLineAllValidDataLine()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, Docket.Lines[0], XsdDocket.DocketLines[0]);
			AssertEquals("Incorrect docket type", GetDocketType(), Docket.WD_DocketType);
			AssertEquals("Line.HasErrors should be false", false, Docket.Lines[0].HasErrors);
			AssertEquals("Docket.HasErrors should be false", false, Docket.HasErrors);
		}

		[TestDate(2007, 6, 1)]
		public void TestLineWE_PartAttrib3Invalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_PartAttrib3 = "PartAA3";
			AssertNotEquals("Precondition", xsdLine.LineAttributes.PartAttribute3, line.WE_PartAttrib3);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Part Attr3): PartA3");
		}

		[TestDate(2007, 6, 1)]
		public void TestLineWE_PartAttrib2Invalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_PartAttrib2 = "PartAA2";
			AssertNotEquals("Precondition", xsdLine.LineAttributes.PartAttribute2, line.WE_PartAttrib2);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Part Attr2): PartA2");
		}

		[TestDate(2007, 6, 1)]
		public void TestLineWE_PartAttrib1Invalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_PartAttrib1 = "PartAA1";
			AssertNotEquals("Precondition", xsdLine.LineAttributes.PartAttribute1, line.WE_PartAttrib1);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Part Attr1): PartA1");
		}

		[TestDate(2007, 6, 1)]
		public void TestLineExpiryDateInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			xsdLine.LineAttributes.ExpiryDate = ZDate.Empty;
			AssertNotEquals("Precondition", xsdLine.LineAttributes.ExpiryDate, line.WE_ExpiryDate);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Expiry Date): Empty");
		}

		[TestDate(2007, 6, 1)]
		public void TestLineExpiryDateInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_ExpiryDate = ZDate.Today.AddDays(2);
			AssertNotEquals("Precondition", xsdLine.LineAttributes.ExpiryDate, line.WE_ExpiryDate);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Expiry Date): " + xsdLine.LineAttributes.ExpiryDate.ToShortDateString());
		}

		[TestDate(2007, 6, 1)]
		public void TestLinePackingDateInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			xsdLine.LineAttributes.PackingDate = ZDate.Empty;
			AssertNotEquals("Precondition", xsdLine.LineAttributes.PackingDate, line.WE_PackingDate);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Packing Date): Empty");
		}

		[TestDate(2007, 6, 1)]
		public void TestLinePackingDateInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_PackingDate = ZDate.Today.AddDays(2);
			AssertNotEquals("Precondition", xsdLine.LineAttributes.PackingDate, line.WE_PackingDate);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Packing Date): " + xsdLine.LineAttributes.PackingDate.ToShortDateString());
		}

		[TestDate(2007, 6, 1)]
		public void TestLineBondedEntryKeyInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_BondedEntryKey = "BondedKey2";
			AssertNotEquals("Precondition", xsdLine.LineAttributes.BondedEntryKey, line.WE_BondedEntryKey);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Entry Key): BondedKey");
		}

		[TestDate(2007, 6, 1)]
		public void TestLineCommentInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_LineComment = "TEST COMMENT2";
			AssertNotEquals("Precondition", xsdLine.LineComments, line.WE_LineComment);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Comment): TEST COMMENT");
		}

		[TestDate(2007, 6, 1)]
		public void TestLinePartInvalid()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_OP = Helper.CreateProduct(Docket.Client, "PR2").PK;
			AssertNotEquals("Precondition", xsdLine.Product, line.SupplierPart.OP_PartNum);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Product): PRD");
		}

		[TestDate(2007, 6, 1)]
		public virtual void TestLineWE_PackQuantityInvalid1()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_PackQuantity = 110m;
			AssertNotEquals("Precondition", xsdLine.QuantityActuallyOrdered, line.WE_PackQuantity);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Qty): 100");
		}

		[TestDate(2007, 6, 1)]
		public virtual void TestLineWE_TransactionQuantityInvalid2()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			line.WE_TransactionQuantity = 110m;
			xsdLine.QuantityFromClientOrder = 0m;
			AssertNotEquals("Precondition", xsdLine.QuantityActuallyOrdered, line.WE_TransactionQuantity);

			DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
			AssertDocketErrorTypeAndLog(Docket, XsdDocket);
			GetDocketLogEvent(Docket, Events.DataImport.Description, "Line 1.1 Error (Qty): 100");
		}

		[TestDate(2007, 6, 1)]
		public void TestLine_SetDocketTypeAndLogDataErrorsFromLineTemporarilyResumesValidation()
		{
			PopulateWithValidData(Docket, XsdDocket);
			AssertPrecondition(Docket, XsdDocket);

			var line = Docket.Lines[0];
			Xsd.WhsDocketLine xsdLine = XsdDocket.DocketLines[0];

			try
			{
				Factory.SuspendValidation();
				line.WE_WD = ZGuid.Empty;

				Assert(!line.HasErrors);
				AssertNull(line.Docket);

				DocketErrorHandler.SetDocketTypeAndLogDataErrorsFromLine(Docket, line, xsdLine);
				AssertDocketErrorTypeAndLog(Docket, XsdDocket);
				Assert(line.HasErrors);
				Assert(Factory.IsValidationSuspended);
			}
			finally
			{
				Factory.ResumeValidation();
			}
		}

		#endregion

		#region Implementation

		protected StmALogCollection GetDocketLogEvents(TBusinessObject docket, ZString eventDescription, ZString eventReference)
		{
			var logs = new StmALogCollection(Factory);
			foreach (var log in docket.Logs.LogsNotInDB)
			{
				if (log.SL_EventDescription == eventDescription && log.SL_Reference == eventReference)
				{
					logs.Add(log);
				}
			}
			return logs;
		}

		protected StmALog GetDocketLogEvent(TBusinessObject docket, ZString eventDescription, ZString eventReference)
		{
			var logs = GetDocketLogEvents(docket, eventDescription, eventReference);
			AssertEquals("Should find only one log with description '" + eventDescription + "' and reference '" + eventReference + "'", 1, logs.Count);
			AssertNotNull("Log should be created for event '" + eventDescription + "' and reference '" + eventReference + "'", logs[0]);
			return logs[0];
		}

		protected virtual void AssertDocketErrorTypeAndLog(TBusinessObject docket, Xsd.WhsDocket xsdDocket)
		{
			docket.RunPreSaveValidation();
			AssertEquals("DocketType incorrect", DocketStatus.Codes.Error, docket.WD_DocketStatus);
		}

		protected void AssertPrecondition(TBusinessObject docket, Xsd.WhsDocket xsdDocket)
		{
			AssertDocketDataPrecondition(docket);
			AssertXsdDocketDataPrecondition(docket, xsdDocket);
		}

		protected virtual void AssertDocketDataPrecondition(TBusinessObject docket)
		{
			docket.RunPreSaveValidation();
			AssertEquals("Precondition: DocketType incorrect", GetDocketType(), docket.WD_DocketType);
			AssertEquals("Precondition: Number of Logs incorrect", 0, docket.Logs.LogsNotInDB.Length);

			AssertNotNull("Precondition: Docket Warehouse should not be null", docket.Warehouse);

			AssertEquals("Precondition: Docket.HasErrors should be false", false, docket.HasErrors);
		}

		protected virtual void AssertXsdDocketDataPrecondition(TBusinessObject docket, Xsd.WhsDocket xsdDocket)
		{
			AssertEquals("Precondition: XsdDocket Warehouse code incorrect", docket.Warehouse.WW_WarehouseCode, xsdDocket.DocketDetail.WarehouseCode);
		}

		protected void PopulateWithValidData(TBusinessObject docket, Xsd.WhsDocket xsdDocket)
		{
			PopulateDocketWithValidData(docket);
			PopulateXsdDocketWithValidDataFromDocket(xsdDocket, docket);
		}

		void PopulateXsdDocketWithValidDataFromDocket(Xsd.WhsDocket xsdDocket, TBusinessObject docket)
		{
			xsdDocket.DocketDetail.WarehouseCode = docket.Warehouse.WW_WarehouseCode;
			xsdDocket.DocketDetail.CustomerReference = docket.WD_CustomerReference;
			xsdDocket.DocketDetail.TransportReference = docket.WD_TransportReference;
			xsdDocket.DocketDetail.TransportServiceLevel = docket.WD_PL_NKCarrierServiceLevel;
			xsdDocket.DocketDetail.ServiceLevel = docket.WD_RS_NKServiceLevel;

			if (docket is IJobWithTransportCompany job)
			{
				xsdDocket.DocketDetail.TransportCompany.AddressReference.Organisation.OwnerCode = job.TransportCoDocAddress.Organisation?.OH_Code ?? ZString.Empty;
			}

			xsdDocket.DocketDetail.TransportBilledTo.AddressReference.Organisation.OwnerCode = (docket.TransportBillToDocAddress.Organisation != null ? docket.TransportBillToDocAddress.Organisation.OH_Code : ZString.Empty);

			var reference = xsdDocket.DocketDetail.References.AddNew();
			reference.Type = docket.References[0].WX_RefType;
			reference.Value = docket.References[0].WX_Reference;

			reference = xsdDocket.DocketDetail.References.AddNew();
			reference.Type = "HSB";
			reference.Value = "123456";

			xsdDocket.Identifier.Client.OwnerCode = (docket.Client != null ? docket.Client.OH_Code : ZString.Empty);
			xsdDocket.Identifier.Reference = docket.WD_ExternalReference;

			var line = docket.Lines[0];
			var xsdLine = xsdDocket.DocketLines[0];
			xsdLine.Product = line.SupplierPart.OP_PartNum;
			xsdLine.LineComments = line.WE_LineComment;
			xsdLine.ProductUQ = line.WE_F3_NKPackType;
			xsdLine.QuantityActuallyOrdered = line.WE_TransactionQuantity;
			xsdLine.QuantityFromClientOrder = line.WE_TransactionQuantity;
			xsdLine.LineNumber = line.WE_LineNo;
			xsdLine.SubLineNumber = line.WE_SubLineNo;

			xsdLine.LineAttributes.BondedEntryKey = line.WE_BondedEntryKey;
			xsdLine.LineAttributes.ExpiryDate = line.WE_ExpiryDate;
			xsdLine.LineAttributes.PackingDate = line.WE_PackingDate;
			xsdLine.LineAttributes.PartAttribute1 = line.WE_PartAttrib1;
			xsdLine.LineAttributes.PartAttribute2 = line.WE_PartAttrib2;
			xsdLine.LineAttributes.PartAttribute3 = line.WE_PartAttrib3;

			PopulateXsdDocketWithValidDataFromDocketCore(xsdDocket, docket);
		}

		protected virtual void PopulateXsdDocketWithValidDataFromDocketCore(Xsd.WhsDocket xsdDocket, TBusinessObject docket)
		{
		}

		void PopulateDocketWithValidData(TBusinessObject docket)
		{
			var client = Helper.CreateClient("TST", "TEST CLIENT");
			var warehouse = Helper.CreateWarehouse("TST WAREHOUSE");
			warehouse.WW_WarehouseCode = "WHS";

			docket.WD_OH_Client = client.PK;
			docket.WD_WW_Whs = warehouse.PK;
			docket.WD_CustomerReference = "CustomerRef";
			docket.WD_TransportReference = "TransportRef";

			var reference = docket.References.AddNew();
			reference.WX_RefType = "CAN";
			reference.WX_Reference = "12345";

			var transportCo = Helper.CreateClient("TC1", "TRANSPORT CO1");
			var transportServiceLevel = transportCo.MiscServ.CarrierServiceLevels.AddNew();
			transportServiceLevel.PL_Code = "TT1";

			if (docket is IJobWithTransportCompany job)
			{
				job.TransportCoDocAddress.OrganisationPK = transportCo.PK;
			}

			docket.WD_PL_NKCarrierServiceLevel = "TT1";

			var serviceLevel = Factory.New<RefServiceLevel>();
			serviceLevel.RS_Code = "EXP";
			serviceLevel.RS_Description = "Express Service";
			docket.WD_RS_NKServiceLevel = "EXP";

			var transportBillTo = Helper.CreateClient("TB1", "TRANSPORT BILL TO1");
			docket.TransportBillToDocAddress.OrganisationPK = transportBillTo.PK;
			docket.WD_ExternalReference = "ExternalRef";

			var line = docket.Lines[0];
			line.WE_LineNo = 1;
			line.WE_SubLineNo = 1;
			line.WE_OP = Helper.CreateProduct(client, "PRD").PK;
			Helper.CreateProductUnit(line.SupplierPart, "UM", "UNT", 10);
			line.WE_LineComment = "TEST COMMENT";
			line.WE_F3_NKPackType = "UM";
			line.WE_TransactionQuantity = 100;

			line.WE_BondedEntryKey = "BondedKey";
			line.WE_CustomAttrib1 = "CustomA1";
			line.WE_CustomAttrib2 = "CustomA2";
			line.WE_CustomAttrib3 = "CustomA3";
			line.WE_ExpiryDate = ZDate.Today.AddDays(1);
			line.WE_PackingDate = ZDate.Today;
			line.WE_PartAttrib1 = "PartA1";
			line.WE_PartAttrib2 = "PartA2";
			line.WE_PartAttrib3 = "PartA3";
			line.WE_SerialNumber = "SerialN#";

			PopulateDocketWithValidDataCore(docket, client, warehouse);
		}

		protected virtual void PopulateDocketWithValidDataCore(TBusinessObject docket, OrgHeader client, WhsWarehouse warehouse)
		{
		}

		protected abstract ZString GetDocketType();

		protected override void SetUp()
		{
			base.SetUp();
			XsdDocket = GetNewXsdDocket();
			Docket = GetNewBusinessObject();
		}

		protected virtual TBusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<TBusinessObject>();
			result.Lines.AddNew();
			return result;
		}

		protected virtual Xsd.WhsDocket GetNewXsdDocket()
		{
			Xsd.WhsDockets xsdDockets = new Xsd.WhsDockets();
			Xsd.WhsDocket xsdDocket = xsdDockets.WhsDocket.AddNew();
			xsdDocket.DocketLines.AddNew();
			return xsdDocket;
		}

		protected WhsDocketErrorHandler<TBusinessObject> DocketErrorHandler
		{
			get
			{
				if (docketErrorHandler == null)
				{
					docketErrorHandler = GetNewDocketErrorHandler();
				}
				return docketErrorHandler;
			}
		}

		protected abstract WhsDocketErrorHandler<TBusinessObject> GetNewDocketErrorHandler();

		protected Xsd.WhsDocket XsdDocket;
		protected TBusinessObject Docket;
		WhsDocketErrorHandler<TBusinessObject> docketErrorHandler;

		#endregion
	}
}
