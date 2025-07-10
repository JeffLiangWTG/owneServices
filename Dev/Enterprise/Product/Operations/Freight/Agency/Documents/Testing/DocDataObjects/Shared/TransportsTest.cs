using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(Transports))]
	class TransportsTest : NonPersistentBusinessObjectTestCase
	{
		#region TestMain

		public void TestMain()
		{
			var transports = CreateTransports();

			AssertEquals("AUSYD", transports.Main.PortOfLoading.Code);
			AssertEquals("NZAKL", transports.Main.PortOfDischarge.Code);
		}

		#endregion

		#region TestCollection

		public void TestCollection()
		{
			var transports = CreateTransports();

			AssertArrayEqualsByElements(new[]
			{
				"AUBNE|AUSYD",
				"AUSYD|NZAKL",
				"NZAKL|NZCHC"
			},
			transports.Select(t => $"{t.PortOfLoading.Code}|{t.PortOfDischarge.Code}").ToArray());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Transports.Create(new CommonContext(Factory), new List<Freight.Business.Transport>());
		}

		Transports CreateTransports()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_RL_NKLoadPort = "AUBNE";
			billOfLading.JS_RL_NKDischargePort = "NZCHC";

			var transport1 = billOfLading.Transports.AddNew();
			transport1.JW_TransportMode = TransportModes.Road;
			transport1.JW_TransportType = TransportPlanningType.PreCarriage;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = billOfLading.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_TransportType = TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var transport3 = billOfLading.Transports.AddNew();
			transport3.JW_TransportMode = TransportModes.Rail;
			transport3.JW_TransportType = TransportPlanningType.OnForwarding;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "NZCHC";

			return Transports.Create(Context, new[]
			{
				transport1,
				transport2,
				transport3
			});
		}

		IContext Context => context ?? (context = new CommonContext(Factory.GetCachedReadOnlyFactory()));
		IContext context;

		#endregion
	}
}
