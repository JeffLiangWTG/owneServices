using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business
{
	class TravelDocumentsValidation : GenRegCertAccredMaintListValidation
	{
		public TravelDocumentsValidation(GenRegCertAccredMaintList parent)
			: base(parent)
		{
		}

		#region CheckXZ_Comment

		protected override void CheckXZ_Comment()
		{
			//Description should not be mandatory.
		}

		#endregion

		#region CheckXZ_RefNumber

		protected override void CheckXZ_RefNumber()
		{
			base.CheckXZ_RefNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.XZ_RefNumberInfo);
		}

		#endregion

		#region CheckXZ_RN_NKCountryOfIssuance

		protected override void CheckXZ_RN_NKCountryOfIssuance()
		{
			base.CheckXZ_RN_NKCountryOfIssuance();
			if (TravelDocumentTypes.IsCountryOfIssuanceRequired(Parent.XZ_Type))
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.XZ_RN_NKCountryOfIssuanceInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.XZ_RN_NKCountryOfIssuanceInfo);
			}
		}

		#endregion

		#region CheckXZ_StateOrProvinceOfIssuance

		protected override void CheckXZ_StateOrProvinceOfIssuance()
		{
			base.CheckXZ_StateOrProvinceOfIssuance();
			if (Parent.Lookups.StatesOrProvinces.Count > 0)
			{
				if (TravelDocumentTypes.IsStateOrProvinceOfIssuanceRequired(Parent.XZ_Type))
				{
					ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.XZ_StateOrProvinceOfIssuanceInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.XZ_StateOrProvinceOfIssuanceInfo);
				}
			}
		}

		#endregion

		#region CheckXZ_Type

		protected override void CheckXZ_Type()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.XZ_TypeInfo);
		}

		#endregion
	}
}
