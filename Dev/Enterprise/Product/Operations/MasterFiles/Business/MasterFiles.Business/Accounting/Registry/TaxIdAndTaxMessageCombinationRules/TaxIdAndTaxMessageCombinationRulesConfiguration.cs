using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxIdAndTaxMessageCombinationRulesConfiguration : RegistryBusinessObjectTemplate
	{
		#region schema

		public abstract class Schema
		{
			public const string ValidationOption = "ValidationOption";
		}

		#endregion

		public TaxIdAndTaxMessageCombinationRulesConfiguration()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new TaxIdAndTaxMessageCombinationRulesConfiguration();
			result.CurrentFallbackLevel = fallbackLevel;
			return result;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (TaxIdAndTaxMessageCombinationRulesConfiguration)clone;
			if (TaxIdAndTaxMessageCombinationRulesCollection != null)
			{
				castedClone.taxIdAndTaxMessageCombinationRulesCollection = (TaxIdAndTaxMessageCombinationRulesCollection)TaxIdAndTaxMessageCombinationRulesCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.TaxIdAndTaxMessageCombinationRulesCollection);
			}
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			ValidationOption = DefaultValidationOption;
		}

		protected override FallbackLevel CurrentFallbackLevelCore
		{
			get => base.CurrentFallbackLevelCore;
			set
			{
				base.CurrentFallbackLevelCore = value;
				if (taxIdAndTaxMessageCombinationRulesCollection != null)
				{
					taxIdAndTaxMessageCombinationRulesCollection.CurrentFallbackLevel = value;
					foreach (TaxIdAndTaxMessageCombinationRules item in taxIdAndTaxMessageCombinationRulesCollection)
					{
						item.CurrentFallbackLevel = value;
					}
				}
			}
		}

		public TaxIdAndTaxMessageCombinationRulesCollection TaxIdAndTaxMessageCombinationRulesCollection
		{
			get
			{
				if (taxIdAndTaxMessageCombinationRulesCollection == null)
				{
					taxIdAndTaxMessageCombinationRulesCollection = new TaxIdAndTaxMessageCombinationRulesCollection();
					taxIdAndTaxMessageCombinationRulesCollection.CurrentFallbackLevel = CurrentFallbackLevel;
					RegisterEditableChildObject(taxIdAndTaxMessageCombinationRulesCollection);
				}
				return taxIdAndTaxMessageCombinationRulesCollection;
			}
		}
		TaxIdAndTaxMessageCombinationRulesCollection taxIdAndTaxMessageCombinationRulesCollection;

		ZXmlSerializer TaxIdAndTaxMessageCombinationRulesCollectionSerialiser
			=> taxIdAndTaxMessageCombinationRulesCollectionSerialiser ?? (taxIdAndTaxMessageCombinationRulesCollectionSerialiser = ZXmlSerializer.New(typeof(TaxIdAndTaxMessageCombinationRulesCollection)));

		ZXmlSerializer taxIdAndTaxMessageCombinationRulesCollectionSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ValidationOption, ValidationOption);
			TaxIdAndTaxMessageCombinationRulesCollectionSerialiser.Serialize(writer, TaxIdAndTaxMessageCombinationRulesCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ValidationOption = reader.ReadElementString(Schema.ValidationOption);
			if (string.IsNullOrEmpty(ValidationOption))
			{
				ValidationOption = DefaultValidationOption;
			}
			taxIdAndTaxMessageCombinationRulesCollection = (TaxIdAndTaxMessageCombinationRulesCollection)TaxIdAndTaxMessageCombinationRulesCollectionSerialiser.Deserialize(reader);
			taxIdAndTaxMessageCombinationRulesCollection.Sort(TaxIdAndTaxMessageCombinationRules.Schema.LineType);
			RegisterEditableChildObject(TaxIdAndTaxMessageCombinationRulesCollection);
		}

		#region Validation Option

		ZString validationOption;

		[MaxLength(3)]
		[List("ValidationOptionsList")]
		public ZString ValidationOption
		{
			get { return validationOption; }
			set
			{
				SetNonPersistentPropertyValue(ValidationOptionInfo, ref validationOption, value);
				if (!IsValidationSuspended)
				{
					ValidateValidationOption();
				}
			}
		}

		public ZPropertyInfo ValidationOptionInfo
		{
			get { return GetZPropertyInfo(Schema.ValidationOption, "Validation Option"); }
		}

		public void ValidateValidationOption()
		{
			ValidationOptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ValidationOptionInfo);
			ListValidation.ErrorIfInvalidCode(ValidationOptionInfo, ValidationOptionsList);
		}

		CodeDescriptionPairList validationOptionsList;
		public CodeDescriptionPairList ValidationOptionsList
		{
			get
			{
				if (validationOptionsList == null)
				{
					validationOptionsList = new CodeDescriptionPairList();
					validationOptionsList.AddPair(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage, Res.GetString("f9c972fe-e949-40b4-85fe-7dc9820b98b3", "Limit permitted Tax Message"));
					validationOptionsList.AddPair(AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxID, Res.GetString("b9aad925-8097-471a-9de6-781211b8386c", "Limit permitted Tax IDs"));
				}
				return validationOptionsList;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateValidationOption();
		}

		string DefaultValidationOption => AccountingMasterFilesConstants.TaxIDAndTaxMessageValidationOption.TaxMessage;

		#endregion
	}
}
