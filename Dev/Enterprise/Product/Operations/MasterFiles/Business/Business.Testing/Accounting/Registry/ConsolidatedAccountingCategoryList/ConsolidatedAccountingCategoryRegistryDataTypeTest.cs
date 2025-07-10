using System.Collections.Generic;
using System.Text;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ConsolidatedAccountingCategoryCollection))]
	sealed class ConsolidatedAccountingCategoryRegistryDataTypeTest : AccountingCodeDescriptionWithGroupRegistryDataTypeTest<ConsolidatedAccountingCategoryCollection>
	{
		protected override IEnumerable<(ConsolidatedAccountingCategoryCollection, byte[])> CreateValidSample()
		{
			var collection1 = new ConsolidatedAccountingCategoryCollection();
			var element1_1 = collection1.AddNew();
			element1_1.Code = "AA1";
			element1_1.Description = (NoResString)"XYZ";
			element1_1.Group = "TPY";
			var element1_2 = collection1.AddNew();
			element1_2.Code = "BB2";
			element1_2.Description = (NoResString)"ZZZ";
			element1_2.Group = "INT";

			var collection2 = new ConsolidatedAccountingCategoryCollection();
			var element2_1 = collection2.AddNew();
			element2_1.Code = "AA2";
			element2_1.Description = (NoResString)"XYZ";
			element2_1.Group = "INT";

			const string expectedXmlValue_collection1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfConsolidatedAccountingCategoryItem xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><ConsolidatedAccountingCategoryItem><CodeMaxLength>3</CodeMaxLength><Code>AA1</Code><Description>XYZ</Description><Group>TPY</Group><SystemDefined>False</SystemDefined></ConsolidatedAccountingCategoryItem><ConsolidatedAccountingCategoryItem><CodeMaxLength>3</CodeMaxLength><Code>BB2</Code><Description>ZZZ</Description><Group>INT</Group><SystemDefined>False</SystemDefined></ConsolidatedAccountingCategoryItem></ArrayOfConsolidatedAccountingCategoryItem>";

			const string expectedXmlValue_collection2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfConsolidatedAccountingCategoryItem xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><ConsolidatedAccountingCategoryItem><CodeMaxLength>3</CodeMaxLength><Code>AA2</Code><Description>XYZ</Description><Group>INT</Group><SystemDefined>False</SystemDefined></ConsolidatedAccountingCategoryItem></ArrayOfConsolidatedAccountingCategoryItem>";

			return new[] {
				(collection1,Encoding.Unicode.GetBytes(expectedXmlValue_collection1)),
				(collection2,Encoding.Unicode.GetBytes(expectedXmlValue_collection2)),
			};
		}
	}
}
