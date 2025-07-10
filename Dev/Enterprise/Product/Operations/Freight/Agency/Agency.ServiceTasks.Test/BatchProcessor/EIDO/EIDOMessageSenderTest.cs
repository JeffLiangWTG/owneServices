using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	sealed class EIDOMessageSenderTest : ShippingManagerInterchangeSenderTest
	{
		[UseSnapshotProtection]
		[EIDOMessagingConfiguration(Testing = false, SenderId = "EDIAL", RecipientId = "1STOP")]
		[TestDate]
		public void TestSendEIDOEmail()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Blaticus";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "666";

			JobSailing sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort);

			OrgHeader principal = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal");

			AgencyShipment shipment = NewShipment(sailing, null, false, true);
			shipment.JS_HouseBill = "BillOfLading";
			shipment.JS_OH_DeliveryAgent = principal.PK;

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_IsEmptyContainer = true;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "FAKE4100027";
			container2.JC_IsEmptyContainer = true;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			Env.NumberFountains.EDIFACTNumberFountain("I", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);
			Env.NumberFountains.EDIFACTNumberFountain("M", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);

			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			AddMessage(messages, container2);

			Factory.Save();

			EIDOInterchangeSender sender = new EIDOInterchangeSender();
			sender.ExecuteBatch();

			const string attachmentTemplate = "UNB+UNOA:4+EDIAL+1STOP+{0:yyMMdd:HHmm}+1'BEGIN+1+END'UNZ+1+1'";
			AssertEmailSent("message1", "bob@freadnet.org", "IFCSUM", "00000001.edi", string.Format(attachmentTemplate, ZDateTime.Now));
			AssertNoEmailSent("no error sent", "err@freadnet.org");
		}

		[UseSnapshotProtection]
		[EIDOMessagingConfiguration(Testing = true, SenderId = "EDIAL", RecipientId = "1STOP")]
		[TestDate]
		public void TestSendTestingEIDOEmail()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = "Blaticus";
			voyage.JV_VoyageFlight = "666";

			JobSailing sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort);

			OrgHeader principal = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal");

			AgencyShipment shipment = NewShipment(sailing, null, false, true);
			shipment.JS_HouseBill = "BillOfLading";
			shipment.JS_OH_DeliveryAgent = principal.PK;

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_IsEmptyContainer = true;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "FAKE4100027";
			container2.JC_IsEmptyContainer = true;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			Env.NumberFountains.EDIFACTNumberFountain("I", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);
			Env.NumberFountains.EDIFACTNumberFountain("M", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);

			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			AddMessage(messages, container2);

			Factory.Save();

			EIDOInterchangeSender sender = new EIDOInterchangeSender();
			sender.ExecuteBatch();

			const string attachmentTemplate = "UNB+UNOA:4+EDIAL+1STOP+{0:yyMMdd:HHmm}+1++++++1'BEGIN+1+END'UNZ+1+1'";
			AssertEmailSent("message1", "bob@freadnet.org", "IFCSUM", "00000001.edi", string.Format(attachmentTemplate, ZDateTime.Now));
			AssertNoEmailSent("no error sent", "err@freadnet.org");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		[EIDOMessagingConfiguration(Enabled = false)]
		[TestDate(2010, 04, 21, 11, 23, 00)]
		public void TestDissabled()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = "Blaticus";
			voyage.JV_VoyageFlight = "666";

			JobSailing sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort);

			OrgHeader principal = BaseAgencyTest.NewPrincipal(Factory);

			AgencyShipment shipment = NewShipment(sailing, null, false, true);
			shipment.JS_HouseBill = "BillOfLading";
			shipment.JS_OH_DeliveryAgent = principal.PK;

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_IsEmptyContainer = true;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_IsEmptyContainer = true;
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			Env.NumberFountains.EDIFACTNumberFountain("I", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);
			Env.NumberFountains.EDIFACTNumberFountain("M", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);

			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);
			AddMessage(messages, container2);

			Factory.Save();

			EIDOInterchangeSender sender = new EIDOInterchangeSender();
			sender.ExecuteBatch();

			Factory.ReloadAll<EIDOMessage>();

			AssertEquals(EDIMessage.Status.Failed, messages[0].EM_Status);
			AssertEmailSent("message1", "err@freadnet.org", "Error Sending E-IDO Message", TestFileHelper.EIDO.GetFailure1());
			AssertNoEmailSent("no interchange sent", "bob@freadnet.org");
		}

		[UseSnapshotProtection]
		[EIDOMessagingConfiguration(Testing = false, SenderId = "EDIAL", RecipientId = "1STOP")]
		[TestDate(2015, 08, 17)]
		public void TestMessagesForAllBranchesAreFound()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = "Blaticus";
			voyage.JV_VoyageFlight = "666";

			JobSailing sailing = FindOrCreateSailing(voyage, HomePort, OverseasPort);

			OrgHeader principal = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "Principal");

			AgencyShipment shipment = NewShipment(sailing, null, false, true);
			shipment.JS_HouseBill = "BillOfLading";
			shipment.JS_OH_DeliveryAgent = principal.PK;

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_IsEmptyContainer = true;
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			TestCaseHelper.ClearTable(EDIInterchangeSchema.Constants.TableName);
			Env.NumberFountains.EDIFACTNumberFountain("I", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);
			Env.NumberFountains.EDIFACTNumberFountain("M", EIDOMessage.UsKeyPart, EIDOMessage.ThemKeyPart).SetNext(Factory, 1);

			NonDependentEDIMessageCollection messages = new NonDependentEDIMessageCollection(Factory);

			var messageFromOtherBranch = AddMessage(messages, container1);
			var alternativeBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			messageFromOtherBranch.EM_GB = alternativeBranch.PK;

			Factory.Save();

			EIDOInterchangeSender sender = new EIDOInterchangeSender();
			sender.ExecuteBatch();

			const string attachmentTemplate = "UNB+UNOA:4+EDIAL+1STOP+{0:yyMMdd:HHmm}+1'BEGIN+1+END'UNZ+1+1'";
			AssertEmailSent("message1", "bob@freadnet.org", "IFCSUM", "00000001.edi", string.Format(attachmentTemplate, new ZDateTime(2015, 08, 17)));
			AssertNoEmailSent("no error sent", "err@freadnet.org");
		}

		#region Implementation

		static EDIMessage AddMessage(NonDependentEDIMessageCollection messages, AgencyShipmentContainer container)
		{
			EDIMessage result = container.Messages.AddNew(typeof(EIDOMessage));
			result.FillWithValidTestData();
			result.EM_MessageText = "BEGIN+" + EDIMessage.MessageNumberPlaceHolder + "+END'";
			result.EM_MessageOwner = String.Empty;
			result.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EIDO;
			result.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;

			messages.Add(result);

			return result;
		}

		#endregion
	}
}
