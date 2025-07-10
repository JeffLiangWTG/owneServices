using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	sealed class WhsReleaseLinePartAttributeValidation : PartAttributeValidation
	{
		public WhsReleaseLinePartAttributeValidation(WhsReleaseLine releaseLine)
			: base(releaseLine)
		{
		}

		new WhsReleaseLine Parent => (WhsReleaseLine)base.Parent;

		protected override ZString GetMandatoryAttributeErrorMessageToAppend(OrgHeader org, OrgSupplierPart part, IZType value, int attributeNumber)
		{
			var result = base.GetMandatoryAttributeErrorMessageToAppend(org, part, value, attributeNumber);
			if (value.IsEmpty && part != null)
			{
				var product = WhsProduct.GetWhsProduct(part);
				var isAttributeReleaseCaptured = attributeNumber > 3
					? product.IsSerialNumberReleaseCaptured(org)
					: product.IsPartAttribReleaseCaptured(org, attributeNumber);

				if (isAttributeReleaseCaptured && Parent.IsPacked)
				{
					result = Res.GetString("e72e5a63-4cc0-4559-9910-7d908bfdb70e", "Release Captured Attributes should be entered prior to Packing when not using RF. Unpack items, enter Release Captured Attributes and then Pack.");
				}
			}

			return result;
		}

		protected override ZString GetSerialNumberDuplicateErrorMessageToAppend(ZString partAttribName)
		{
			return Parent.IsPacked
				? (ZString)Res.GetString("17119116-9ced-491e-b6b8-ee70cd470f50", "The item must first be Unpacked before entering a new {0}.", partAttribName)
				: base.GetSerialNumberDuplicateErrorMessageToAppend(partAttribName);
		}

		protected override bool IsReadyToCheckInventoryForDuplicateSerialNumber
		{
			get
			{
				var result = WhsEnvironment.IsRF;
				if (!result)
				{
					var parentDocket = Parent.PickableDocket;
					result = parentDocket != null && (parentDocket.IsFinalising || parentDocket.IsPickFinalising);
				}

				return result;
			}
		}

		protected override bool IsSerialNumberUniqueCore(OrgHeader client, OrgSupplierPart part, ZPropertyInfo info)
		{
			bool result = base.IsSerialNumberUniqueCore(client, part, info);
			var value = (ZString)info.Value;
			if (result && !value.IsEmpty)
			{
				result = IsSerialNumberExistsOnReleaseCapturedAttributesWithRCAsInPickLines(client, part, value);
			}

			return result;
		}

		bool IsSerialNumberExistsOnReleaseCapturedAttributesWithRCAsInPickLines(OrgHeader client, OrgSupplierPart part, ZString value)
		{
			var partAttributesQuery = new ZQuery();
			partAttributesQuery.AddToFilter(WhsPickLineSchema.WZ_ReleaseCapturedSerialNumber, value);

			var releaseCapturedQuery = WhsReleaseLine.GetOtherReleaseCapturedSerialNumbersQuery(client.PK, part.PK, ZGuid.Empty, Parent.OrderLinePK, partAttributesQuery);

			return client.Factory.LoadTop1<WhsPickLine>(releaseCapturedQuery) == null;
		}
	}
}
