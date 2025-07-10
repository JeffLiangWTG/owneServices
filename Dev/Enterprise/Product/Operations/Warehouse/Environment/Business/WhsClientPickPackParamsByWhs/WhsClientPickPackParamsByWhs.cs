using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientPickPackParamsByWhs : AutoWhsClientPickPackParamsByWhs
	{
		public WhsClientPickPackParamsByWhs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Entities

		#region SalesChannel

		public WhsSalesChannel SalesChannel => Factory.Load<WhsSalesChannel>(WPP_WSH_SalesChannel);

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WPP_WW_Warehouse); }
		}

		#endregion

		#endregion

		#region Properties

		// persistent

		#region WPP_IsPickAndPackEnabled

		public override ZBool WPP_IsPickAndPackEnabled
		{
			get { return base.WPP_IsPickAndPackEnabled; }
			set
			{
				base.WPP_IsPickAndPackEnabled = value;

				// tested in WhsClientPickPackParamsByWhsValidationTest
				if (!IsValidationSuspended)
				{
					Validation.ValidateWPP_NumberOfLabelsToPrintOnClose();
					Validation.ValidateWPP_NumberOfLabelsToPrintOnNew();
					Validation.ValidateWPP_PromptForWeightAndDimensions();
				}
			}
		}

		#endregion

		#region WPP_NumberOfLabelsToPrintOnClose

		[ReadOnlyMember(nameof(IsPickAndPackDisabled))]
		public override ZInt WPP_NumberOfLabelsToPrintOnClose
		{
			get { return base.WPP_NumberOfLabelsToPrintOnClose; }
			set
			{
				base.WPP_NumberOfLabelsToPrintOnClose = value;

				if (!IsValidationSuspended)
				{
					// tested in WhsClientPickPackParamsByWhsValidationTest
					Validation.ValidateWPP_NumberOfLabelsToPrintOnNew();
				}
			}
		}

		#endregion

		#region WPP_NumberOfLabelsToPrintOnNew

		[ReadOnlyMember(nameof(IsPickAndPackDisabled))]
		public override ZInt WPP_NumberOfLabelsToPrintOnNew
		{
			get { return base.WPP_NumberOfLabelsToPrintOnNew; }
			set
			{
				base.WPP_NumberOfLabelsToPrintOnNew = value;

				if (!IsValidationSuspended)
				{
					// tested in WhsClientPickPackParamsByWhsValidationTest
					Validation.ValidateWPP_NumberOfLabelsToPrintOnClose();
				}
			}
		}

		#endregion

		#region WPP_F3_NKPackType

		[ReadOnlyMember(nameof(IsPickAndPackDisabled))]
		public override ZString WPP_F3_NKPackType
		{
			get { return base.WPP_F3_NKPackType; }
			set { base.WPP_F3_NKPackType = value; }
		}

		#endregion

		#region WPP_WSH_SalesChannel

		[List("Lookups.SalesChannels")]
		[RelatedBusinessObject(nameof(SalesChannel))]
		public override ZGuid WPP_WSH_SalesChannel
		{
			get => base.WPP_WSH_SalesChannel;
			set
			{
				base.WPP_WSH_SalesChannel = value;

				if (!IsValidationSuspended)
				{
					// tested in WhsClientPickPackParamsByWhsValidationTest
					Validation.ValidateWPP_WW_Warehouse();
					Validation.ValidateWPP_CycleCountOnShort();
					Validation.ValidateWPP_AllowPickDockDoorLocationOverride();
				}
			}
		}

		#endregion

		#region WPP_WW_Warehouse

		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WPP_WW_Warehouse
		{
			get => base.WPP_WW_Warehouse;
			set
			{
				base.WPP_WW_Warehouse = value;

				if (!IsValidationSuspended)
				{
					// tested in WhsClientPickPackParamsByWhsValidationTest
					Validation.ValidateWPP_WSH_SalesChannel();
				}
			}
		}

		#endregion

		#region WPP_IsUsingOwnLabel

		[ReadOnlyMember(nameof(IsPickAndPackDisabled))]
		public override ZBool WPP_IsUsingOwnLabel
		{
			get { return base.WPP_IsUsingOwnLabel; }
			set { base.WPP_IsUsingOwnLabel = value; }
		}

		#endregion

		#region WPP_CartonizeByProduct

		public override ZBool WPP_CartonizeByProduct
		{
			get => base.WPP_CartonizeByProduct;
			set
			{
				base.WPP_CartonizeByProduct = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWPP_CartonizeByProductCategory();
				}
			}
		}

		#endregion

		#region WPP_CartonizeByProductCategory

		public override ZBool WPP_CartonizeByProductCategory
		{
			get => base.WPP_CartonizeByProductCategory;
			set
			{
				base.WPP_CartonizeByProductCategory = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWPP_CartonizeByProduct();
				}
			}
		}

		#endregion

		#region WPP_CycleCountOnShort

		public override ZBool WPP_CycleCountOnShort
		{
			get => base.WPP_CycleCountOnShort;
			set
			{
				base.WPP_CycleCountOnShort = value;

				if (!IsValidationSuspended)
				{
					// tested in WhsClientPickPackParamsByWhsValidationTest
					Validation.ValidateWPP_WSH_SalesChannel();
				}
			}
		}

		#endregion

		#region WPP_AllowPickDockDoorLocationOverride

		public override ZBool WPP_AllowPickDockDoorLocationOverride
		{
			get => base.WPP_AllowPickDockDoorLocationOverride;
			set
			{
				base.WPP_AllowPickDockDoorLocationOverride = value;

				if (!IsValidationSuspended)
				{
					// tested in WhsClientPickPackParamsByWhsValidationTest
					Validation.ValidateWPP_WSH_SalesChannel();
				}
			}
		}

		#endregion

		// calculated

		#region WarehouseName

		[ResourceStringData("WhsClientPickPackParamsByWhs|WarehouseName", Caption = "Warehouse Name")]
		public ZString WarehouseName
		{
			get
			{
				var warehouse = Warehouse;
				return warehouse != null ? warehouse.WW_WarehouseNameMultilingual : ZString.Empty;
			}
		}

		#endregion

		bool IsPickAndPackDisabled => !WPP_IsPickAndPackEnabled;

		#endregion
	}
}
