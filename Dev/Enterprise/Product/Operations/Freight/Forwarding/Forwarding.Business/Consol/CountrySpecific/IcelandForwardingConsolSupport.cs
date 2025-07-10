using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class IcelandForwardingConsolSupport : CountrySpecificJobSupport<ForwardingConsol>
	{
		public static string LoadPortChangedCause
		{
			get { return Res.GetString("9ecd8c5b-f8e0-4b89-b163-1669485ca7dc", "due to change of Port of Loading by user"); }
		}
		public static string DischargePortChangedCause
		{
			get { return Res.GetString("0a5bbe96-e595-4213-88bf-fe6f682fe405", "due to change of Port of Discharge by user"); }
		}
		public static string ReceivingForwarderChangedCause
		{
			get { return Res.GetString("dbf1d7d0-1aac-48f0-8d19-c1067a6f2519", "due to change of Receiving Agent by user"); }
		}

		protected override ZString CountryCode
		{
			get { return Constants.CountryCodes.Iceland; }
		}

		protected override bool Apply()
		{
			return base.Apply() && CountryCode == GlbBranch.CurrentBranch.Country.Code;
		}

		protected override void RegisterCore()
		{
			base.RegisterCore();

			SupportedBO.JK_OA_ReceivingForwarderAddressInfo.ValueChanged += new EventHandler(OnReceivingForwarderAddressValueChanged);
			SupportedBO.ImportExportChangedAfterDischargePortChange += new EventHandler(OnImportExportChanged);
			SupportedBO.JK_RL_NKLoadPortInfo.ValueChanged += new EventHandler(OnLoadPortValueChanged);
			SupportedBO.NumbersLoaded += new EventHandler(Consol_NumbersLoaded);
			SupportedBO.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(Consol_HasChangesChanged);

			SupportedBO.JK_OA_ShippingLineAddressInfo.ValueChanged += new EventHandler(OnValueChangedToSetCarrierAddress);
			SupportedBO.JK_TransportModeInfo.ValueChanged += new EventHandler(OnValueChangedToSetTransportMode);
		}

		void Consol_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), Env.Security.MaintainConsolCOCOverride.IsAllowed);
		}

		void Consol_NumbersLoaded(object sender, EventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.ProcessIcelandCustomsOfficeCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), Env.Security.MaintainConsolCOCOverride.IsAllowed);
		}

		void OnReceivingForwarderAddressValueChanged(object sender, EventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), ReceivingForwarderChangedCause, Env.Security.MaintainConsolCOCOverride.IsAllowed);
		}

		void OnImportExportChanged(object sender, EventArgs e)
		{
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(SupportedBO.Numbers, FindIcelandCustomsOfficeCode(), DischargePortChangedCause, Env.Security.MaintainConsolCOCOverride.IsAllowed);
		}

		void OnLoadPortValueChanged(object sender, EventArgs e)
		{
			ZString coc = FindIcelandCustomsOfficeCode();
			IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(SupportedBO.Numbers, coc, LoadPortChangedCause, Env.Security.MaintainConsolCOCOverride.IsAllowed);
			if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland)
			{
				foreach (ForwardingShipment shipment in SupportedBO.Shipments)
				{
					if (shipment.IsExport() && shipment.DepartureConsol == SupportedBO)
					{
						IcelandCustomsOfficeCodeHelper.SetIcelandCustomsHouseCode(shipment.Numbers, coc, LoadPortChangedCause, Env.Security.MaintainShipmentCOCOverride.IsAllowed);
					}
				}
			}
		}

		ZString FindIcelandCustomsOfficeCode()
		{
			RefUNLOCO relevantLoco = null;
			if (SupportedBO.IsImport() && SupportedBO.ReceivingForwarder != null)
			{
				relevantLoco = SupportedBO.ReceivingForwarder.UNLOCO;
			}
			else if (SupportedBO.IsExport())
			{
				relevantLoco = SupportedBO.LoadPort;
			}

			return relevantLoco != null ? relevantLoco.RefLocoMaps.LocalCodeForUsageAndCountry(ISLocoMapSystemUsageList.Codes.CustomsOfficeCode,
				Core.Constants.CountryCodes.Iceland) : ZString.Empty;
		}

		#region Sendingarnumer

		void OnValueChangedToSetTransportMode(object sender, EventArgs e)
		{
			SupportedBO.MarkAsNeedingValidation();
		}

		void OnValueChangedToSetCarrierAddress(object sender, EventArgs e)
		{
			if (GlbBranch.CurrentBranch.Country.Code == Core.Constants.CountryCodes.Iceland &&
				!(SupportedBO.SendingarnumerHelper.Sendingarnumer.IsAir && SupportedBO.SendingarnumerHelper.Sendingarnumer.IsImport)
				&& SupportedBO.ShippingLine != null)
			{
				OrgHeader carrier = SupportedBO.ShippingLine;
				ZQuery carrierCodeFilter = new ZQuery();
				carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				carrierCodeFilter.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);

				if (carrier != null && carrier.CustomsCodes.Find(carrierCodeFilter).Length > 0)
				{
					SupportedBO.SendingarnumerHelper.Sendingarnumer.CarrierCode = ((OrgCusCode)carrier.CustomsCodes.Find(carrierCodeFilter)[0]).OK_CustomsRegNo;
					SupportedBO.SendingarnumerHelper.RefreshCRN();
				}
			}
		}

		#endregion

	}
}
