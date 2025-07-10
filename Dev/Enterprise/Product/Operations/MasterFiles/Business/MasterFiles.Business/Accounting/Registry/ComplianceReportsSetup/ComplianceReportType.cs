using System;
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
	[CodeProperty(Schema.ReportType), DescriptionProperty(Schema.ReportTypeDescription)]
	public class ComplianceReportType : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ReportType = "ReportType";
			public const string ReportTypeDescription = "ReportTypeDescription";
			public const string ReportTypeCategories = "ReportTypeCategories";
			public const string CountryCode = "CountryCode";
		}

		#endregion

		#region Construction

		public ComplianceReportType() { }

		public ComplianceReportType(FallbackLevel fallbackLevel) : base(fallbackLevel) { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public ComplianceReportType(ZString reportType, ZString reportTypeDescription, ComplianceReportsSetupCategoryCollection categories, ZString countryCode)
		{
			AddDefaultReportTypes(reportType, reportTypeDescription, categories, countryCode);
		}

		public void AddDefaultReportTypes(ZString reportType, ZString reportTypeDesc, ComplianceReportsSetupCategoryCollection categories, ZString countryCode)
		{
			if (!reportType.IsEmpty)
			{
				ReportType = reportType;
			}

			fCountryCode = countryCode;
			ReportTypeDescription = reportTypeDesc;
			fReportTypeCategories = categories;
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportType(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			ComplianceReportType complianceReportTypeClone = (ComplianceReportType)clone;
			if (fReportTypeCategories != null)
			{
				complianceReportTypeClone.fReportTypeCategories = (ComplianceReportsSetupCategoryCollection)fReportTypeCategories.Clone(complianceReportTypeClone.CurrentFallbackLevel, complianceReportTypeClone.Factory);
				complianceReportTypeClone.fReportTypeCategories.CurrentFallbackLevel = fReportTypeCategories.CurrentFallbackLevel;
				complianceReportTypeClone.fReportTypeCategories.ReportType = ReportType;
				complianceReportTypeClone.RegisterEditableChildObject(complianceReportTypeClone.fReportTypeCategories);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateReportType();
		}

		ZString fCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		public ZString CountryCode
		{
			get { return fCountryCode; }
#if DEBUG
			set { fCountryCode = value; }
#endif
		}

		#region Bound Properties

		#region Report Type

		ZString fReportType = ZString.Empty;

		[MaxLength(3)]
		public ZString ReportType
		{
			get { return fReportType; }
			set
			{
				CheckMaximumLength(ReportTypeInfo, value);
				SetNonPersistentPropertyValue(ReportTypeInfo, ref fReportType, value);
				ReportTypeCategories.RefreshBinding();
				ReportTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateReportType();
				}
			}
		}

		public ZPropertyInfo ReportTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ReportType); }
		}

		void ValidateReportType()
		{
			ReportTypeInfo.ClearAllNotifications();
			ClearRowNotifications();
			MandatoryValidation.CheckEntered(ReportTypeInfo, Res.GetString("34563e2f-abf1-44e5-b3a7-d7df7ecc5123", "Report Type"));
			CheckRecordIsValid();
		}

		void CheckRecordIsValid()
		{
			foreach (BusinessObjectCollection collection in ParentCollections)
			{
				foreach (ComplianceReportType item in collection)
				{
					if (item != this &&
						item.ReportType == ReportType)
					{
						ZString duplicateMessage = Res.GetString("2274CFA3-F043-40D2-B018-23716B3BE9FC", "Duplicate Report Type");
						AddRowError(duplicateMessage);
						break;
					}
				}

				var complianceReportTypeCollection = (collection as ComplianceReportTypeCollection);

				if (!HasRowErrors && complianceReportTypeCollection != null && complianceReportTypeCollection.RegistryName.ToString().Equals("ComplianceReportsSetupsUserDefined", StringComparison.OrdinalIgnoreCase) &&
					complianceReportTypeCollection.DefaultReportTypes.ContainsKey(ReportType))
				{
					ZString duplicateMessage = Res.GetString("B45AD5A2-604B-40DE-B396-A1211621B738", "The report type cannot be {0}, because these are system default Settings.", complianceReportTypeCollection.DefaultReportTypesAsString);
					AddRowError(duplicateMessage);
				}
			}
		}

		#endregion

		#region Report Type Description

		ZString reportTypeDescription = ZString.Empty;

		[MaxLength(256)]
		public ZString ReportTypeDescription
		{
			get { return reportTypeDescription; }
			set
			{
				SetNonPersistentPropertyValue(ReportTypeDescriptionInfo, ref reportTypeDescription, value, false);
			}
		}

		public ZPropertyInfo ReportTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ReportTypeDescription); }
		}

		#endregion

		#region Report Type Categories

		ComplianceReportsSetupCategoryCollection fReportTypeCategories;
		public ComplianceReportsSetupCategoryCollection ReportTypeCategories
		{
			get
			{
				if (fReportTypeCategories == null)
				{
					fReportTypeCategories = new ComplianceReportsSetupCategoryCollection(ReportType, CountryCode);
					RegisterEditableChildObject(fReportTypeCategories);
				}
				fReportTypeCategories.CurrentFallbackLevel = CurrentFallbackLevel;
				return fReportTypeCategories;
			}
		}

		ZXmlSerializer fReportTypeCategoriesSerialiser;
		ZXmlSerializer ReportTypeCategoriesSerialiser
		{
			get
			{
				return fReportTypeCategoriesSerialiser ?? (fReportTypeCategoriesSerialiser = ZXmlSerializer.New(typeof(ComplianceReportsSetupCategoryCollection)));
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ReportType, ReportType);
			writer.WriteElementString(Schema.ReportTypeDescription, ReportTypeDescription);
			ReportTypeCategoriesSerialiser.Serialize(writer, ReportTypeCategories);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReportType = reader.ReadElementString(Schema.ReportType);
			ReportTypeDescription = (NoResString)reader.ReadElementString(Schema.ReportTypeDescription);
			fReportTypeCategories = (ComplianceReportsSetupCategoryCollection)ReportTypeCategoriesSerialiser.Deserialize(reader);
			RegisterEditableChildObject(fReportTypeCategories);
		}

		#endregion
	}
}
