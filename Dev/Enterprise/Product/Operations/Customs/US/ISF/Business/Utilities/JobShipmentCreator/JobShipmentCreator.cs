using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class JobShipmentCreator
	{
		public JobShipmentCreator(ISFBillRow billRow)
		{
			Argument.NotNull(billRow, "billRow");
			this.billRow = billRow;
		}

		public ForwardingShipment GetShipment(BusinessObjectFactory factory, ZGuid consolPK)
		{
			ForwardingConsol consol = factory.Load<ForwardingConsol>(consolPK);
			ForwardingShipment shipment = consol == null ? factory.New<ForwardingShipment>() : consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRefInfo.ValueChanged += new EventHandler(JS_UniqueConsignRefInfo_ValueChanged);
			PopulateShipmentData(shipment, consol);
			return shipment;
		}

		void PopulateShipmentData(ForwardingShipment shipment, ForwardingConsol consol)
		{
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.ConsigneePK = Header.BF_OH_Importer;
			if (!Header.MainShipToParty.IsEmpty)
			{
				PopulateOrganization(Header.MainShipToParty, shipment.ConsigneeDeliveryAddress);
			}

			UpdateHouseBill(shipment);
			PopulateLines(shipment, consol);

			if (!Header.BuyingParty.IsEmpty)
			{
				PopulateOrganization(Header.BuyingParty, shipment.BuyerDocAddress);
			}
			if (!billRow.SellingPartyPK.IsEmpty)
			{
				var jobDocAddress = shipment.Factory.Load<JobDocAddress>(billRow.SellingPartyPK);

				if (jobDocAddress != null)
				{
					shipment.ConsignorPK = jobDocAddress.OrganisationPK;
				}
			}
			else if (!Header.SellingParty.IsEmpty)
			{
				if (Header.SellingParty.OrganisationPK.IsValid)
				{
					shipment.ConsignorPK = Header.SellingParty.OrganisationPK;
				}
				PopulateOrganization(Header.SellingParty, shipment.ConsignorDocumentaryAddress);
			}
			if (!Header.BF_RL_NKPlaceOfDelivery.IsEmpty)
			{
				shipment.JS_RL_NKDestination = Header.BF_RL_NKPlaceOfDelivery;
			}
			shipment.JS_TotalPackageCount = Header.BF_EstimatedQuantity;
			shipment.JS_F3_NKTotalCountPackType = Header.BF_EstimatedQuantityUQ;
			shipment.JS_GoodsValue = Header.BF_EstimatedValue;
			shipment.JS_RX_NKGoodsValueCurr = Core.Constants.CurrencyCodes.UnitedStates;
			shipment.JS_ActualWeight = new ZDecimal(Header.BF_EstimatedWeight);
			shipment.JS_UnitOfWeight = Header.BF_EstimatedWeightUQ;

			CopyeDocsToShipment(shipment);
		}

		void CopyeDocsToShipment(ForwardingShipment shipment)
		{
			var docManagerInfo = ((IDocManagerSupport)Header).DocManagerInfo;
			docManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			var eDocs = docManagerInfo.AllEDocs;
			shipment.DocManagerInfo.UseBusinessEntityFactoryAsInternal = true;
			foreach (IeDoc document in eDocs)
			{
				if (!document.IsCustomisableDocTypes)
				{
					shipment.DocManagerInfo.AddFileOrDocument(document.ImageData, document.FileName, document.DocType);
				}
				else
				{
					shipment.DocManagerInfo.AddFileOrDocument(document.ImageData, document.FileName, document.DocType, description: document.Description);
				}
			}
		}

		void UpdateHouseBill(ForwardingShipment shipment)
		{
			CusISFBill houseBill = HouseBill;
			if (houseBill != null)
			{
				shipment.JS_HouseBill = houseBill.BB_BillNum.Left(shipment.JS_HouseBillInfo.MaxLength);
			}
		}

		void PopulateOrganization(JobDocAddress sourceDocAddress, JobDocAddress destinationDocAddress)
		{
			BusinessObjectCloneArgs args = new BusinessObjectCloneArgs(new string[] { JobDocAddress.Schema.E2_ParentTableCode, JobDocAddress.Schema.E2_ParentID, JobDocAddress.Schema.E2_AddressSequence, JobDocAddress.Schema.E2_AddressType, JobDocAddress.Schema.E2_AddressOverride });
			destinationDocAddress.E2_AddressOverride = sourceDocAddress.E2_AddressOverride;
			destinationDocAddress.CopyPersistentValuesFrom(sourceDocAddress, args);
		}

		void PopulateLines(ForwardingShipment shipment, ForwardingConsol consol)
		{
			if (consol != null)
			{
				consol.AutomaticallyUpdatePackLineContainers = false;
			}

			foreach (ISFLineRow line in billRow.Lines)
			{
				if (line.ShouldCopy)
				{
					ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();
					packLine.JL_HarmonisedCode = line.HarmonisedNum;
					packLine.JL_RN_NKOrigin = line.GoodsOrigin;
					CusISFEquip equipment = line.Container;
					if (equipment != null && !equipment.BE_ContainerNum.IsEmpty)
					{
						CommonContainer container = null;
						if (consol != null)
						{
							container = consol.Containers.FindAnyByContainerNumber(equipment.BE_ContainerNum);
						}

						if (container == null)
						{
							container = shipment.Containers.FirstOrDefault(x => x.JC_ContainerNum == equipment.BE_ContainerNum);
						}

						if (container != null)
						{
							packLine.JL_JC = container.PK;
						}
					}
					if (line.ManufacturerAddress != null)
					{
						PopulateOrganization(line.ManufacturerAddress, shipment.ManufacturerDocAddress);
					}
				}
			}
		}

		CusISFBill HouseBill
		{
			get { return billRow.HouseBill; }
		}

		CusISFBill OceanBill
		{
			get { return billRow.OceanBill; }
		}

		CusISFHeader Header
		{
			get { return billRow.HeaderRow.Header; }
		}

		readonly ISFBillRow billRow;

		void JS_UniqueConsignRefInfo_ValueChanged(object sender, EventArgs e)
		{
			ForwardingShipment shipment = (ForwardingShipment)sender;
			if (shipment != null)
			{
				CusISFHeader header = shipment.Factory.Load<CusISFHeader>(Header.PK);
				shipment.JS_UniqueConsignRefInfo.ValueChanged -= new EventHandler(JS_UniqueConsignRefInfo_ValueChanged);
				CusISFBill bill = HouseBill ?? OceanBill;
				if (bill != null && header != null)
				{
					header.Logs.AddNew(Events.Transferred, bill.BB_BillType + ":" + bill.BB_BillNum + " to " + shipment.JS_UniqueConsignRef);
				}
			}
		}
	}
}
