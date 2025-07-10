using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	public class LocalTransportLegDataContextManager : EventDataContextManager<CommonCartageLeg>
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.LocalTransportLeg; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.UniqueIDWithJobNumber; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var dataContextKey = matchingValues.Key;
			var indexOfSlash = dataContextKey.LastIndexOf("/", StringComparison.Ordinal);
			var jobNumber = dataContextKey.SubstringSafe(0, indexOfSlash);
			var legNumber = dataContextKey.SubstringSafe(indexOfSlash + 1);

			var cartageSubQuery = new ZDBOnlySubQuery(typeof(CommonCartage), JobBookedCtgMoveSchema.EW_JJ);
			cartageSubQuery.AddToFilter(JobCartageSchema.JJ_ConsignmentID, jobNumber);

			var bookedSubQuery = new ZDBOnlySubQuery(typeof(CommonBookedCtgMove), JobContainerLegsSchema.JU_EW);
			bookedSubQuery.AddSubQuery(cartageSubQuery, JoinCondition.And);

			var cartageLegQuery = new ZDBOnlyQuery(typeof(CommonCartageLeg));
			cartageLegQuery.AddToFilter(JobContainerLegsSchema.JU_SplitDeliverySuffix, legNumber);
			cartageLegQuery.AddSubQuery(bookedSubQuery, JoinCondition.And);

			return cartageLegQuery;
		}

		/// <summary>
		/// Does not currently support incoming events.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new LocalTransportLegEventParentFinder(factory, this, logger);
		}

		/// <summary>
		/// When exporting events, add these contexts so the importer can match.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();

			if (ParentBO != null)
			{
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportBookingJobID, ParentBO.Cartage.TransportBookingPartyReference);
				result.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportReference, ParentBO.Cartage.JJ_ConsignmentID);

				if (ParentBO.IsContainerised)
				{
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.ContainerNumber, ParentBO.Container.JC_ContainerNum);
				}
				else
				{
					var move = ParentBO.BookedCtgMove;
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.NumberOfPieces, move.EW_BookedPackCount);
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.PackageType, move.EW_F3_NKPackType);
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.WeightOfGoods, GetFormatedValueAndUnit(move.EW_BookedWeight, move.EW_WeightUQ));
					result.AddIfNotEmpty(UniversalEvent.ContextTypes.VolumeOfGoods, GetFormatedValueAndUnit(move.EW_BookedVolume, move.EW_VolumeUQ));
				}

				result.AddIfNotEmpty(UniversalEvent.ContextTypes.ReceivedFromName, ParentBO.JU_DeliverySignedFor);
			}

			return result;
		}

		ZString GetFormatedValueAndUnit(ZDecimal value, ZString unit)
		{
			return ZString.Join(" ", new ZString[] { value.ToString(), unit });
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}
	}
}
