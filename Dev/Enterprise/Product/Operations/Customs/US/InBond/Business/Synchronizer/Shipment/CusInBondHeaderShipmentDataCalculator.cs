using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	class CusInBondHeaderShipmentDataCalculator : CusInBondHeaderDataCalculator, IDisposable
	{
		readonly ForwardingShipment shipment;

		public CusInBondHeaderShipmentDataCalculator(ForwardingShipment shipment, CusInBondHeader header)
			: base(header)
		{
			this.shipment = Argument.NotNull(shipment, "shipment");
			HookToEventsAffectingRelevantConsol();
		}

		public override ForwardingConsol RelevantConsol => CalculateRelevantConsol();

		protected override ZString InBondTransportMode => header.BH_ImportTransportMode;

		protected override bool IsSourceAir => shipment.IsAir;

		protected override BusinessObjectFactory SourceFactory => shipment.Factory;

		protected override ForwardingConsol ArrivalConsol => shipment.ArrivalConsol;

		protected override ZString SourceLoadingPort => shipment.JS_RL_NKOrigin;

		protected override ZString SourceTransportMode => shipment.JS_TransportMode;

		public override ZDateTime GetSailingDate() => shipment.JS_E_DEP;

		public override ZDateTime GetETADate() => shipment.JS_E_ARV;

		protected override List<Transport> Transports
		{
			get
			{
				var transports = new List<Transport>();
				if (RelevantConsol != null)
				{
					transports.AddRange(RelevantConsol.Transports.ToArray<Transport>());
				}
				transports.AddRange(shipment.Transports.ToArray<Transport>());
				return transports;
			}
		}

		protected override ForwardingConsol InBondParentConsol => header.Consol;

		protected override (ZString transportMode, ZString packingMode) GetTransportAndPacking()
		{
			var transportMode = ZString.Empty;
			var packingMode = ZString.Empty;
			var declaration = Declaration;
			if (declaration != null)
			{
				transportMode = declaration.JE_TransportMode;
				packingMode = declaration.JE_ContainerMode;
			}
			else
			{
				transportMode = shipment.JS_TransportMode;
				packingMode = shipment.JS_PackingMode;
			}
			return (transportMode, packingMode);
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingMasterBill()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_MasterBillInfo;
			}
			else
			{
				foreach (var info in GetInfosAffectingRelevantConsol())
				{
					yield return info;
				}

				var consol = RelevantConsol;
				if (consol != null)
				{
					yield return consol.JK_MasterBillNumInfo;
				}

				yield return shipment.JS_TransportModeInfo;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingMasterBillIssuerCode()
		{
			var declaration = Declaration;
			if (declaration != null)
			{
				yield return declaration.JE_MasterBillIssuerSCACInfo;
			}
			else
			{
				foreach (var info in GetInfosAffectingRelevantConsol())
				{
					yield return info;
				}

				var consol = RelevantConsol;
				if (consol != null)
				{
					yield return consol.JK_MasterBillNumInfo;
				}

				yield return shipment.JS_TransportModeInfo;
			}
		}

		public IEnumerable<ZPropertyInfo> GetInfosAffectingRelevantConsol()
		{
			yield return shipment.JS_RL_NKOriginInfo;
			yield return shipment.JS_RL_NKDestinationInfo;
			foreach (var info in GetConsolsInfos(ForwardingConsol.Schema.JK_RL_NKLoadPort, ForwardingConsol.Schema.JK_RL_NKDischargePort))
			{
				yield return info;
			}
		}

		public IEnumerable<ZPropertyInfo> GetConsolsInfos(params string[] propertyNames)
		{
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				foreach (var propertyName in propertyNames)
				{
					if (consol.ZPropertyInfoHash.ContainsKey(propertyName))
					{
						yield return consol.ZPropertyInfoHash[propertyName];
					}
				}
			}
		}

		#region Events

		void HookToEventsAffectingRelevantConsol()
		{
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				HookConsol(consol);
			}
			shipment.Consols.CountChanged += Consols_CountChanged;
			HookRelevantConsolCalculation(shipment.JS_RL_NKOriginInfo);
			HookRelevantConsolCalculation(shipment.JS_RL_NKDestinationInfo);
		}

		void RelevantConsolCalculation_ValueChanged(object sender, EventArgs e)
		{
			shouldCalculateRelevantConsol = true;
		}

		ForwardingConsol CalculateRelevantConsol()
		{
			if (shouldCalculateRelevantConsol)
			{
				shouldCalculateRelevantConsol = false;
				relevantConsol = null;
				var consols = shipment.Consols;
				if (consols.Count > 0)
				{
					var company = header.Company;
					var country = company != null ? company.Country : null;
					var countryCode = country != null ? country.Code : ZString.Empty;

					ZString localPortPropertyName = shipment.IsExport() ? ForwardingConsol.Schema.JK_RL_NKLoadPort
								: shipment.IsImport() ? ForwardingConsol.Schema.JK_RL_NKDischargePort : string.Empty;

					if (!localPortPropertyName.IsEmpty)
					{
						relevantConsol = (from ForwardingConsol consol in consols
										  where ((ZString)consol[localPortPropertyName]).Left(2) == countryCode && !consol.IsDomestic()
										  select consol).FirstOrDefault();
					}
					else if (consols.Count == 1)
					{
						relevantConsol = consols[0];
					}
				}
			}
			return relevantConsol;
		}
		ForwardingConsol relevantConsol;
		bool shouldCalculateRelevantConsol = true;

		void UnHookToEventsAffectingRelevantConsol()
		{
			foreach (ForwardingConsol consol in shipment.Consols)
			{
				UnHookConsol(consol);
			}
			shipment.Consols.CountChanged -= Consols_CountChanged;
			UnHookRelevantConsolCalculation(shipment.JS_RL_NKOriginInfo);
			UnHookRelevantConsolCalculation(shipment.JS_RL_NKDestinationInfo);
		}

		void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				HookConsol((ForwardingConsol)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnHookConsol((ForwardingConsol)e.BizObject);
			}
			RelevantConsolCalculation_ValueChanged(sender, e);
		}

		void UnHookRelevantConsolCalculation(ZPropertyInfo info)
		{
			info.ValueChanged -= RelevantConsolCalculation_ValueChanged;
		}

		void HookRelevantConsolCalculation(ZPropertyInfo info)
		{
			UnHookRelevantConsolCalculation(info);
			info.ValueChanged += RelevantConsolCalculation_ValueChanged;
		}

		void UnHookConsol(ForwardingConsol consol)
		{
			UnHookRelevantConsolCalculation(consol.JK_RL_NKLoadPortInfo);
			UnHookRelevantConsolCalculation(consol.JK_RL_NKDischargePortInfo);
		}

		void HookConsol(ForwardingConsol consol)
		{
			HookRelevantConsolCalculation(consol.JK_RL_NKLoadPortInfo);
			HookRelevantConsolCalculation(consol.JK_RL_NKDischargePortInfo);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			UnHookToEventsAffectingRelevantConsol();
		}

		#endregion
	}
}
