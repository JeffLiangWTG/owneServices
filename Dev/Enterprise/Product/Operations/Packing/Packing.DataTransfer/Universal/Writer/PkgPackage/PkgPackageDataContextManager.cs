using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using EventConstants = CargoWise.EventReference.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Packing.DataTransfer.Universal
{
	public class PkgPackageDataContextManager : EventDataContextManager<PkgPackage>
	{
		#region DataContextKey

		public override ZString DataContextKey => ParentBO.KP_PackageID;

		#endregion

		#region DataContextType

		public override DataContextType DataContextType => DataContextType.PkgPackage;

		#endregion

		#region DefaultOutputDirectory

		public override string DefaultOutputDirectory => null;

		#endregion

		#region GetDataContextKeyMatchingQuery

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return ZQuery.NoResultQuery;
		}

		#endregion

		#region GetEventContextValues

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var contextValues = new List<KeyValuePair<TypeWithDescription, IZType>>();

			AddPackageContextValues(contextValues, ParentBO);

			var jobParent = ParentBO.PackageJob?.ParentJob;
			if (jobParent != null)
			{
				contextValues.AddRange(jobParent.GetAdditionalEventContextValuesFromParent());
			}

			return contextValues;
		}

		#region AddPackageContextValues

		void AddPackageContextValues(List<KeyValuePair<TypeWithDescription, IZType>> contextValues, PkgPackage package)
		{
			AddTransportBookingJobIDContextValue(contextValues);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportBookingPackageID, package.KP_PackageID);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.PackageSequence, package.KP_Sequence);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.PackageType, package.KP_F3_NKPackType);
			AddPackageDimensionContextValue(contextValues, package);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.WeightOfGoods, GetFormatedValueAndUnit(package.KP_Weight, package.KP_WeightUQ));
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.VolumeOfGoods, GetFormatedValueAndUnit(package.KP_Volume, package.KP_VolumeUQ));
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.PackageTypeUOM, package.PackType?.F3_UOMType ?? ZString.Empty);
			contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.NumberOfPieces, package.KP_PackageQty);
		}

		void AddTransportBookingJobIDContextValue(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			if (ParentBO.PackageJob?.ParentJob is IDtbConsignment consignment)
			{
				var booking = ParentBO.Factory.Load<IDtbBooking>(consignment.LTC_KM_Booking);

				if (booking != null)
				{
					contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.TransportBookingJobID, booking.KM_JobID);
				}
			}
		}

		void AddPackageDimensionContextValue(List<KeyValuePair<TypeWithDescription, IZType>> contextValues, PkgPackage package)
		{
			if (package.KP_Length > 0 || package.KP_Width > 0 || package.KP_Height > 0)
			{
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.DimensionUnit, package.KP_DimensionUQ);
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.Length, GetDecimalValueFormattedString(package.KP_Length));
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.Width, GetDecimalValueFormattedString(package.KP_Width));
				contextValues.AddIfNotEmpty(UniversalEvent.ContextTypes.Height, GetDecimalValueFormattedString(package.KP_Height));
			}
		}

		ZString GetFormatedValueAndUnit(ZDecimal value, ZString unit)
		{
			return value > 0
				? string.Format(CultureInfo.InvariantCulture, "{0} {1}", GetDecimalValueFormattedString(value), unit)
				: string.Empty;
		}

		ZString GetDecimalValueFormattedString(ZDecimal value)
		{
			return value.ToString("0.000", CultureInfo.InvariantCulture);
		}

		#endregion

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new PkgPackageEventParentFinder(factory, this, logger);
		}

		#endregion

		#region OnUniversalEventAdded

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			switch (eventAdded.EventType)
			{
				case AutoEvents.StatusUpdatedCode:
					UpdatePackageOnStatusUpdatedEvent(eventAdded, ParentBO);
					break;
				case AutoEvents.ScannedCode:
					VolCamScanData.PopulatePackageFromEvent(eventAdded, ParentBO);
					break;
			}
		}

		void UpdatePackageOnStatusUpdatedEvent(UniversalEvent eventAdded, PkgPackage package)
		{
			var referenceParameters = StmALog.GetParametersFromReference(eventAdded.EventReference);
			referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Type, out var eventReferenceType);

			switch (eventReferenceType)
			{
				case EventConstants.EventReferenceTypeTypes.Codes.CarrierBarcodeNumber:
					referenceParameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, out var eventReferenceNumber);
					if (string.IsNullOrWhiteSpace(eventReferenceNumber) || IsDuplicatePackageId(package, eventReferenceNumber))
					{
						throw new DataObjectReadFailureException(Res.GetString("685AED06-4AB0-442F-9B3A-523F3010CB03",
							"Package ID has not been updated because the Carrier Barcode Number is either empty or is already assigned to another package."));
					}
					else
					{
						package.KP_PreviousPackageID = package.KP_PackageID;
						package.KP_PackageID = eventReferenceNumber;
					}
					break;
			}
		}

		bool IsDuplicatePackageId(PkgPackage package, string barcodeNumber)
		{
			return package.PackageJob.GetAllPackagesOnJob().Any(jobPackage => jobPackage.KP_PackageID == barcodeNumber);
		}

		#endregion
	}
}
