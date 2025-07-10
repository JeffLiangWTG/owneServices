using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class XsdMovementTest : TestCaseWithFactory
	{
		public void TestToPortEstimatedActualDates_HasValidSmallDateTime()
		{
			MapToTargetObject = Factory.New<OrgPatternMatchOverride>();
			var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			MapToTargetObject.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			MapToTargetObject.OO_LocalGuid = port.PK;
			MapToTargetObject.OO_OH = OtherOrg.PK;
			MapToTargetObject.OO_ForeignCode = "SYD";

			Factory.Save();

			Movement.EstimatedDateTime = new ZDateTime(1900, 01, 23, 0, 0, 0);
			Movement.ActualDateTime = new ZDateTime(1999, 01, 25, 0, 0, 0);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Interchange, Notify);
			XsdMovement.ToPortEstimatedActualDates(Movement, Transport.JW_RL_NKLoadPortInfo, Transport.JW_ETAInfo, Transport.JW_ATAInfo, "XsdMovementTest", context);

			AssertEquals("ETA value should be set when importing date is valid", Movement.EstimatedDateTime.ToSmallDateTime(), Transport.JW_ETA);
			AssertEquals("ATA value should be set when importing date is valid", Movement.ActualDateTime.ToSmallDateTime(), Transport.JW_ATA);
			AssertEquals("Should show sucessful message.", "Successfully matched organization with code 'T5GISEQI2KZR', Mapping Organization: EDICUS, Matching by Foreign code: MAPPINGOWNERCODE, Using: Foreign Code Matcher, Found match: True", context.LastNotificationMessage);
		}

		public void TestToPortEstimatedActualDates_HasInvalidSmallDateTime()
		{
			MapToTargetObject = Factory.New<OrgPatternMatchOverride>();
			var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			MapToTargetObject.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			MapToTargetObject.OO_LocalGuid = port.PK;
			MapToTargetObject.OO_OH = OtherOrg.PK;
			MapToTargetObject.OO_ForeignCode = "SYD";

			Factory.Save();

			Movement.EstimatedDateTime = new ZDateTime(190, 01, 23);
			Movement.ActualDateTime = new ZDateTime(2018, 01, 25);
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Interchange, Notify);
			XsdMovement.ToPortEstimatedActualDates(Movement, Transport.JW_RL_NKLoadPortInfo, Transport.JW_ETAInfo, Transport.JW_ATAInfo, "XsdMovementTest", context);

			AssertEquals("ETA should be empty.", true, Transport.JW_ETA.IsEmpty);
			AssertEquals("Should show error message.", "Error: XsdMovementTest specified estimated date time is out out of range.", context.LastNotificationMessage);

			Movement.EstimatedDateTime = new ZDateTime(2018, 01, 23);
			Movement.ActualDateTime = new ZDateTime(190, 01, 23);

			Transport.JW_ETAInfo.ClearValue();
			Transport.JW_ATAInfo.ClearValue();
			Notify.Clear();

			context = new ValueObjectImportContext(Factory, Interchange, Notify);
			XsdMovement.ToPortEstimatedActualDates(Movement, Transport.JW_RL_NKLoadPortInfo, Transport.JW_ETAInfo, Transport.JW_ATAInfo, "XsdMovementTest", context);

			AssertEquals("ATA should be empty.", true, Transport.JW_ATA.IsEmpty);
			AssertEquals("Should show error message.", "Error: XsdMovementTest specified actual date time is out out of range.", context.LastNotificationMessage);
		}

		public void TestToPortEstimatedActualDates_InterchangeIsNotSpecified()
		{
			Xsd.XmlInterchange emptyInterchange = Xsd.XmlInterchange.Empty;
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, emptyInterchange, Notify);

			XsdMovement.ToPortEstimatedActualDates(Movement, Transport.JW_RL_NKLoadPortInfo, Transport.JW_ETAInfo, Transport.JW_ATAInfo, "XsdMovementTest", context);

			AssertEquals("SYD", Transport.JW_RL_NKLoadPort);
		}

		public void TestToPortEstimatedActualDates_FoundMatchOnPort()
		{
			MapToTargetObject = Factory.New<OrgPatternMatchOverride>();
			var port = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			MapToTargetObject.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			MapToTargetObject.OO_LocalGuid = port.PK;
			MapToTargetObject.OO_OH = OtherOrg.PK;
			MapToTargetObject.OO_ForeignCode = "SYD";

			Factory.Save();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Interchange, Notify);

			XsdMovement.ToPortEstimatedActualDates(Movement, Transport.JW_RL_NKLoadPortInfo, Transport.JW_ETAInfo, Transport.JW_ATAInfo, "XsdMovementTest", context);

			AssertEquals("Should have returned AUSYD as the matched port", "AUSYD", Transport.JW_RL_NKLoadPort);
		}

		public void TestToPortEstimatedActualDates_NoMatchOnPort()
		{
			Factory.Save();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, Interchange, Notify);
			XsdMovement.ToPortEstimatedActualDates(Movement, Transport.JW_RL_NKLoadPortInfo, Transport.JW_ETAInfo, Transport.JW_ATAInfo, "XsdMovementTest", context);

			AssertEquals("Should not have found a match; should have used original value", "SYD", Transport.JW_RL_NKLoadPort);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Movement = new Xsd.Movement();
			Notify = new NotificationBuffer();
			Interchange = new Xsd.XmlInterchange();
			Converter = StringToBusinessObjectFieldConverter.InstanceForCurrentCompany;
			Transport = Factory.New<CommonConsol>().Transports.AddNew();

			Movement.Port.Value = "SYD";

			Interchange = new Xsd.XmlInterchange();
			Interchange.InterchangeInfo.EDIOrganisation.OwnerCode = "MAPPINGOWNERCODE";

			OtherOrg = Factory.New<OrgHeader>();
			OtherOrg.OH_FullName = "Importer";
			OtherOrg.OH_IsConsignee = true;
			OtherOrg.OH_IsConsignor = true;
			OtherOrg.OH_RL_NKClosestPort = "AUSYD";
			OrgAddress address1 = OtherOrg.Addresses[0];
			address1.OA_Address1 = "FAWGE3AR6PKAFVWWY42VZ7TMTNXEH7G5RX0EM2HRJ13ND7WE0J";
			OtherOrg.OH_Code = "T5GISEQI2KZR";

			Assert(OtherOrg.PK != ZGuid.Empty);

			MatchOwnerCodeToCurrentCompany = Factory.New<OrgPatternMatchOverride>();
			MatchOwnerCodeToCurrentCompany.SuspendValidation();
			MatchOwnerCodeToCurrentCompany.OO_Relationship = "ORG";
			MatchOwnerCodeToCurrentCompany.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			MatchOwnerCodeToCurrentCompany.OO_ForeignCode = "MAPPINGOWNERCODE";
			MatchOwnerCodeToCurrentCompany.OO_LocalGuid = OtherOrg.PK;
		}

		Xsd.Movement Movement;
		Transport Transport;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetUp")]
		StringToBusinessObjectFieldConverter Converter;
		NotificationBuffer Notify;

		OrgPatternMatchOverride MapToTargetObject;
		OrgPatternMatchOverride MatchOwnerCodeToCurrentCompany;
		Xsd.XmlInterchange Interchange;
		OrgHeader OtherOrg;

		#endregion
	}
}
