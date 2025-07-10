using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class OrgsEvaluatedForCreditControl : ChargeGroupSetting
	{
		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string OrganizationType = "OrganizationType";
			public const string INCOTerm = "INCOTerm";
			public const string FreightPaymentTerm = "FreightPaymentTerm";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrgsEvaluatedForCreditControl();
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel, BusinessObjectFactory factory)
		{
			clone.CurrentFallbackLevel = currentFallbackLevel;
		}

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(OrgsEvaluatedForCreditControlCollection));
				return parentCollection != null ? parentCollection.Cast<OrgsEvaluatedForCreditControl>().ToArray() : Array.Empty<OrgsEvaluatedForCreditControl>();
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganizationType();
		}

		public new OrgsEvaluatedForCreditControlValidation Validation
		{
			get { return (OrgsEvaluatedForCreditControlValidation)base.Validation; }
		}

		protected override JobConfigurationSelectorValidation GetNewValidation()
		{
			return new OrgsEvaluatedForCreditControlValidation(this);
		}

		#endregion

		#region Properties

		#region OrganizationType

		[MaxLength(3)]
		[List("OrganizationTypeList")]
		public ZString OrganizationType
		{
			get { return OrganizationTypeInfo.ReadOnly ? ZString.Empty : fOrganizationType; }
			set
			{
				CheckMaximumLength(OrganizationTypeInfo, value);
				SetNonPersistentPropertyValue(OrganizationTypeInfo, ref fOrganizationType, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganizationType();
				}
			}
		}

		public ZPropertyInfo OrganizationTypeInfo
		{
			get { return GetZPropertyInfo(Schema.OrganizationType); }
		}

		internal bool OrganizationType_ReadOnly
		{
			get { return !HasOrganizationTypes; }
		}

		public CodeDescriptionPairList OrganizationTypeList
		{
			get { return OrgsEvaluatedForCreditControlLookups.OrganizationTypeList; }
		}

		void ValidateOrganizationType()
		{
			OrganizationTypeInfo.ClearAllNotifications();
			Validation.ValidateOrganizationsType();
		}

		ZString fOrganizationType;

		#endregion

		#region INCOTerm

		[MaxLength(3)]
		[List("INCOTermList")]
		[BusinessObjectTestExclude]
		public ZString INCOTerm
		{
			get { return INCOTermInfo.ReadOnly ? ZString.Empty : incoTerm; }
			set
			{
				CheckMaximumLength(INCOTermInfo, value);
				SetNonPersistentPropertyValue(INCOTermInfo, ref incoTerm, value);

				if (INCOTermList.GetAllCodes().Where(x => x != INCOTermCodes.All).Contains(INCOTerm.ToString(), StringComparer.OrdinalIgnoreCase))
				{
					FreightPaymentTerm = FreightPaymentTermCodes.All;
				}

				if (!IsValidationSuspended)
				{
					ValidateINCOTerm();
				}
			}
		}

		public ZPropertyInfo INCOTermInfo
		{
			get { return GetZPropertyInfo(Schema.INCOTerm); }
		}

		internal bool INCOTerm_ReadOnly
		{
			get { return !HasINCOTerms; }
		}

		public CodeDescriptionPairList INCOTermList
		{
			get { return OrgsEvaluatedForCreditControlLookups.INCOTermList; }
		}

		void ValidateINCOTerm()
		{
			INCOTermInfo.ClearAllNotifications();
			Validation.ValidateINCOTerm();
		}

		ZString incoTerm;

		#endregion

		#region FreightPaymentTerm

		[MaxLength(10)]
		[List("FreightPaymentTermList")]
		[BusinessObjectTestExclude]
		public ZString FreightPaymentTerm
		{
			get { return FreightPaymentTermInfo.ReadOnly ? ZString.Empty : freightPaymentTerm; }
			set
			{
				CheckMaximumLength(FreightPaymentTermInfo, value);
				SetNonPersistentPropertyValue(FreightPaymentTermInfo, ref freightPaymentTerm, value);

				if (FreightPaymentTermList.GetAllCodes().Where(x => x != FreightPaymentTermCodes.All).Contains(FreightPaymentTerm.ToString(), StringComparer.OrdinalIgnoreCase))
				{
					INCOTerm = INCOTermCodes.All;
				}

				if (!IsValidationSuspended)
				{
					ValidateFreightPaymentTerm();
				}
			}
		}

		public ZPropertyInfo FreightPaymentTermInfo
		{
			get { return GetZPropertyInfo(Schema.FreightPaymentTerm); }
		}

		internal bool FreightPaymentTerm_ReadOnly
		{
			get { return !HasFreightPaymentTerms; }
		}

		public CodeDescriptionPairList FreightPaymentTermList
		{
			get { return OrgsEvaluatedForCreditControlLookups.FreightPaymentTermList; }
		}

		void ValidateFreightPaymentTerm()
		{
			FreightPaymentTermInfo.ClearAllNotifications();
			Validation.ValidateFreightPaymentTerm();
		}

		ZString freightPaymentTerm;

		#endregion

		#region Use these properties to avoid creating Organisation Types Code Description Lists as they touch Resource Strings and we cannot touch them when making Default Value.

		public bool HasOrganizationTypes
		{
			get { return OrgsEvaluatedForCreditControlLookups.HasOrganisationTypes(); }
		}

		public bool HasAllDebtors
		{
			get { return OrgsEvaluatedForCreditControlLookups.HasAllDebtors(); }
		}

		public bool HasINCOTerms
		{
			get { return OrgsEvaluatedForCreditControlLookups.HasINCOTerms(); }
		}

		public bool HasFreightPaymentTerms
		{
			get { return OrgsEvaluatedForCreditControlLookups.HasFreightPaymentTerms(); }
		}

		#endregion

		#endregion

		#region OrgsEvaluatedForCreditControlLookups

		public OrgsEvaluatedForCreditControlLookups OrgsEvaluatedForCreditControlLookups
		{
			get { return (OrgsEvaluatedForCreditControlLookups)ChargeGroupSettingLookups; }
		}

		protected override JobConfigurationSelectorLookups GetNewLookups()
		{
			return new OrgsEvaluatedForCreditControlLookups(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrganizationType, OrganizationType);
			writer.WriteElementString(Schema.INCOTerm, INCOTerm);
			writer.WriteElementString(Schema.FreightPaymentTerm, FreightPaymentTerm);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			OrganizationType = reader.ReadElementString(Schema.OrganizationType);
			INCOTerm = reader.ReadElementString(Schema.INCOTerm);
			FreightPaymentTerm = reader.ReadElementString(Schema.FreightPaymentTerm);
		}

		#endregion
	}
}
