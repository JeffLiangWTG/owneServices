using System;
using System.Drawing;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbReleaseNoteValidation : AutoGlbReleaseNoteValidation
	{
		public GlbReleaseNoteValidation(AutoGlbReleaseNote parent) : base(parent)
		{
		}

		protected new GlbReleaseNote Parent
		{
			get { return (GlbReleaseNote)base.Parent; }
		}

		#region GF_Category

		protected override void CheckGF_Category()
		{
			base.CheckGF_Category();
			if (Parent.GF_Section == NewsSectionTypeList.Codes.ProductUpdates)
			{
				MandatoryValidation.CheckEntered(Parent.GF_CategoryInfo);
			}
			ListValidation.ErrorIfInvalidCode(Parent.GF_CategoryInfo);
		}

		#endregion

		#region GF_Summary

		protected override void CheckGF_Summary()
		{
			base.CheckGF_Summary();
			MandatoryValidation.CheckEntered(Parent.GF_SummaryInfo, Res.GetString("755b5a47-b69d-4a7c-b57c-c247c6edfc31", "Summary"));
			TranslatableDataFieldAttribute.Validate(Parent.GF_SummaryInfo);
		}

		#endregion

		#region GF_URL

		protected override void CheckGF_URL()
		{
			base.CheckGF_URL();
			MandatoryValidation.CheckEntered(Parent.GF_URLInfo, Res.GetString("132c24da-8fb4-4536-bf41-09b27459c34b", "Link"));
		}

		#endregion

		#region GF_RN_NKCountryForReleaseNote

		protected override void CheckGF_RN_NKCountryForReleaseNote()
		{
			base.CheckGF_RN_NKCountryForReleaseNote();
			ListValidation.ErrorIfInvalidCode(Parent.GF_RN_NKCountryForReleaseNoteInfo);
		}

		#endregion

		#region GF_Section

		protected override void CheckGF_Section()
		{
			base.CheckGF_Section();
			MandatoryValidation.CheckEntered(Parent.GF_SectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.GF_SectionInfo);
		}

		#endregion

		#region GF_Thumbnail

		protected override void CheckGF_Thumbnail()
		{
			if (Parent.GF_Thumbnail.Length > MaxThumbnailImageBytes)
			{
				Parent.GF_ThumbnailInfo.AddError(Res.GetString("0418b9ae-06bf-4312-8b91-35de7ed792fa", "This image is too large. It should be an image with a size no greater than 64KB."));
			}

			if (!Parent.GF_Thumbnail.IsEmpty && !IsValidImage(Parent.GF_Thumbnail))
			{
				Parent.GF_ThumbnailInfo.AddError(Res.GetString("f4c07545-aed6-4c8d-8524-8824992e443e", "The image supplied is invalid or corrupt."));
			}
		}

		protected bool IsValidImage(byte[] data)
		{
			try
			{
				using var img = new Bitmap(new MemoryStream(data));
			}
			catch (ArgumentException)
			{
				return false;
			}

			return true;
		}

		const int MaxThumbnailImageBytes = 65536; // 64KB

		#endregion
	}
}
