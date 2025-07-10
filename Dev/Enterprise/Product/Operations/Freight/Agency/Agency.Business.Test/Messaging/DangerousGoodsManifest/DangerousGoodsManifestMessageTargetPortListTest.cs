using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class DangerousGoodsManifestMessageTargetPortListTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL", false);
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			AssertLoad(Guid.Empty, portCollection);
			AssertLoad(GlbCompany.CurrentCompany.PK.ToGuid(), portCollection);
		}

		public void TestDirections()
		{
			var portCollection = new DangerousGoodsManifestPortCollection();

			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUSYD");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUMEL");
			DangerousGoodsManifestMessageTestHelper.CreateDangerousGoodsManifestPort(portCollection, "AUBNE");

			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin1 = voyage.Origins.AddNew();
				origin1.FillWithValidTestData();
				origin1.JA_RL_NKPortOfLoading = "AUSYD";
				origin1.JA_E_DEP = new DateTime(2022, 11, 29);

				var origin2 = voyage.Origins.AddNew();
				origin2.FillWithValidTestData();
				origin2.JA_RL_NKPortOfLoading = "AUMEL";
				origin2.JA_E_DEP = new DateTime(2022, 12, 01);

				var destinations1 = voyage.Destinations.AddNew();
				destinations1.FillWithValidTestData();
				destinations1.JB_RL_NKPortOfDischarge = "AUMEL";
				destinations1.JB_E_ARV = new DateTime(2022, 11, 30);

				var destinations2 = voyage.Destinations.AddNew();
				destinations2.FillWithValidTestData();
				destinations2.JB_RL_NKPortOfDischarge = "AUBNE";
				destinations2.JB_E_ARV = new DateTime(2022, 12, 02);

				Factory.Save();

				var mssageTargetPortList = new DangerousGoodsManifestMessageTargetPortList(voyage);
				mssageTargetPortList.Load();

				AssertContainsExactElementsInAnyOrder(new List<ZString> { "AUSYD", "AUMEL", "AUBNE" }, mssageTargetPortList.GetAllCodesZString());

				Assert(mssageTargetPortList["AUSYD"].Directions.HasFlag(PortDirections.Load));
				Assert(!mssageTargetPortList["AUSYD"].Directions.HasFlag(PortDirections.Transit));
				Assert(!mssageTargetPortList["AUSYD"].Directions.HasFlag(PortDirections.Discharge));

				Assert(mssageTargetPortList["AUMEL"].Directions.HasFlag(PortDirections.Load));
				Assert(mssageTargetPortList["AUMEL"].Directions.HasFlag(PortDirections.Transit));
				Assert(mssageTargetPortList["AUMEL"].Directions.HasFlag(PortDirections.Discharge));

				Assert(!mssageTargetPortList["AUBNE"].Directions.HasFlag(PortDirections.Load));
				Assert(!mssageTargetPortList["AUBNE"].Directions.HasFlag(PortDirections.Transit));
				Assert(mssageTargetPortList["AUBNE"].Directions.HasFlag(PortDirections.Discharge));
			}
		}

		void AssertLoad(Guid guid, DangerousGoodsManifestPortCollection dangerousGoodsManifestPortCollection)
		{
			using (AgencyRegistry.Instance.DangerousGoodsManifestPorts.SetTemporaryValue(guid, Guid.Empty, Guid.Empty, dangerousGoodsManifestPortCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin1 = voyage.Origins.AddNew();
				origin1.FillWithValidTestData();
				origin1.JA_RL_NKPortOfLoading = "AUSYD";
				origin1.JA_E_DEP = new DateTime(2022, 11, 29);

				var origin2 = voyage.Origins.AddNew();
				origin2.FillWithValidTestData();
				origin2.JA_RL_NKPortOfLoading = "AUMEL";
				origin2.JA_E_DEP = new DateTime(2022, 12, 01);

				var destinations1 = voyage.Destinations.AddNew();
				destinations1.FillWithValidTestData();
				destinations1.JB_RL_NKPortOfDischarge = "AUMEL";
				destinations1.JB_E_ARV = new DateTime(2022, 11, 30);

				var destinations2 = voyage.Destinations.AddNew();
				destinations2.FillWithValidTestData();
				destinations2.JB_RL_NKPortOfDischarge = "AUARD";
				destinations2.JB_E_ARV = new DateTime(2022, 12, 02);

				var origin3 = voyage.Origins.AddNew();
				origin3.FillWithValidTestData();
				origin3.JA_RL_NKPortOfLoading = "AUBNE";
				origin3.JA_E_DEP = new DateTime(2022, 12, 05);

				Factory.Save();

				var mssageTargetPortList = new DangerousGoodsManifestMessageTargetPortList(voyage);
				mssageTargetPortList.Load();

				AssertContainsExactElementsInAnyOrder(new List<ZString> { "AUSYD", "AUBNE" }, mssageTargetPortList.GetAllCodesZString());
			}
		}
	}
}
