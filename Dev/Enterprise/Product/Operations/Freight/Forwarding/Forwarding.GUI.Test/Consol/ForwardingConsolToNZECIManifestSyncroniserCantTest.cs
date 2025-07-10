using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[CountrySpecificTest(Constants.CountryCodes.Australia)]
	internal sealed class ForwardingConsolToNZECIManifestSyncroniserCantTest : TestCaseWithFactory
	{
		public void TestCantSyncronise()
		{
			var nzRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.NZ.INZCustomsDataRegistry>();
			nzRegistry.UpdateAttachedManifestedECIsWhenConsolDetailsChange.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				consol.FillWithValidTestData();

				var shipment1 = consol.Shipments.AddNew();
				shipment1.FillWithValidTestData();
				var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.NZ.IJobDeclaration>();
				declaration1.FillWithValidTestData();
				declaration1[JobDeclarationSchema.JE_MessageSubType] = "ECI";
				declaration1[JobDeclarationSchema.JE_JS] = shipment1.PK;

				Factory.Save();

				consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
				Factory.Save();
				AssertNotEquals(org1.PK, declaration1[JobDeclarationSchema.JE_OH_ShippingLine]);

				consol.JK_OA_ShippingLineAddress = org2.MainAddress.PK;
				Factory.Save();
				AssertNotEquals(org2.PK, declaration1[JobDeclarationSchema.JE_OH_ShippingLine]);
			}
		}
	}
}
