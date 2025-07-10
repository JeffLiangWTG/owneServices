using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefZoneHeaderValidation : AutoRefZoneHeaderValidation
	{
		public RefZoneHeaderValidation(AutoRefZoneHeader parent) : base(parent)
		{
		}

		#region FZ_Code

		protected override void CheckFZ_Code()
		{
			base.CheckFZ_Code();
			MandatoryValidation.CheckEntered(Parent.FZ_CodeInfo);

			if (Parent.FZ_Code.Length != 4)
			{
				Parent.FZ_CodeInfo.AddError(Res.GetString("ddadac29-18bd-4157-8d0e-41fec23d8c2d", "Zone Code must be 4 characters in length."));
			}
			if (!IsFieldValueUnique(RefZoneHeaderSchema.FZ_Code, Parent.FZ_Code))
			{
				Parent.FZ_CodeInfo.AddError(Res.GetString("ba6128b3-1ece-4e2d-a199-e1192fa334ea", "Zone Code must be unique."));
			}
		}

		#endregion

		#region FZ_Description

		protected override void CheckFZ_Description()
		{
			base.CheckFZ_Description();
			MandatoryValidation.CheckEntered(Parent.FZ_DescriptionInfo, Res.GetString("df62791d-f693-4fa3-8f33-a7669f3d21d9", "Zone Description"));
			TranslatableDataFieldAttribute.Validate(Parent.FZ_DescriptionInfo);
			if (!IsFieldValueUnique(RefZoneHeaderSchema.FZ_Description, Parent.FZ_Description))
			{
				Parent.FZ_DescriptionInfo.AddError(Res.GetString("1e8575f6-7c62-486c-90ec-1ca6edc76daf", "Zone Description must be unique."));
			}
		}

		#endregion

		#region FZ_ZoneType

		protected override void CheckFZ_ZoneType()
		{
			base.CheckFZ_ZoneType();
			MandatoryValidation.CheckEntered(Parent.FZ_ZoneTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FZ_ZoneTypeInfo, Parent.Lookups.ZoneTypes);

			var hasChanges = !Parent.IsInDatabase || Parent.FZ_ZoneTypeInfo.HasChanges;
			if (hasChanges && Parent.FZ_ZoneType == ZoneTypeCodeDescriptionPair.WiseRatesOcean.Code)
			{
				var errorMessage = Res.GetString("67a8f937-6535-47cb-b8ab-998d6ed4136f", "'{0}' code cannot be manually selected.", ZoneTypeCodeDescriptionPair.WiseRatesOcean.Code);
				Parent.FZ_ZoneTypeInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region FZ_ZoneMode

		protected override void CheckFZ_ZoneMode()
		{
			base.CheckFZ_ZoneMode();
			MandatoryValidation.CheckEntered(Parent.FZ_ZoneModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FZ_ZoneModeInfo, Parent.Lookups.ZoneModes);
		}

		#endregion

		#region FZ_OH_RelatedParty

		protected override void CheckFZ_OH_RelatedParty()
		{
			if (Parent.FZ_ZoneType == ZoneTypeCodeDescriptionPair.OriginGateway.Code
				|| Parent.FZ_ZoneType == ZoneTypeCodeDescriptionPair.DestinationGateway.Code
				|| Parent.FZ_ZoneType == ZoneTypeCodeDescriptionPair.HVLVGateway.Code)
			{
				MandatoryValidation.CheckEntered(Parent.FZ_OH_RelatedPartyInfo, Res.GetString("b4228fb8-511e-8bbb-4c1a-2ba06cf2a2c6", "Gateway"));
			}
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUNLOCOsIfRequired();
		}

		#region Implementation

		bool IsFieldValueUnique(SchemaStringColumn fieldName, ZString valueToFind)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(RefZoneHeader));
			query.AddToFilter(RefZoneHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.NotEqual, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
			query.AddToFilter(fieldName, SQLComparisonOperator.Equal, valueToFind);
			query.IgnoreActiveFilter = true;
			RefZoneHeader otherZone = Parent.Factory.LoadTop1<RefZoneHeader>(query);

			return (otherZone == null);
		}

		void ValidateUNLOCOsIfRequired()
		{
			var header = Parent as RefZoneHeader;
			header.UNLOCOs.ForEach(unloco => unloco.RemoveRowError(IsDuplicateErrorString));
			if (Parent.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.HVLVGateway)
			{
				CheckHVLVGatewayUNLOCOUniquenessPerCountry();
			}
		}

		void CheckHVLVGatewayUNLOCOUniquenessPerCountry()
		{
			var header = Parent as RefZoneHeader;
			var query = new ZQuery();
			query.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.Equal, header.FZ_ZoneType);
			query.AddToFilter(RefZoneHeaderSchema.FZ_OH_RelatedParty, SQLComparisonOperator.Equal, header.FZ_OH_RelatedParty);

			var allZoneHeadersBelongToCurrentGatewayOrg = Parent.Factory.Load<RefZoneHeader>(query);
			var allUNLOCOs = allZoneHeadersBelongToCurrentGatewayOrg.SelectMany(zoneHeader => zoneHeader.UNLOCOs).OfType<RefUNLOCO>();
			var countriesWithMultipleUNLOCOs = allUNLOCOs.GroupBy(x => x.RL_RN_NKCountryCode)
				.Where(group => group.Count() > 1).Select(group => group.Key);
			if (countriesWithMultipleUNLOCOs.Any())
			{
				var currentUNLOCOs = header.UNLOCOs.OfType<RefUNLOCO>();
				foreach (var duplicateUNLOCO in currentUNLOCOs.Where(unloco => countriesWithMultipleUNLOCOs.Contains(unloco.RL_RN_NKCountryCode)))
				{
					duplicateUNLOCO.AddRowError(IsDuplicateErrorString);
				}
			}
		}

		#endregion

		internal static string IsDuplicateErrorString => Res.GetString("50f33a9f-0241-4b45-a93f-e9aa44220c29", "HVLV Gateway does not support multiple UNLOCOs with the same country. Please ensure there is only one UNLOCO per country.");
	}
}
