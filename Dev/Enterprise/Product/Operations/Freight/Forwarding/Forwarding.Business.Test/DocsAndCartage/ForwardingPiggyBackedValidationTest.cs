using System;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingPiggyBackedValidationTest : JobDocsAndCartagePiggyBackedValidationTest
	{
		protected override Type GetJobDocsAndCartageType()
		{
			return typeof(ForwardingDocsAndCartage);
		}

		[ExpectNoExceptions]
		public void TestNoExceptionDuringTemplateCopy()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var shipment = Factory.New<ForwardingShipment>();
				var docsAndCartage = ForwardingDocsAndCartage.New(shipment);

				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_MessageType = "FTZ";

				var shipment2 = shipment.TemplateCopy() as ForwardingShipment;
				AssertNotNull(shipment2);
				AssertEquals(shipment2.PK, shipment2.DocsAndCartage.JP_ParentID);
			}
		}
	}
}
