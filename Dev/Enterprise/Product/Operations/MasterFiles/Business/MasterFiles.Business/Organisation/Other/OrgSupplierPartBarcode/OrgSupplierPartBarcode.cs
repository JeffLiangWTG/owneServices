using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DeferTriggerAndRunBeforeCommit(OrgPartRelationValidationHelper.TG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique, OrgPartRelationValidationHelper.WhsCheckOwnerAndBarcodeOfProductAreUnique, OrgSupplierPartBarcodeSchema.Constants.PH_OP, typeof(IWhsCheckOwnerAndBarcodeOfProductAreUniqueForOrgSupplierPartBarcode_DeferTriggerStrategy))]
	public class OrgSupplierPartBarcode : AutoOrgSupplierPartBarcode
	{
		#region Constructors

		public OrgSupplierPartBarcode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Static Methods

		public static OrgSupplierPartBarcode FindBarcode(ZString barcode, BusinessObjectFactory factory)
		{
			OrgSupplierPartBarcode result;
			ZQuery query = new ZQuery(OrgSupplierPartBarcodeSchema.PH_Barcode, barcode);
			result = factory.LoadTop1<OrgSupplierPartBarcode>(query);
			return result;
		}

		#endregion

		#region Properties

		#region PH_Barcode

		public override ZString PH_Barcode
		{
			get { return base.PH_Barcode; }
			set
			{
				base.PH_Barcode = value;

				var supplierPart = SupplierPart;
				if (supplierPart != null)
				{
					supplierPart.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region PH_F3_NKPackType

		[List("Lookups.ProductUQList")]
		public override ZString PH_F3_NKPackType
		{
			get => base.PH_F3_NKPackType;
			set
			{
				if (!base.PH_F3_NKPackType.EqualsIgnoringCase(value))
				{
					var part = SupplierPart;
					var sku = part?.OP_StockKeepingUnit ?? ZString.Empty;
					if (value.EqualsIgnoringCase(sku) && part?.PartBarcodes.FindUseForDocumentsPartBarcodeByPackage((sku)) == null)
					{
						PH_UseForDocuments = true;
					}
				}

				base.PH_F3_NKPackType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidatePH_UseForDocuments();
				}
			}
		}

		#endregion

		#region PH_UseForDocuments

		public override ZBool PH_UseForDocuments
		{
			get { return base.PH_UseForDocuments; }
			set
			{
				base.PH_UseForDocuments = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidatePH_F3_NKPackType();
				}
			}
		}

		#endregion

		#endregion
	}
}
