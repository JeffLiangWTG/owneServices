using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business
{
	public class QueryUserSelectVesselFromLloydsNumber : NonPersistentBusinessObject, IObsoleteValidation, IQueryUserEventArgs
	{
		public QueryUserSelectVesselFromLloydsNumber(BusinessObjectFactory factory, ZString vesselName, ZString lloydsNumber)
			: base(factory)
		{
			fVesselName = vesselName;
			fLloydsNumber = lloydsNumber;
		}

		public RefVessel SelectedVessel;

		#region VesselName

		public ZString VesselName
		{
			get { return fVesselName; }
		}
		readonly ZString fVesselName;

		public ZPropertyInfo VesselNameInfo
		{
			get { return GetZPropertyInfo(nameof(VesselName)); }
		}

		#endregion

		#region LloydsNumber

		public ZString LloydsNumber
		{
			get { return fLloydsNumber; }
		}
		readonly ZString fLloydsNumber;

		public ZPropertyInfo LloydsNumberInfo
		{
			get { return GetZPropertyInfo(nameof(LloydsNumber)); }
		}

		#endregion

		#region AvailableVessels

		public RefVesselCollection AvailableVessels
		{
			get
			{
				if (fAvailableVessels == null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(RefVesselSchema.RV_Code, VesselName);
					query.AddToFilter(JoinCondition.Or, RefVesselSchema.RV_LloydsNumber, LloydsNumber);

					fAvailableVessels = new RefVesselCollection(Factory);
					fAvailableVessels.SetReadOnlyIncludingChildren(true);
					fAvailableVessels.AdditionalFilter = query;
				}
				return fAvailableVessels;
			}
		}
		RefVesselCollection fAvailableVessels;

		#endregion
	}
}
