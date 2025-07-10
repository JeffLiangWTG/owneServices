using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business
{
	public sealed class WhsClientParameterByWarehouse : AutoWhsClientParameterByWarehouse, IWhsClientParameterByWarehouse
	{
		#region Constructors

		public WhsClientParameterByWarehouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Related Business Objects

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WY_WW_Whs);

		#endregion

		#region Properties

		[List("Lookups.Organisations")]
		[RelatedBusinessObject("Client")]
		public override ZGuid WY_OH_Client
		{
			get { return base.WY_OH_Client; }
			set
			{
				base.WY_OH_Client = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWY_WW_Whs();
					Validation.ValidateWY_ReceiveCategory();
				}
			}
		}

		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WY_WW_Whs
		{
			get { return base.WY_WW_Whs; }
			set
			{
				base.WY_WW_Whs = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWY_OH_Client();
					Validation.ValidateWY_ReceiveCategory();
				}
			}
		}

		[List("Lookups.ReceiveCategories")]
		public override ZString WY_ReceiveCategory
		{
			get { return base.WY_ReceiveCategory; }
			set
			{
				base.WY_ReceiveCategory = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWY_OH_Client();
					Validation.ValidateWY_WW_Whs();
				}
			}
		}

		#endregion
	}
}
