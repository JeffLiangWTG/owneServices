using System.Xml.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer
{
	public class PackLineValueObjectDataAdapter<TBusinessObject, TValueObject> : InnerPackLineValueObjectDataAdapter<TBusinessObject, TValueObject>
		where TBusinessObject : PackLine
		where TValueObject : Xsd.Package
	{
		public override string RootCollectionElementName { get { return (NoResString)"Packages"; } }
		public override string RootElementName { get { return (NoResString)"Package"; } }
		public override XmlSchema Schema { get { return FreightXmlSchemaDefinitions.Instance.SinglePackage; } }

		#region Import

		protected override void ImportFromValueObjectCore(TBusinessObject packLine, TValueObject value, IValueObjectImportContext context)
		{
			base.ImportFromValueObjectCore(packLine, value, context);
			context.SetPropertyInfoValue(packLine.JL_RN_NKOriginInfo, value.Origin, ForeignKeyType.CountryNK);
			context.SetPropertyInfoValue(packLine.JL_MarksAndNumbersInfo, value.MarksAndNumbers, value.MarksAndNumbersSpecified);
			context.SetPropertyInfoValue(packLine.JL_RH_NKCommodityCodeInfo, value.CommodityCode, value.CommodityCodeSpecified);
			context.SetPropertyInfoValue(packLine.JL_HarmonisedCodeInfo, value.HarmonisedCode, value.HarmonisedCodeSpecified);

			if (value.LoadingMetersSpecified)
			{
				context.SetPropertyInfoValue(packLine.JL_LoadingMetersInfo, value.LoadingMeters, JobPackLinesSchema.JL_LoadingMeters);
			}

			ImportPackLineContainers(packLine, value);
			ImportHazardousGoods(packLine, value, context);
			ImportCustomAttributes(packLine, value, context);
		}

		void ImportPackLineContainers(PackLine packLine, Xsd.Package packageValue)
		{
			if (packLine.CurrentConsol != null)
			{
				foreach (CommonContainer container in packLine.CurrentConsol.Containers)
				{
					if (!packageValue.ContainerNumber.IsEmpty &&
						container.JC_ContainerNum.ToLower() == packageValue.ContainerNumber.ToLower())
					{
						packLine.SetContainer(packLine.CurrentConsol, container);
					}
				}
			}
		}

		void ImportHazardousGoods(PackLine packLine, Xsd.Package packageValue, IValueObjectImportContext context)
		{
			string errorContext = (packLine.Shipment == null) ? "PackLine" : Res.GetString("d3d19e26-202d-412b-baef-6b57072d138b", "Pack Line on {0}", packLine.Shipment.HumanReadableName);

			var hasData1 = packageValue.DangerousGoods.ImportToUNDGDataItems(() => packLine.UNDGs, errorContext, context);

			// Legacy
			var hasData2 = packageValue.HazardousGoods.ImportSingleItemToUNDGDataItems(() => packLine.UNDGs);
			if ((hasData1 || hasData2) && packLine.UNDGs.Count > 0)
			{
				UNDGDataItem dgItem = packLine.UNDGs[packLine.UNDGs.Count - 1];
				ZGuid contactPK = new ContactValueObjectHelper(errorContext).FromContactReferenceGetContactPK(packageValue.DGContact, context);
				if (!contactPK.IsEmpty)
				{
					dgItem.DI_OC_DGContact = contactPK;
				}
			}
		}

		void ImportCustomAttributes(PackLine packLine, Xsd.Package packageValue, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib1Info, packageValue.Custom.Text1, packageValue.Custom.Text1Specified, "Text1");
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib2Info, packageValue.Custom.Text2, packageValue.Custom.Text2Specified, "Text2");
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib3Info, packageValue.Custom.Text3, packageValue.Custom.Text3Specified, "Text3");
			context.SetPropertyInfoValue(packLine.JL_CustomAttrib4Info, packageValue.Custom.Text4, packageValue.Custom.Text4Specified, "Text4");

			if (!packageValue.Custom.Date1.IsEmpty)
			{
				context.SetPropertyInfoValue(packLine.JL_CustomDate1Info, packageValue.Custom.Date1.ToDateTime());
			}

			if (!packageValue.Custom.Date2.IsEmpty)
			{
				context.SetPropertyInfoValue(packLine.JL_CustomDate2Info, packageValue.Custom.Date2.ToDateTime());
			}

			if (packageValue.Custom.Decimal1Specified)
			{
				context.SetPropertyInfoValue(packLine.JL_CustomDecimal1Info, packageValue.Custom.Decimal1, JobPackLinesSchema.JL_CustomDecimal1);
			}

			if (packageValue.Custom.Decimal2Specified)
			{
				context.SetPropertyInfoValue(packLine.JL_CustomDecimal2Info, packageValue.Custom.Decimal2, JobPackLinesSchema.JL_CustomDecimal2);
			}

			if (packageValue.Custom.Flag1Specified)
			{
				packLine.JL_CustomFlag1 = (packageValue.Custom.Flag1 == Xsd.TrueFalse.@true);
			}

			if (packageValue.Custom.Flag2Specified)
			{
				packLine.JL_CustomFlag2 = (packageValue.Custom.Flag2 == Xsd.TrueFalse.@true);
			}
			Xsd.TrueFalse test = new Xsd.TrueFalse();
			if (test == Xsd.TrueFalse.@true)
			{
			}
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(TBusinessObject packline, TValueObject value, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(packline, value, context);

			if (!packline.JL_Calc_ContainerNum.IsEmpty)
			{
				value.ContainerNumber = packline.JL_Calc_ContainerNum;
			}

			if (!packline.JL_MarksAndNumbers.IsEmpty)
			{
				value.MarksAndNumbers = packline.JL_MarksAndNumbers;
			}

			if (!packline.JL_RN_NKOrigin.IsEmpty)
			{
				value.Origin = packline.JL_RN_NKOrigin;
			}

			if (!packline.JL_RH_NKCommodityCode.IsEmpty)
			{
				value.CommodityCode = packline.JL_RH_NKCommodityCode;
			}

			if (!packline.JL_HarmonisedCode.IsEmpty)
			{
				value.HarmonisedCode = packline.JL_HarmonisedCode;
			}

			if (!packline.JL_LoadingMeters.IsEmpty)
			{
				value.LoadingMeters = packline.JL_LoadingMeters;
			}

			ExportDangerousGoodsDetails(packline, value, context);
			ExportCustomAttributes(packline, value);
		}

		void ExportDangerousGoodsDetails(PackLine packline, Xsd.Package packageValue, IValueObjectExportContext context)
		{
			string errorContext = (packline.Shipment == null) ? "PackLine" : Res.GetString("34c2cbcd-3a6a-4306-b73c-3acabeac7ea9", "Pack Line on {0}", packline.Shipment.HumanReadableName);
			packageValue.DangerousGoods.ExportFromUNDGDataItems(packline.UNDGs.ToArray(), errorContext, context);

			// Legacy
			packageValue.HazardousGoods.ExportSingleItemFromUNDGDataItems(packline.UNDGs.ToArray());
		}

		void ExportCustomAttributes(PackLine packLine, Xsd.Package packageValue)
		{
			packageValue.Custom.Date1 = packLine.JL_CustomDate1;
			packageValue.Custom.Date2 = packLine.JL_CustomDate2;
			packageValue.Custom.Decimal1 = packLine.JL_CustomDecimal1;
			packageValue.Custom.Decimal2 = packLine.JL_CustomDecimal2;
			packageValue.Custom.Flag1 = packLine.JL_CustomFlag1 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			packageValue.Custom.Flag2 = packLine.JL_CustomFlag2 ? Xsd.TrueFalse.@true : Xsd.TrueFalse.@false;
			packageValue.Custom.Text1 = packLine.JL_CustomAttrib1;
			packageValue.Custom.Text2 = packLine.JL_CustomAttrib2;
			packageValue.Custom.Text3 = packLine.JL_CustomAttrib3;
			packageValue.Custom.Text4 = packLine.JL_CustomAttrib4;

			packageValue.Custom.Decimal1Specified = true;
			packageValue.Custom.Decimal2Specified = true;
			packageValue.Custom.Flag1Specified = true;
			packageValue.Custom.Flag2Specified = true;
		}

		#endregion
	}
}
