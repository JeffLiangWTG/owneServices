using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFLineLookups : AutoCusISFLineLookups
	{
		public CusISFLineLookups(AutoCusISFLine parent)
			: base(parent)
		{
		}

		public US.Business.USCTariffCollection Tariffs
		{
			get { return new US.Business.USCTariffCollection(Factory); }
		}

		public override MasterFiles.Business.OrgSupplierPartCollection SupplierParts
		{
			get
			{
				var header = Parent.Header;
				var importer = header != null ? header.Importer : null;
				var supplier = Parent.SellingParty;
				var supplierPart = Parent.SupplierPart;

				var result = new MasterFiles.Business.OrgSupplierPartCollection(Factory, supplier, importer, Parent.BL_OPInfo.HasWarnings(), ZString.Empty, ZString.Empty, false);
				if (supplierPart != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", supplierPart.OP_PartNum));
				}
				if (importer != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", importer.PK));
				}
				if (supplier != null)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", supplier.PK));
				}
				return result;
			}
		}

		protected new CusISFLine Parent
		{
			get { return (CusISFLine)base.Parent; }
		}

		#region PartAttribList

		public CodeDescriptionPairList PartAttrib1List
		{
			get { return GetPartAttribList(x => x.Attributes1); }
		}

		public CodeDescriptionPairList PartAttrib2List
		{
			get { return GetPartAttribList(x => x.Attributes2); }
		}

		public CodeDescriptionPairList PartAttrib3List
		{
			get { return GetPartAttribList(x => x.Attributes3); }
		}

		CodeDescriptionPairList GetPartAttribList(GetAttributesDelegate getAttributes)
		{
			var result = new CodeDescriptionPairList();
			var part = Parent.USSupplierPart;
			if (part != null)
			{
				List<BaseCusClassPartPivot> pivots = new List<BaseCusClassPartPivot>();
				var header = Parent.Header;
				var pk = header != null ? header.BF_OH_Importer : ZGuid.Empty;
				if (pk.IsValid)
				{
					pivots.AddRange(part.PivotsForBinding.GetMatchesIgnoringAttributes(US.Business.ClassificationTypeList.Codes.HTI, pk, ZGuid.Empty));
				}
				var supplier = Parent.SellingParty;
				if (supplier != null)
				{
					pivots.AddRange(part.PivotsForBinding.GetMatchesIgnoringAttributes(US.Business.ClassificationTypeList.Codes.HTI, ZGuid.Empty, supplier.PK));
				}
				foreach (var pivot in pivots)
				{
					foreach (var attribute in getAttributes(pivot))
					{
						result.AddPairIfNotExist(attribute.BG_AttributeValue1, attribute.BG_AttributeValue1);
					}
				}
			}
			result.Sort();
			return result;
		}

		delegate CusAttributeFilterCollection GetAttributesDelegate(BaseCusClassPartPivot pivot);

		#endregion
	}
}
