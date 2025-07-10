using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class CustomsDSBCreditorOverride : RegistryBusinessObjectTemplate
	{
		public CustomsDSBCreditorOverride()
		{
		}

		public CustomsDSBCreditorOverride(FallbackLevel fallbackLevel, BusinessObjectFactory factory, CustomsDSBCreditorOverrideCollection collection)
			: base(fallbackLevel, factory)
		{
			this.collection = collection;
		}

		readonly CustomsDSBCreditorOverrideCollection collection;

		public static class Schema
		{
			public const string DistrictOfficeCode = "DistrictOfficeCode";
			public const string CreditorPK = "CreditorPK";
		}

		#region DistrictOfficeCode

		[List(nameof(CustomsOfficeCodeList))]
		[MaxLength(3)]
		public ZString DistrictOfficeCode
		{
			get { return districtOfficeCode; }
			set
			{
				SetNonPersistentPropertyValue(DistrictOfficeCodeInfo, ref districtOfficeCode, value);

				if (!IsValidationSuspended)
				{
					ValidateDistrictOfficeCode();
				}
			}
		}
		ZString districtOfficeCode;

		public CodeDescriptionPairList CustomsOfficeCodeList
		{
			get { return ZARefCusCodeListTypes.GetCustomsOfficeList(CurrentFactory); }
		}

		public ZPropertyInfo DistrictOfficeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DistrictOfficeCode); }
		}

		void ValidateDistrictOfficeCode()
		{
			DistrictOfficeCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(DistrictOfficeCodeInfo, "District Office Code");
			ListValidation.ErrorIfInvalidCode(DistrictOfficeCodeInfo, CustomsOfficeCodeList);

			if (collection != null)
			{
				foreach (CustomsDSBCreditorOverride creditorMapping in collection)
				{
					if (creditorMapping != this && creditorMapping.DistrictOfficeCode == this.DistrictOfficeCode)
					{
						DistrictOfficeCodeInfo.AddError(DuplicateDistrictOfficeCode);
						break;
					}
				}
			}
		}

		public const string DuplicateDistrictOfficeCode = "You have already entered this district port code.";

		#endregion

		#region CreditorPK

		[List(nameof(Creditors))]
		public ZGuid CreditorPK
		{
			get { return creditorPK; }
			set
			{
				SetNonPersistentPropertyValue(CreditorPKInfo, ref creditorPK, value);

				if (!IsValidationSuspended)
				{
					ValidateCreditorPK();
				}
			}
		}
		ZGuid creditorPK;

		public ZPropertyInfo CreditorPKInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorPK); }
		}

		void ValidateCreditorPK()
		{
			CreditorPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CreditorPKInfo, "Creditor");
			ListValidation.ErrorIfInvalidPK(CreditorPKInfo, Creditors);
		}

		public CreditorCollection Creditors
		{
			get { return new CreditorCollection(CurrentFactory); }
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDistrictOfficeCode();
			ValidateCreditorPK();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomsDSBCreditorOverride(fallbackLevel, factory, null);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DistrictOfficeCode, DistrictOfficeCode);
			writer.WriteElementString(Schema.CreditorPK, CreditorPK.ToString());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			DistrictOfficeCode = reader.ReadElementString(Schema.DistrictOfficeCode);
			CreditorPK = new ZGuid(reader.ReadElementString(Schema.CreditorPK));
		}

		#endregion
	}
}
