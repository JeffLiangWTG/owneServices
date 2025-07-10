using System.Collections.Generic;
using CargoWise.ComponentModel.Design;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public class BulkUpdateGUIInfo : RateEntryCollectionGUIInfo
	{
		public override string Category
		{
			get { return ""; }
		}

		public override List<ZGridColumnInfo> Columns => new List<ZGridColumnInfo>
		{
			IncludeInUpdate,
			EntryType,
			OrganisationCode,
			OrganisationName,
			RateCategory,
			Mode,
			Origin,
			Destination,
			Via,
			Supplier,
			Carrier("Lookups.ShippingProviders"),
			ControllingCustomer,
			Consignor,
			Consignee,
			ServiceLevel,
			CarrierServiceLevel,
			{ GatewayServiceLevel, IsIntercompanyTariff },
			{ ShipmentGatewayServiceLevel, IsIntercompanyTariff },
			CommodityCode,
			{ CommodityDescription, null, false },
			Container,
			RateStartDate,
			RateEndDate,
			ContractNumber,
			ContractNumberLinked,
			{ ShipmentConsolidationStatus, null, false }
		};

		ZTextBoxColumnStyleInfo EntryType()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).EntryType);
			var entryType = new ZTextBoxColumnStyleInfo();
			entryType.Caption = Res.GetString("52dc44ed-82db-4c01-a1d1-30aa2103e9ca", "Entry Type");
			entryType.ColumnName = "EntryType";
			ControlDpiScalingHelper.SetWidth(ref entryType, 100, true);

			return entryType;
		}

		ZTextBoxColumnStyleInfo OrganisationCode()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).Organisation);
			var organisationCode = new ZTextBoxColumnStyleInfo();
			organisationCode.CaptionResourceString = Res.GetData("6933a57a-7e97-476e-a248-2d4c596b2f24", "Org. Code", "Organization Code", "The Code for this Organization");
			organisationCode.ColumnName = "Organisation";
			organisationCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref organisationCode, 70, true);

			return organisationCode;
		}

		ZTextBoxColumnStyleInfo OrganisationName()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).Parent.TH_ClientFullName);
			var organisationName = new ZTextBoxColumnStyleInfo();
			organisationName.Caption = Res.GetString("B6A71524-C17A-4F07-9CB9-62868E475E84", "Organization Name");
			organisationName.ColumnName = "Parent+TH_ClientFullName";
			organisationName.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			ControlDpiScalingHelper.SetWidth(ref organisationName, 150, true);

			return organisationName;
		}

		protected ZCheckBoxColumnStyleInfo IncludeInUpdate()
		{
			CompileTimeCheckBindingMember.Check(((RateEntry)(null)).IncludeInUpdate);
			var includeInUpdate = new ZCheckBoxColumnStyleInfo();
			includeInUpdate.Caption = Res.GetString("ec21b20a-f6de-45e5-b10a-22e0cd58a9da", "Update");
			includeInUpdate.ColumnName = "IncludeInUpdate";
			ControlDpiScalingHelper.SetWidth(ref includeInUpdate, 50, true);

			return includeInUpdate;
		}
	}
}
