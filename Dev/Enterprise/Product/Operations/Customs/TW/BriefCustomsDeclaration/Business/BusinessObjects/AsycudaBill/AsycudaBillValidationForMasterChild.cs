using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
	{
		public AsycudaBillValidationForMasterChild(AsycudaBill parent)
			: base(parent)
		{
			registrationNumberValidation = new ManifestRegistrationNumberValidation(parent);
		}
		readonly ManifestRegistrationNumberValidation registrationNumberValidation;

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override void CheckABL_GoodsLocation()
		{
			base.CheckABL_GoodsLocation();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_GoodsLocationInfo);
		}

		protected override void CheckABL_BillNumberCore()
		{
			var parent = Parent;
			if (parent.Header is AsycudaManifestHeader header && ((header.IsImport && header.IsAir) || header.IsExport))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_BillNumberInfo);
			}
		}

		static string ImporterString => Res.GetString("C2F7FE7A-FC4B-462E-9D5C-E425B19AE788", "Importer");
		static string ExporterString => Res.GetString("4D2C12B4-0B67-46F6-91F5-97AC6D06250E", "Exporter");
		static string IDTypeString => Res.GetString("1336BB1F-6127-4167-8C66-F8342227979D", "ID Type");
		static string IDString => Res.GetString("BEE31097-203E-415F-903C-E2FEF9E29805", "ID");

		protected override void CheckABL_ConsigneeName()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_ConsigneeNameInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ImporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ConsigneeStreet1()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_ConsigneeStreet1Info;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ImporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ConsigneeCity()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_ConsigneeCityInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ImporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ConsigneePostcode()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_ConsigneePostcodeInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ImporterString} {fieldName}");
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_RN_NKConsigneeCountryInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ImporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_ConsigneeRegNoTypeInfo;
				var typeList = Parent.ConsigneeRegNoTypes();
				var propertyDescription = $"{ImporterString} {IDTypeString}";
				if (propertyInfo.Value.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription);
				}
				else if (!typeList.Contains(Parent.ABL_ConsigneeRegNoType))
				{
					propertyInfo.AddMessageError(string.Format(Res.GetString("288766D6-7F2B-4BD3-BED5-EC07CE0F3EE1", "The entered {0} is invalid"), propertyDescription));
				}
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			var header = Parent.Header;
			if (header?.IsImport ?? false)
			{
				var propertyInfo = Parent.ABL_ConsigneeRegNoInfo;
				if (propertyInfo.Value.IsEmpty)
				{
					var propertyDescription = $"{ImporterString} {IDString}";
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription);
				}
				else
				{
					OrgCusCodeValidation.ValidateCustomsCode(registrationNumberValidation, Parent.ABL_RN_NKConsigneeCountry, Parent.ABL_ConsigneeRegNoType, Parent.ABL_ConsigneeRegNo, Parent.ABL_ConsigneeRegNoInfo);
				}
			}
		}

		protected override void CheckABL_ShipperName()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_ShipperNameInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ExporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ShipperStreet1()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_ShipperStreet1Info;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ExporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ShipperCity()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_ShipperCityInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ExporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_ShipperPostcodeInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ExporterString} {fieldName}");
			}
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_RN_NKShipperCountryInfo;
				var fieldName = MandatoryValidation.GetErrorFieldFromProperyInfo(propertyInfo);
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, $"{ExporterString} {fieldName}");
			}
		}

		protected override void CheckABL_ShipperRegNoType()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_ShipperRegNoTypeInfo;
				var typeList = Parent.ShipperRegNoTypes();
				var propertyDescription = $"{ExporterString} {IDTypeString}";
				if (propertyInfo.Value.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription);
				}
				else if (!typeList.Contains(Parent.ABL_ShipperRegNoType))
				{
					propertyInfo.AddMessageError(string.Format(Res.GetString("288766D6-7F2B-4BD3-BED5-EC07CE0F3EE1", "The entered {0} is invalid"), propertyDescription));
				}
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			var header = Parent.Header;
			if (header?.IsExport ?? false)
			{
				var propertyInfo = Parent.ABL_ShipperRegNoInfo;
				if (propertyInfo.Value.IsEmpty)
				{
					var propertyDescription = $"{ImporterString} {IDString}";
					MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, propertyDescription);
				}
				else
				{
					OrgCusCodeValidation.ValidateCustomsCode(registrationNumberValidation, Parent.ABL_RN_NKShipperCountry, Parent.ABL_ShipperRegNoType, Parent.ABL_ShipperRegNo, Parent.ABL_ShipperRegNoInfo);
				}
			}
		}

		protected override void CheckABL_CarrierReference()
		{
			base.CheckABL_CarrierReference();
			var parent = Parent;
			var carrierReference = parent.ABL_CarrierReference;
			if (!carrierReference.IsEmpty && carrierReference.Length != 4)
			{
				var propInfo = parent.ABL_CarrierReferenceInfo;
				propInfo.AddMessageError(Res.GetString("EDE014C1-960D-446E-AF89-BDCE3FD4BDFB", "{0} must be exactly 4 characters long", DataBoundResourceStrings.GetDataForProperty(propInfo).Caption));
			}
		}

		protected override void CheckMandatoryABL_E_ARV()
		{
			var parent = Parent;
			if (parent.Header is AsycudaManifestHeader header && header.IsImport)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_E_ARVInfo);
			}
		}

		protected override bool IsPortOfDischargesRequired => false;

		protected override bool IsPortOfLoadingRequired => false;

		protected override bool IsABL_E_DEPRequired => false;

		protected override bool ShouldCheckHasAsycudaCountry => false;
	}
}
