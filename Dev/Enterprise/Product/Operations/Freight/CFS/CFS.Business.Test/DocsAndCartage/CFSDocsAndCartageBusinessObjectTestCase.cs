using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSDocsAndCartage))]
	public class CFSDocsAndCartageBusinessObjectTestCase : JobDocsAndCartageBusinessObjectTestCase
	{
		public void TestJP_LCLAvailable()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			CFSDocsAndCartage docsAndCartage = shipment.DocsAndCartage;

			ZDateTime validDateTime = new ZDateTime(2003, 11, 21);
			docsAndCartage.JP_LCLAvailable = validDateTime;
			AssertEquals("Valid ZDateTime on JP_LCLAvailable", validDateTime, docsAndCartage.JP_LCLAvailable);

			ZDateTime invalidDateTime = ZDateTime.Invalid;
			docsAndCartage.JP_LCLAvailable = invalidDateTime;
			AssertEquals("Invalid ZDateTime on JP_LCLAvailable", invalidDateTime, docsAndCartage.JP_LCLAvailable);

			ZDateTime maxSmallDateTime = ZDateTime.MaxSmallDateTime.AddMonths(-1);
			docsAndCartage.JP_LCLAvailable = maxSmallDateTime;
			AssertEquals("Valid ZDateTime on JP_LCLAvailable", maxSmallDateTime, docsAndCartage.JP_LCLAvailable);

			ZDateTime minDateTime = new ZDateTime(DateTime.MinValue);
			docsAndCartage.JP_LCLAvailable = minDateTime;
			AssertEquals("Valid ZDateTime on JP_LCLAvailable", minDateTime, docsAndCartage.JP_LCLAvailable);
			docsAndCartage.JP_LCLAvailable = new ZDateTime(2003, 11, 21);
			docsAndCartage.JP_LCLAvailable = new ZDateTime(2003, 11, 22);
			docsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
			docsAndCartage.JP_LCLAvailable = ZDateTime.Empty;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			CFSShipment shipment = Factory.New<CFSShipment>();
			return shipment.DocsAndCartage;
		}
	}
}
