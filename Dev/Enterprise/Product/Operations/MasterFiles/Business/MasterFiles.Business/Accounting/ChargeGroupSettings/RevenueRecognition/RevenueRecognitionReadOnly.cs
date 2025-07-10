using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RevenueRecognitionReadOnly : JobConfigurationSelectorReadOnly
	{
		public RevenueRecognitionReadOnly(IRevenueRecognition parent) : base(parent)
		{
		}

		protected new IRevenueRecognition Parent
		{
			get { return (IRevenueRecognition)base.Parent; }
		}

		#region Broker

		public bool BrokerCode_ReadOnly
		{
			get
			{
				bool result = Parent.JobType != "SHP" || (Parent.DirectionCode != Constants.FreightShipmentDirection.Code.Export && Parent.DirectionCode != Constants.FreightShipmentDirection.Code.Import);
#if DEBUG
				if (Globals.IsTest && !Parent.JobTypeList.ContainsCode(Parent.JobType))
				{
					result = false;
				}
#endif
				return result;
			}
		}

		#endregion

		#region Offset

		public bool Offset_ReadOnly
		{
			get
			{
				return Parent.RecognitionDateOptionCode == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			}
		}

		#endregion

		#region Offset Type

		public bool OffsetType_ReadOnly
		{
			get
			{
				return Parent.RecognitionDateOptionCode == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			}
		}

		#endregion

	}
}
