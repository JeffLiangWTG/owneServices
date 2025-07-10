using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class AMSApplicationControlGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2010, 11, 2)]
		public void TestContstructor_SendCBPViaEHub()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAAA", new RefCountry.Loader(Factory).LoadForCountry(Core.Constants.CountryCodes.UnitedStates));
			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			branch.GB_OH_OrgProxy = orgProxy.PK;
			var generator = new AMSApplicationControlGenerator("", branch);
			AssertEquals("AAAA", generator.A.AMSUserCode);
		}

		[TestDate(2010, 11, 2)]
		public void TestContstructor()
		{
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "Z!Z";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "Z!Z";
			otherBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();
			var applicationControlGenerator = ApplicationControlGenerator.New(AMSEDIMessage.ApplicationCodes.AMS, AMSApplicationIdentifierCodeList.Codes.ManifestCreate, GlbBranch.CurrentBranch);
			AssertEquals(typeof(AMSApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(APLACR), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(APLZCR), applicationControlGenerator.Z.GetType());
			AssertEquals("", applicationControlGenerator.GetBody());
			var message = Factory.New<AMSEDIMessage>();
			message.EM_MessageNum = "320989";
			message.EM_MessageText = "HELLO WORLD";
			applicationControlGenerator.AddMessage(message);
			AssertEquals("ACR          MI                                                                 ", applicationControlGenerator.A.Serialise());
			AssertEquals("ZCR          MI                   00000                                         ", applicationControlGenerator.Z.Serialise());
			AssertEquals("HELLO WORLD".PadRight(80), applicationControlGenerator.GetBody());
			message.EM_MessageText = new APLACR().Serialise() + "HELLO".PadRight(80) + "WORLD".PadRight(80) + new APLZCR().Serialise();
			applicationControlGenerator = ApplicationControlGenerator.New(AMSEDIMessage.ApplicationCodes.AMS, AMSApplicationIdentifierCodeList.Codes.ManifestCreate, GlbBranch.CurrentBranch);
			applicationControlGenerator.AddMessage(message);
			AssertEquals("HELLO".PadRight(80) + "WORLD".PadRight(80), applicationControlGenerator.GetBody());
			applicationControlGenerator = ApplicationControlGenerator.New(AMSEDIMessage.ApplicationCodes.AMS, AMSApplicationIdentifierCodeList.Codes.ManifestCreate, GlbBranch.CurrentBranch);
			AssertEquals(typeof(AMSApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(APLACR), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(APLZCR), applicationControlGenerator.Z.GetType());
			AssertEquals("", applicationControlGenerator.GetBody());
		}

		[TestDate(2020, 2, 2)]
		public void TestGetSettingDetails()
		{
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "Z!Z";
			otherCompany.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "Z!Z";
			otherBranch.GB_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();
			var obj = new AMSInputBlockControlGenerator();
			var aplacr = new APLACR()
			{ ApplicationIdentifier = AMSApplicationIdentifierCodeList.Codes.EquipmentInventory };
			var equc01 = new EQUC01()
			{ ContainerEquipmentDescriptionCode = "C1" };
			var aplzcr = new APLZCR()
			{ ApplicationIdentifier = "Y1" };
			obj.Deserialise(aplacr.Serialise() + equc01.Serialise() + aplzcr.Serialise());
			var message = obj.CreateMessage<AMSEDIMessage>(Factory);
			message.EM_GB = otherBranch.PK;
			var collection = new NonDependentEDIMessageCollection(Factory);
			collection.Add(message);
			var interchanges = new CBPInterchangeProvider(collection).Interchanges;
			AssertEquals(0, interchanges.Length);
			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
			AssertEquals(@"Message  will be discarded for the following reason:
There is no Customs Interchange Sender ID set up
Please configure a Carrier Code (CCC) for US in Organization Proxy 'EDICUS' (Maintain > User Admin > Companies > Z!Z - Company > Company Info. > Organization Proxy > Details > Config > Registration Numbers / Codes)", message.Notes.FindByDescription("Processing Log").Single().ST_NoteText);
		}
	}
}
