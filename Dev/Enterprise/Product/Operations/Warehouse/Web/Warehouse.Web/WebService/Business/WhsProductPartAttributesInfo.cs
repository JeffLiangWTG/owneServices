using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsProductPartAttributesInfo : WhsOrgPartAttributesInfo
	{
		#region Constructors

		public WhsProductPartAttributesInfo()
		{
			ExpiryDateFormatString = "";
			PackingDateFormatString = "";
			Attribute1MaxLength = WhsDocketLineSchema.WE_PartAttrib1.MaxLength;
			Attribute2MaxLength = WhsDocketLineSchema.WE_PartAttrib2.MaxLength;
			Attribute3MaxLength = WhsDocketLineSchema.WE_PartAttrib3.MaxLength;
			SerialNumberMaxLength = WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.MaxLength;
			CompletePalletPickingUsed = false;
			IsAttributeNeutral = false;
			RFAttributeConfirm = 0;
		}

		public WhsProductPartAttributesInfo(OrgHeader client, OrgSupplierPart part, WhsWarehouse whs)
			: base(client)
		{
			Argument.NotNull(whs, nameof(WhsWarehouse));

			if (part != null)
			{
				ProductPK = part.PK.ToGuid();
				var product = WhsProduct.GetWhsProduct(part);
				Attribute1IsUsed = product.IsPartAttributeUsed(client, 1);
				Attribute1IsBatchNoAndUsed = IsAttributeBatchNoAndUsed(client, product, 1);
				Attribute1IsReleaseCaptured = product.IsPartAttribReleaseCaptured(client, 1);
				Attribute1IsJulianBatchNumberAndUsed = product.IsPartAttributeAJulianBatchNumberAndUsed(client, 1);
				Attribute1MaxLength = WhsDocketLineSchema.WE_PartAttrib1.MaxLength;
				Attribute2IsUsed = product.IsPartAttributeUsed(client, 2);
				Attribute2IsBatchNoAndUsed = IsAttributeBatchNoAndUsed(client, product, 2);
				Attribute2IsReleaseCaptured = product.IsPartAttribReleaseCaptured(client, 2);
				Attribute2IsJulianBatchNumberAndUsed = product.IsPartAttributeAJulianBatchNumberAndUsed(client, 2);
				Attribute2MaxLength = WhsDocketLineSchema.WE_PartAttrib2.MaxLength;
				Attribute3IsUsed = product.IsPartAttributeUsed(client, 3);
				Attribute3IsBatchNoAndUsed = IsAttributeBatchNoAndUsed(client, product, 3);
				Attribute3IsReleaseCaptured = product.IsPartAttribReleaseCaptured(client, 3);
				Attribute3IsJulianBatchNumberAndUsed = product.IsPartAttributeAJulianBatchNumberAndUsed(client, 3);
				Attribute3MaxLength = WhsDocketLineSchema.WE_PartAttrib3.MaxLength;
				SerialNumberMaxLength = WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber.MaxLength;
				IsSerialNumberUsedByProduct = product.IsSerialNumberUsed(client);
				IsSerialNumberReleaseCaptured = product.IsSerialNumberReleaseCaptured(client);
				ExpiryDateIsUsed = product.IsExpiryDateUsed(client);
				PackingDateIsUsed = product.IsPackingDateUsed(client);
				ExpiryDateFormatString = GetDateFormat(ExpiryDateIsUsed, product.ExpiryDateFormatString(client), whs);
				PackingDateFormatString = GetDateFormat(PackingDateIsUsed, product.PackingDateFormatString(client), whs);
				CompletePalletPickingUsed = product.IsCompletePalletPickingUsed(client);
				RFAttributeConfirm = RFAttributeHelper.GetRFAttributeConfirm(part, client);
			}

			if (client != null)
			{
				ClientPK = client.PK.ToGuid();
				IsAttributeNeutral = client.PartAttributeManager.IsAttributeNeutralUsedByProduct(part);
			}

			if (part != null && client != null)
			{
				var relation = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(client.PK, OrgPartRelation.RelationshipTypes.Owner);
				if (relation != null)
				{
					Hi = relation.OU_Hi;
					Ti = relation.OU_Ti;
				}
			}
		}

		static string GetDateFormat(ZBool isDateUsed, ZString format, WhsWarehouse whs)
		{
			return format.IsEmpty && isDateUsed
				? DateFormatHelper.GetDateFormatForCountry(whs.CountryCode)
				: (string)format;
		}

		bool IsAttributeBatchNoAndUsed(OrgHeader client, WhsProduct product, int attribNo)
		{
			return
				client != null &&
				client.PartAttributeManager.PartAttributeType(attribNo).EqualsIgnoringCase(PartAttributeTypeList.Codes.BatchNumber) &&
				product.IsPartAttributeUsed(client, attribNo);
		}

		#endregion

		#region GetInfo

		public static WhsProductPartAttributesInfo GetInfo(OrgHeader client, OrgSupplierPart part, WhsWarehouse whs)
		{
			return client != null && part != null && whs != null
				? client.Factory.GetCachedValue("WhsProductPartAttributesInfo|GetInfo|" + part.PK + client.PK + whs.PK, () => new WhsProductPartAttributesInfo(client, part, whs))
				: new WhsProductPartAttributesInfo(client, part, whs);
		}

		#endregion

		#region Properties

		#region Attribute 1

		public bool Attribute1IsUsed { get; set; }
		public bool Attribute1IsBatchNoAndUsed { get; set; }
		public bool Attribute1IsReleaseCaptured { get; set; }
		public bool Attribute1IsJulianBatchNumberAndUsed { get; set; }
		public int Attribute1MaxLength { get; set; }

		#endregion

		#region Attribute 2

		public bool Attribute2IsUsed { get; set; }
		public bool Attribute2IsBatchNoAndUsed { get; set; }
		public bool Attribute2IsReleaseCaptured { get; set; }
		public bool Attribute2IsJulianBatchNumberAndUsed { get; set; }
		public int Attribute2MaxLength { get; set; }

		#endregion

		#region Attribute 3

		public bool Attribute3IsUsed { get; set; }
		public bool Attribute3IsBatchNoAndUsed { get; set; }
		public bool Attribute3IsReleaseCaptured { get; set; }
		public bool Attribute3IsJulianBatchNumberAndUsed { get; set; }
		public int Attribute3MaxLength { get; set; }

		#endregion

		#region SerialNumber

		public bool IsSerialNumberUsedByProduct { get; set; }

		public bool IsSerialNumberReleaseCaptured { get; set; }

		public int SerialNumberMaxLength { get; set; }

		#endregion

		#region ExpiryDate

		public string ExpiryDateFormatString { get; set; }

		#endregion

		#region PackingDate

		public string PackingDateFormatString { get; set; }

		#endregion

		#region CompletePalletPickingUsed

		public bool CompletePalletPickingUsed { get; set; }

		#endregion

		#region IsAttributeNeutral

		public bool IsAttributeNeutral { get; set; }

		#endregion

		#region RFAttributeConfirm

		public int RFAttributeConfirm { get; set; }

		#endregion

		#region HasReleaseCapturedAttribute

		public bool HasReleaseCapturedAttribute
			=> Attribute1IsReleaseCaptured
				|| Attribute2IsReleaseCaptured
				|| Attribute3IsReleaseCaptured
				|| IsSerialNumberReleaseCaptured;

		#endregion

		#region HasSerialNumberAttribute

		public bool HasSerialNumberAttribute => IsSerialNumberUsedByProduct;

		#endregion

		#region ProductPK

		public Guid ProductPK { get; set; }

		#endregion

		#region ClientPK

		public Guid ClientPK { get; set; }

		#endregion

		#region DateRange

		public int PackingDateMaximumPastYears { get; set; }

		public int PackingDateMaximumFutureYears { get; set; }

		public int ExpiryDateMaximumPastYears { get; set; }

		public int ExpiryDateMaximumFutureYears { get; set; }

		#endregion

		public short Hi { get; set; }
		public short Ti { get; set; }

		#endregion
	}
}
