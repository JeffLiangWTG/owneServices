using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	public abstract class TemplateRecordZController : ZController
	{
		protected virtual bool IsQuotedBooking => false;

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			var bizo = base.LoadBusinessEntity(factory, sourceEntityPK);
			if (bizo == null && ModuleID != null)
			{
				using (var module = ZModuleFactory.Instance.Create(ModuleID))
				{
					if (module is ZFilterGridModule filterGridModule && ((IZFilterGridModule)filterGridModule).AllowTemplateRecords)
					{
						bizo = filterGridModule.LoadFromTemplateRecordPk(factory, sourceEntityPK);
					}
				}
			}
			return bizo;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			if (sourceEntity is ViewQuotedBooking viewQuotedBooking && viewQuotedBooking.QuotedBooking != null)
			{
				sourceEntity = viewQuotedBooking.QuotedBooking;
			}

			if (sourceEntity is ITemplateRecord templateRecord)
			{
				return GetLoadedBusinessEntityWithTemplateRecord(templateRecord);
			}
			else if (
				sourceEntity is ITemplateRecordProvider templateRecordProvider &&
				templateRecordProvider != null &&
				templateRecordProvider.IsTemplateRecord &&
				templateRecordProvider.TemplateRecord != null
			)
			{
				templateRecord = templateRecordProvider.TemplateRecord;
				return GetLoadedBusinessEntityWithTemplateRecord(templateRecord);
			}

			if (!IsQuotedBooking)
			{
				return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}

			var quotedBooking = sourceEntity as QuotedBooking;
			if (quotedBooking == null || (quotedBooking.Booking != null || quotedBooking.IsInDatabase))
			{
				return GetLoadedQuotedBooking(quotedBooking);
			}
			else
			{
				return base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
			}
		}

		BusinessObject GetLoadedQuotedBooking(QuotedBooking quotedBooking)
		{
			if (quotedBooking != null && quotedBooking.Factory != Factory)
			{
				var quotePK = quotedBooking.Quote?.PK ?? ZGuid.Empty;
				var bookingPK = quotedBooking.Booking?.PK ?? ZGuid.Empty;
				return QuotedBooking.New(quotePK, bookingPK, Factory);
			}

			return quotedBooking;
		}

		BusinessObject GetLoadedBusinessEntityWithTemplateRecord(ITemplateRecord templateRecordInterface)
		{
			if (templateRecordInterface is StmTemplateRecord templateRecord)
			{
				var factory = new TemplateRecordBusinessObjectFactory();
				SetStrategyProvider(factory);

				var localTemplateRecord = factory.TemplateRecordFactory.Load<StmTemplateRecord>(templateRecord.PK);
				if (localTemplateRecord != null)
				{
					var localBizObj = IsQuotedBooking
						? QuotedBooking.New(factory, localTemplateRecord)
						: factory.New(TypeOfTopLevelBusinessObject);

					if (localBizObj is ITemplateRecordProvider localTemplateRecordProvider)
					{
						if (!IsQuotedBooking)
						{
							localTemplateRecordProvider.LoadFromTemplateRecord(localTemplateRecord);
						}

						factory.TemplateRecordProvider = localTemplateRecordProvider;

						return localBizObj;
					}
				}
			}

			return null;
		}
	}
}
