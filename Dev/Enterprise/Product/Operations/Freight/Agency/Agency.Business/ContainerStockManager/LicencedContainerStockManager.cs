using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal class LicencedContainerStockManager : ContainerStockManager
	{
		public LicencedContainerStockManager(AgencyShipmentContainer container)
			: base(container) { }

		protected override void ContainerNumberChangingCore(ZString oldValue, ZString newValue)
		{
			RefContainerStock oldStock = RefContainerStock.Load(Factory, oldValue);
			RefContainerStock newStock = RefContainerStock.Load(Factory, newValue);

			if (oldStock != null && !oldStock.IsInDatabase)
			{
				if (newStock == null && IsContainerNumberAcceptable(newValue))
				{
					newStock = oldStock;
					newStock.R6_ContainerNum = newValue;
				}
				else
				{
					oldStock.Delete();
				}
			}

			if (IsContainerNumberAcceptable(newValue))
			{
				if (newStock != null)
				{
					Container.JC_RC = newStock.R6_RC;
					Container.JC_IsShipperOwned = (newStock.R6_OwnerType == Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned);
				}
				else if (Container.Container != null)
				{
					RefContainerStock stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = newValue;
					stock.R6_RC = Container.JC_RC;
					stock.R6_OwnerType = Container.JC_IsShipperOwned ? Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned : "";
				}
			}
		}

		protected override void ContainerTypeChangingCore(ZGuid oldValue, ZGuid newValue)
		{
			RefContainerStock stock = RefContainerStock.Load(Factory, Container.JC_ContainerNum);
			RefContainer type = Factory.Load<RefContainer>(newValue);

			if (stock != null)
			{
				if (!stock.IsInDatabase)
				{
					if (type == null)
					{
						stock.Delete();
					}
					else
					{
						stock.R6_RC = newValue;
					}
				}
			}
			else if (type != null && IsContainerNumberAcceptable(Container.JC_ContainerNum))
			{
				stock = Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = Container.JC_ContainerNum;
				stock.R6_RC = newValue;
				stock.R6_OwnerType = Container.JC_IsShipperOwned ? Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned : "";
			}
		}

		protected override void IsShipperOwnedChangingCore(ZBool oldValue, ZBool newValue)
		{
			RefContainerStock stock = RefContainerStock.Load(Factory, Container.JC_ContainerNum);

			if (stock != null && !stock.IsInDatabase)
			{
				stock.R6_OwnerType = newValue ? Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned : "";
			}
		}

		protected override JobContainerValidation CreateExtraValidationCore()
		{
			return new ContainerStockManagerContainerValidation(Container);
		}

		bool IsContainerNumberAcceptable(string containerNumber)
		{
			if (string.IsNullOrEmpty(containerNumber) || containerNumber.Length > RefContainerStockSchema.R6_ContainerNum.MaxLength)
			{
				return false;
			}
			else
			{
				return ContainerNumberValidation.IsValidContainerNumber(containerNumber)
					|| AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.Value;
			}
		}
	}
}
