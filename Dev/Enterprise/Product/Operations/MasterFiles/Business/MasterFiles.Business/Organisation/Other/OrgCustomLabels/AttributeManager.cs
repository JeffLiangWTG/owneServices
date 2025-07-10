using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	public class AttributeManager
	{
		public enum AttributeModules
		{
			Order,
			Warehouse,
			WhsInventory,
			CommercialInvoice
		}

		#region Public

		public IEnumerable<CustomAttribute> GetAllAttributes(AttributeModules module, OrgHeader relatedOrg, OrgHeader loggedInWebUsersOrg)
		{
			Dictionary<ZString, SchemaColumn>[] dictionaries;

			switch (module)
			{
				case AttributeModules.Order:
					dictionaries = new[] { OrderPartAttributes, OrderLineAttributes, OrderHeaderAttributes, OrderHeaderUserTrackDates };
					break;
				case AttributeModules.Warehouse:
					dictionaries = new[] { WhsDocketLinePartAttributes, DocketLineAttributes, DocketAttributes };
					break;
				case AttributeModules.WhsInventory:
					dictionaries = new[] { WhsInventoryPartAttributes, DocketLineAttributes, DocketAttributes };
					break;
				case AttributeModules.CommercialInvoice:
					dictionaries = new[] { CommercialInvoiceLinePartAttributes, CommercialInvoiceLineAttributes };
					break;
				default:
					throw new NotImplementedException();
			}

			return GetAttributes(dictionaries, relatedOrg, loggedInWebUsersOrg);
		}

		public IEnumerable<CustomAttribute> GetPartAttributes(AttributeModules module, OrgHeader relatedOrg, OrgHeader loggedInWebUsersOrg)
		{
			Dictionary<ZString, SchemaColumn> dictionary;

			switch (module)
			{
				case AttributeModules.Order:
					dictionary = OrderPartAttributes;
					break;
				case AttributeModules.Warehouse:
					dictionary = WhsDocketLinePartAttributes;
					break;
				case AttributeModules.WhsInventory:
					dictionary = WhsInventoryPartAttributes;
					break;
				case AttributeModules.CommercialInvoice:
					dictionary = CommercialInvoiceLinePartAttributes;
					break;
				default:
					throw new NotImplementedException();
			}

			return GetAttributes(new[] { dictionary }, relatedOrg, loggedInWebUsersOrg);
		}

		public IEnumerable<CustomAttribute> GetLineAttributes(AttributeModules module, OrgHeader relatedOrg, OrgHeader loggedInWebUsersOrg)
		{
			return GetAttributes(new[] { module == AttributeModules.Order ? OrderLineAttributes : DocketLineAttributes }, relatedOrg, loggedInWebUsersOrg);
		}

		IEnumerable<CustomAttribute> GetAttributes(Dictionary<ZString, SchemaColumn>[] dictionaries, OrgHeader relatedOrg, OrgHeader loggedInWebUsersOrg)
		{
			foreach (var dictionary in dictionaries)
			{
				foreach (var pair in dictionary)
				{
					var description = GetDescription(pair.Key, relatedOrg, loggedInWebUsersOrg, out var isCustomDescription);
					if (!description.IsEmpty)
					{
						yield return new CustomAttribute(GetKey(pair.Key, description, isCustomDescription), pair.Value, description);
					}
				}
			}
		}

		#endregion

		#region Implementation

		ZString GetKey(ZString key, MultilingualString description, bool isCutomDescription)
		{
			if (isCutomDescription || IsPartAttribute(key))
			{
				key = description.GetUnresolvedString();
			}
			return key;
		}

		bool IsPartAttribute(ZString key)
			=> key.StartsWith(OM_IMPartAttribNamePrefix, StringComparison.Ordinal) || key.Equals(OrgMiscServSchema.Constants.OM_IMUseSerialNumber);

		internal MultilingualString GetDescription(ZString attributeName, OrgHeader relatedOrg, OrgHeader loggedInWebUsersOrg, out bool isCustom)
		{
			isCustom = false;
			return Globals.IsWeb
					? GetCaption(attributeName, relatedOrg, loggedInWebUsersOrg, out isCustom)
					: GetCaptionBasedOnAttributeName(attributeName);
		}

		const string OM_IMPartAttribNamePrefix = "OM_IMPartAttrib";

		MultilingualString GetCaption(ZString attributeName, OrgHeader relatedOrg, OrgHeader loggedInWebUsersOrg, out bool isCustom)
		{
			isCustom = false;
			MultilingualString result = (NoResString)"";

			if (Globals.IsWeb)
			{
				result = GetAttributeCaption(attributeName, loggedInWebUsersOrg, out isCustom);
			}

			if (result.IsEmpty)
			{
				result = GetAttributeCaption(attributeName, relatedOrg, out isCustom);
			}

			if (result.IsEmpty && (!attributeName.StartsWith(OM_IMPartAttribNamePrefix, StringComparison.Ordinal) || attributeName.EqualsIgnoringCase(OrgMiscServSchema.OM_IMUseSerialNumber.Name))) //Parts Attributes are not being inherited from OrgProxy
			{
				result = GetAttributeCaption(attributeName, GlbCompany.CurrentCompany.OrgProxy, out isCustom);
			}

			return result;
		}

		MultilingualString GetAttributeCaption(ZString attributeName, OrgHeader ownerOrg, out bool isCustom)
		{
			isCustom = false;
			MultilingualString caption = (NoResString)"";
			if (ownerOrg != null)
			{
				if (attributeName.StartsWith(OM_IMPartAttribNamePrefix, StringComparison.Ordinal))
				{
					try
					{
						isCustom = true;
						MultilingualString attNameWithoutPerfix = (NoResString)attributeName.Replace(OM_IMPartAttribNamePrefix, "").Left(1);
						caption = MakeUniqueName((NoResString)ownerOrg.MiscServ[attributeName].ToString(), MultilingualString.Join("", ResString.GetMultilingualString("GetAttributeCaption -PartAttributeName", "PA"), attNameWithoutPerfix));
					}
					catch (ArgumentException)
					{
						ErrorReporter.ReportOnce("PartAttributeName", "Unknown Field: " + attributeName);
					}
				}
				else if (attributeName.Equals(OrgMiscServSchema.OM_IMUseSerialNumber.Name))
				{
					isCustom = true;
					caption = ownerOrg.MiscServ.OM_IMUseSerialNumber ? SerialNumberCaption : (NoResString)"";
				}
				else
				{
					OrgCustomLabels customLabel = ownerOrg.CustomLabels.FindByFieldName(attributeName);
					if (customLabel != null)
					{
						isCustom = true;
						caption = MakeUniqueName((NoResString)customLabel.OT_Caption, ResString.GetMultilingualString("GetAttributeCaption-CustomLabels", "CA"));
					}
				}
			}
			return caption;
		}

		MultilingualString MakeUniqueName(MultilingualString caption, MultilingualString addToCaption)
		{
			if (!caption.IsEmpty && Globals.IsWeb)
			{
				return MultilingualString.Join(" - ", caption, addToCaption);
			}
			else
			{
				return caption;
			}
		}

		MultilingualString GetCaptionBasedOnAttributeName(string attributeName)
		{
			return attributeName == Invariant($"{OM_IMPartAttribNamePrefix}1Name") ?
					 ResString.GetMultilingualString("196b1f79-3861-4dee-b649-ac18657cc30d", "Part Attribute 1") :
					 attributeName == Invariant($"{OM_IMPartAttribNamePrefix}2Name") ?
					 ResString.GetMultilingualString("7833113e-2774-4933-a0ac-4e6793f0ee82", "Part Attribute 2") :
					 attributeName == Invariant($"{OM_IMPartAttribNamePrefix}3Name") ?
					 ResString.GetMultilingualString("9af43329-8fd2-4be1-ac23-c172ac2611e0", "Part Attribute 3") :
					 attributeName == OrgMiscServSchema.OM_IMUseSerialNumber.Name ? SerialNumberCaption :
					 CustomLabelsList.GetMultilingualDescriptionFromCode(attributeName) ?? (NoResString)attributeName;
		}

		MultilingualString SerialNumberCaption => ResString.GetMultilingualString("e4d8a78e-aca4-463d-9b9e-42eb142ada31", "Serial Number");

		CodeDescriptionPairList CustomLabelsList
		{
			get
			{
				return customLabelsList ?? (customLabelsList = new CodeDescriptionPairList(OLookUpEditType.CustomLabels));
			}
		}
		CodeDescriptionPairList customLabelsList;

		#endregion

		#region Dictionaries

		Dictionary<ZString, SchemaColumn> docketLineAttributes;
		Dictionary<ZString, SchemaColumn> DocketLineAttributes
		{
			get
			{
				return docketLineAttributes ?? (docketLineAttributes = new Dictionary<ZString, SchemaColumn>
				{
					{ Constants.CustomLabels.WhsDocketLine.CustomAttribute1, WhsDocketLineSchema.WE_CustomAttrib1 },
					{ Constants.CustomLabels.WhsDocketLine.CustomAttribute2, WhsDocketLineSchema.WE_CustomAttrib2 },
					{ Constants.CustomLabels.WhsDocketLine.CustomAttribute3, WhsDocketLineSchema.WE_CustomAttrib3 },
					{ Constants.CustomLabels.WhsDocketLine.CustomAttribute4, WhsDocketLineSchema.WE_CustomAttrib4 },
					{ Constants.CustomLabels.WhsDocketLine.CustomAttribute5, WhsDocketLineSchema.WE_CustomAttrib5 },
					{ Constants.CustomLabels.WhsDocketLine.CustomAttribute6, WhsDocketLineSchema.WE_CustomAttrib6 },
					{ Constants.CustomLabels.WhsDocketLine.CustomTextBlob1, WhsDocketLineSchema.WE_CustomTextBlob1 },
					{ Constants.CustomLabels.WhsDocketLine.CustomFlag1, WhsDocketLineSchema.WE_CustomFlag1 },
					{ Constants.CustomLabels.WhsDocketLine.CustomFlag2, WhsDocketLineSchema.WE_CustomFlag2 },
					{ Constants.CustomLabels.WhsDocketLine.CustomFlag3, WhsDocketLineSchema.WE_CustomFlag3 },
					{ Constants.CustomLabels.WhsDocketLine.CustomFlag4, WhsDocketLineSchema.WE_CustomFlag4 },
					{ Constants.CustomLabels.WhsDocketLine.CustomFlag5, WhsDocketLineSchema.WE_CustomFlag5 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDate1, WhsDocketLineSchema.WE_CustomDate1 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDate2, WhsDocketLineSchema.WE_CustomDate2 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDate3, WhsDocketLineSchema.WE_CustomDate3 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDate4, WhsDocketLineSchema.WE_CustomDate4 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDate5, WhsDocketLineSchema.WE_CustomDate5 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDecimal1, WhsDocketLineSchema.WE_CustomDecimal1 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDecimal2, WhsDocketLineSchema.WE_CustomDecimal2 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDecimal3, WhsDocketLineSchema.WE_CustomDecimal3 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDecimal4, WhsDocketLineSchema.WE_CustomDecimal4 },
					{ Constants.CustomLabels.WhsDocketLine.CustomDecimal5, WhsDocketLineSchema.WE_CustomDecimal5 },
				});
			}
		}

		Dictionary<ZString, SchemaColumn> docketAttributes;
		Dictionary<ZString, SchemaColumn> DocketAttributes
		{
			get
			{
				return docketAttributes ?? (docketAttributes = new Dictionary<ZString, SchemaColumn>
				{
					{ Constants.CustomLabels.WhsDocket.CustomAttribute1, WhsDocketSchema.WD_CustomAttrib1 },
					{ Constants.CustomLabels.WhsDocket.CustomAttribute2, WhsDocketSchema.WD_CustomAttrib2 },
					{ Constants.CustomLabels.WhsDocket.CustomAttribute3, WhsDocketSchema.WD_CustomAttrib3 },
					{ Constants.CustomLabels.WhsDocket.CustomAttribute4, WhsDocketSchema.WD_CustomAttrib4 },
					{ Constants.CustomLabels.WhsDocket.CustomAttribute5, WhsDocketSchema.WD_CustomAttrib5 },
					{ Constants.CustomLabels.WhsDocket.CustomFlag1, WhsDocketSchema.WD_CustomFlag1 },
					{ Constants.CustomLabels.WhsDocket.CustomFlag2, WhsDocketSchema.WD_CustomFlag2 },
					{ Constants.CustomLabels.WhsDocket.CustomFlag3, WhsDocketSchema.WD_CustomFlag3 },
					{ Constants.CustomLabels.WhsDocket.CustomFlag4, WhsDocketSchema.WD_CustomFlag4 },
					{ Constants.CustomLabels.WhsDocket.CustomFlag5, WhsDocketSchema.WD_CustomFlag5 },
					{ Constants.CustomLabels.WhsDocket.CustomDate1, WhsDocketSchema.WD_CustomDate1 },
					{ Constants.CustomLabels.WhsDocket.CustomDate2, WhsDocketSchema.WD_CustomDate2 },
					{ Constants.CustomLabels.WhsDocket.CustomDecimal1, WhsDocketSchema.WD_CustomDecimal1 },
					{ Constants.CustomLabels.WhsDocket.CustomDecimal2, WhsDocketSchema.WD_CustomDecimal2 },
					{ Constants.CustomLabels.WhsDocket.CustomDecimal3, WhsDocketSchema.WD_CustomDecimal3 },
					{ Constants.CustomLabels.WhsDocket.CustomDecimal4, WhsDocketSchema.WD_CustomDecimal4 },
					{ Constants.CustomLabels.WhsDocket.CustomDecimal5, WhsDocketSchema.WD_CustomDecimal5 },
				});
			}
		}

		Dictionary<ZString, SchemaColumn> WhsDocketLinePartAttributes
		{
			get
			{
				if (whsDocketLinePartAttributes == null)
				{
					whsDocketLinePartAttributes = new Dictionary<ZString, SchemaColumn>
						{
							{ OrgMiscServSchema.OM_IMPartAttrib1Name.Name, WhsDocketLineSchema.WE_PartAttrib1 },
							{ OrgMiscServSchema.OM_IMPartAttrib2Name.Name, WhsDocketLineSchema.WE_PartAttrib2 },
							{ OrgMiscServSchema.OM_IMPartAttrib3Name.Name, WhsDocketLineSchema.WE_PartAttrib3 },
							{ OrgMiscServSchema.OM_IMUseSerialNumber.Name, WhsDocketLineSchema.WE_SerialNumber }
						};
				}

				return whsDocketLinePartAttributes;
			}
		}
		Dictionary<ZString, SchemaColumn> whsDocketLinePartAttributes;

		Dictionary<ZString, SchemaColumn> WhsInventoryPartAttributes
		{
			get
			{
				if (whsInventoryPartAttributes == null)
				{
					var serialNumberColumnName = WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value ?
						WhsSerialNumberSchema.WSN_SerialNumber :
						WhsInventoryViewSchema.WI_SerialNumber;
					whsInventoryPartAttributes = new Dictionary<ZString, SchemaColumn>
						{
							{ OrgMiscServSchema.OM_IMPartAttrib1Name.Name, WhsInventoryViewSchema.WI_PartAttrib1 },
							{ OrgMiscServSchema.OM_IMPartAttrib2Name.Name, WhsInventoryViewSchema.WI_PartAttrib2 },
							{ OrgMiscServSchema.OM_IMPartAttrib3Name.Name, WhsInventoryViewSchema.WI_PartAttrib3 },
							{ OrgMiscServSchema.OM_IMUseSerialNumber.Name, serialNumberColumnName }
						};
				}

				return whsInventoryPartAttributes;
			}
		}
		Dictionary<ZString, SchemaColumn> whsInventoryPartAttributes;

		Dictionary<ZString, SchemaColumn> orderLineAttributes;
		Dictionary<ZString, SchemaColumn> OrderLineAttributes
		{
			get
			{
				return orderLineAttributes ?? (orderLineAttributes = new Dictionary<ZString, SchemaColumn>
				{
					{ Constants.CustomLabels.OrderLine.CustomAttribute1, JobOrderLineSchema.JO_CustomAttrib1 },
					{ Constants.CustomLabels.OrderLine.CustomAttribute2, JobOrderLineSchema.JO_CustomAttrib2 },
					{ Constants.CustomLabels.OrderLine.CustomAttribute3, JobOrderLineSchema.JO_CustomAttrib3 },
					{ Constants.CustomLabels.OrderLine.CustomAttribute4, JobOrderLineSchema.JO_CustomAttrib4 },
					{ Constants.CustomLabels.OrderLine.CustomAttribute5, JobOrderLineSchema.JO_CustomAttrib5 },
					{ Constants.CustomLabels.OrderLine.CustomAttribute6, JobOrderLineSchema.JO_CustomAttrib6 },
					{ Constants.CustomLabels.OrderLine.CustomText1, JobOrderLineSchema.JO_CustomTextBlob1 },
					{ Constants.CustomLabels.OrderLine.CustomFlag1, JobOrderLineSchema.JO_CustomFlag1 },
					{ Constants.CustomLabels.OrderLine.CustomFlag2, JobOrderLineSchema.JO_CustomFlag2 },
					{ Constants.CustomLabels.OrderLine.CustomFlag3, JobOrderLineSchema.JO_CustomFlag3 },
					{ Constants.CustomLabels.OrderLine.CustomFlag4, JobOrderLineSchema.JO_CustomFlag4 },
					{ Constants.CustomLabels.OrderLine.CustomFlag5, JobOrderLineSchema.JO_CustomFlag5 },
					{ Constants.CustomLabels.OrderLine.CustomDate1, JobOrderLineSchema.JO_CustomDate1 },
					{ Constants.CustomLabels.OrderLine.CustomDate2, JobOrderLineSchema.JO_CustomDate2 },
					{ Constants.CustomLabels.OrderLine.CustomDate3, JobOrderLineSchema.JO_CustomDate3 },
					{ Constants.CustomLabels.OrderLine.CustomDate4, JobOrderLineSchema.JO_CustomDate4 },
					{ Constants.CustomLabels.OrderLine.CustomDate5, JobOrderLineSchema.JO_CustomDate5 },
					{ Constants.CustomLabels.OrderLine.CustomDecimal1, JobOrderLineSchema.JO_CustomDecimal1 },
					{ Constants.CustomLabels.OrderLine.CustomDecimal2, JobOrderLineSchema.JO_CustomDecimal2 },
					{ Constants.CustomLabels.OrderLine.CustomDecimal3, JobOrderLineSchema.JO_CustomDecimal3 },
					{ Constants.CustomLabels.OrderLine.CustomDecimal4, JobOrderLineSchema.JO_CustomDecimal4 },
					{ Constants.CustomLabels.OrderLine.CustomDecimal5, JobOrderLineSchema.JO_CustomDecimal5 }
				});
			}
		}

		Dictionary<ZString, SchemaColumn> orderHeaderUserTrackDates;
		Dictionary<ZString, SchemaColumn> OrderHeaderUserTrackDates
		{
			get
			{
				return orderHeaderUserTrackDates ?? (orderHeaderUserTrackDates = new Dictionary<ZString, SchemaColumn>
				{
					{ Constants.CustomLabels.Order.UserTrackDate1, JobOrderHeaderSchema.JD_EstimateUserDate1 },
					{ Constants.CustomLabels.Order.UserTrackDate2, JobOrderHeaderSchema.JD_EstimateUserDate2 },
					{ Constants.CustomLabels.Order.UserTrackDate3, JobOrderHeaderSchema.JD_EstimateUserDate3 },
					{ Constants.CustomLabels.Order.UserTrackDate4, JobOrderHeaderSchema.JD_EstimateUserDate4 }
				});
			}
		}

		Dictionary<ZString, SchemaColumn> orderHeaderAttributes;
		Dictionary<ZString, SchemaColumn> OrderHeaderAttributes
		{
			get
			{
				return orderHeaderAttributes ?? (orderHeaderAttributes = new Dictionary<ZString, SchemaColumn>
				{
					{ Constants.CustomLabels.Order.CustomDate1, JobOrderHeaderSchema.JD_CustomDate1 },
					{ Constants.CustomLabels.Order.CustomDate2, JobOrderHeaderSchema.JD_CustomDate2 },
					{ Constants.CustomLabels.Order.CustomAttribute1, JobOrderHeaderSchema.JD_CustomAttrib1 },
					{ Constants.CustomLabels.Order.CustomAttribute2, JobOrderHeaderSchema.JD_CustomAttrib2 },
					{ Constants.CustomLabels.Order.CustomAttribute3, JobOrderHeaderSchema.JD_CustomAttrib3 },
					{ Constants.CustomLabels.Order.CustomAttribute4, JobOrderHeaderSchema.JD_CustomAttrib4 },
					{ Constants.CustomLabels.Order.CustomAttribute5, JobOrderHeaderSchema.JD_CustomAttrib5 },
					{ Constants.CustomLabels.Order.CustomFlag1, JobOrderHeaderSchema.JD_CustomFlag1 },
					{ Constants.CustomLabels.Order.CustomFlag2, JobOrderHeaderSchema.JD_CustomFlag2 },
					{ Constants.CustomLabels.Order.CustomFlag3, JobOrderHeaderSchema.JD_CustomFlag3 },
					{ Constants.CustomLabels.Order.CustomFlag4, JobOrderHeaderSchema.JD_CustomFlag4 },
					{ Constants.CustomLabels.Order.CustomFlag5, JobOrderHeaderSchema.JD_CustomFlag5 },
					{ Constants.CustomLabels.Order.CustomDecimal1, JobOrderHeaderSchema.JD_CustomDecimal1 },
					{ Constants.CustomLabels.Order.CustomDecimal2, JobOrderHeaderSchema.JD_CustomDecimal2 },
					{ Constants.CustomLabels.Order.CustomDecimal3, JobOrderHeaderSchema.JD_CustomDecimal3 },
					{ Constants.CustomLabels.Order.CustomDecimal4, JobOrderHeaderSchema.JD_CustomDecimal4 },
					{ Constants.CustomLabels.Order.CustomDecimal5, JobOrderHeaderSchema.JD_CustomDecimal5 },
					{ Constants.CustomLabels.Order.CustomContact1, JobOrderHeaderSchema.JD_FirstBuyerContact },
					{ Constants.CustomLabels.Order.CustomContact2, JobOrderHeaderSchema.JD_SecondBuyerContact },
					{ Constants.CustomLabels.Order.GoodsOrigin, JobOrderHeaderSchema.JD_RL_NKGoodsAvailableAt },
					{ Constants.CustomLabels.Order.GoodsDestination, JobOrderHeaderSchema.JD_RL_NKGoodsDeliveredTo },
				});
			}
		}

		Dictionary<ZString, SchemaColumn> orderPartAttributes;
		Dictionary<ZString, SchemaColumn> OrderPartAttributes
		{
			get
			{
				if (orderPartAttributes == null)
				{
					orderPartAttributes = new Dictionary<ZString, SchemaColumn>
					{
						{ OrgMiscServSchema.OM_IMPartAttrib1Name.Name, JobOrderLineSchema.JO_PartAttrib1 },
						{ OrgMiscServSchema.OM_IMPartAttrib2Name.Name, JobOrderLineSchema.JO_PartAttrib2 },
						{ OrgMiscServSchema.OM_IMPartAttrib3Name.Name, JobOrderLineSchema.JO_PartAttrib3 },
						{ OrgMiscServSchema.OM_IMUseSerialNumber.Name, JobOrderLineSchema.JO_SerialNumber }
					};
				}

				return orderPartAttributes;
			}
		}

		Dictionary<ZString, SchemaColumn> commercialInvoiceLinePartAttributes;
		Dictionary<ZString, SchemaColumn> CommercialInvoiceLinePartAttributes
		{
			get
			{
				if (commercialInvoiceLinePartAttributes == null)
				{
					commercialInvoiceLinePartAttributes = new Dictionary<ZString, SchemaColumn>
					{
						{ OrgMiscServSchema.OM_IMPartAttrib1Name.Name, JobComInvoiceLineSchema.JI_PartAttrib1 },
						{ OrgMiscServSchema.OM_IMPartAttrib2Name.Name, JobComInvoiceLineSchema.JI_PartAttrib2 },
						{ OrgMiscServSchema.OM_IMPartAttrib3Name.Name, JobComInvoiceLineSchema.JI_PartAttrib3 },
						{ OrgMiscServSchema.OM_IMUseSerialNumber.Name, JobComInvoiceLineSchema.JI_SerialNumber }
					};
				}

				return commercialInvoiceLinePartAttributes;
			}
		}

		Dictionary<ZString, SchemaColumn> commercialInvoiceLineAttributes;
		Dictionary<ZString, SchemaColumn> CommercialInvoiceLineAttributes
		{
			get
			{
				return commercialInvoiceLineAttributes ?? (commercialInvoiceLineAttributes = new Dictionary<ZString, SchemaColumn>
				{
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute1, JobComInvoiceLineSchema.JI_CustomAttrib1 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute2, JobComInvoiceLineSchema.JI_CustomAttrib2 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute3, JobComInvoiceLineSchema.JI_CustomAttrib3 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute4, JobComInvoiceLineSchema.JI_CustomAttrib4 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute5, JobComInvoiceLineSchema.JI_CustomAttrib5 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomAttribute6, JobComInvoiceLineSchema.JI_CustomAttrib6 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomText1, JobComInvoiceLineSchema.JI_CustomTextBlob1 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomFlag1, JobComInvoiceLineSchema.JI_CustomFlag1 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomFlag2, JobComInvoiceLineSchema.JI_CustomFlag2 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomFlag3, JobComInvoiceLineSchema.JI_CustomFlag3 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDate1, JobComInvoiceLineSchema.JI_CustomDate1 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDate2, JobComInvoiceLineSchema.JI_CustomDate2 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDate3, JobComInvoiceLineSchema.JI_CustomDate3 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDecimal1, JobComInvoiceLineSchema.JI_CustomDecimal1 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDecimal2, JobComInvoiceLineSchema.JI_CustomDecimal2 },
					{ Constants.CustomLabels.ComInvoiceLine.CustomDecimal3, JobComInvoiceLineSchema.JI_CustomDecimal3 },
				});
			}
		}

		#endregion
	}
}
