using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	[CodeProperty(CertificateType.Schema.Code), DescriptionProperty(CertificateType.Schema.Description)]
	public class CertificateType : AutoCertificateType, ICodeDescription
	{
		public CertificateType()
			: base()
		{
		}

		public CertificateType(CertificateTypeCollection parent)
			: base()
		{
			Parent = parent;
		}

		public void SetupValues(ZString code, ZString description, bool isMandatory, bool isUnique, bool isSystem, ZString alertType)
		{
			Code = code;
			Description = description;
			IsMandatory = isMandatory;
			IsUnique = isUnique;
			IsSystem = isSystem;
			AlertType = alertType;
		}

		#region Overrides

		[MaxLength(Schema.DescriptionMaxLength)]
		public MultilingualString DescriptionMultilingual
		{
			get => descriptionMultilingual ?? (NoResString)"";
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}

				CheckMaximumLength(DescriptionMultilingualInfo, value.GetUnresolvedString());
				SetNonPersistentPropertyValue(DescriptionMultilingualInfo, ref descriptionMultilingual, value, false);
				DescriptionMultilingualInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		MultilingualString descriptionMultilingual;

		public ZPropertyInfo DescriptionMultilingualInfo => GetZPropertyInfo(Schema.Description);

		[MaxLength(Schema.DescriptionMaxLength)]
		public override ZString Description
		{
			get => DescriptionMultilingual.GetUnresolvedString();
			set => DescriptionMultilingual = (NoResString)value;
		}

		public override void Delete()
		{
			if (this.IsSystem)
			{
				throw new CannotDeleteException(Res.GetString("9c1d9346-6b04-48ff-941e-ed972c172272", "You cannot delete or disable this type."));
			}
			else
			{
				base.Delete();
			}
		}

		public override bool ReadOnly
		{
			get { return this.IsSystem || base.ReadOnly; }
			set { base.ReadOnly = this.IsSystem || value; }
		}

		[List(nameof(AlertTypeList))]
		public override ZString AlertType
		{ get => base.AlertType; set => base.AlertType = value; }

		#endregion

		#region Validation

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);

			if (Parent.IsDuplicated(Code))
			{
				CodeInfo.AddError(Res.GetString("fbd77537-4009-44e0-8a47-b72efb3c82c2", "Duplicate Code is entered."));
			}
		}

		public override void ValidateDescription()
		{
			base.ValidateDescription();
			MandatoryValidation.CheckEntered(DescriptionInfo);
		}

		public override void ValidateAlertType()
		{
			AlertTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AlertTypeInfo);
			ListValidation.ErrorIfInvalidCode(AlertTypeInfo);
		}

		#endregion

		#region AlertTypeList

		public CodeDescriptionPairList AlertTypeList => CurrentFactory.GetCachedValue("9944a75f-2f3d-496f-b48f-d0dc2d4e3f45", () => new AlertTypeList());

		#endregion

		#region ICodeDescription Members

		string ICodeDescription.Code => Code;

		string ICodeDescription.Description => DescriptionMultilingual;

		object ICodeDescription.PK => null;

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CertificateType(Parent);
		}

		readonly CertificateTypeCollection Parent;

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var target = (CertificateType)(clone);
			target.DescriptionMultilingual = DescriptionMultilingual;
		}

		#endregion
	}
}
