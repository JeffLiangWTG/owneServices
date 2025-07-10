using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[CodeProperty(nameof(RefComplianceCommodityAlertCode)), DescriptionProperty(Schema.RCR_AlertName)]
	public class RefComplianceCommodityAlert : AutoRefComplianceCommodityAlert
	{
		public RefComplianceCommodityAlert(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString RefComplianceCommodityAlertCode => $"{RCR_CountryRegion}-{RCR_TradeDirection}";
		public ZPropertyInfo RefComplianceCommodityAlertCodeInfo => GetZPropertyInfo(nameof(RefComplianceCommodityAlertCode));

		[List(nameof(Lookups) + "." + nameof(Lookups.TradeDirections))]
		public override ZString RCR_TradeDirection { get => base.RCR_TradeDirection; set => base.RCR_TradeDirection = value; }

		[List(nameof(Lookups) + "." + nameof(Lookups.AlertTypes))]
		public override ZString RCR_AlertType { get => base.RCR_AlertType; set => base.RCR_AlertType = value; }

		public ZString AlertType => Lookups.AlertTypes.GetDescriptionFromCode(RCR_AlertType);

		public ZDateTime LastEditedTime => RCR_SystemLastEditTimeUtc.ToLocalBranchTime();

		public ZDateTime CreatedTime => RCR_SystemCreateTimeUtc.ToLocalBranchTime();

		public ZString CountryRegionDetails => RCR_CountryRegion + " - " + (RCR_CountryRegion == Core.Constants.CountryCodes.EuropeanUnion ? Res.GetString("D16BBD5A-547B-40EC-A42B-F6BFF847A269", "European Union") : Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, RCR_CountryRegion)?.Description);

		public ZString SourceURL => sourceURL ??= GetSourceURL();

		ZString? sourceURL;

		ZString GetSourceURL()
		{
			var baseURL = DataRegistry.Instance.BorderWiseWebAddress;
			return $"{(baseURL != null ? baseURL.TrimEnd('/') : "")}{RCR_SourceURL}";
		}

		[List(nameof(Lookups) + "." + nameof(Lookups.CommodityRiskStatus))]
		public override ZString RCR_CommodityRiskStatus { get => base.RCR_CommodityRiskStatus; set => base.RCR_CommodityRiskStatus = value; }

		public override string CanCancel()
		{
			return RefComplianceList.PreventionMessage;
		}

		public override string CanReactivate()
		{
			return RefComplianceList.PreventionMessage;
		}

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			var shouldBeReadOnly = true;

			if (property.Name == RefComplianceCommodityAlertSchema.RCR_CommodityRiskStatus.Name)
			{
				shouldBeReadOnly = !Env.Security.RefComplianceCommodityAlertEdit.IsAllowed;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#region For Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			RCR_AlertType = "NOM";
			RCR_AlertCode = "DUMMYCODE";
			RCR_AlertName = "DUMMY NAME";
			RCR_CountryRegion = "EU";
			RCR_PublishYear = 2025;
			RCR_TradeDirection = "IMP";
		}

#endif
		#endregion
	}
}
