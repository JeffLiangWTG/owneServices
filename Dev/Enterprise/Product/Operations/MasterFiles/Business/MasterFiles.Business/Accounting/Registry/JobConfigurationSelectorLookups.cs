using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class JobConfigurationSelectorLookups : ZLookups
	{
		public JobConfigurationSelectorLookups(IJobConfigurationSelector parent)
			: base((BusinessObject)parent)
		{
		}

		protected new IJobConfigurationSelector Parent => base.Parent as IJobConfigurationSelector;

		#region Job Type List

		public CodeDescriptionPairList JobTypeList => jobTypeList ?? (jobTypeList = GetJobTypeList());
		CodeDescriptionPairList jobTypeList;

		protected virtual CodeDescriptionPairList GetJobTypeList() => GetBaseJobTypeList();

		public static CodeDescriptionPairList GetBaseJobTypeList()
		{
			var result = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
			result.Insert(0, new AllJobsConsumerType());
			return result;
		}

		public static class JobTypeAdditionalCodes
		{
			public const string All = "ALL";
		}

		#endregion

		#region Direction List

		public virtual CodeDescriptionPairList DirectionList => directionList ?? (directionList = GetBaseDirectionList());
		CodeDescriptionPairList directionList;

		public static CodeDescriptionPairList GetBaseDirectionList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All);
			result.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
			result.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
			result.AddPair(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic);
			result.AddPair(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other);
			return result;
		}

		#endregion

		#region Mode List

		public virtual CodeDescriptionPairList ModeList => GetModeListForJobType(Parent.JobType);

		CodeDescriptionPairList GetModeListForJobType(string jobType)
		{
			var modeList = GetBaseModeList();

			if (jobType == JobInvoicingConsumerTypes.QuotedBookingCode)
			{
				modeList.RemoveCode(Constants.TransportModes.AirSea);
				modeList.RemoveCode(Constants.TransportModes.SeaAir);
			}

			return modeList;
		}

		public static CodeDescriptionPairList GetBaseModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.TransportType);
			result.Insert(0, new CodeDescriptionPair(ModeAdditionalCodes.All, ModeAdditionalDescriptions.All));
			return result;
		}

		public static class ModeAdditionalCodes
		{
			public const string All = "ALL";
		}

		public static class ModeAdditionalDescriptions
		{
			public static string All
			{
				get { return Res.GetString("24a1b8e8-ee30-477f-821a-b3e766be8ed8", "Any Transport Mode"); }
			}
		}

		#endregion
	}
}
