using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(RefFacility.Schema.RFT_Code)]
	[DescriptionProperty(RefFacility.Schema.RFT_Name)]

	public class RefFacility : AutoRefFacility
	{
		public RefFacility(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected RefFacilityLocalCodeCollection fRefFacilityLocalCodes;

		#region RFT_FacilityType

		[List("Lookups.FacilityTypes")]
		public override ZString RFT_FacilityType
		{
			get { return base.RFT_FacilityType; }
			set { base.RFT_FacilityType = value; }
		}

		#endregion

		#region RFT_TerminalType
		public ZString RFT_TerminalType
		{
			get
			{
				if (RFT_IsAir)
				{
					return Core.Constants.TransportModes.Air;
				}
				else if (RFT_IsSea)
				{
					return Core.Constants.TransportModes.Sea;
				}
				else if (RFT_IsRail)
				{
					return Core.Constants.TransportModes.Rail;
				}
				else if (RFT_IsRoad)
				{
					return Core.Constants.TransportModes.Road;
				}
				else if (RFT_IsInlandWaterway)
				{
					return Core.Constants.TransportModes.InlandWaterwayTransport;
				}
				return ZString.Empty;
			}
		}

		#endregion

		#region Lookups RFT_Code

		[List("Lookups.RefFacilities")]
		public override ZString RFT_Code
		{
			get { return base.RFT_Code; }
			set { base.RFT_Code = value; }
		}

		#endregion

		#region State

		public RefCountryStates State
		{
			get
			{
				if (!RFT_State.IsEmpty)
				{
					ZQuery query = new ZQuery(RefCountryStatesSchema.RW_Code, RFT_State);
					query.AddToFilter(RefCountryStatesSchema.RW_RN_NKCountryCode, RFT_RN_NKCountryCode);
					return Factory.LoadTop1<RefCountryStates>(query);
				}
				return null;
			}
		}

		public ZString RFT_StateCodeDesc
		{
			get
			{
				return State?.RW_Code + " - " + State?.RW_Description;
			}
		}

		#endregion

		#region Location

		public ZString RFT_CountryCodeDesc
		{
			get
			{
				return Country?.Code + " - " + Country?.Description;
			}
		}

		public ZString RFT_UNLOCODesc
		{
			get
			{
				return LocationCode?.Code + " - " + LocationCode?.Description;
			}
		}

		#endregion

		#region FacilityTypeFullName

		public ZString RFT_FacilityTypeFullName
		{
			get
			{
				switch (base.RFT_FacilityType)
				{
					case Constants.FacilityType.Code.Terminal:
						return Constants.FacilityType.Code.Terminal + " - " + Constants.FacilityType.Description.Terminal;
					case Constants.FacilityType.Code.ContainerYard:
						return Constants.FacilityType.Code.ContainerYard + " - " + Constants.FacilityType.Description.ContainerYard;
					case Constants.FacilityType.Code.TransitWarehouse:
						return Constants.FacilityType.Code.TransitWarehouse + " - " + Constants.FacilityType.Description.TransitWarehouse;
					default:
						return base.RFT_FacilityType;
				}
			}
		}

		#endregion

		[ChildEditable(true)]
		public RefFacilityLocalCodeCollection RefFacilityLocalCodes
		{
			get
			{
				if (fRefFacilityLocalCodes == null)
				{
					fRefFacilityLocalCodes = new RefFacilityLocalCodeCollection(this);
					RegisterEditableChildObject(fRefFacilityLocalCodes);
				}

				return fRefFacilityLocalCodes;
			}
		}

		protected override ZString HumanReadableNameCore => Res.GetString("dce592e3-f95f-4135-89c8-55c61b545fa8", "Facility ({0})", RFT_Code);

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
		}

#endif
	}
}
