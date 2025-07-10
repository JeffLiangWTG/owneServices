using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefUNLOCOValidation : AutoRefUNLOCOValidation
	{
		public RefUNLOCOValidation(AutoRefUNLOCO parent)
			: base(parent)
		{
		}

		protected RefUNLOCO RefUNLOCO => Parent as RefUNLOCO;

		#region Code

		protected override void CheckRL_Code()
		{
			base.CheckRL_Code();

			ZQuery query = new ZQuery();
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, Parent.RL_Code);
			query.AddToFilter(RefUNLOCOSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			var result = Parent.Factory.Load<RefUNLOCO>(query);

			if (result.Length != 0)
			{
				Parent.RL_CodeInfo.AddError(Res.GetString("C9D277B2-0CF6-49BA-891C-8AC8C9E38F46", "There is already an UNLOCO record with same code."));
			}

			if (Parent.RL_Code.Length != 5)
			{
				Parent.RL_CodeInfo.AddError(Res.GetString("ea05aa4c-1393-403e-9300-c89032086ef6", "UNLOCO must be 5 characters long."));
			}

			if (!Parent.RL_Code.IsLettersAndNumbersOnlyOrEmpty)
			{
				Parent.RL_CodeInfo.AddError(Res.GetString("a8b4390e-4569-4426-8c01-ac9cdef1c80d", "A UNLOCO must consist of only letters and numbers."));
			}

			if (Parent.RL_Code.Length > 2)
			{
				if (!IsValidCountry(Parent.RL_Code.Substring(0, 2).ToUpper()))
				{
					Parent.RL_CodeInfo.AddError(Res.GetString("5de655fd-0bbc-4585-8bd2-43fa9cf6ec88", "First two characters of UNLOCO must be a valid country/region code."));
				}
			}
		}

		#endregion

		#region IATA Code

		protected override void CheckRL_IATA()
		{
			base.CheckRL_IATA();

			if (!Parent.RL_IATA.IsLettersOnlyOrEmpty)
			{
				Parent.RL_IATAInfo.AddError(Res.GetString("68ce6192-d0b6-4fbd-b2fb-3d7a7edaf3c7", "The IATA Code must consist of only letters."));
			}
		}

		#endregion

		#region IATA Region Code

		protected override void CheckRL_IATARegionCode()
		{
			base.CheckRL_IATARegionCode();

			if ((Parent.RL_IATARegionCode.Length != 3 && Parent.RL_IATARegionCode != string.Empty) || !Parent.RL_IATARegionCode.IsLettersOnlyOrEmpty)
			{
				Parent.RL_IATARegionCodeInfo.AddError(Res.GetString("{9c165c6a-0741-47fe-8fea-367917161161}", "The IATA Region Code must be exactly three letters."));
			}
		}

		#endregion

		#region CountryCode

		protected override void CheckRL_RN_NKCountryCode()
		{
			base.CheckRL_RN_NKCountryCode();
			ListValidation.ErrorIfInvalidCode(Parent.RL_RN_NKCountryCodeInfo);

			if (Parent.RL_Code.Length > 2)
			{
				if (Parent.Country == null || !CompareValidation.Equals(Parent.Country.RN_Code.ToUpper(), Parent.RL_Code.Substring(0, 2).ToUpper()))
				{
					Parent.RL_RN_NKCountryCodeInfo.AddError(Res.GetString("11cc4586-2b06-4f47-bacc-eed7740b1eb9", "Country/Region code must match the first two characters of UNLOCO."));
				}
			}
		}

		#endregion

		#region Port Name

		protected override void CheckRL_PortName()
		{
			base.CheckRL_PortName();
			MandatoryValidation.CheckEntered(Parent.RL_PortNameInfo);
		}

		#endregion

		#region Name With Diacriticals

		protected override void CheckRL_NameWithDiacriticals()
		{
			base.CheckRL_NameWithDiacriticals();
			MandatoryValidation.CheckEntered(Parent.RL_NameWithDiacriticalsInfo);
		}

		#endregion

		#region RL_IsUpdatable

		protected override void CheckRL_IsUpdatable()
		{
			base.CheckRL_IsUpdatable();
			if (Parent.RL_IsUpdatable && (!Parent.IsInDatabase || Parent.HasChanges))
			{
				Parent.RL_IsUpdatableInfo.AddWarning(Res.GetString("c4489432-3cd0-4f77-8e2d-da8caf53f192", "All user's changes will be lost when data is updated from system reference source."));
			}
		}

		#endregion

		#region Latitude

		public void ValidateLatitude()
		{
			ValidateCalculatedProperty(RefUNLOCO.LatitudeInfo);
		}

		protected virtual void CheckLatitude()
		{
			if (!RefUNLOCO.IsValidLatitude(RefUNLOCO.Latitude))
			{
				RefUNLOCO.LatitudeInfo.AddError(Res.GetString("b5a66312-138d-40ff-8591-731d7263f352", "Latitude value should be between -90 to 90 degree."));
			}
		}

		#endregion

		#region Longitude

		public void ValidateLongitude()
		{
			ValidateCalculatedProperty(RefUNLOCO.LongitudeInfo);
		}

		protected virtual void CheckLongitude()
		{
			if (!RefUNLOCO.IsValidLongitude(RefUNLOCO.Longitude))
			{
				RefUNLOCO.LongitudeInfo.AddError(Res.GetString("83f6f824-2f4c-42f6-b649-4d5a02f6dbfd", "Longitude value should be between -180 to 180 degree."));
			}
		}

		#endregion

		#region Helper Methods

		ZBool IsValidCountry(string code)
		{
			return ((RefUNLOCO)Parent).GetCountryFromCode(code) != null;
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateLatitude();
			ValidateLongitude();
		}

		#endregion
	}
}
