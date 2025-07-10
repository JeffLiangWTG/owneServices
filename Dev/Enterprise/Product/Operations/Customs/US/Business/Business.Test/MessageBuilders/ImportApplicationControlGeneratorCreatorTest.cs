using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ImportApplicationControlGeneratorCreatorTest : TestCaseWithFactory
	{
		[TestDate(1971, 9, 18)]
		public void TestNew()
		{
			DeclarationTestHelper.SetupForSendMessage();
			var filer = new Registry.Business.Customs.US.ExportEntryFilerID();
			filer.EntryFilerID = "364-33-1434";
			filer.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageNum = "320989";
			var creator = new ImportApplicationControlGeneratorCreator();
			var applicationControlGenerator = creator.New(ACEApplicationIdentifierCodeList.Codes.QueryImporterBond, GlbBranch.CurrentBranch);
			AssertEquals(typeof(ACEApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(AABIInputA), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(AABIInputZ), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A3901XJ5      091871     KI          89".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("Z3901XJ5      091871                 89".PadRight(80), applicationControlGenerator.Z.Serialise());

			applicationControlGenerator = creator.New(ACEApplicationIdentifierCodeList.Codes.EntrySummary, GlbBranch.CurrentBranch);
			AssertEquals(typeof(ACEApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(AABIInputA), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(AABIInputZ), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A3901XJ5      091871     AE          89".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("Z3901XJ5      091871                 89".PadRight(80), applicationControlGenerator.Z.Serialise());
			applicationControlGenerator = creator.New("XN", GlbBranch.CurrentBranch);
			AssertEquals(typeof(ABIApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(APLA), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(APLZ), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("Z3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.Z.Serialise());

			applicationControlGenerator = creator.New(ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling, GlbBranch.CurrentBranch);
			AssertEquals(typeof(ABIApplicationControlGenerator), applicationControlGenerator.GetType());
			AssertEquals(typeof(APLA), applicationControlGenerator.A.GetType());
			AssertEquals(typeof(APLZ), applicationControlGenerator.Z.GetType());
			applicationControlGenerator.AddMessage(message);
			AssertEquals("A3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.A.Serialise());
			AssertEquals("Z3901XJ5      09187101               89".PadRight(80), applicationControlGenerator.Z.Serialise());
		}
	}
}
