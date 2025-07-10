//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRefPackTypeValidation
//
//    This class should be used for overriding validation in AutoRefPackTypeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class RefPackTypeValidation : AutoRefPackTypeValidation
	{
		public RefPackTypeValidation(AutoRefPackType parent)
			: base(parent)
		{
		}

		protected override void CheckF3_UnitOfDimension()
		{
			base.CheckF3_UnitOfDimension();

			if (!Parent.F3_Height.IsEmpty || !Parent.F3_Width.IsEmpty || !Parent.F3_Length.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.F3_UnitOfDimensionInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.F3_UnitOfDimensionInfo);
		}

		protected override void CheckF3_UnitOfWeight()
		{
			base.CheckF3_UnitOfWeight();

			if (!Parent.F3_Weight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.F3_UnitOfWeightInfo);
			}

			ListValidation.ErrorIfInvalidCode(Parent.F3_UnitOfWeightInfo);
		}

		protected override void CheckF3_Code()
		{
			base.CheckF3_Code();

			MandatoryValidation.CheckEntered(Parent.F3_CodeInfo);

			if (!Parent.F3_Code.IsEmpty)
			{
				var standardUnits = Parent.Factory.GetCachedValue("RefPackTypeValidation.StandardUnitListIncludingRatingUnits", () => StandardUnitListIncludingRatingUnits);

				if (standardUnits.Contains(Parent.F3_Code))
				{
					Parent.F3_CodeInfo.AddError(Res.GetString("0077b16e-d2d8-4055-8f9c-10283ee22a18", "This Package Type Code is a standard quantity unit. Please enter different Package Type Code."));
				}

				var query = new ZQuery(RefPackTypeSchema.F3_Code, Parent.F3_Code);
				query.AddToFilter(JoinCondition.And, RefPackTypeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				var alreadyExists = Parent.Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(RefPackType)), query);
				if (alreadyExists)
				{
					Parent.F3_CodeInfo.AddError(Res.GetString("ccad7662-2f4f-4431-8fd7-62d2282bca2b", "This Package Type Code already exists. Please ensure you have entered the correct Package Type Code."));
				}

				if (Parent.IsInDatabase && Parent.F3_CodeInfo.HasChanges)
				{
					Parent.F3_CodeInfo.AddWarning(Res.GetString("fbee46bd-ebd3-4e9c-bba3-7f1be0d400a0", "Changing this Package Type Code may have unwanted effects."));
				}
			}
		}

		protected override void CheckF3_Description()
		{
			base.CheckF3_Description();
			MandatoryValidation.CheckEntered(Parent.F3_DescriptionInfo);
			TranslatableDataFieldAttribute.Validate(Parent.F3_DescriptionInfo);
		}

		protected override void CheckF3_IsUpdatable()
		{
			base.CheckF3_IsUpdatable();
			if (Parent.F3_IsUpdatable && (!Parent.IsInDatabase || Parent.HasChanges))
			{
				Parent.F3_IsUpdatableInfo.AddWarning(Res.GetString("c4489432-3cd0-4f77-8e2d-da8caf53f192", "All user's changes will be lost when data is updated from system reference source."));
			}
		}

		protected override void CheckF3_UOMType()
		{
			base.CheckF3_UOMType();
			ListValidation.ErrorIfInvalidCode(Parent.F3_UOMTypeInfo);
		}

#if DEBUG
		public static
#endif
 List<string> StandardUnitListIncludingRatingUnits
		{
			get
			{
				var list = RefPackTypeCollection.GetStandardUnitsAsCodeDescriptionPairs().Cast<ICodeDescription>().Select(x => x.Code).ToList();
				list.AddRange(new[] { "HR", "DY", "WK", "SV", "CN", "HB", "PK", "LI", "CP", "PL", "JU", "JP", "JW", "JV", "FD", "PN", "OM", "CS", "DE", "FC", "DO", "LY", "LW", "TU", "LM", "FI", "NR",
					"IC", "TC", "HC", "PH", "EC", "DF", "SC", "GC", "NP", "OP", "N3", "NA", "NC", "NS", "AM", "AS", "AF", "DC", "FS", "FW", "PS", "HF", "TB", "VE", "OD", "TS", "PI", "CL", "NO",
					"ASD", "CSD", "DED", "DOD", "ODD", "PSD", "HFD", "TSD", "VED", "FCD", "FDD", "FWD", "LYD", "N3D", "NAD", "NSD", "OMD", "TBD", "AMD", "NOD", "FSD", "BOM",
					"DOC", "L01", "L02", "L03", "L04", "L05", "L06", "L07", "L08", "L09", "L10", "L11", "L12", "L13", "L14",
					"L16", "L17", "L18", "L19", "L20", "L21", "L22", "L23", "L25", "L26", "L27", "L28", "L29", "L30", "L31", "LKR", "VIS", "PGA", "PGD", "H92", "H93"
				}); //autorating units
				return list;
			}
		}
	}
}
