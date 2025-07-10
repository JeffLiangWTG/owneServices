using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsJobInvoicingAdditionalDataPropertyProvider
	{
		#region Properties

		public static class Schema
		{
			public const string CartageZone = "CartageZone";
			public const string Commodity = "Commodity";
			public const string ConsigneeAddress1 = "ConsigneeAddress1";
			public const string ConsigneeAddress2 = "ConsigneeAddress2";
			public const string ConsigneeCity = "ConsigneeCity";
			public const string ConsigneeCode = "ConsigneeCode";
			public const string ConsigneePostCode = "ConsigneePostCode";
			public const string ConsigneeState = "ConsigneeState";
			public const string ConsigneeUNLOCO = "ConsigneeUNLOCO";
			public const string CustomerReference = "CustomerReference";
			public const string DocketReference = "DocketReference";
			public const string DocketID = "DocketID";
			public const string Location = "Location";
			public const string LocationType = "LocationType";
			public const string PartAttrib1 = "PartAttrib1";
			public const string PartAttrib2 = "PartAttrib2";
			public const string PartAttrib3 = "PartAttrib3";
			public const string SerialNumber = "SerialNumber";
			public const string Product = "Product";
			public const string ServiceLevel = "ServiceLevel";
			public const string TransportCo = "TransportCo";
			public const string Weight = "Weight";
			public const string WeightUQ = "WeightUQ";
			public const string Volume = "Volume";
			public const string VolumeUQ = "VolumeUQ";
		}

		public CustomPropertyContainer<JobCharge> GetAdditionalProperties()
		{
			var properties = new CustomPropertyContainer<JobCharge>();
			AddCartageZoneProperty(properties);
			AddCommodityProperty(properties);
			AddCustomerReferenceProperty(properties);
			AddDocketProperties(properties);
			AddLocationProperties(properties);
			AddPartAttributeProperties(properties);
			AddProcuctProperty(properties);
			AddServiceLevelProperty(properties);
			AddTransportCoProperty(properties);
			AddWeightProperties(properties);
			AddVolumeProperties(properties);

			AddModuleSpecificAdditionalProperties(properties);

			return properties;
		}

		void AddTransportCoProperty(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.TransportCo, Res.GetString("43e9c4ca-6763-4dff-9c75-a5ddf5e1a159", "Transport Company"), typeof(ZString), (c) => GetTransportCo(c), visible: false);
		}

		void AddServiceLevelProperty(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.ServiceLevel, Res.GetString("73131a7e-58a5-4ba0-a82d-6877fe5f6865", "Service Level"), typeof(ZString), (c) => GetServiceLevel(c), visible: false);
		}

		void AddProcuctProperty(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.Product, Res.GetString("5cb67e17-0def-4ea8-8064-faf37359aa78", "Product"), typeof(ZString), (c) => c.JobChargeAttrib_Product, visible: true);
		}

		void AddCustomerReferenceProperty(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.CustomerReference, Res.GetString("b964bac6-92eb-4e27-a4b6-0e686d3ed5af", "Customer Reference"), typeof(ZString), (c) => GetCustomerReference(c), visible: false);
		}

		void AddCommodityProperty(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.Commodity, Res.GetString("b1392cdc-3806-4ce0-a9d6-a83237f85bfb", "Commodity"), typeof(ZString), (c) => c.JobChargeAttrib_Commodity, visible: false);
		}

		void AddCartageZoneProperty(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.CartageZone, Res.GetString("514ea02d-273c-4a30-905e-4577a8363ff6", "Cartage Zone"), typeof(ZString), (c) => c.JobChargeAttrib_CartageZoneDescription, visible: false);
		}

		void AddDocketProperties(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.DocketReference, Res.GetString("83865690-65e9-44c3-9db5-91a0a8be5cfa", "Job Reference"), typeof(ZString), (c) => c.JobChargeAttrib_DocketReference, visible: false);
			properties.AddCustomProperty(Schema.DocketID, Res.GetString("1a133dd3-ef3c-49d0-8c15-294a7c78d882", "Job ID"), typeof(ZString), (c) => GetDocketID(c), visible: false);
		}

		void AddLocationProperties(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.Location, Res.GetString("070499a4-5f71-41c8-9b1f-9f55bc8ad88a", "Location"), typeof(ZString), (c) => c.JobChargeAttrib_LocationDesc, visible: false);
			properties.AddCustomProperty(Schema.LocationType, Res.GetString("acddbf59-c65d-460e-a44d-128d1a7d75fc", "Location Type"), typeof(ZString), (c) => c.JobChargeAttrib_LocationType, visible: false);
		}

		void AddPartAttributeProperties(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.PartAttrib1, Res.GetString("49052009-bff4-45ca-9a6d-12e5068f7288", "Part Attribute 1"), typeof(ZString), (c) => c.JobChargeAttrib_PartAttrib1, visible: false);
			properties.AddCustomProperty(Schema.PartAttrib2, Res.GetString("f9283fb8-402b-443f-9cea-d7105feadf25", "Part Attribute 2"), typeof(ZString), (c) => c.JobChargeAttrib_PartAttrib2, visible: false);
			properties.AddCustomProperty(Schema.PartAttrib3, Res.GetString("169b4ba3-c025-40c7-9326-fb253fa00665", "Part Attribute 3"), typeof(ZString), (c) => c.JobChargeAttrib_PartAttrib3, visible: false);
			properties.AddCustomProperty(Schema.SerialNumber, Res.GetString("71f40d4c-c5e2-47d5-8fbd-72eb9dffe7df", "Serial Number"), typeof(ZString), (c) => c.JobChargeAttrib_SerialNumber, visible: false);
		}

		void AddWeightProperties(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.Weight, Res.GetString("50445fb3-2571-444d-b22b-001afd95ee23", "Weight"), typeof(ZDecimal), (c) => GetWeight(c), visible: false);
			properties.AddCustomProperty(Schema.WeightUQ, Res.GetString("5000ecaa-d887-4818-889b-3c6224bdc503", "Weight UQ"), typeof(ZString), (c) => GetWeightUQ(c), visible: false);
		}

		void AddVolumeProperties(CustomPropertyContainer<JobCharge> properties)
		{
			properties.AddCustomProperty(Schema.Volume, Res.GetString("96503cde-32dd-49b0-9b40-9dcf4738ed05", "Volume"), typeof(ZDecimal), (c) => GetVolume(c), visible: false);
			properties.AddCustomProperty(Schema.VolumeUQ, Res.GetString("e3aa0cd7-b207-4989-b3b7-f36a2c7ce725", "Volume UQ"), typeof(ZString), (c) => GetVolumeUQ(c), visible: false);
		}

		protected virtual CustomPropertyContainer<JobCharge> AddModuleSpecificAdditionalProperties(CustomPropertyContainer<JobCharge> properties)
		{
			return properties;
		}

		#region DocketID

		static ZString GetDocketID(JobCharge charge)
		{
			var docket = WhsChargeHelper.GetDocket(charge);
			return docket != null ? docket.WD_DocketID : ZString.Empty;
		}

		#endregion

		#region CustomerReference

		static ZString GetCustomerReference(JobCharge charge)
		{
			var docket = WhsChargeHelper.GetDocket(charge);
			return docket != null ? docket.WD_CustomerReference : ZString.Empty;
		}

		#endregion

		#region TransportCo

		static ZString GetTransportCo(JobCharge charge)
		{
			ZString result = "";

			var docket = WhsChargeHelper.GetDocket(charge);
			if (docket is IJobWithTransportCompany job)
			{
				var docAddress = job.TransportCoDocAddress;
				if (docAddress != null)
				{
					if (docAddress.E2_AddressOverride)
					{
						result = docAddress.E2_CompanyNameTruncated;
					}
					else
					{
						result = docAddress.Organisation?.OH_Code ?? ZString.Empty;
					}
				}
			}

			return result;
		}

		#endregion

		#region ServiceLevel

		static ZString GetServiceLevel(JobCharge charge)
		{
			var docket = WhsChargeHelper.GetDocket(charge);
			return docket != null ? docket.WD_PL_NKCarrierServiceLevel : ZString.Empty;
		}

		#endregion

		#region Weight

		static ZDecimal GetWeight(JobCharge charge)
		{
			var result = ZDecimal.Zero;
			var part = GetPart(charge);
			if (part != null)
			{
				var productUQ = GetProductUQ(charge, part);
				var ratingByWeight = (productUQ == part.OP_WeightUQ);
				result = ratingByWeight ? charge.JobChargeAttrib_ItemsToRate : part.UnitConverter.Convert(charge.JobChargeAttrib_UnroundedItemsToRate, productUQ, part.OP_WeightUQ).Round(2);
			}
			else
			{
				var docket = WhsChargeHelper.GetDocket(charge);
				if (docket != null)
				{
					result = docket.WD_TotalWeight;
				}
			}

			return result;
		}

		#endregion

		#region WeightUQ

		static ZString GetWeightUQ(JobCharge charge, OrgSupplierPart part = null)
		{
			var weightUQ = ZString.Empty;
			part = part ?? GetPart(charge);

			if (part != null)
			{
				weightUQ = part.OP_WeightUQ;
			}
			else
			{
				var docket = WhsChargeHelper.GetDocket(charge);
				if (docket != null)
				{
					weightUQ = docket.WD_TotalWeightUnit;
				}
			}

			return weightUQ;
		}

		#endregion

		#region Volume

		static ZDecimal GetVolume(JobCharge charge)
		{
			var result = ZDecimal.Zero;
			var part = GetPart(charge);
			if (part != null)
			{
				var productUQ = GetProductUQ(charge, part);
				var ratingByVolume = productUQ == part.OP_CubicUQ;
				result = ratingByVolume ? charge.JobChargeAttrib_ItemsToRate : part.UnitConverter.Convert(charge.JobChargeAttrib_UnroundedItemsToRate, productUQ, part.OP_CubicUQ).Round(2);
			}
			else
			{
				var docket = WhsChargeHelper.GetDocket(charge);
				if (docket != null)
				{
					result = docket.WD_TotalCubic;
				}
			}

			return result;
		}

		#endregion

		#region VolumeUQ

		static ZString GetVolumeUQ(JobCharge charge, OrgSupplierPart part = null)
		{
			var volumeUQ = ZString.Empty;
			part = part ?? GetPart(charge);

			if (part != null)
			{
				volumeUQ = part.OP_CubicUQ;
			}
			else
			{
				var docket = WhsChargeHelper.GetDocket(charge);
				if (docket != null)
				{
					volumeUQ = docket.WD_TotalCubicUnit;
				}
			}

			return volumeUQ;
		}

		#endregion

		#endregion

		#region Implementation

		static OrgSupplierPart GetPart(JobCharge charge)
		{
			return (!charge.JobChargeAttrib_Product.IsEmpty) ? charge.Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, charge.JobChargeAttrib_Product)) : null;
		}

		#region ProductUQ

		static string GetProductUQ(JobCharge charge, OrgSupplierPart part)
		{
			return MapRatingUnitToProductUQ(charge.JobChargeAttrib_ItemsToRateUnit, part);
		}

		static string MapRatingUnitToProductUQ(string itemsToRateUnit, OrgSupplierPart part)
		{
			if (string.IsNullOrEmpty(itemsToRateUnit))
			{
				return part != null ? part.OP_StockKeepingUnit.ToString() : "";
			}
			else
			{
				return (itemsToRateUnit == Rating.Business.RatingConstants.Units.PL) ? Core.Constants.PkgUnit.Pallet : itemsToRateUnit;
			}
		}

		#endregion

		#endregion
	}
}
