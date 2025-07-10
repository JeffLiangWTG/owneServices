using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public sealed class AccSurchargeApplication : AutoAccSurchargeApplication, IBusinessObjectLogging
	{
		public const string ALL = "ALL";

		public AccSurchargeApplication(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Logging

		public ZString GetLogReference()
		{
			if (this.GetLogStatusCode() == Events.EditedARecord.Code)
			{
				var originalValues = $"Surcharge Application {BusinessObjectLoggingExtension.DeleteLogStatus}: Job Type:{ASP_JobTypeInfo.OriginalValue}, Supply Type:{ASP_SupplyTypeInfo.OriginalValue}, Organization Country or Zone:{ASP_HomeCountryOrZoneInfo.OriginalValue}, Organization Category:{ASP_OrganizationCategoryInfo.OriginalValue}, Surcharge Code:{ASP_ASC_NKSurchargeCodeInfo.OriginalValue}" + "\r\n";

				var currentValues = $"Surcharge Application {BusinessObjectLoggingExtension.AddedLogStatus}: Job Type:{ASP_JobType}, Supply Type:{ASP_SupplyType}, Organization Country or Zone:{ASP_HomeCountryOrZone}, Organization Category:{ASP_OrganizationCategory}, Surcharge Code:{ASP_ASC_NKSurchargeCode}";

				return originalValues + currentValues;
			}
			else
			{
				return $"Surcharge Application {this.GetLogStatusShortDescription()}: Job Type:{ASP_JobType}, Supply Type:{ASP_SupplyType}, Organization Country or Zone:{ASP_HomeCountryOrZone}, Organization Category:{ASP_OrganizationCategory}, Surcharge Code:{ASP_ASC_NKSurchargeCode}";
			}
		}

		#endregion

		public override void OnSaving()
		{
			AccSurchargeApplicationLogHelper.AddLog(Company, this);
			base.OnSaving();
		}

		[List("Lookups.JobTypes")]
		public override ZString ASP_JobType { get => base.ASP_JobType; set => base.ASP_JobType = value; }

		[List("Lookups.SupplyTypes")]
		public override ZString ASP_SupplyType { get => base.ASP_SupplyType; set => base.ASP_SupplyType = value; }

		[List("Lookups.Locations")]
		public override ZString ASP_HomeCountryOrZone { get => base.ASP_HomeCountryOrZone; set => base.ASP_HomeCountryOrZone = value; }

		[List("Lookups.OrganisationCategoryList")]
		public override ZString ASP_OrganizationCategory { get => base.ASP_OrganizationCategory; set => base.ASP_OrganizationCategory = value; }

		#region ASP_ASC_NKSurchargeCode

		[List("Lookups.SurchargeCodes")]
		public override ZString ASP_ASC_NKSurchargeCode { get => base.ASP_ASC_NKSurchargeCode; set => base.ASP_ASC_NKSurchargeCode = value; }

		[ResourceStringData("00EB2D87-8285-471C-99EA-F04694FADEBF", Caption = "Surcharge Code Description")]
		public ZString SurchargeCodeDescription
		{
			get
			{
				var filter = new ZQuery(AccSurchargeConfigurationSchema.ASC_GC_Company, ASP_GC_Company);
				var surchargeCodeFilter = new ZQuery(AccSurchargeConfigurationSchema.ASC_Code, ASP_ASC_NKSurchargeCode);
				filter.AddToFilter(surchargeCodeFilter, JoinCondition.And);
				var surchargeConfiguration = Factory.LoadTop1<AccSurchargeConfiguration>(filter);
				return surchargeConfiguration?.ASC_Description ?? ZString.Empty;
			}
		}

		#endregion

		[List("Lookups.Locations")]
		public override ZString ASP_PlaceOfSupply { get => base.ASP_PlaceOfSupply; set => base.ASP_PlaceOfSupply = value; }

		[SuppressMessage("CodeQuality", "IDE0051", Justification="Used by reflection; cannot be protected because class is sealed.")]
		bool ASP_AT_ReadOnly
		{
			get
			{
				bool isTaxIdSupported = GetCountryFactory() is ISurchargeApplicationConfigurationUsesTaxId;

				return !isTaxIdSupported;
			}
		}

		IAccountingCountryFactory GetCountryFactory() => ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(CountryCode);

		internal ZString CountryCode => Company?.GC_RN_NKCountryCode ?? ZString.Empty;

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ASP_JobType = "ALL";
		}

#endif
	}
}
