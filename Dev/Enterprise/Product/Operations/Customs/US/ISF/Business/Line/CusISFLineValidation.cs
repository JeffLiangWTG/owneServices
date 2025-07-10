using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFLineValidation : AutoCusISFLineValidation
	{
		public CusISFLineValidation(AutoCusISFLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateBL_FormattedHarmonisedNum();
			}
		}

		public void ValidateBL_FormattedHarmonisedNum()
		{
			ValidateCalculatedProperty(Parent.BL_FormattedHarmonisedNumInfo);
		}

		protected new CusISFLine Parent
		{
			get { return (CusISFLine)base.Parent; }
		}

		protected override void CheckBL_RN_NKGoodsOrigin()
		{
			base.CheckBL_RN_NKGoodsOrigin();
			if (Parent.Header != null && Parent.Header.IsISF10Entry)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BL_RN_NKGoodsOriginInfo);
			}
			else
			{
				ListValidation.WarnIfInvalidCode(Parent.BL_RN_NKGoodsOriginInfo);
			}
		}

		protected override void CheckBL_ManufacturerDocAddressPK()
		{
			base.CheckBL_ManufacturerDocAddressPK();
			CusISFHeader header = Parent.Header;
			if (header.IsISF10Entry)
			{
				if (Parent.BL_ManufacturerDocAddressPK.IsEmpty)
				{
					Parent.BL_ManufacturerDocAddressPKInfo.AddMessageError(ManufacturerRequired);
				}
			}
		}
		internal const string ManufacturerRequired = "Manufacturer is required when Entry Type is '1', '3' or '5'.";

		protected override void CheckBL_TextProductCode()
		{
			if (Parent.BL_TextProductCode_CanBeSetByCustomer && !Parent.BL_TextProductCode.IsEmpty && Parent.PartSyncManager.Enabled)
			{
				ListValidation.WarnIfInvalidCode(Parent.BL_TextProductCodeInfo, Parent.Lookups.SupplierParts, (NoResString)ProductCodeNotBelongToImporter);

				var part = Parent.USSupplierPart;
				if (part != null && Parent.Pivot == null)
				{
					var importer = Parent.Header.Importer;
					var supplier = Parent.SellingParty;
					ZString partAttrib1 = ZString.Empty;
					ZString partAttrib2 = ZString.Empty;
					ZString partAttrib3 = ZString.Empty;
					if (importer != null)
					{
						partAttrib1 = importer.PartAttributeManager.PartAttributeName1;
						partAttrib2 = importer.PartAttributeManager.PartAttributeName2;
						partAttrib3 = importer.PartAttributeManager.PartAttributeName3;
					}
					Parent.BL_TextProductCodeInfo.AddWarning(CannotMatchClassificationForPart(part.OP_PartNum,
						importer == null ? ZString.Empty : importer.OH_Code,
						supplier == null ? ZString.Empty : supplier.OH_Code,
						partAttrib1, Parent.BL_PartAttrib1, partAttrib2, Parent.BL_PartAttrib2, partAttrib3, Parent.BL_PartAttrib3, Parent.ISFDate));
				}
			}
		}

		internal const string ProductCodeNotBelongToImporter = "This product does not belong to the importer; please select one that belongs to the importer.";

		static string CannotMatchClassificationForPart(string partNum, string importerCode, string supplierCode, string partAttrib1Name, string partAttrib1Value, string partAttrib2Name, string partAttrib2Value, string partAttrib3Name, string partAttrib3Value, ZDateTime effectiveDate)
		{
			return string.Format(CultureInfo.CurrentCulture, "Cannot match classification for product ({0}) based on Importer ({1}), Supplier ({2}), {3} ({4}), {5} ({6}), {7} ({8}) and Effective Date ({9}).",
				partNum, importerCode, supplierCode, partAttrib1Name, partAttrib1Value, partAttrib2Name, partAttrib2Value, partAttrib3Name, partAttrib3Value, effectiveDate.ToString("d-MMM-yy", CultureInfo.CurrentCulture));
		}

		protected override void CheckBL_PartAttrib1()
		{
			base.CheckBL_PartAttrib1();
			CheckPartAttribute(Parent.BL_PartAttrib1Info, 1);
			ValidateBL_TextProductCode();
		}

		protected override void CheckBL_PartAttrib2()
		{
			base.CheckBL_PartAttrib2();
			CheckPartAttribute(Parent.BL_PartAttrib2Info, 2);
			ValidateBL_TextProductCode();
		}

		protected override void CheckBL_PartAttrib3()
		{
			base.CheckBL_PartAttrib3();
			CheckPartAttribute(Parent.BL_PartAttrib3Info, 3);
			ValidateBL_TextProductCode();
		}

		void CheckPartAttribute(ZPropertyInfo info, int attribNumber)
		{
			var header = Parent.Header;
			if (header != null)
			{
				new PartAttributeValidation().CheckAttribute(header.Importer, Parent.USSupplierPart, info, attribNumber);
			}
		}

		protected override void CheckBL_OPIsValidZGuid()
		{
			// is checked in CheckBL_OP()
		}

		protected sealed override void CheckBL_HarmonisedNum()
		{
			ValidateBL_FormattedHarmonisedNum();
		}

		protected void CheckBL_FormattedHarmonisedNum()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.BL_FormattedHarmonisedNumInfo);
			int numberOfHarmonisedDigitsRequired = Parent.Header?.NumberOfHarmonisedDigitsRequired ?? 10;

			ISFTariffValidator.ValidateFormattedHarmonisedNum(Parent.Factory, Parent.BL_FormattedHarmonisedNumInfo, Parent.HarmonisedNumToReportToCustoms, numberOfHarmonisedDigitsRequired);
		}
	}
}
