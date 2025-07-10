using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class UAEForwardingShipmentSupport : CountrySpecificForwardingShipmentSupport
	{
		protected override ZString CountryCode
		{
			get { return Constants.CountryCodes.UnitedArabEmirates; }
		}

		protected override void RegisterCore()
		{
			base.RegisterCore();
			SupportedBO.OnSavingShipment += new EventHandler(Shipment_OnSavingShipment);
		}

		void Shipment_OnSavingShipment(object sender, EventArgs e)
		{
			SetUAEDeliveryOrderNumberIfNotExist();
		}

		CusEntryNumber FindUAEDeliveryOrderNumberEntry()
		{
			ZQuery query = new ZQuery(CusEntryNumSchema.CE_EntryType, UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.DeliveryOrderNumber);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedArabEmirates);
			CusEntryNumber[] results = (CusEntryNumber[])SupportedBO.Numbers.Find(query);
			return results.Length > 0 ? results[0] : null;
		}

		void SetUAEDeliveryOrderNumberIfNotExist()
		{
			if (SupportedBO.IsImport() && SupportedBO.IsSea)
			{
				if (FindUAEDeliveryOrderNumberEntry() == null)
				{
					CusEntryNumber result = SupportedBO.Numbers.AddNew();
					result.CE_EntryType = UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.DeliveryOrderNumber;
					result.CE_EntryNum = ZString.Format("{0}{1}", FreightDataRegistry.Instance.UAEDeliveryOrderNumberPrefix.Value,
							Env.NumberFountains.UAEDeliveryOrderNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(SupportedBO.Factory));
				}
			}
		}
	}
}
