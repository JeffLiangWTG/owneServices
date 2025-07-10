using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PartAttributeValidation : MasterFiles.Business.PartAttributeValidation
	{
		#region Constructors

		public PartAttributeValidation()
			: base()
		{
		}

		public PartAttributeValidation(IPartAttributeValidationConsumer iPartAttributeValidationConsumer)
			: base()
		{
			iParentConsumer = Argument.NotNull(iPartAttributeValidationConsumer, "IPartAttributeValidationConsumer iPartAttributeValidationConsumer");
		}

		readonly IPartAttributeValidationConsumer iParentConsumer;

		protected IPartAttributeValidationConsumer Parent
		{
			get { return iParentConsumer; }
		}

		#endregion

		#region CheckAttributeWhenNotEmpty

		protected override bool CheckAttributeWhenNotEmpty(ZPropertyInfo info)
		{
			return true;
		}

		#endregion

		#region Checks on Serial Number

		protected sealed override void CheckSerialNumberCore(OrgHeader client, OrgSupplierPart part, ZPropertyInfo info)
		{
			base.CheckSerialNumberCore(client, part, info);

			if (iParentConsumer != null && iParentConsumer.IsRegisteredForUniqueSerialNumberChecking)
			{
				CheckSerialNumberIsUnique(client, part, info);
			}
		}

		void CheckSerialNumberIsUnique(OrgHeader client, OrgSupplierPart part, ZPropertyInfo info)
		{
			var value = (ZString)info.Value;
			if (iParentConsumer.IsValidForUniqueSerialNumberChecking(value))
			{
				if (iParentConsumer.IsSerialNumberUsedOnSiblings(value))
				{
					AddSerialNumberDuplicateError(client, info);
				}
				else if (IsReadyToCheckInventoryForDuplicateSerialNumber)
				{
					CheckSerialNumberIsUniqueCore(client, part, info);
				}
			}
		}

		void CheckSerialNumberIsUniqueCore(OrgHeader client, OrgSupplierPart part, ZPropertyInfo info)
		{
			var query = new ZQuery();
			if (WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, part.PK);
			}

			query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			var inventory = iParentConsumer as WhsInventoryView;
			if (inventory != null)
			{
				query.AddToFilter(WhsInventoryViewSchema.PK, SQLComparisonOperator.NotEqual, inventory.PK);
			}

			query.AddToFilter(WhsInventoryViewSchema.WI_SerialNumber, info.Value);

			// do below to simplify the checking of attribute type / usage
			foreach (var inventoryItem in client.Factory.Load<WhsInventoryView>(query))
			{
				if (inventoryItem.IsSerialNumberUsedOnThis(info.Value.ToString()) && !iParentConsumer.IsInventoryAdjustedOutOnSiblings(inventoryItem))
				{
					AddSerialNumberDuplicateError(client, info);
					break;
				}
			}

			if (!info.HasErrors() && !IsSerialNumberUniqueCore(client, part, info))
			{
				AddSerialNumberDuplicateError(client, info);
			}
		}

		void AddSerialNumberDuplicateError(OrgHeader client, ZPropertyInfo info)
		{
			if (client != null)
			{
				var messageToAppend = GetSerialNumberDuplicateErrorMessageToAppend(client.PartAttributeManager.SerialNumberName);
				var duplicateErrorMessage = Res.GetString("99dfd59b-5022-42bb-a0be-2b465c9e172e", "Serial # already used.");

				var errorMessage = messageToAppend.IsEmpty
					? duplicateErrorMessage
					: string.Format(Culture.Current, "{0} {1}", duplicateErrorMessage, messageToAppend);

				info.AddError(errorMessage);
			}
		}

		protected virtual ZString GetSerialNumberDuplicateErrorMessageToAppend(ZString partAttribName) => ZString.Empty;

		protected virtual bool IsReadyToCheckInventoryForDuplicateSerialNumber => true;

		protected virtual bool IsSerialNumberUniqueCore(OrgHeader client, OrgSupplierPart part, ZPropertyInfo info)
		{
			return true;
		}

		#endregion

		#region CheckQtyForSerialNumber

		// Tested in WhsDocketLine and WhsReceiveLine and Inventory and WhsReleaseLine Validation
		public void CheckQtyForSerialNumber(WhsProduct product, OrgHeader client, ZPropertyInfo qtyInfo, ZPropertyInfo serialNumberInfo, bool checkForReleaseSerial = false)
		{
			if (product != null && (ZDecimal)qtyInfo.Value > 1)
			{
				var hasSerialNumberEntered = CheckSerialNumberEntered(product, client, serialNumberInfo);

				var checkReleaseCapturedSerial = hasSerialNumberEntered &&
					(
						(checkForReleaseSerial && product.IsSerialNumberReleaseCaptured(client)) ||
						(!checkForReleaseSerial && !product.IsSerialNumberReleaseCaptured(client))
					);

				if (checkReleaseCapturedSerial)
				{
					qtyInfo.AddError(MustBeOneOrLessForSerialNumberProductsMessage);
				}
			}
		}

		static bool CheckSerialNumberEntered(WhsProduct product, OrgHeader client, ZPropertyInfo attributeInfo)
			=> !attributeInfo.Value.IsEmpty && product.IsSerialNumberUsed(client);

		public static string MustBeOneOrLessForSerialNumberProductsMessage => Res.GetString("91ae048f-1fb4-4b65-a40c-d9b82b0a275d", "Must always be 1 or less for serial number controlled products");

		#endregion

		#region ValidatePartAttribIsNotReleaseCapturedWithValue

		// Tested in WhsDocketline and Inventory and WhsStocktakeLine Validation
		public void ValidatePartAttribIsNotReleaseCapturedWithValue(WhsProduct product, OrgHeader client, ZPropertyInfo info, int attributeNumber)
		{
			if (!info.HasErrors() && product != null && !info.Value.IsEmpty && product.IsPartAttribReleaseCaptured(client, attributeNumber))
			{
				AddNotification(info, Res.GetString("2b82f689-990d-4d8c-a094-565fbd28be21",
					"This attribute is specified as Release Captured for this Product, no value should be entered."));
			}
		}

		#endregion

		#region ValidateSerialIsNotReleaseCapturedWithValue

		public void ValidateSerialIsNotReleaseCapturedWithValue(WhsProduct product, OrgHeader client, ZPropertyInfo info)
		{
			if (!info.HasErrors() && product != null && !info.Value.IsEmpty && product.IsSerialNumberReleaseCaptured(client))
			{
				AddNotification(info, Res.GetString("687843eb-3d65-44ac-98ff-ea11250ddc7a",
					"Serial Number is specified as Release Captured for this Product, no value should be entered."));
			}
		}

		#endregion

		#region ValidateExpiryDateAgainstExpiryNotificationPeriod

		// Tested in Inventory
		public void ValidateExpiryDateAgainstExpiryNotificationPeriod(OrgHeader client, WhsWarehouse warehouse, WhsProduct product, ZDate expiryDate, ZPropertyInfo info)
		{
			if (!info.HasErrors())
			{
				if (ExpiryDateLessThanExpiryNotificationPeriod(client, warehouse, product, expiryDate))
				{
					info.AddWarning(ExpiryDateLessThanExpiryNotificationPeriodMessage);
				}
			}
		}

		public static bool ExpiryDateLessThanExpiryNotificationPeriod(OrgHeader client, WhsWarehouse warehouse, WhsProduct product, ZDate expiryDate)
		{
			bool result = false;

			if (client != null && warehouse != null && product != null)
			{
				var param = product.GetParamsByWhsAndClient(warehouse, client);
				if (param != null && product.IsExpiryDateUsed(client) && param.W3_ExpiryNotificationPeriod > 0 && !expiryDate.IsEmpty)
				{
					var days = (expiryDate - ZDate.Today).Days;
					result = (days <= param.W3_ExpiryNotificationPeriod);
				}
			}

			return result;
		}

		#endregion

		#region ValidateCanCalculateExpiryDateIfJulianBatchNumberIsUsed

		// Tested in WhsDocketline and Inventory and WhsStocktakeLine Validation
		public void ValidateCanCalculateExpiryDateIfJulianBatchNumberIsUsed(OrgHeader client, WhsWarehouse whs, WhsProduct product, ZPropertyInfo info)
		{
			if (client != null && whs != null && product != null && !info.HasErrors())
			{
				if (product.IsAJulianBatchNumberAttributeUsed(client))
				{
					var param = product.GetParamsByWhsAndClient(whs, client);
					if (param == null || param.W3_MaximumShelfLife <= 0m)
					{
						AddNotification(info, JulianBatchNumberNoMaximumShelfLifeErrorMessage);
					}
				}
			}
		}

		#endregion

		#region CheckJulianBatchNumberAttributeFormat

		// Tested in WhsDocketline and Inventory and WhsStocktakeLine Validation
		public void CheckJulianBatchNumberAttributeFormat(OrgHeader client, WhsProduct product, ZPropertyInfo info, int attributeNumber)
		{
			if (client != null && product != null && !info.HasErrors() && product.IsPartAttributeAJulianBatchNumberAndUsed(client, attributeNumber) && !product.IsAValidJulianBatchNumberFormat(client, (ZString)info.Value))
			{
				AddNotification(info, JulianBatchNumberIncorrectFormatMessage);
			}
		}

		#endregion

		#region Error Messages

		public static string JulianBatchNumberIncorrectFormatMessage
		{
			get { return Res.GetString("54724a02-eb37-4f3d-8284-a0805c2b5eac", "This is not a valid Julian Batch Number format. Please ensure that the Julian Batch number specified matches the required format defined on the Product Master."); }
		}

		public static string JulianBatchNumberNoMaximumShelfLifeErrorMessage
		{
			get { return Res.GetString("4953fcf6-789b-4d62-bc74-37930dfb3442", "This product does not have Maximum Shelf Life defined for this client and warehouse."); }
		}

		public static string ExpiryDateLessThanExpiryNotificationPeriodMessage
		{
			get { return Res.GetString("BA30BDB8-6089-4C0F-970D-0B8543296590", "The Expiry Date allows for less days before expiry, than the nominated minimum shelf life requirement i.e. Expiry Notification Period (Days)."); }
		}

		#endregion
	}
}
