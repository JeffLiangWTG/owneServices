using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class CustomsDataSourceHelper<T> : IDocketType
		where T : WhsDocket
	{
		protected CustomsDataSourceHelper(TopLevelDataObject topLevelDataObject, IDataContextDataObject topLevelDataContext)
		{
			TopLevelDataObject = Argument.NotNull(topLevelDataObject, nameof(topLevelDataObject));
			TopLevelDataContext = Argument.NotNull(topLevelDataContext, nameof(topLevelDataContext));
		}

		TopLevelDataObject TopLevelDataObject { get; }
		IDataContextDataObject TopLevelDataContext { get; }

		string IDocketType.DocketType => DocketType;

		protected abstract string DocketType { get; }

		public bool IsDataSourceCustoms => CustomsDataSource != null;

		public ZString? CustomsJobNo => IsDataSourceCustoms ? CustomsDataSource.Key : null;

		public ZString CustomsParentReferenceToMatchForDocket => CustomsJobNo + "-" + TopLevelDataContext.DataProviderForCodeMapping;

		public bool IsWarehouseBondedChangeOfOwnership => TopLevelDataContext.IsWarehouseBondedChangeOfOwnership();

		public bool IsWarehouseBondedChangeOfRegime => TopLevelDataContext.IsWarehouseBondedChangeOfRegime();

		public bool IsWarehouseBondedChangeOfInventory => TopLevelDataContext.IsWarehouseBondedChangeOfInventory();

		public IEnumerable<WhsInventoryView> CancelOutDocketAndThrowImportFailureExceptionIfCancellingOutFailed(T docket)
		{
			var amendedInventory = CancelOutDocket(docket);

			// We want to cancel the Docket as it has been amended out, which in a virtual warehouse means it has been replaced.
			// Adding the event means the job was *successfully* cancelled, rather than just having the customs cancelled event applied.
			docket.Logs.AddNew(Events.Cancelled);

			return amendedInventory;
		}

		protected abstract IEnumerable<WhsInventoryView> CancelOutDocket(T docket);

		IDataSourceDataObject CustomsDataSource
		{
			get
			{
				if (!haveCalculatedCustomsDataSource)
				{
					haveCalculatedCustomsDataSource = true;

					var recipientRoles = TopLevelDataContext.RecipientRoleCollection;
					if (recipientRoles != null && recipientRoles.Any(x => x.Code == CustomsDeclarationRecipientRoleType || x.Code == RecipientRoleType.BCO || x.Code == RecipientRoleType.BCR))
					{
						customsDataSource = TopLevelDataObject.GetMatchingDataSource(DataContextType.CustomsDeclaration);
					}

					if (customsDataSource == null)
					{
						customsDataSource = TopLevelDataObject.GetMatchingDataSource(DataContextType.WarehouseInBond);
					}

					if (customsDataSource == null)
					{
						customsDataSource = TopLevelDataObject.GetMatchingDataSource(DataContextType.WarehouseCustomsEntry);
					}

					if (customsDataSource == null)
					{
						customsDataSource = TopLevelDataObject.GetMatchingDataSource(DataContextType.NctsHeader);
					}
				}
				return customsDataSource;
			}
		}
		IDataSourceDataObject customsDataSource;
		bool haveCalculatedCustomsDataSource;

		protected abstract RecipientRoleType CustomsDeclarationRecipientRoleType { get; }
	}
}
