using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[CountrySpecificTest(Constants.CountryCodes.NewZealand)]
	internal sealed class ForwardingConsolToNZECIManifestSyncroniserCanTest : TestCaseWithFactory
	{
		public void TestCanSyncronise()
		{
			var nzRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.NZ.INZCustomsDataRegistry>();
			nzRegistry.UpdateAttachedManifestedECIsWhenConsolDetailsChange.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var consol = Factory.New<ForwardingConsol>();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				consol.FillWithValidTestData();

				var shipment1 = consol.Shipments.AddNew();
				shipment1.FillWithValidTestData();
				var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.NZ.IJobDeclaration>();
				declaration1.FillWithValidTestData();
				declaration1[JobDeclarationSchema.JE_MessageSubType] = "ECI";
				declaration1[JobDeclarationSchema.JE_DeclarationReference] = "M00009898-1";
				declaration1[JobDeclarationSchema.JE_JS] = shipment1.PK;

				Factory.Save();

				consol.JK_MasterBillNum = "111-11111111";
				Factory.Save();
				AssertEquals("111-11111111", declaration1[JobDeclarationSchema.JE_MasterBill]);

				consol.JK_MasterBillNum = "222-22222222";
				Factory.Save();
				AssertEquals("222-22222222", declaration1[JobDeclarationSchema.JE_MasterBill]);
			}
		}
		public void TestMasterBillNumFieldWithOldData()
		{
			var nzRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.NZ.INZCustomsDataRegistry>();
			nzRegistry.UpdateAttachedManifestedECIsWhenConsolDetailsChange.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var consol = Factory.New<ForwardingConsol>();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UserIdleWorker.Flush();
				consol.FillWithValidTestData();
				var shipment1 = consol.Shipments.AddNew();
				shipment1.FillWithValidTestData();
				var declaration1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.NZ.IJobDeclaration>();
				declaration1.FillWithValidTestData();
				declaration1[JobDeclarationSchema.JE_MessageSubType] = "ECI";
				declaration1[JobDeclarationSchema.JE_DeclarationReference] = "M00009898-1";
				declaration1[JobDeclarationSchema.JE_JS] = shipment1.PK;

				Factory.Save();
				form.ConsolControl.Consol.JK_TransportMode = "AIR";
				consol.JK_MasterBillNum = "111-11111111222"; //Simulate bad previous database data (before the 8 length limit)
				AssertNoExceptionThrown(() => form.ConsolControl.Consol.MasterBillMAWB = form.ConsolControl.Consol.MasterBillMAWB.SubstringSafe(0, form.ConsolControl.Consol.MasterBillMAWB.Length - 1)); //we simulate the user entering field, and slightly modifying the field, yet leaving it greater than 8 characters
				Factory.Save();
			}
		}
	}
}
