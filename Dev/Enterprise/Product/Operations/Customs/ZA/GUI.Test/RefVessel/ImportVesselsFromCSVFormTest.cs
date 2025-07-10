using System.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(ImportVesselsFromCSVForm))]
	sealed class ImportVesselsFromCSVFormTest : MasterFiles.GUI.Testing.ImportVesselFromCSVFormTest
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new ImportVesselsFromCSVForm();
		}

		protected override void SetCountryForThisTest()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.SouthAfrica);
		}

		protected override void PopulateTestFile(TempFile tempFile)
		{
			using (StreamWriter sw = new StreamWriter(tempFile.Filename))
			{
				sw.WriteLine("IDofMeansofTransport,Name,CarrierCode,CarrierName");
				sw.WriteLine("2DDTS,Sea Navigator,MSC,Mediterranean Shipping Company");
				sw.WriteLine("2VHY,Sassandra Challenger,MOL,Mitsui OSK Lines");
				sw.WriteLine("3BIY,MSC Parana,MSC,Mediterranean Shipping Company");
				sw.Flush();
			}
		}

		protected override string GetConfirmOkMessageText()
		{
			return "Please Note: Only vessels with valid TransportID, Name and CarrierCode will be loaded.";
		}
	}
}
