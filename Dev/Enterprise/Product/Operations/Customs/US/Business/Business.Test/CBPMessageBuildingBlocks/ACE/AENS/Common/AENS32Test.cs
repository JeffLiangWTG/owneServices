using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.Common;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS32Test : TestCaseWithFactory
	{
		public void TestUpdateDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "B00110010";
			declaration.US_ConsolACE = true;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";

			var releaseDeclaration1 = Factory.New<JobDeclaration>();
			releaseDeclaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration1.US_EntryFilerCode = "SV9";
			releaseDeclaration1.JE_DeclarationReference = "B00110020";
			var releaseInvoice1 = releaseDeclaration1.Invoices.AddNew();
			releaseInvoice1.JZ_InvoiceNumber = "REL001";
			var releaseEntry1 = releaseDeclaration1.CustomsEntryHeaders.AddNew();
			releaseEntry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var releaseDeclaration2 = Factory.New<JobDeclaration>();
			releaseDeclaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration2.US_EntryFilerCode = "SV9";
			releaseDeclaration2.JE_DeclarationReference = "B00110040";
			var releaseInvoice2 = releaseDeclaration2.Invoices.AddNew();
			releaseInvoice2.JZ_InvoiceNumber = "REL002";
			var releaseEntry2 = releaseDeclaration2.CustomsEntryHeaders.AddNew();
			releaseEntry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var releaseDeclaration3 = Factory.New<JobDeclaration>();
			releaseDeclaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration3.US_EntryFilerCode = "SV9";
			releaseDeclaration3.JE_DeclarationReference = "B00110060";
			var releaseInvoice3 = releaseDeclaration3.Invoices.AddNew();
			releaseInvoice3.JZ_InvoiceNumber = "REL003";
			var releaseEntry3 = releaseDeclaration3.CustomsEntryHeaders.AddNew();
			releaseEntry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			Factory.Save();

			invoice1.US_ReleaseEntryNumber = releaseEntry3.EntryFilerCode + releaseEntry3.EntryNumber;
			var notifications = new NotificationBuffer();
			var aens32 = new AENS32()
			{
				ReleaseEntryFilerCode1 = "SV9",
				ReleaseEntryNumber1 = releaseEntry1.EntryNumber,
				ReleaseEntryFilerCode2 = "SV9",
				ReleaseEntryNumber2 = releaseEntry1.EntryNumber,
				ReleaseEntryFilerCode3 = "SV9",
				ReleaseEntryNumber3 = releaseEntry2.EntryNumber,
				ReleaseEntryFilerCode4 = "SV9",
				ReleaseEntryNumber4 = releaseEntry3.EntryNumber,
			};

			((IBIRDHeaderRecord)aens32).Update(declaration, notifications);
			AssertEquals(4, declaration.Invoices.Count);
			AssertEquals(releaseEntry1.EntryFilerCode + releaseEntry1.EntryNumber, declaration.Invoices[0].US_ReleaseEntryNumber);
			AssertEquals(releaseEntry2.EntryFilerCode + releaseEntry2.EntryNumber, declaration.Invoices[1].US_ReleaseEntryNumber);
			AssertEquals(releaseEntry3.EntryFilerCode + releaseEntry3.EntryNumber, declaration.Invoices[2].US_ReleaseEntryNumber);
			AssertEquals(releaseEntry3.EntryFilerCode + releaseEntry3.EntryNumber, declaration.Invoices[3].US_ReleaseEntryNumber);
		}
	}
}
