using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eManifest.Business
{
	public class LodgedELoadListCollection : ActiveBusinessObjectCollection<ELoadList>
	{
		public LodgedELoadListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AdditionalFilter.AddToFilter(ELoadListSchema.DO_Status, Constants.ELoadListStatuses.Lodged);

			var bookingLinesQuery = new ZDBOnlySubQuery(typeof(SupplierBookingLine), SupplierBookingLineSchema.DL_DO_LoadList);
			var eLoadListsWithLinesQuery = new ZDBOnlyQuery(typeof(ELoadList));
			eLoadListsWithLinesQuery.AddSubQuery(bookingLinesQuery, JoinCondition.And);

			AdditionalFilter.AddToFilter(eLoadListsWithLinesQuery);
		}

		public LodgedELoadListCollection(BusinessObjectFactory factory, CommonConsol consol)
			: this(factory)
		{
			Argument.NotNull(consol, "consol");

			attachedELoadLists = GetAttachedELoadLists(Factory, consol);

			AdditionalFilter.AddToFilter(ELoadListSchema.PK, SQLComparisonOperator.NotEqual, attachedELoadLists);
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			var eLoadList = (ELoadList)selectedBusinessObject;

			if (eLoadList.DO_Status != Constants.ELoadListStatuses.Lodged)
			{
				errors.Add(Res.GetString("824b03e3-e5ca-4f88-9fe2-c58740bbe459", "Only 'Lodged' eLoadLists can be selected."));
			}
			else
			{
				var bookingLine = Factory.LoadTop1<SupplierBookingLine>(new ZQuery(SupplierBookingLineSchema.DL_DO_LoadList, eLoadList.PK));
				if (bookingLine == null)
				{
					errors.Add(Res.GetString("944365fb-9be9-4264-b9ed-f64690718437", "The eLoadList is empty."));
				}
				else if (attachedELoadLists != null && attachedELoadLists.Contains(eLoadList.PK))
				{
					errors.Add(Res.GetString("8a3b6281-a6d4-48f3-a175-99dac4d4e9bc", "The eLoadList is already attached."));
				}
			}
		}

		static IEnumerable<ZGuid> GetAttachedELoadLists(BusinessObjectFactory factory, CommonConsol consol)
		{
			var bookingLinesQuery = new ZQuery(SupplierBookingLineSchema.DL_JS_ApprovedShipment, consol.Shipments.Select(s => s.PK));
			var relatedBookingLines = factory.Load<SupplierBookingLine>(bookingLinesQuery);

			var eLoadListPKs = relatedBookingLines.Select(l => l.DL_DO_LoadList).Distinct().ToList();
			return eLoadListPKs;
		}

		readonly IEnumerable<ZGuid> attachedELoadLists;
	}
}
