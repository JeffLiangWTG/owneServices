using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRequiredDocumentValidation : AutoRefCountryRequiredDocumentValidation
	{
		public RefCountryRequiredDocumentValidation(AutoRefCountryRequiredDocument parent)
			: base(parent)
		{
		}

		protected override void CheckRD_RN_NKDestination()
		{
			base.CheckRD_RN_NKDestination();

			ListValidation.ErrorIfInvalidCode(Parent.RD_RN_NKDestinationInfo);
			if (!Parent.RD_RN_NKDestinationInfo.HasErrors())
			{
				CheckBothOriginAndDestinationIsNotTheSelectedCountry(Parent.RD_RN_NKDestinationInfo);
			}
		}

		protected override void CheckRD_RN_NKOrigin()
		{
			base.CheckRD_RN_NKOrigin();

			ListValidation.ErrorIfInvalidCode(Parent.RD_RN_NKOriginInfo);
			if (!Parent.RD_RN_NKOriginInfo.HasErrors())
			{
				CheckBothOriginAndDestinationIsNotTheSelectedCountry(Parent.RD_RN_NKOriginInfo);
			}
		}

		void CheckBothOriginAndDestinationIsNotTheSelectedCountry(ZPropertyInfo info)
		{
			if (!Parent.RD_RN_NKDestination.IsEmpty
				&& !Parent.RD_RN_NKOrigin.IsEmpty
				&& !Parent.SelectedCountryCode.IsEmpty
				&& Parent.RD_RN_NKDestination != Parent.SelectedCountryCode
				&& Parent.RD_RN_NKOrigin != Parent.SelectedCountryCode)
			{
				info.AddError(Res.GetString("d14be0f0-c892-46cd-a0da-9bbd11ae2123", "Origin and Destination cannot both be different countries/regions from the one selected."));
			}
		}

		void CheckDuplicateRow()
		{
			Parent.ClearRowNotifications();
			bool isDuplicateRow = false;

			var country = new RefCountry.Loader(Parent.Factory).LoadForCountry(Parent.SelectedCountryCode);
			if (country != null)
			{
				RefCountryRequiredDocumentCollection collection = country.RequiredDocuments;
				foreach (RefCountryRequiredDocument requiredDocument in collection)
				{
					if (Parent.PK != requiredDocument.PK
						&& Parent.RD_DocType == requiredDocument.RD_DocType
						&& Parent.RD_RN_NKDestination == requiredDocument.RD_RN_NKDestination
						&& Parent.RD_RN_NKOrigin == requiredDocument.RD_RN_NKOrigin
						&& Parent.RD_TransportMode == requiredDocument.RD_TransportMode)
					{
						isDuplicateRow = true;
						break;
					}
				}
			}

			if (isDuplicateRow)
			{
				Parent.AddRowError(Res.GetString("04875dd1-d78f-4585-b800-e099f5dd692e", "Duplicate: A Required Document with this Document Type, Origin, Destination and Transport Mode already exists in this list."));
			}
		}
		protected override void CheckRD_DocUsage()
		{
			base.CheckRD_DocUsage();

			MandatoryValidation.CheckEntered(Parent.RD_DocUsageInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RD_DocUsageInfo);

			//DOM - same countries or both blank
			//IMP - different countries
			//EXP - different countries
			//ALL - at least one blank

			if ((Parent.RD_DocUsage == JobRequiredDocument.DocUsage.Domestic && Parent.RD_RN_NKOrigin != Parent.RD_RN_NKDestination)
			|| (Parent.RD_DocUsage == JobRequiredDocument.DocUsage.All && !(Parent.RD_RN_NKOrigin == "" || Parent.RD_RN_NKDestination == "") && Parent.RD_RN_NKOrigin != Parent.RD_RN_NKDestination))
			{
				Parent.RD_DocUsageInfo.AddError(Res.GetString("0e73c25c-df6f-4f41-9058-0a4efb38e8af", "Doc Usage should be either Import, Export or Both if Origin and Destination are different Countries/Regions."));
			}
			else if (Parent.RD_DocUsage != JobRequiredDocument.DocUsage.Domestic
				&& Parent.RD_RN_NKOrigin == Parent.RD_RN_NKDestination
				&& Parent.RD_RN_NKOrigin != "")
			{
				Parent.RD_DocUsageInfo.AddError(Res.GetString("4720ddea-625d-45ba-8a12-e417d6f22093", "Doc Usage should be Domestic if Origin and Destination are the same Country/Region."));
			}
		}

		protected override void CheckRD_DocType()
		{
			base.CheckRD_DocType();

			MandatoryValidation.CheckEntered(Parent.RD_DocTypeInfo, Res.GetString("4b1e877d-3086-45ca-98ae-2366319629af", "Document Type"));
			ListValidation.ErrorIfInvalidCode(Parent.RD_DocTypeInfo);
		}

		protected override void CheckRD_TransportMode()
		{
			base.CheckRD_TransportMode();

			MandatoryValidation.CheckEntered(Parent.RD_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.RD_TransportModeInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckDuplicateRow();
		}

		new RefCountryRequiredDocument Parent
		{
			get { return (RefCountryRequiredDocument)base.Parent; }
		}
	}
}
