using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	sealed class NZPortMessageTargetPortListTest : BaseAgencyTest
	{
		public void TestLoad()
		{
			var portCollection = new PortManifestPortCollection();

			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZAKL");
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZLYT", false);
			PortManifestMessageTestHelper.CreateLoadAndDischargeManifestPort(portCollection, "NZNPE");

			AssertLoad(Guid.Empty, portCollection);
			AssertLoad(GlbCompany.CurrentCompany.PK.ToGuid(), portCollection);
		}

		void AssertLoad(Guid guid, PortManifestPortCollection portManifestPortCollection)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			using (AgencyRegistry.Instance.LoadAndDischargeManifestPorts.SetTemporaryValue(guid, Guid.Empty, Guid.Empty, portManifestPortCollection))
			{
				var voyage = Factory.NewWithValidTestData<JobVoyage>();

				var origin1 = voyage.Origins.AddNew();
				origin1.FillWithValidTestData();
				origin1.JA_RL_NKPortOfLoading = "NZAKL";
				origin1.JA_E_DEP = new DateTime(2022, 11, 29);

				var origin2 = voyage.Origins.AddNew();
				origin2.FillWithValidTestData();
				origin2.JA_RL_NKPortOfLoading = "NZLYT";
				origin2.JA_E_DEP = new DateTime(2022, 12, 01);

				var destinations1 = voyage.Destinations.AddNew();
				destinations1.FillWithValidTestData();
				destinations1.JB_RL_NKPortOfDischarge = "NZLYT";
				destinations1.JB_E_ARV = new DateTime(2022, 11, 30);

				var destinations2 = voyage.Destinations.AddNew();
				destinations2.FillWithValidTestData();
				destinations2.JB_RL_NKPortOfDischarge = "AUARD";
				destinations1.JB_E_ARV = new DateTime(2022, 12, 02);

				voyage.GenerateSailings();

				var origin3 = voyage.Origins.AddNew();
				origin3.FillWithValidTestData();
				origin3.JA_RL_NKPortOfLoading = "NZNPE";
				origin3.JA_E_DEP = new DateTime(2022, 12, 05);

				var aubneSailings = new JobSailingCollection(Factory);

				foreach (JobSailing sailing in voyage.Sailings)
				{
					if (sailing.JX_JA_RL_NKPortOfLoading == "NZNPE")
					{
						aubneSailings.Add(sailing);
					}
				}

				voyage.Sailings.RemoveRange(aubneSailings);

				Factory.Save();

				AssertNull(voyage.Sailings.OfType<JobSailing>().FirstOrDefault(x => x.JX_JA_RL_NKPortOfLoading == "NZNPE"));

				var messageTargetPortList = new NZPortMessageTargetPortList(voyage);
				messageTargetPortList.Load();

				AssertContainsExactElementsInAnyOrder(new List<ZString> { "NZAKL", "NZNPE" }, messageTargetPortList.GetAllCodesZString());
			}
		}
	}
}
