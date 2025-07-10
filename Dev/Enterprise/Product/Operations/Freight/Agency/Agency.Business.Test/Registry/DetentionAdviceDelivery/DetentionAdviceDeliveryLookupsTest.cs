using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DetentionAdviceDeliveryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPrinters()
		{
			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_AllowPrinting = true;
			printer.SQ_DisplayName = "Blaticus";
			printer.SQ_ServerName = "Server";
			Factory.Save();
			DetentionAdviceDelivery delivery = new DetentionAdviceDelivery(Factory);
			const string expected = "Blaticus - Server" + "";
			AssertMultilineASCIIEquals("", expected, delivery.Lookups.Printers.ElementsAsString);
		}

		public void TestGroups()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "BOB";
			group.GG_Desc = "Blaticus";
			Factory.Save();
			DetentionAdviceDelivery delivery = new DetentionAdviceDelivery(Factory);
			GlbGroupCollection groups = delivery.Lookups.Groups;
			AssertType(typeof(GlbGroupCollection), groups);
			groups.Load();
			AssertCollectionContains("", group, groups);
		}
	}
}
