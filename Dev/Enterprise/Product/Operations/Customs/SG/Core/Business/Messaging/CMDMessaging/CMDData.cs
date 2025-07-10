using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	#region class Goods

	public class Goods
	{
		public Goods(ZString description, ZString harmonisedCode)
		{
			ManifestDescription = description;
			this.HarmonisedCode = harmonisedCode;
		}

		public readonly ZString ManifestDescription;
		public readonly ZString HarmonisedCode;
	}

	#endregion

	public class CMDData
	{
		public CMDData(CMDShipmentWrapper shipmentWrapper)
		{
			this.shipmentWrapper = shipmentWrapper;
		}

		#region Properties

		#region Import Consol

		public virtual ForwardingConsol ImportConsol
		{
			get
			{
				if (fImportConsol == null)
				{
					TranshipmentHelper helper = new TranshipmentHelper(Shipment);
					if (helper.TranshipmentPorts.Length > 0 && helper.ImportConsol != null)
					{
						fImportConsol = helper.ImportConsol.Factory.Load<ForwardingConsol>(helper.ImportConsol.PK);
					}
				}
				return fImportConsol;
			}
		}

		ForwardingConsol fImportConsol;

		#endregion

		#region HAWB Serial No

		public virtual ZString HAWBSerialNo
		{
			get { return Shipment.JS_HouseBill; }
		}

		#endregion

		#region HAWB No of Pieces

		public virtual ZInt HAWBNoOfPieces
		{
			get
			{
				if (fHAWBNoOfPieces == null)
				{
					fHAWBNoOfPieces = (Shipment.JS_TotalPackageCount > 0) ? Shipment.JS_TotalPackageCount : Shipment.JS_OuterPacks;
				}
				return fHAWBNoOfPieces.Value;
			}
		}

		ZInt? fHAWBNoOfPieces;

		#endregion

		#region HAWB Weight Code

		public virtual ZString HAWBWeightCode
		{
			get
			{
				if (fHAWBWeightCode == null)
				{
					fHAWBWeightCode = (Env.Registry.FreightWeightUnit == Core.Constants.Weight.Pounds) ? "L" : "K";
				}
				return fHAWBWeightCode;
			}
		}

		string fHAWBWeightCode;

		#endregion

		#region HAWB Gross Weight

		public virtual ZDecimal HAWBGrossWeight
		{
			get
			{
				if (fHAWBGrossWeight == null)
				{
					fHAWBGrossWeight = Shipment.GetWeightForDoc(Env.Registry.Freight.AirWaybill.AirWaybillHAWBWeightAndVolumeDisplay);
				}
				return fHAWBGrossWeight.Value;
			}
		}

		ZDecimal? fHAWBGrossWeight;

		#endregion

		#region HAWB Nature of Goods

		public virtual Goods[] HAWBNatureOfGoods
		{
			get
			{
				if (fHAWBNatureOfGoods == null)
				{
					fHAWBNatureOfGoods = GetGoodsDescriptionFromOuterPackLines();
				}
				return fHAWBNatureOfGoods;
			}
		}

		Goods[] fHAWBNatureOfGoods;

		#region GetGoodsDescriptionFromOuterPackLines

		Goods[] GetGoodsDescriptionFromOuterPackLines()
		{
			Goods[] result = new Goods[Shipment.OuterPackLines.Count];
			for (int i = 0; i < Shipment.OuterPackLines.Count; i++)
			{
				PackLine packLine = Shipment.OuterPackLines[i];
				result[i] = new Goods(packLine.JL_Description, packLine.JL_HarmonisedCode);
			}

			return result;
		}

		#endregion

		#endregion

		#region TDB Permit Nos

		public virtual ZString[] TDBPermitNos
		{
			get
			{
				if (fTDBPermitNos == null)
				{
					List<ZString> permitNos = new List<ZString>();
					foreach (CusCodeData permit in shipmentWrapper.GetTDBPermits())
					{
						permitNos.Add(permit.CY_Data);
					}
					fTDBPermitNos = permitNos.ToArray();
				}
				return fTDBPermitNos;
			}
		}

		ZString[] fTDBPermitNos;

		#endregion

		#region Exemption

		public virtual ZString ExemptionCode
		{
			get
			{
				if (fExemptionCode == null)
				{
					InitialiseExemptionCodeAndRemarks();
				}
				return fExemptionCode;
			}
		}

		public virtual ZString ExemptionRemarks
		{
			get
			{
				if (fExemptionRemarks == null)
				{
					InitialiseExemptionCodeAndRemarks();
				}
				return fExemptionRemarks;
			}
		}

		void InitialiseExemptionCodeAndRemarks()
		{
			CusCodeData tDBExemption = null;
			foreach (CusCodeData permitAndExemptions in shipmentWrapper.GetTDBPermitsAndExemptions())
			{
				if (CustomsEntryTypeList.Singapore.SGExemption.IsTDBExemption(permitAndExemptions.CY_Code))
				{
					tDBExemption = permitAndExemptions;
					break;
				}
			}

			if (tDBExemption != null)
			{
				fExemptionCode = tDBExemption.CY_Code;
				fExemptionRemarks = tDBExemption.CY_Data;
			}
			else
			{
				fExemptionCode = "";
				fExemptionRemarks = "";
			}
		}

		string fExemptionCode;
		string fExemptionRemarks;

		#endregion

		#region Goods Delivered

		public virtual ZBool GoodsDelivered
		{
			get
			{
				ZBool result = false;
				if (!Shipment.IsDeleted)
				{
					result = (!DocsAndCartage.JP_DeliveryCartageCompleted.IsEmpty && DocsAndCartage.JP_DeliveryCartageCompleted <= ZDateTime.Now);
				}
				return result;
			}
		}

		JobDocsAndCartage DocsAndCartage
		{
			get { return Shipment.DocsAndCartage; }
		}

		#endregion

		#endregion

		ForwardingShipment Shipment
		{
			get { return shipmentWrapper.Shipment; }
		}

		readonly CMDShipmentWrapper shipmentWrapper;
	}
}
