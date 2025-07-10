using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.DataTransfer.Universal;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterpise.Customs.TW.DataTransfer.Testing
{
	public sealed class TWJobDeclarationAdditionalAddInfoGroupCollectionDataObjectWriterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCreateCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var gui = declaration.GovernmentUniformInvoices.AddNew();
			gui.CY_Code = "1111";
			gui.Amount = 2222m;
			gui = declaration.GovernmentUniformInvoices.AddNew();
			gui.CY_Code = "3333";
			gui.Amount = 4444m;
			var writer = new TWJobDeclarationAdditionalAddInfoGroupCollectionDataObjectWriter(declaration);
			var collection = writer.CreateCollection();
			NUnit.Framework.Assert.That(collection.Count(), Is.EqualTo(2));
			var first = collection.First();
			NUnit.Framework.Assert.That(first.Type.Code, Is.EqualTo("GUI").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.Type.Description, Is.EqualTo("Government Uniform Invoice").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(first.AddInfoCollection.Count, Is.EqualTo(2));
			var fisrt_guiNum = first.AddInfoCollection.First();
			NUnit.Framework.Assert.That(fisrt_guiNum.Key, Is.EqualTo("GovernmentUniformInvoiceNumber").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(fisrt_guiNum.Value, Is.EqualTo("1111").Using(CustomComparers.TypeComparison));
			var fisrt_guiAmount = first.AddInfoCollection.Skip(1).First();
			NUnit.Framework.Assert.That(fisrt_guiAmount.Key, Is.EqualTo("GovernmentUniformInvoiceAmount").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(fisrt_guiAmount.Value, Is.EqualTo("2222").Using(CustomComparers.TypeComparison));
			var second = collection.Skip(1).First();
			NUnit.Framework.Assert.That(second.Type.Code, Is.EqualTo("GUI").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(second.Type.Description, Is.EqualTo("Government Uniform Invoice").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(second.AddInfoCollection.Count, Is.EqualTo(2));
			var second_guiNum = second.AddInfoCollection.First();
			NUnit.Framework.Assert.That(second_guiNum.Key, Is.EqualTo("GovernmentUniformInvoiceNumber").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(second_guiNum.Value, Is.EqualTo("3333").Using(CustomComparers.TypeComparison));
			var second_guiAmount = second.AddInfoCollection.Skip(1).First();
			NUnit.Framework.Assert.That(second_guiAmount.Key, Is.EqualTo("GovernmentUniformInvoiceAmount").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(second_guiAmount.Value, Is.EqualTo("4444").Using(CustomComparers.TypeComparison));
		}
	}
}
