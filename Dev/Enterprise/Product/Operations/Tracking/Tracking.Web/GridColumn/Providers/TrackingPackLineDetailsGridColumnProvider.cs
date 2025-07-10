using Enterprise.Environment;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingPackLineDetailsGridColumnProvider : GridColumnProvider
	{
		public TrackingPackLineDetailsGridColumnProvider(bool isShipmentPackLines, bool hideCustomsCodeAndPrice, ZTemplateColumn multipleProductsColumn)
			: base()
		{
			this.isShipmentPackLines = isShipmentPackLines;
			this.hideCustomsCodeAndPrice = hideCustomsCodeAndPrice;
			this.multipleProductsColumn = multipleProductsColumn;
		}

		readonly bool hideCustomsCodeAndPrice;
		readonly bool isShipmentPackLines;
		readonly ZTemplateColumn multipleProductsColumn;

		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			bool userIsAuthorised = WebEnv.AppInstance.SiteUser != null && WebEnv.AppInstance.SiteUser is TrackingSiteUser && !(WebEnv.AppInstance.SiteUser as TrackingSiteUser).IsShipmentQuickViewUser;

			AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("c103f694-a821-44f6-bb21-cb521cb2aad2", "Pieces"), JobPackLinesSchema.Constants.JL_PackageCount) { ColumnKey = WebTracker.Grids.TrackingPackLines.Pieces });
			AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("98a2c837-1136-4d12-a572-9912c4bc7102", "Pack Type"), JobPackLinesSchema.Constants.JL_F3_NKPackType, JobPackLinesSchema.Constants.JL_F3_NKPackType + "_List")
			{
				ColumnKey = WebTracker.Grids.TrackingPackLines.PackType,
				ValueFieldName = RefPackTypeSchema.F3_Code.Name,
				TextFieldName = RefPackTypeSchema.F3_Description.Name + (NoResString)"Multilingual"// property name constant
			}); // May be an identifier

			if (userIsAuthorised)
			{
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("6a2725de-7a54-439f-b4f3-fb0a93a14a1b", "Length"), JobPackLinesSchema.Constants.JL_Length) { ColumnKey = WebTracker.Grids.TrackingPackLines.Length, Decimals = 3 });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("b30482ad-580b-4af3-a701-b5ffb0d94d4a", "Width"), JobPackLinesSchema.Constants.JL_Width) { ColumnKey = WebTracker.Grids.TrackingPackLines.Width, Decimals = 3 });
				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("5a113d3e-7c27-4d97-a035-263c7a8f3c9a", "Height"), JobPackLinesSchema.Constants.JL_Height) { ColumnKey = WebTracker.Grids.TrackingPackLines.Height, Decimals = 3 });
				AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("4b2d132d-ce14-4840-bab5-969c0d32c68e", "UD"), JobPackLinesSchema.Constants.JL_UnitOfDimension, JobPackLinesSchema.Constants.JL_UnitOfDimension + "_List")
				{
					ColumnKey = WebTracker.Grids.TrackingPackLines.UnitOfDimension,
					DisplayStyle = OComboBoxDropDownStyle.CodeOnly
				});

				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("1c6eaa14-7afb-4d84-b75e-4149cf312d7d", "Weight"), JobPackLinesSchema.Constants.JL_ActualWeight) { ColumnKey = WebTracker.Grids.TrackingPackLines.Weight, Decimals = 3 });
				AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("3091b174-b9d1-4951-ab9c-6fbee9da6279", "UQ"), JobPackLinesSchema.Constants.JL_ActualWeightUQ, JobPackLinesSchema.Constants.JL_ActualWeightUQ + "_List")
				{
					ColumnKey = WebTracker.Grids.TrackingPackLines.WeightUnit,
					DisplayStyle = OComboBoxDropDownStyle.CodeOnly
				});

				AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("343f2bd3-a4f7-44cc-b336-caf021f3a07a", "Volume"), JobPackLinesSchema.Constants.JL_ActualVolume) { ColumnKey = WebTracker.Grids.TrackingPackLines.Volume, Decimals = 3 });
				AddToDictionaryAsDefault(new ZDropDownListColumn(Res.GetString("3091b174-b9d1-4951-ab9c-6fbee9da6279", "UQ"), JobPackLinesSchema.Constants.JL_ActualVolumeUQ, JobPackLinesSchema.Constants.JL_ActualVolumeUQ + "_List")
				{
					ColumnKey = WebTracker.Grids.TrackingPackLines.VolumeUnit,
					DisplayStyle = OComboBoxDropDownStyle.CodeOnly
				});
			}

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("1a0acb4e-b2ba-40fc-a184-ca6ee638ec30", "Description"), JobPackLinesSchema.Constants.JL_Description) { ColumnKey = WebTracker.Grids.TrackingPackLines.Description });

			if (userIsAuthorised)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("85435eb3-69e7-4aaa-970f-9a2c3f01c013", "Marks and Numbers"), JobPackLinesSchema.Constants.JL_MarksAndNumbers) { ColumnKey = WebTracker.Grids.TrackingPackLines.MarksAndNumbers });

				if (!hideCustomsCodeAndPrice || isShipmentPackLines)
				{
					AddToDictionaryAsDefault(new ZCalcEditColumn(Res.GetString("d6db160c-af34-4a7f-82ab-6b716dd97e82", "Line Price"), JobPackLinesSchema.Constants.JL_LinePrice) { ColumnKey = WebTracker.Grids.TrackingPackLines.LinePrice });
				}

				if (isShipmentPackLines)
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("62eb4ec2-7d95-4190-8a88-a91c70060c03", "Currency"), TrackingPackLine.Schema.CurrencyCode) { ColumnKey = WebTracker.Grids.TrackingPackLines.Currency });
					AddToDictionary(new ZCalcEditColumn(Res.GetString("52b6155c-0991-4cda-8874-57cccf0ff242", "Loading Meters"), TrackingPackLine.Schema.JL_LoadingMeters) { ColumnKey = WebTracker.Grids.TrackingPackLines.LoadingMeters });
				}

				if (!hideCustomsCodeAndPrice)
				{
					AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b2e311a8-539b-47cb-851d-1b3abd8ce316", "Tariff Num."), JobPackLinesSchema.Constants.JL_HarmonisedCode) { ColumnKey = WebTracker.Grids.TrackingPackLines.TariffNumber });
				}
			}

			if (isShipmentPackLines)
			{
				AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("88274f05-4235-4841-8b16-59b1c0a79ff7", "Container"), Enterprise.Freight.Business.PackLine.JL_Calc_ContainerNumberName) { ColumnKey = WebTracker.Grids.TrackingPackLines.ContainerNumber });
			}

			if (userIsAuthorised)
			{
				string customAttribute1Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute1Caption;
				if (string.IsNullOrEmpty(customAttribute1Caption))
				{
					customAttribute1Caption = Res.GetString("f2d50eaa-45cf-4b2e-b6f3-c475bea5b76e", "Custom Text 1");
				}

				string customAttribute2Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute2Caption;
				if (string.IsNullOrEmpty(customAttribute2Caption))
				{
					customAttribute2Caption = Res.GetString("c76019e1-f225-4dd2-8e12-25d2ba4c1ee8", "Custom Text 2");
				}

				string customAttribute3Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute3Caption;
				if (string.IsNullOrEmpty(customAttribute3Caption))
				{
					customAttribute3Caption = Res.GetString("8bdfe317-5bc7-44ae-a0cc-f97dba35691a", "Custom Text 3");
				}

				string customAttribute4Caption = Env.Registry.Freight.PackLine.PackLineCustomAttribute4Caption;
				if (string.IsNullOrEmpty(customAttribute4Caption))
				{
					customAttribute4Caption = Res.GetString("d1e76bf0-394f-47e6-8650-67e64505490d", "Custom Text 4");
				}

				AddToDictionary(new ZTextEditColumn(Res.GetString("3dc93068-d99b-4d11-a8bd-274c2d1bc70b", "Reference Number"), JobPackLinesSchema.Constants.JL_RefNumber) { ColumnKey = WebTracker.Grids.TrackingPackLines.ReferenceNumber });
				AddToDictionary(new ZTextEditColumn(customAttribute1Caption, JobPackLinesSchema.Constants.JL_CustomAttrib1) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute1 });
				AddToDictionary(new ZTextEditColumn(customAttribute2Caption, JobPackLinesSchema.Constants.JL_CustomAttrib2) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute2 });
				AddToDictionary(new ZTextEditColumn(customAttribute3Caption, JobPackLinesSchema.Constants.JL_CustomAttrib3) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute3 });
				AddToDictionary(new ZTextEditColumn(customAttribute4Caption, JobPackLinesSchema.Constants.JL_CustomAttrib4) { ColumnKey = WebTracker.Grids.TrackingPackLines.CustomAttribute4 });
			}

			if (multipleProductsColumn != null)
			{
				multipleProductsColumn.ColumnKey = WebTracker.Grids.TrackingPackLines.MultipleProducts;
				AddToDictionaryAsDefault(multipleProductsColumn);
			}
		}
	}
}
