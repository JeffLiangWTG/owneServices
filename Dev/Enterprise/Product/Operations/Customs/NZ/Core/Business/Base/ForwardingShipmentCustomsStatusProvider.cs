using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business
{
	public class ForwardingShipmentCustomsStatusProvider : Forwarding.Business.ForwardingShipmentCustomsStatusProvider, Integration.Customs.NZ.IForwardingShipmentCustomsStatusProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public ForwardingShipmentCustomsStatusProvider(ForwardingShipment shipment)
			: base(shipment)
		{
			BusinessObjectFactory factory = shipment.Factory;

			var declarationQuery = JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, shipment.PK);
			var declaration = factory.LoadTop1<JobDeclaration>(declarationQuery);

			if (declaration != null)
			{
				var entryStatus = Extensions.RemoveConsolidatedStatus(declaration.JE_EntryStatus);
				if (declaration.IsECIWriteoff)
				{
					customsMessageStatus = new LowValueManifestStatusList().GetDescriptionFromCode(declaration.ExistingCusEntryHeader?.CH_EntryStatus ?? LowValueManifestStatusList.Codes.NotSentToCustoms);
					customsCargoStatus = new LowValueConsignmentStatusList().GetDescriptionFromCode(entryStatus);
				}
				else
				{
					customsMessageStatus = new FormalEntryStatusList().GetDescriptionFromCode(entryStatus);
					customsCargoStatus = customsMessageStatus;
				}

				GetCustomsCargoStatusCode = () => declaration.JE_EntryStatus;
				SetCustomsCargoStatusCode = status => declaration.JE_EntryStatus = status;
				GetCustomsMessageStatusCode = () => declaration.JE_MessageStatus;
				SetCustomsMessageStatusCode = status => declaration.JE_MessageStatus = status;
			}
			else
			{
				var eciHAWB = CusHAWB.LoadECILinkedTo(shipment);
				if (eciHAWB != null)
				{
					customsMessageStatus = new LowValueManifestStatusList().GetDescriptionFromCode(eciHAWB.MAWB.CM_CustomsStatus);
					customsCargoStatus = eciHAWB.CustomsStatusDescription;

					GetCustomsCargoStatusCode = () => eciHAWB.CS_CustomsStatus;
					SetCustomsCargoStatusCode = status => eciHAWB.CS_CustomsStatus = status;
					GetCustomsMessageStatusCode = () => eciHAWB.CS_MsgStatus;
					SetCustomsMessageStatusCode = status => eciHAWB.CS_MsgStatus = customsMessageStatus;
				}
				else
				{
					var scaHouse = CusSCAHouse.LoadECILinkedTo(shipment);
					if (scaHouse != null)
					{
						customsMessageStatus = new LowValueManifestStatusList().GetDescriptionFromCode(scaHouse.OceanBill.CB_CustomsStatus);
						customsCargoStatus = scaHouse.ShipmentStatusDescription;

						GetCustomsCargoStatusCode = () => scaHouse.CA_ShipmentStatus;
						SetCustomsCargoStatusCode = status => scaHouse.CA_ShipmentStatus = status;
						GetCustomsMessageStatusCode = () => scaHouse.CA_MessageStatus;
						SetCustomsMessageStatusCode = status => scaHouse.CA_MessageStatus = customsMessageStatus;
					}
					else
					{
						customsMessageStatus = "";
						customsCargoStatus = "";

						GetCustomsCargoStatusCode = () => ZString.Empty;
						SetCustomsCargoStatusCode = status => { };
						GetCustomsMessageStatusCode = () => ZString.Empty;
						SetCustomsMessageStatusCode = status => { };
					}
				}
			}
		}
		readonly ZString customsMessageStatus;
		readonly ZString customsCargoStatus;

		public readonly Func<ZString> GetCustomsCargoStatusCode;
		public readonly Action<ZString> SetCustomsCargoStatusCode;
		public readonly Func<ZString> GetCustomsMessageStatusCode;
		public readonly Action<ZString> SetCustomsMessageStatusCode;

		public override ZString CustomsMessageStatus()
		{
			return customsMessageStatus;
		}

		public override ZString CustomsCargoStatus()
		{
			return customsCargoStatus;
		}
	}
}
