using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateAttachmentSetLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentsAditionalFilter()
		{
			var item1 = Factory.New<StmMenuItem>();
			item1.SU_MenuName = "Item";
			item1.SU_BusinessContext = nameof(BusinessContext.Quotation);

			var item2 = Factory.New<StmMenuItem>();
			item2.SU_MenuName = "Item";
			item2.SU_BusinessContext = nameof(BusinessContext.Rating);

			Factory.Save();

			var collection = Factory.New<RateAttachmentSet>().Lookups.Documents;

			var filter = new ZQuery();
			filter.AddToFilter(collection.CompleteFilter);
			filter.AddToFilter(StmMenuItemSchema.PK, new ZGuid[] { item1.PK, item2.PK });

			AssertContainsExactElementsInAnyOrder("Should only contain item1",
				(i) => string.Format("{0}:{1}", i.SU_BusinessContext, i.SU_MenuName),
				new StmMenuItem[] { item1 },
				Factory.Load<StmMenuItem>(filter));
		}

		public void TestDocumentsDefaultFilter()
		{
			var collection = Factory.New<RateAttachmentSet>().Lookups.Documents;
			var actual = new List<string>();

			foreach (FilterBusinessObjectDefault def in collection.FilterBusinessObjectDefaults)
			{
				actual.Add(string.Format("{0}:{1} => {2}", def.FilterName, def.PropertyName, def.Value));
			}

			AssertContainsExactElementsInAnyOrder(
				"Defaults",
				new string[] { "Business Context:Property => Quotation", },
				actual);
		}
	}
}
