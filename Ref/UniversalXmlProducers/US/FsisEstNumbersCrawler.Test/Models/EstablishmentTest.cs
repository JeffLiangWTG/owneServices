using System;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FsisEstNumbersCrawler.Models;
using NUnit.Framework;

namespace FsisEstNumbersCrawler.Test.Models
{
	[TestFixture]
	public class EstablishmentTest
	{
		[Test]
		public void ToCodeListsShouldReturnOneItemIfCodeContainsNoSeparator()
		{
			var establishment = new Establishment
			{
				EstNumber = "0001",
				Company = "s`ome company",
				Street = "some street",
				City = "some city",
				State = "some state",
				Zip = "some zip",
				Phone = "12345678",
				GrantDate = DateTime.Today
			};

			var codeLists = establishment.ToCodeLists().ToArray();
			Assert.AreEqual(1, codeLists.Length);

			Assert.AreEqual("0001", codeLists[0].ZZD_Code);
			Assert.AreEqual("0001", codeLists[0].ZZD_Description);
			Assert.AreEqual(DateTime.Today, codeLists[0].ZZD_StartDate);
			Assert.AreEqual(DateTime.Parse("2079-06-06 23:59", CultureInfo.CurrentCulture), codeLists[0].ZZD_EndDate);
			Assert.AreEqual("US", codeLists[0].ZZD_ZZZ_NKDataGrouping);
			Assert.AreEqual("FSIS", codeLists[0].ZZD_ZZK_NKCodeType);

			Assert.AreEqual(6, codeLists[0].RefCusCodeListAttributes.Length);

			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberCompany", "some company");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberStreet", "some street");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberCity", "some city");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberState", "some state");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberZip", "some zip");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberPhone", "12345678");
		}

		[Test]
		public void ParseIfMissingNumbers()
		{
			FsisEstNumbersCrawler.Services.ICsvParser Parser = new FsisEstNumbersCrawler.Services.CsvParser();
			var filePath = Utilities.CurrentFolder() + "\\Resources\\MPI_TestSmallData.csv";
			var establishments = Parser.Parse<Establishment>(filePath);
			Assert.AreEqual(4, establishments.Count);

			foreach (var item in establishments)
			{
				var value = item.ToCodeLists().ToList();
				if (value[0].ZZD_Code == "I48")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "Lineage Logistics PFS, LLC");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "2500 S. Damen");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Chicago");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "IL");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "60608");
				}
				else if (value[0].ZZD_Code == "I499")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "Davy Cold Storage");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "2073 West Garden Rd");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Vineland");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "NJ");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "8360");
				}
				else if (value[0].ZZD_Code == "I49")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "Lineage Logistics PFS, LLC");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "60 Commercial Street");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Everett");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "MA");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "2149");
				}
				else if (value[0].ZZD_Code == "M31827")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "PelMeni Inc.");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "1920 Main Street #10");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Ferndale");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "WA");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "98248");
				}
			}
		}

		[Test]
		public void Parse23072024IfMissingNumbers()
		{
			FsisEstNumbersCrawler.Services.ICsvParser Parser = new FsisEstNumbersCrawler.Services.CsvParser();
			var filePath = Utilities.CurrentFolder() + "\\Resources\\MPI_2307_byNumber.csv";
			var establishments = Parser.Parse<Establishment>(filePath);

			Assert.AreEqual(7049, establishments.Count);

			foreach (var item in establishments)
			{
				var value = item.ToCodeLists().ToList();
				if (value[0].ZZD_Code == "I48")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "Lineage Logistics PFS, LLC");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "2500 S. Damen");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Chicago");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "IL");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "60608");
				}
				else if (value[0].ZZD_Code == "I499")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "Davy Cold Storage");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "2073 West Garden Rd");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Vineland");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "NJ");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "8360");
				}
				else if (value[0].ZZD_Code == "I499")
				{
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCompany", "Lineage Logistics PFS, LLC");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberStreet", "60 Commercial Street");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberCity", "Everett");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberState", "MA");
					AssertHasAttribute(value[0], "USFSISEstablishmentNumberZip", "2149");
				}
			}
		}

		[Test]
		public void ToCodeListsShouldReturnMultipleItemsIfCodeContainsSeparator()
		{
			var establishment = new Establishment
			{
				EstNumber = "0001+0002+0003",
				Company = "some company",
				Street = "some street",
				City = "some city",
				State = "some state",
				Zip = "some zip",
				Phone = "12345678",
				GrantDate = DateTime.Today
			};
			var codeLists = establishment.ToCodeLists().ToList();

			Assert.AreEqual(3, codeLists.Count);

			Assert.AreEqual("0001", codeLists[0].ZZD_Code);
			Assert.AreEqual("0002", codeLists[1].ZZD_Code);
			Assert.AreEqual("0003", codeLists[2].ZZD_Code);

			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberCompany", "some company");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberStreet", "some street");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberCity", "some city");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberState", "some state");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberZip", "some zip");
			AssertHasAttribute(codeLists[0], "USFSISEstablishmentNumberPhone", "12345678");
		}

		[Test]
		public void ToCodeListsShouldTrimStrings()
		{
			var establishment = new Establishment
			{
				EstNumber = "0001+ 0002 + 0003",
				Company = " some company  ",
				Street = "  some street ",
				City = "  some city  ",
				State = "   some state ",
				Zip = " some zip ",
				Phone = " 12345678  ",
				GrantDate = DateTime.Today
			};
			var codeLists = establishment.ToCodeLists().ToList();

			Assert.AreEqual(3, codeLists.Count);

			Assert.AreEqual("0001", codeLists[0].ZZD_Code);
			Assert.AreEqual("0002", codeLists[1].ZZD_Code);
			Assert.AreEqual("0003", codeLists[2].ZZD_Code);
		}

		void AssertHasAttribute(RefCusCodeList codeList, string name, string value)
		{
			Assert.IsTrue(codeList.RefCusCodeListAttributes.Any(x => x.ZZE_ZXE_NKName == name && x.ZZE_Value == value));
		}
	}
}
