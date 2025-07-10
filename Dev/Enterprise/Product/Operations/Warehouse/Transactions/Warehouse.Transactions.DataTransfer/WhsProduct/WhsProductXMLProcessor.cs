using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsProductXMLProcessor :
		IValueObjectExport,
		IValueObjectImport
	{
		public WhsProductXMLProcessor()
		{
		}

		#region IValueObjectImport Members

		public bool CanImport(IValueObject valueObject)
		{
			return valueObject.IsSpecified;
		}

		public void Import(BusinessObject businessObject, IValueObject valueObject, IValueObjectImportContext importContext)
		{
			var product = (OrgSupplierPart)businessObject;
			var value = (Xsd.Product)valueObject;

			ImportClientWarehouseDetails(product, value, importContext);
		}

		void ImportClientWarehouseDetails(OrgSupplierPart product, Xsd.Product value, IValueObjectImportContext context)
		{
			var whsProduct = WhsProduct.GetWhsProduct(product);
			var factory = product.Factory;

			ImportParamsByWhsAndClients(factory, value.ClientWarehouseDetails, whsProduct, context);
			ImportPickFaces(factory, value.PickfaceDetails, whsProduct, context);
		}

		void ImportParamsByWhsAndClients(BusinessObjectFactory factory, Xsd.ClientWarehouseDetailCollection clientWarehouseDetails, WhsProduct whsProduct, IValueObjectImportContext context)
		{
			foreach (Xsd.ClientWarehouseDetail wareh in clientWarehouseDetails)
			{
				var whsDetail = whsProduct.ParamsByWhsAndClient.FindWhsProductParamsByWhsAndClient(wareh.Client.EDICode, new ZGuid(wareh.WarehouseCode));

				if (whsDetail == null)
				{
					whsDetail = whsProduct.ParamsByWhsAndClient.AddNew();

					if (wareh.ClientSpecified)
					{
						whsDetail.W3_OH = context.FindOrCreateTempOrganisationPK(wareh.Client, null, OrganisationTypes.None);
					}

					if (wareh.WarehouseCodeSpecified)
					{
						var whsCode = new ZGuid(wareh.WarehouseCode);
						var warehouse = LoadWarehouseFromEnterprise(factory, whsCode);

						if (warehouse != null)
						{
							whsDetail.W3_WW = whsCode;
						}
					}

					if (whsDetail.W3_WW.IsEmpty)
					{
						context.Notify(new InfoNotification(
							Res.GetString("C687992C-47FF-43A0-9A67-CAD68DD8B8C7", "Client Warehouse Details from XML file cannot be added, because the warehouse specified in client warehouse detail does not exist or was not provided.")));
						whsDetail.Delete();
						return;
					}

					whsDetail.W3_OP = whsProduct.Parent.PK;
				}

				whsDetail.W3_EconomicQuantity = wareh.EconomicQty;
				whsDetail.W3_ExpiryNotificationPeriod = wareh.ExpiryPeriod;
				whsDetail.W3_ReplenishmentMinimum = wareh.ReplenishMinimum;
				whsDetail.W3_ReplenishmentMultiple = wareh.ReplenishMultiple;
				whsDetail.W3_StockTakeCycle = wareh.StockTakeCycle;
			}
		}

		void ImportPickFaces(BusinessObjectFactory factory, Xsd.PickFaceDetailCollection pickfaceDetails, WhsProduct whsProduct, IValueObjectImportContext context)
		{
			foreach (Xsd.PickFaceDetail pickFaceDetail in pickfaceDetails)
			{
				var pickFace = whsProduct.PickFaces.FindByLocation(new ZGuid(pickFaceDetail.LocationCode));

				if (pickFace == null)
				{
					pickFace = factory.New<WhsPickFace>();

					if (pickFaceDetail.ClientSpecified)
					{
						pickFace.WF_OH_Client = context.FindOrCreateTempOrganisationPK(pickFaceDetail.Client, null, OrganisationTypes.None);
					}

					WhsWarehouse warehouse = null;
					if (pickFaceDetail.WarehouseCodeSpecified)
					{
						var whsCode = new ZGuid(pickFaceDetail.WarehouseCode);
						warehouse = LoadWarehouseFromEnterprise(factory, whsCode);
						if (warehouse != null)
						{
							pickFace.LocationWhsGuid = whsCode;
						}
					}

					bool locationFound = false;
					if (pickFaceDetail.LocationCodeSpecified)
					{
						var locationCode = new ZGuid(pickFaceDetail.LocationCode);
						var location = LoadLocationFromEnterprise(factory, locationCode);
						if (location != null)
						{
							pickFace.WF_WL = locationCode;
							locationFound = true;
						}
					}

					if (!locationFound)
					{
						if (warehouse != null && warehouse.DefaultLocation != null)
						{
							pickFace.WF_WL = warehouse.DefaultLocation.PK;
							locationFound = true;
						}
						else
						{
							context.Notify(new InfoNotification(
								Res.GetString("00bfc84f-c1f8-4d7a-9c1e-712378493013",
									"Pick Faces from XML file cannot be added, because Pick Face Location not specified or doesn't exist in {0} or Pick Face not associated with Warehouse or associated Warehouse does not have default Location.",
									Core.Constants.ProductName)));
							pickFace.Delete();
							return;
						}
					}

					if (locationFound)
					{
						pickFace.LocationString = pickFaceDetail.Location;
						pickFace.WF_OP = whsProduct.Parent.PK;
						whsProduct.PickFaces.Add(pickFace);
					}
					else
					{
						return;
					}
				}

				pickFace.WF_ReplenishMaximum = (ZDecimal)pickFaceDetail.ReplenishMaximum;
				pickFace.WF_ReplenishMinimum = (ZDecimal)pickFaceDetail.ReplenishMinimum;
			}
		}

		WhsWarehouse LoadWarehouseFromEnterprise(BusinessObjectFactory factory, ZGuid whsCode)
		{
			return factory.Load<WhsWarehouse>(whsCode);
		}

		WhsLocation LoadLocationFromEnterprise(BusinessObjectFactory factory, ZGuid locationCode)
		{
			return factory.Load<WhsLocation>(locationCode);
		}

		#endregion

		#region IValueObjectExport Members

		public bool CanExport(IValueObject valueObject) => valueObject.IsSpecified;

		public void Export(BusinessObject businessObject, IValueObject valueObject, IValueObjectExportContext context)
		{
			var product = (OrgSupplierPart)businessObject;
			var value = (Xsd.Product)valueObject;

			ExportClientWarehouseDetails(product, value.ClientWarehouseDetails, value.PickfaceDetails, context);
		}

		void ExportClientWarehouseDetails(OrgSupplierPart product, Xsd.ClientWarehouseDetailCollection clientWarehouseDetails, Xsd.PickFaceDetailCollection pickfaceDetails, IValueObjectExportContext context)
		{
			var whsProduct = WhsProduct.GetWhsProduct(product);

			foreach (var whsDetail in whsProduct.ParamsByWhsAndClient)
			{
				Xsd.ClientWarehouseDetail wareh = new Xsd.ClientWarehouseDetail();

				if (whsDetail.Header != null)
				{
					wareh.Client = OrganisationDataAdapter.ExportToValueObject(whsDetail.Header, context);
					wareh.ClientSpecified = true;
				}

				if (whsDetail.W3_EconomicQuantity > 0)
				{
					wareh.EconomicQty = whsDetail.W3_EconomicQuantity;
					wareh.EconomicQtySpecified = true;
				}

				if (whsDetail.W3_ExpiryNotificationPeriod > 0)
				{
					wareh.ExpiryPeriod = whsDetail.W3_ExpiryNotificationPeriod;
					wareh.ExpiryPeriodSpecified = true;
				}

				if (whsDetail.W3_ReplenishmentMinimum > 0)
				{
					wareh.ReplenishMinimum = whsDetail.W3_ReplenishmentMinimum;
					wareh.ReplenishMinimumSpecified = true;
				}

				if (whsDetail.W3_ReplenishmentMultiple > 0)
				{
					wareh.ReplenishMultiple = whsDetail.W3_ReplenishmentMultiple;
					wareh.ReplenishMultipleSpecified = true;
				}

				if (!whsDetail.W3_StockTakeCycle.IsEmpty)
				{
					wareh.StockTakeCycle = whsDetail.W3_StockTakeCycle;
					wareh.StockTakeCycleSpecified = true;
				}

				if (!whsDetail.W3_WW.IsEmpty)
				{
					wareh.WarehouseCode = whsDetail.W3_WW.ToString();
					wareh.WarehouseCodeSpecified = true;
				}

				clientWarehouseDetails.Add(wareh);
			}

			foreach (WhsPickFace pickFace in whsProduct.PickFaces)
			{
				var pickFaceDetail = new Xsd.PickFaceDetail();

				if (pickFace.Client != null)
				{
					pickFaceDetail.Client = OrganisationDataAdapter.ExportToValueObject(pickFace.Client, context);
					pickFaceDetail.ClientSpecified = true;
				}

				if (!pickFace.LocationString.IsEmpty)
				{
					pickFaceDetail.Location = pickFace.LocationString;
					pickFaceDetail.LocationSpecified = true;
				}

				if (pickFace.WF_ReplenishMaximum > 0)
				{
					pickFaceDetail.ReplenishMaximum = pickFace.WF_ReplenishMaximum.ToZInt();
					pickFaceDetail.ReplenishMaximumSpecified = true;
				}

				if (pickFace.WF_ReplenishMinimum > 0)
				{
					pickFaceDetail.ReplenishMinimum = pickFace.WF_ReplenishMinimum.ToZInt();
					pickFaceDetail.ReplenishMinimumSpecified = true;
				}

				if (!pickFace.LocationWhsGuid.IsEmpty)
				{
					pickFaceDetail.WarehouseCode = pickFace.LocationWhsGuid.ToString();
					pickFaceDetail.WarehouseCodeSpecified = true;
				}

				if (!pickFace.WF_WL.IsEmpty)
				{
					pickFaceDetail.LocationCode = pickFace.WF_WL.ToString();
					pickFaceDetail.LocationCodeSpecified = true;
				}

				pickfaceDetails.Add(pickFaceDetail);
			}
		}

		#endregion

		OrganisationValueObjectDataAdapter OrganisationDataAdapter => organisationDataAdapter ?? (organisationDataAdapter = new OrganisationValueObjectDataAdapter());
		OrganisationValueObjectDataAdapter organisationDataAdapter;
	}
}
