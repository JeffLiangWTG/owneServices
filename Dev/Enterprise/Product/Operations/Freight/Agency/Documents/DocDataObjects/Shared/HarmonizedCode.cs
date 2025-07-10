using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class HarmonizedCode : DocDataObject, IHarmonizedCode
	{
		#region Code

		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
					Validate(CodeInfo);
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		#endregion

		#region Country

		public ICountry Country
		{
			get => country;
			set => country = SetChild(country, value);
		}

		ICountry country;

		#endregion

		#region Implementation

		public override string ToString()
		{
			if (!Code.IsEmpty && Country != null && !Country.Code.IsEmpty)
			{
				var prefix = Country.Code == Core.Constants.CountryCodes.Brazil
						? Core.Constants.HarmonizedCodes.NCMCode
						: Core.Constants.HarmonizedCodes.HSCode;

				return ZString.Format("{0} ({1}): {2}", prefix, Country.Code, Code);
			}

			return Code;
		}

		#endregion
	}
}
