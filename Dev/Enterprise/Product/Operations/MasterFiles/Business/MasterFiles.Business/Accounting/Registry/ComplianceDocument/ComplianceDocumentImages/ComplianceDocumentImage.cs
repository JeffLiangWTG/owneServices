using System;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceDocumentImage : RegistryBusinessObject
	{
		#region Schema

		public new abstract class Schema
		{
			public const string Country = "Country";
			public const string ImagePK = "ImagePK";
			public const string ComplianceSubType = "ComplianceSubType";
			public const string Remark = "Remark";
		}

		#endregion

		protected override bool IsCodeMandatory => false;

		protected bool IsEmptyImageAllowed => false;

		public MultilingualString ErrorMessage => ResString.GetMultilingualString("4EFD7A88-1A18-4788-A298-4CBF7003272A", "The combination of Country/Region '{0}' and Compliance Sub Type '{1}' have already exists.", Country, ComplianceSubType);

		#region Lookups

		public ComplianceDocumentImageLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ComplianceDocumentImageLookups(this);
				}
				return fLookups;
			}
		}

		ComplianceDocumentImageLookups fLookups;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceDocumentImage();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			ComplianceDocumentImage typeCastClone = (ComplianceDocumentImage)clone;
			typeCastClone.ComplianceSubType = complianceSubType;
			typeCastClone.Remark = remark;
			typeCastClone.imagePk = imagePk;

			if (image != null)
			{
				typeCastClone.image = (Image)image.Clone();
			}
		}

		#endregion

		#region Properties

		#region Compliance Sub Type

		[MaxLength(3)]
		[List("Lookups.ComplianceSubTypeList")]
		public ZString ComplianceSubType
		{
			get { return complianceSubType; }
			set
			{
				CheckMaximumLength(ComplianceSubTypeInfo, value);
				SetNonPersistentPropertyValue(ComplianceSubTypeInfo, ref complianceSubType, value);
				if (!IsValidationSuspended)
				{
					ValidateComplianceSubType();
				}
			}
		}
		ZString complianceSubType;

		public ZPropertyInfo ComplianceSubTypeInfo => GetZPropertyInfo(Schema.ComplianceSubType);

		#endregion

		#region Remark

		public ZString Remark
		{
			get
			{
				return remark;
			}
			set
			{
				CheckMaximumLength(RemarkInfo, value);
				SetNonPersistentPropertyValue(RemarkInfo, ref remark, value);
			}
		}
		ZString remark;

		public ZPropertyInfo RemarkInfo => GetZPropertyInfo(Schema.Remark);

		[MaxLength(2)]
		[List("Lookups.CountryList")]
		public ZString Country
		{
			get { return country; }
			set
			{
				CheckMaximumLength(CountryInfo, value);
				SetNonPersistentPropertyValue(CountryInfo, ref country, value);
				if (!IsValidationSuspended)
				{
					ValidateCountry();
				}
			}
		}
		ZString country;

		public ZPropertyInfo CountryInfo => GetZPropertyInfo(Schema.Country);

		#endregion

		#endregion

		#region Validation Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCountry();
			ValidateComplianceSubType();
			ValidateImage();
		}

		protected void ValidateImage()
		{
			ClearRowNotifications();
			if (Image == null && !IsEmptyImageAllowed)
			{
				AddRowError(Res.GetString("DDD5DD53-1A54-4622-A554-69934CACBBAD", "Please select an image."));
			}
		}

		public void ValidateComplianceSubType()
		{
			ComplianceSubTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ComplianceSubTypeInfo, (IMultilingualString)ResString.GetMultilingualString("8554ED8F-A58E-4CEE-B754-F74FD0E9CBA4", "Compliance Sub Type"));
			if (!ComplianceSubTypeInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(ComplianceSubTypeInfo, Lookups.ComplianceSubTypeList);
			}

			if (!ComplianceSubTypeInfo.HasErrors() && CheckComplianceSubTypeExists())
			{
				ComplianceSubTypeInfo.AddError(ErrorMessage);
			}
		}

		ZBool CheckComplianceSubTypeExists()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				var sameItmes = ParentCollection.Cast<ComplianceDocumentImage>().Where(x => x.ComplianceSubType == ComplianceSubType);
				if (sameItmes.Count() > 1)
				{
					result = ZBool.True;
				}
			}
			return result;
		}

		internal ComplianceDocumentImageCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ComplianceDocumentImageCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new ComplianceDocumentImageCollection();
				}
			}
		}

		public void ValidateCountry()
		{
			CountryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryInfo, (IMultilingualString)ResString.GetMultilingualString("0A64AD23-CBE1-4D1D-B11F-E29FD498B462", "Country/Region"));
			if (!CountryInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(CountryInfo, Lookups.CountryList);
			}

			if (!ComplianceSubTypeInfo.HasErrors() && CheckComplianceSubTypeExists())
			{
				ComplianceSubTypeInfo.AddError(ErrorMessage);
			}
		}

		#endregion

		#region Image

		public Image Image
		{
			get
			{
				if (!isImageSet && (image == null || image.IsDisposed()))
				{
					if (!imagePk.IsEmpty)
					{
						image = AccountingMasterFilesRegistry.Instance.ComplianceDocumentReceiptImageContainer.GetValueWithoutFallback(imagePk.ToGuid(), Guid.Empty, Guid.Empty);
					}
				}

				return image;
			}
			set
			{
				if (image != value)
				{
					shouldSaveImage = true;
				}

				isImageSet = true;
				image = value;
				RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateImage();
				}
			}
		}

		internal void DeleteImage()
		{
			if (!imagePk.IsEmpty)
			{
				((IRegistryItemInternals)AccountingMasterFilesRegistry.Instance.ComplianceDocumentReceiptImageContainer).DeleteValue(imagePk.ToGuid(), Guid.Empty, Guid.Empty);
			}
		}

		bool isImageSet;
		bool shouldSaveImage;
		ZGuid imagePk;
		Image image;

		#endregion

		public override bool Equals(object obj)
		{
			var other = obj as ComplianceDocumentImage;

			return other != null &&
				   other.Country == Country &&
				   other.ComplianceSubType == ComplianceSubType &&
				   other.Remark.Equals(Remark) &&
				   Utilities.IsImageEqual(other.Image, Image);
		}

		public override int GetHashCode()
		{
			var hashCode = HashCodeHelper.GetCompositeHashCode(new object[]
			{
				Country,
				ComplianceSubType,
				Remark,
			});

			return HashCodeHelper.GetCompositeHashCode(hashCode, GetImageHashCode());
		}

		int GetImageHashCode()
		{
			var img = Image;

			if (img != hashedImage)
			{
				imageHashCode = Utilities.GetImageContentsHashCode(img);
				hashedImage = img;
			}

			return imageHashCode;
		}

		int imageHashCode;
		Image hashedImage;

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			if (shouldSaveImage)
			{
				shouldSaveImage = false;

				if (Image == null)
				{
					DeleteImage();
					imagePk = Guid.Empty;
				}
				else
				{
					if (imagePk.IsEmpty)
					{
						imagePk = Guid.NewGuid();
					}
					AccountingMasterFilesRegistry.Instance.ComplianceDocumentReceiptImageContainer.SetValue(imagePk.ToGuid(), Guid.Empty, Guid.Empty, Image);
				}
			}

			writer.WriteElementString(Schema.Country, Country);
			writer.WriteElementString(Schema.ComplianceSubType, ComplianceSubType);
			writer.WriteElementString(Schema.Remark, Remark);
			writer.WriteElementString(Schema.ImagePK, imagePk.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			country = reader.ReadElementString(Schema.Country);
			complianceSubType = reader.ReadElementString(Schema.ComplianceSubType);
			remark = reader.ReadElementString(Schema.Remark);
			imagePk = new ZGuid(reader.ReadElementString(Schema.ImagePK));
		}

		#endregion

		#region Test
#if DEBUG
		internal ZGuid ImagePkForTest
		{
			get { return imagePk; }
			set { imagePk = value; }
		}

#endif
		#endregion
	}
}
