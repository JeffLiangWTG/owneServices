using System.Collections.Generic;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class ShipmentDeclarationFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public ShipmentDeclarationFetchStrategy(ShipmentDeclarationCollection collection)
			: base(collection)
		{
			Factory = collection.Factory;
		}

		readonly BusinessObjectFactory Factory;

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			if (businessObjects.Length > 0)
			{
				ZGuid[] pks = new ZGuid[businessObjects.Length];
				List<ZGuid> shipmentsPKs = new List<ZGuid>();
				List<ZGuid> declarationsPKs = new List<ZGuid>();
				bool hasShipments = false;
				bool hasDeclarations = false;
				for (int i = 0; i < businessObjects.Length; i++)
				{
					if (businessObjects[i] is TrackingShipment)
					{
						pks[i] = businessObjects[i].PK;
						shipmentsPKs.Add(businessObjects[i].PK);
					}
					else
					{
						TrackingDeclaration trackingDeclaration = ((TrackingDeclaration)businessObjects[i]);
						pks[i] = trackingDeclaration.Declaration.PK;
						declarationsPKs.Add(trackingDeclaration.Declaration.PK);
					}
				}

				hasShipments = shipmentsPKs.Count > 0;
				hasDeclarations = declarationsPKs.Count > 0;

				foreach (TableColumn column in columns)
				{
					switch (column.ColumnName)
					{
						case ShipmentDeclarationSchema.Constants.EstimatedPickupDate:
						case ShipmentDeclarationSchema.Constants.PickupDateRequiredBy:
						case ShipmentDeclarationSchema.Constants.EstimatedDeliveryDate:
						case ShipmentDeclarationSchema.Constants.DeliveryDateRequiredBy:
						case ShipmentDeclarationSchema.Constants.DeliveryDate:
						case ShipmentDeclarationSchema.Constants.ActualPickupDate:
							Factory.AddFetchHint(JobDocsAndCartageSchema.Instance, new ZQuery(JobDocsAndCartageSchema.JP_ParentID, pks));
							break;

						case ShipmentDeclarationSchema.Constants.OrderReference:
							if (hasShipments)
							{ AddFetchHintForOrderReferences(businessObjects, pks, shipmentsPKs.ToArray(), declarationsPKs.ToArray()); }
							break;

						case ShipmentDeclarationSchema.Constants.Top3Containers:
							if (hasShipments)
							{ Factory.AddFetchHint(JobPackLinesSchema.Instance, new ZQuery(JobPackLinesSchema.JL_JS, shipmentsPKs.ToArray())); }
							if (hasDeclarations)
							{ Factory.AddFetchHint(CusContainerSchema.Instance, new ZQuery(CusContainerSchema.CO_JE, declarationsPKs.ToArray())); }
							break;

						case ShipmentDeclarationSchema.Constants.HouseBill:
						case ShipmentDeclarationSchema.Constants.MasterBill:
						// *** These hints are not working atm but should turn useful when WebShipmentTransport will be deleted
						case ShipmentDeclarationSchema.Constants.CurrentLoadPort:
						case ShipmentDeclarationSchema.Constants.CurrentDischargePort:
						case ShipmentDeclarationSchema.Constants.MainLoadPort:
						case ShipmentDeclarationSchema.Constants.MainDischargePort:
							// ***
							if (hasShipments)
							{ Factory.AddFetchHint(JobConShipLinkSchema.Instance, new ZQuery(JobConShipLinkSchema.JN_JS, shipmentsPKs.ToArray())); }
							break;

						// *** These hints are not working atm but should turn useful when WebShipmentTransport will be deleted
						case ShipmentDeclarationSchema.Constants.MainVessel:
						case ShipmentDeclarationSchema.Constants.MainVoyageWithSuppression:
						case ShipmentDeclarationSchema.Constants.CurrentVessel:
						case ShipmentDeclarationSchema.Constants.CurrentVoyageWithSuppression:
							// ***
							if (hasShipments)
							{ AddFetchHintsForVesselVoyage(businessObjects, pks, shipmentsPKs.ToArray()); }
							break;
						case ShipmentDeclarationSchema.Constants.Top3JobNotes:
							Factory.AddFetchHint(StmNoteSchema.Instance, new ZQuery(StmNoteSchema.ST_ParentID, pks));
							break;
					}

					if (column.ColumnName.StartsWith((NoResString)"Consignor") && hasShipments)
					{
						AddFetchHintForConsignor(businessObjects, shipmentsPKs.ToArray());
					}
					else if (column.ColumnName.StartsWith((NoResString)"Consignee") && hasShipments)
					{
						AddFetchHintForConsignee(businessObjects, shipmentsPKs.ToArray());
					}
					else if (column.ColumnName.Contains((NoResString)"Milestone"))
					{
						Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, pks));
					}
					Factory.AddFetchHint(JobHeaderSchema.Instance, new ZQuery(JobHeaderSchema.JH_ParentID, pks).AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK));
					Factory.AddFetchHint(CusEntryNumSchema.Instance, new ZQuery(CusEntryNumSchema.CE_ParentID, pks));
					if (hasShipments)
					{
						Factory.AddFetchHint(CusSCAHouseSchema.Instance, new ZQuery(CusSCAHouseSchema.CA_JS, shipmentsPKs.ToArray()));
						Factory.AddFetchHint(JobPackLinesSchema.Instance, new ZQuery(JobPackLinesSchema.JL_JS, shipmentsPKs.ToArray()));
						Factory.AddFetchHint(JobConShipLinkSchema.Instance, new ZQuery(JobConShipLinkSchema.JN_JS, shipmentsPKs.ToArray()));
						Factory.AddFetchHint(JobDeclarationSchema.Instance, new ZQuery(JobDeclarationSchema.JE_JS, shipmentsPKs.ToArray()));
						Factory.AddFetchHint(JobDocsAndCartageSchema.Instance, new ZQuery(JobDocsAndCartageSchema.JP_ParentID, shipmentsPKs.ToArray()));
					}
					if (hasDeclarations)
					{
						Factory.AddFetchHint(CusEntryHeaderSchema.Instance, new ZQuery(CusEntryHeaderSchema.CH_JE, declarationsPKs.ToArray()));
						Factory.AddFetchHint(CusContainerSchema.Instance, new ZQuery(CusContainerSchema.CO_JE, declarationsPKs.ToArray()));
					}
				}
			}

			base.FetchForViewCore(businessObjects, columns);
		}

		#region Implementation

		void AddFetchHintsForVesselVoyage(BusinessObject[] businessObjects, ZGuid[] pks, ZGuid[] shipmentsPKs)
		{
			Factory.AddFetchHint(JobDocsAndCartageSchema.Instance, new ZQuery(JobDocsAndCartageSchema.JP_ParentID, pks));
			Factory.AddFetchHint(JobConShipLinkSchema.Instance, new ZQuery(JobConShipLinkSchema.JN_JS, shipmentsPKs));

			List<ZString> ports = new List<ZString>();

			for (int i = 0; i < businessObjects.Length; i++)
			{
				ZString originPortCode = businessObjects[i] is TrackingShipment ? ((TrackingShipment)businessObjects[i]).JS_RL_NKOrigin : ((TrackingDeclaration)businessObjects[i]).Declaration.JE_RL_NKOrigin;
				ZString destinationPortCode = businessObjects[i] is TrackingShipment ? ((TrackingShipment)businessObjects[i]).JS_RL_NKDestination : ((TrackingDeclaration)businessObjects[i]).Declaration.JE_RL_NKFinalDestination;

				if (!originPortCode.IsEmpty && !ports.Contains(originPortCode))
				{
					ports.Add(originPortCode);
				}

				if (!destinationPortCode.IsEmpty && !ports.Contains(destinationPortCode))
				{
					ports.Add(destinationPortCode);
				}
			}

			if (ports.Count > 0)
			{
				Factory.AddFetchHint(RefUNLOCOSchema.Instance, new ZQuery(RefUNLOCOSchema.RL_Code, ports));
			}

			List<ZString> countries = new List<ZString>();
			foreach (ZString port in ports)
			{
				ZString country = port.Substring(0, 2);
				if (!countries.Contains(country))
				{
					countries.Add(country);
				}
			}

			if (countries.Count > 0)
			{
				Factory.AddFetchHint(RefCountrySchema.Instance, new ZQuery(RefCountrySchema.RN_Code, countries));
			}
		}

		void AddFetchHintForOrderReferences(BusinessObject[] businessObjects, ZGuid[] pks, ZGuid[] shipmentsPKs, ZGuid[] declarationsPKs)
		{
			Factory.AddFetchHint(JobDocsAndCartageSchema.Instance, new ZQuery(JobDocsAndCartageSchema.JP_ParentID, pks));
			Factory.AddFetchHint(JobDeclarationSchema.Instance, new ZQuery(JobDeclarationSchema.JE_JS, shipmentsPKs));
			Factory.AddFetchHint(JobOrderHeaderSchema.Instance, new ZQuery(JobOrderHeaderSchema.JD_JS, shipmentsPKs));
			Factory.AddFetchHint(JobOrderHeaderSchema.Instance, new ZQuery(JobOrderHeaderSchema.JD_JE, declarationsPKs));
			for (int i = 0; i < businessObjects.Length; i++)
			{
				if (businessObjects[i] is TrackingShipment)
				{
					Factory.AddFetchHint(JobOrderItemSchema.JT_JP, (businessObjects[i] as TrackingShipment).DocsAndCartage.PK);
				}
				else
				{
					Factory.AddFetchHint(JobOrderItemSchema.JT_JP, (businessObjects[i] as TrackingDeclaration).Declaration.DocsAndCartage.PK);
				}
			}
		}

		void AddFetchHintForConsignor(BusinessObject[] businessObjects, ZGuid[] shipmentsPKs)
		{
			AddFetchHintForConsignorOrConsignee(true, businessObjects, shipmentsPKs);
		}

		void AddFetchHintForConsignee(BusinessObject[] businessObjects, ZGuid[] shipmentsPKs)
		{
			AddFetchHintForConsignorOrConsignee(false, businessObjects, shipmentsPKs);
		}

		void AddFetchHintForConsignorOrConsignee(bool isConsignorColumn, BusinessObject[] businessObjects, ZGuid[] shipmentsPKs)
		{
			for (int i = 0; i < businessObjects.Length; i++)
			{
				if (businessObjects[i] is TrackingShipment)
				{
					Factory.AddFetchHint(JobDocAddressSchema.Instance, new ZQuery(JobDocAddressSchema.E2_ParentID, shipmentsPKs));

					TrackingShipment shipment = (TrackingShipment)businessObjects[i];
					if (isConsignorColumn && !shipment.ConsignorDocumentaryAddress.E2_AddressOverride)
					{
						Factory.AddFetchHint(OrgAddressSchema.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
					}
					else if (!isConsignorColumn && !shipment.ConsigneeDocumentaryAddress.E2_AddressOverride)
					{
						Factory.AddFetchHint(OrgAddressSchema.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);
					}
				}
				else
				{
					TrackingDeclaration declaration = (TrackingDeclaration)businessObjects[i];
					if (isConsignorColumn)
					{
						Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.Declaration.JE_OH_Supplier);
						Factory.AddFetchHint(OrgAddressSchema.OA_OH, declaration.Declaration.JE_OH_Supplier);
					}
					else
					{
						Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.Declaration.JE_OH_Importer);
						Factory.AddFetchHint(OrgAddressSchema.OA_OH, declaration.Declaration.JE_OH_Importer);
					}
				}
			}
		}

		#endregion
	}
}
