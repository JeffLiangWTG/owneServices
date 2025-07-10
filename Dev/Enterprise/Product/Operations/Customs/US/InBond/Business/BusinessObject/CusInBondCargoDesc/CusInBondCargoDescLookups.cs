using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.PartAttribListCreator;
using AMSBusiness = Enterprise.Customs.US.AMS.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondCargoDescLookups : Customs.Business.CusInBondCargoDescLookups
	{
		public CusInBondCargoDescLookups(CusInBondCargoDesc parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ManifestUnitList
		{
			get { return new CodeDescriptionPairList(new AMSBusiness.ManifestUnitList()); }
		}

		public CodeDescriptionPairList WeightUnitList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public USCTariffCollection Tariffs
		{
			get { return new USCTariffCollection(Factory); }
		}

		public override OrgHeaderCollection Suppliers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public override Customs.Business.OrgSupplierPartCollection Parts
		{
			get
			{
				var parent = Parent;
				var importer = parent.Importer;
				var supplier = parent.Supplier;

				var result = new US.Business.OrgSupplierPartCollection(Factory, supplier, importer, false);
				if (result != null)
				{
					if (!parent.BY_PartNumber.IsEmpty)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Product Code", "Property", parent.BY_PartNumber));
					}
					if (supplier != null)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property2", supplier.PK));
					}
					if (importer != null)
					{
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Importer/Supplier", "Property1", importer.PK));
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList PartAttrib1List => GetPartAttribList(x => x.Attributes1);

		public CodeDescriptionPairList PartAttrib2List => GetPartAttribList(x => x.Attributes2);

		public CodeDescriptionPairList PartAttrib3List => GetPartAttribList(x => x.Attributes3);

		CodeDescriptionPairList GetPartAttribList(GetAttributesDelegate getAttributes)
		{
			return Parent.Part.GetPartAttribList(Parent.ImporterPK, Parent.BY_OH_Supplier, ClassificationTypeList.Codes.HTI, getAttributes, Core.Constants.CountryCodes.UnitedStates);
		}

		new CusInBondCargoDesc Parent => (CusInBondCargoDesc)base.Parent;
	}
}
