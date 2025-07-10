using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MarketingManager.Business
{
	[CodeProperty(OrgSalesProduct.Schema.MP_Code), DescriptionProperty(OrgSalesProduct.Schema.MP_Name)]
	public class OrgSalesProduct : AutoOrgSalesProduct, IOrgSalesProduct
	{
		public OrgSalesProduct(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region MP_Code

		protected bool MP_Code_ReadOnly
		{
			get { return MP_IsSystemDefined; }
		}

		#endregion

		#region MP_Name

		[TranslatableDataField(Schema.TableName, Schema.MP_Name, @"Database\Odyssey\Data\Public\OrgSalesProduct\OrgSalesProduct.xml", MaxLength = Schema.MP_NameMaxLength, Type = typeof(OrgSalesProduct), Asmid = ResString.AssemblyId)]
		public override ZString MP_Name
		{
			get { return base.MP_Name; }
			set { base.MP_Name = value; }
		}

		public MultilingualString MP_NameMultilingual
		{
			get { return GetMultilingual(MP_NameInfo); }
		}

		protected bool MP_Name_ReadOnly
		{
			get { return MP_IsSystemDefined; }
		}

		#endregion

		#region MP_IsSystemDefined

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		protected bool MP_IsSystemDefined_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region MP_FormLayoutData

		public SalesProductFormLayout FormLayout
		{
			get
			{
				if (formLayout == null)
				{
					formLayout = new SalesProductFormLayout(this);
					RegisterEditableChildObject(formLayout);
					DeserializeFormLayout();
				}

				return formLayout;
			}
		}
		SalesProductFormLayout formLayout;

		void DeserializeFormLayout()
		{
			if (!MP_FormLayoutData.IsEmpty)
			{
				var xmlSerializer = FormLayoutDataSerializer;
				using (StringReader tempReader = new StringReader(MP_FormLayoutData))
				{
					var formLayoutData = (SalesProductFormLayoutData)xmlSerializer.Deserialize(tempReader);
					formLayout.Import(formLayoutData);
				}
			}
		}

		void SerializeFormLayoutData()
		{
			using (var tempWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
			{
				FormLayoutDataSerializer.Serialize(tempWriter, FormLayout.ToFormLayoutData());
				MP_FormLayoutData = tempWriter.ToString();
			}
		}

		ZXmlSerializer FormLayoutDataSerializer
		{
			get { return serializer ?? (serializer = ZXmlSerializer.New(typeof(SalesProductFormLayoutData))); }
		}
		ZXmlSerializer serializer;

		#endregion

		#region IsFreight

		public bool IsFreight
		{
			get
			{
				return
					MP_Code == SystemDefinedSalesProductList.Codes.ForwardingShipment ||
					MP_Code == SystemDefinedSalesProductList.Codes.LinerAgency ||
					MP_Code == SystemDefinedSalesProductList.Codes.Transport;
			}
		}

		#endregion

		#region IsActualsSupported

		public ZBool IsActualsSupported
		{
			get { return MP_IsSystemDefined; }
		}

		#endregion

		#region IsAutoGenerateQuoteSupported

		public ZBool IsAutoGenerateQuoteSupported
		{
			get
			{
				return MP_IsSystemDefined &&
					(
						MP_Code == SystemDefinedSalesProductList.Codes.ForwardingShipment ||
						MP_Code == SystemDefinedSalesProductList.Codes.CustomsBrokerage ||
						MP_Code == SystemDefinedSalesProductList.Codes.LinerAgency ||
						MP_Code == SystemDefinedSalesProductList.Codes.Warehouse
					);
			}
		}

		#endregion

		#region IsAutoGenerateSpotQuoteSupported

		public ZBool IsAutoGenerateSpotQuoteSupported
		{
			get { return MP_IsSystemDefined && MP_Code == SystemDefinedSalesProductList.Codes.ForwardingShipment; }
		}

		#endregion

		#region Location

		public ViewLocationType AllowedLocationTypes
		{
			get
			{
				switch (MP_Code)
				{
					case SystemDefinedSalesProductList.Codes.CustomsBrokerage:
						return ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.InternationalZone;

					case SystemDefinedSalesProductList.Codes.ForwardingShipment:
					case SystemDefinedSalesProductList.Codes.LinerAgency:
						return ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.City | ViewLocationType.InternationalZone;

					case SystemDefinedSalesProductList.Codes.Transport:
						return ViewLocationType.UNLOCO | ViewLocationType.Country | ViewLocationType.State | ViewLocationType.City | ViewLocationType.TransportZone;

					case SystemDefinedSalesProductList.Codes.Warehouse:
						return ViewLocationType.UNLOCO | ViewLocationType.Country;
				}

				return ViewLocationType.All;
			}
		}

		public OrgSalesProductLocationArrangement LocationArrangement
		{
			get
			{
				if (!MP_IsSystemDefined)
				{
					return OrgSalesProductLocationArrangement.SingleLocation;
				}
				else if (
					MP_Code == SystemDefinedSalesProductList.Codes.CustomsBrokerage ||
					MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
				{
					return OrgSalesProductLocationArrangement.SingleLocation;
				}
				else
				{
					return OrgSalesProductLocationArrangement.OriginDestination;
				}
			}
		}

		#endregion

		#region AllowedAssociationTargets

		public OrgSalesProductAssociationTarget AllowedAssociationTargets
		{
			get
			{
				return MP_IsSystemDefined ? OrgSalesProductAssociationTarget.OrgSales | OrgSalesProductAssociationTarget.OrgTradeDetail : OrgSalesProductAssociationTarget.OrgSales;
			}
		}

		#endregion

		#region SalesMatching

		/// <summary>
		/// Properties that should be compared when determining if a new OrgSales could be the matched against an existing one.
		/// </summary>
		/// <param name="sales"></param>
		/// <returns></returns>
		public SalesMatchingOptions SalesMatchingOptions
		{
			get
			{
				if (salesMatchingOptions == null)
				{
					salesMatchingOptions = new SalesMatchingOptions(
						GetSalesMatchingProperties().ToArray(),
						GetTradeDetailMatchingProperties().ToArray());
				}

				return salesMatchingOptions;
			}
		}

		SalesMatchingOptions salesMatchingOptions;

		IEnumerable<Tuple<string, Type>> GetSalesMatchingProperties()
		{
			if (MP_IsSystemDefined)
			{
				if (MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
				{
					yield return Tuple.Create(OrgSales.Schema.OW_ServiceDescription, typeof(ZString));
					yield return Tuple.Create(OrgSales.Schema.OW_WW, typeof(ZGuid));
				}

				if (LocationArrangement == OrgSalesProductLocationArrangement.SingleLocation)
				{
					yield return Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid));
				}
				else
				{
					yield return Tuple.Create(OrgSales.Schema.OW_OriginID, typeof(ZGuid));
					yield return Tuple.Create(OrgSales.Schema.OW_DestinationID, typeof(ZGuid));
				}
			}
		}

		IEnumerable<Tuple<string, Type>> GetTradeDetailMatchingProperties()
		{
			if (MP_IsSystemDefined)
			{
				if (MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
				{
					yield return Tuple.Create(OrgTradeDetail.Schema.PA_OP, typeof(ZGuid));
				}
				else
				{
					yield return Tuple.Create(OrgTradeDetail.Schema.PA_TradeMode, typeof(ZString));
					yield return Tuple.Create(OrgTradeDetail.Schema.PA_TradeType, typeof(ZString));
				}
			}
		}

		#endregion

		#region MandatoryFields

		public bool ServiceIsMandatory
		{
			get { return MP_IsSystemDefined && MP_Code == SystemDefinedSalesProductList.Codes.Warehouse; }
		}

		public bool ModeIsMandatory
		{
			get { return MP_IsSystemDefined && MP_Code != SystemDefinedSalesProductList.Codes.CustomsBrokerage && MP_Code != SystemDefinedSalesProductList.Codes.Warehouse; }
		}

		public bool TypeIsMandatory
		{
			get { return MP_IsSystemDefined && MP_Code == SystemDefinedSalesProductList.Codes.CustomsBrokerage; }
		}

		#endregion

		#region AllowedFields

		public bool IsBuyerAllowed(IOrgSales sales)
		{
			if (MP_IsSystemDefined && MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				return sales.OW_Service == OrgSalesWarehouseServiceTypesList.Codes.Orders;
			}

			return true;
		}

		public bool IsSupplierAllowed(IOrgSales sales)
		{
			if (MP_IsSystemDefined && MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				return sales.OW_Service == OrgSalesWarehouseServiceTypesList.Codes.Receipts;
			}

			return true;
		}

		public bool IsContainerTypeAllowed(IOrgTradeDetail tradeDetail)
		{
			if (MP_IsSystemDefined && MP_Code == SystemDefinedSalesProductList.Codes.Warehouse)
			{
				return tradeDetail.PA_RequiresPacking;
			}

			return true;
		}

		#endregion

		#endregion

		#region Save

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!IsInDatabase || HasChanges)
			{
				SerializeFormLayoutData();
			}
		}

		#endregion

		#region Activate and Deactivate

		public override string CanCancel()
		{
			return MP_IsSystemDefined ? ResString.GetMultilingualString("4b83d373-3849-47e6-b80a-e1ae66c1b55f", "{0} cannot be deactivated as it is a system defined sales product.", HumanReadableNameCore) : null;
		}

		public override string CanReactivate()
		{
			return MP_IsSystemDefined ? ResString.GetMultilingualString("254b56e9-0ade-454a-957f-cb0a91c81943", "{0} cannot be activated as it is a system defined sales product.", HumanReadableNameCore) : null;
		}

		protected override ZString HumanReadableNameCore => $"{base.HumanReadableNameCore} - {MP_Code} - {MP_NameMultilingual}";

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region CustomColumnsBusinessObject

		public CustomBusinessObject GetNewCustomTradeDetailFieldColumnsBusinessObject(BusinessObject bizObj)
		{
			IUserDefinedPropertyCollection properties;
			if (bizObj == null)
			{
				properties = new UserDefinedPropertyCollectionView();
			}
			else
			{
				properties = new UserDefinedPropertyCollection(bizObj);
			}

			var customColumnCollectionByPk = FormLayout.TradeDetailCustomColumnDefinitionCollection.Cast<OrgSalesProductCustomColumnDefinition>().ToLookup(x => x.PK);
			foreach (SalesProductFieldLayoutDefinition fieldLayoutDefintion in FormLayout.TradeDetailFieldLayoutDefinitions)
			{
				var customColumn = customColumnCollectionByPk[fieldLayoutDefintion.GenCustomColumnDefinitionFk].FirstOrDefault();
				if (customColumn != null)
				{
					properties.AddProperty(customColumn, fieldLayoutDefintion.Order);
				}
			}

			return new CustomBusinessObject(bizObj.Factory, bizObj, properties);
		}

		public static IEnumerable<ICustomProperty> GetCustomGridColumns<T>(BusinessObject bizObj, IList<OrgSalesProductCustomColumnDefinition> allColumns, SalesProductGridColumnDefinitionCollection<T> columnsToInclude)
			where T : SalesProductGridColumnDefinition
		{
			IUserDefinedPropertyCollection properties;
			if (bizObj == null)
			{
				properties = new UserDefinedPropertyCollectionView();
			}
			else
			{
				properties = new UserDefinedPropertyCollection(bizObj);
			}

			var customColumnCollectionByPk = allColumns.ToLookup(x => x.PK);
			foreach (SalesProductGridColumnDefinition gridColumnDef in columnsToInclude)
			{
				var customColumn = customColumnCollectionByPk[gridColumnDef.GenCustomColumnDefinitionFk].FirstOrDefault();
				if (customColumn != null)
				{
					properties.AddProperty(customColumn, gridColumnDef.Order, DynamicMetaData.Visible(gridColumnDef.IsVisible));
				}
			}

			return properties;
		}

		public static CustomBusinessObject GetNewCustomGridColumnsBusinessObject<T>(BusinessObject bizObj, IList<OrgSalesProductCustomColumnDefinition> allColumns, SalesProductGridColumnDefinitionCollection<T> columnsToInclude)
			where T : SalesProductGridColumnDefinition
		{
			var properties = GetCustomGridColumns(bizObj, allColumns, columnsToInclude);
			return new CustomBusinessObject(bizObj.Factory, bizObj, new UserDefinedPropertyCollection(bizObj, properties));
		}

		#endregion
	}

	public class SalesMatchingOptions
	{
		public SalesMatchingOptions(Tuple<string, Type>[] salesPropertiesForMatching, Tuple<string, Type>[] tradeDetailPropertiesForMatching)
		{
			SalesPropertiesForMatching = salesPropertiesForMatching;
			TradeDetailPropertiesForMatching = tradeDetailPropertiesForMatching;
		}

		public Tuple<string, Type>[] SalesPropertiesForMatching;
		public Tuple<string, Type>[] TradeDetailPropertiesForMatching;
	}
}
