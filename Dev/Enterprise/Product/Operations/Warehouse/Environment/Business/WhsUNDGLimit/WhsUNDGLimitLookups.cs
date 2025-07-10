//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoWhsUNDGLimitLookups
//
//    This class should be used for overriding collections in AutoWhsUNDGLimitLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsUNDGLimitLookups : AutoWhsUNDGLimitLookups
	{
		public WhsUNDGLimitLookups(AutoWhsUNDGLimit parent) : base(parent)
		{
		}

		#region WeightUnits

		public CodeDescriptionPairList WeightUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		#endregion

		#region VolumeUnits

		public CodeDescriptionPairList VolumeUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		#endregion

		#region UNDGCountryReferences

		public override UNDGCountryReferenceCollection UNDGCountryReferences
		{
			get
			{
				var result = base.UNDGCountryReferences;
				var undgLimit = (WhsUNDGLimit)Parent;
				if (undgLimit.Warehouse.CountryCode != string.Empty)
				{
					result = new UNDGCountryReferenceCollection(Factory, undgLimit);
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Country", "Property", ((WhsUNDGLimit)Parent).Warehouse.WarehouseAddress.Country.Code, true));
				}

				return result;
			}
		}

		#endregion

		#region UNDGSubstanceCollection

		public UNDGSubstanceCollection UNDGSubstances
		{
			get
			{
				return new UNDGSubstanceCollection(Factory);
			}
		}

		#endregion

		#region UNDGClass

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Resource string default text")]
		public CodeDescriptionPairList UNDGClass
		{
			get
			{
				var classList = new CodeDescriptionPairList();
				for (var i = 1; i <= 9; i++)
				{
					classList.AddPair($"{i}", Res._GetString(58657, "42e9bf16-a9be-4eee-bf81-9b1920e71a7f", "Class {0}", i));
				}

				classList.AddPair("Comb", Res._GetString(58657, "0713A2DE-4DBD-4F4B-9C2C-A14A2A50EDB1", "Combustible liquid"));
				return classList;
			}
		}

		#endregion
	}
}
