using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class UniversalShipmentTesting : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestExporting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Taiwan))
			{
				var declaration = Factory.New<JobDeclaration>();
				var instruction = declaration.CusEntryInstruction;
				var controllingMessageHeader = instruction.ControllingMessageHeaders.AddNew();
				controllingMessageHeader.TW1_FunctionalReferenceId = "1";
				controllingMessageHeader.PermitNumber = "A1";
				controllingMessageHeader.TW1_ControllingAgency = "A";
				controllingMessageHeader.TW1_BusinessType = "A";
				controllingMessageHeader.TW1_ProcessingUnit = "KG";
				controllingMessageHeader.TW1_PaymentMethod = "1";
				controllingMessageHeader.TW1_ProofOfPaper = true;
				controllingMessageHeader.TW1_ElectronicReceipt = true;
				controllingMessageHeader.TW1_AppointmentDate = new ZDate(2019, 02, 12);
				controllingMessageHeader.TW1_AppointmentPeriod = "A";
				controllingMessageHeader.TW1_RequestDescription = "AAA";

				var writer = new TWJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())));
				var output = writer.GetDataObject(declaration);
				NUnit.Framework.Assert.That(output.EntryInstructionCollection.Count, Is.EqualTo(1));
				var outInstruction = output.EntryInstructionCollection.First();

				NUnit.Framework.Assert.That(outInstruction.AddInfoGroupCollection.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(outInstruction.AddInfoGroupCollection.Any(group =>
					group.Type.Code == new ZString?("CM")
					&& group.AddInfoCollection.Count == 12
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("MessageNumber") && innerAddinfo.Value == new ZString?("1"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("PermitNumber") && innerAddinfo.Value == new ZString?("A1"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("ControllingAgency") && innerAddinfo.Value == new ZString?("A"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("Link") && innerAddinfo.Value == new ZString?("1"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("BusinessType") && innerAddinfo.Value == new ZString?("A"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("ProcessingUnit") && innerAddinfo.Value == new ZString?("KG"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("PaymentMethod") && innerAddinfo.Value == new ZString?("1"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("ProofOfPaper") && innerAddinfo.Value == new ZString?("Y"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("ElectronicReceipt") && innerAddinfo.Value == new ZString?("Y"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("AppointmentDate") && innerAddinfo.Value == new ZString?("2019-02-12"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("AppointmentPeriod") && innerAddinfo.Value == new ZString?("A"))
					&& group.AddInfoCollection.Any(innerAddinfo => innerAddinfo.Key == new ZString?("Request") && innerAddinfo.Value == new ZString?("AAA"))
				), Is.True);
			}
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
