using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class VisitedPort : CusCodeData, IShortSequenceNumberLine
	{
		public VisitedPort(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		#region Override

		public new VisitedPortValidation Validation => (VisitedPortValidation)base.Validation;
		protected override CusCodeDataValidation GetNewValidation()
		{
			if (Parent?.GetType() == typeof(AsycudaManifestHeader))
			{
				return new VisitedPortValidationForHeader(this);
			}
			return new VisitedPortValidation(this);
		}
		public new VisitedPortLookups Lookups => (VisitedPortLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new VisitedPortLookups(this);

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaBill), typeof(AsycudaManifestHeader));

		#endregion

		[List(nameof(Lookups) + "." + nameof(VisitedPortLookups.VisitedPortList))]
		[ResourceStringData("VisitedPorts.CY_Code", Caption = "Port(TR)")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set => base.CY_Code = value;
		}

		[List(nameof(Lookups) + "." + nameof(VisitedPortLookups.UNLOCOCodeList))]
		[ResourceStringData("VisitedPorts.TRPort", Caption = "Port(UNLOCO)")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = base.CY_Data;
				base.CY_Data = value;
				if (!IsCopying && oldValue != value && IsTRPort(CY_Data) && VisitedPortParent.SupportsCustomsPorts)
				{
					var localPortCode = DefaultTRLocalPortFromUnLocoPort();
					if (!localPortCode.IsEmpty && VisitedPortParent.SupportsCustomsPorts)
					{
						CY_Code = localPortCode;
					}
					else
					{
						CY_Code = string.Empty;
					}
				}
			}
		}
		public RefUNLOCO PortOfUNLOCO
		{
			get
			{
				if (!string.IsNullOrEmpty(CY_Data))
				{
					return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CY_Data);
				}
				return null;
			}
		}

		ZString DefaultTRLocalPortFromUnLocoPort()
		{
			var result = ZString.Empty;
			var unlocoPort = PortOfUNLOCO;
			if (unlocoPort != null)
			{
				var portParent = VisitedPortParent;
				result = portParent.ManifestHeader?.GetCustomsLocalCodeList(unlocoPort).FirstOrDefault()?.RY_LocalPortCode ?? ZString.Empty;
			}
			return result;
		}

		public bool IsTRPort(ZString portCode)
		{
			return portCode.StartsWith(Core.Constants.CountryCodes.Turkey, System.StringComparison.Ordinal);
		}

		[ResourceStringData("VisitedPorts.CY_Date", Caption = "Departure Date", ShortCaption = "Departure")]
		public override ZDateTime CY_Date { get => base.CY_Date; set => base.CY_Date = value; }

		#region IZShortSequenceNumberLine

		ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => CY_Order; set => CY_Order = value; }

		[ResourceStringData("VisitedPorts.CY_Order", Caption = "Leg Number", ShortCaption = "Leg")]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					VisitedPortParent?.VisitedPorts.SequenceNumberCalculator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		public override ZGuid CY_ParentID
		{
			get => base.CY_ParentID;
			set
			{
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID && !CY_ParentID.IsValid)
				{
					DetachedFromParent(oldValue);
				}
			}
		}

		public override ZString CY_ParentTableCode
		{
			get => base.CY_ParentTableCode;
			set
			{
				var oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode && !CY_ParentTableCode.IsEmpty)
				{
					AttachedToParent();
				}
			}
		}

		void DetachedFromParent(ZGuid oldValue)
		{
			var visitedPortParent = parentLoaders.LoadBusinessObject(Factory, CY_ParentTableCode, oldValue) as IVisitedPortParent;
			if (visitedPortParent != null)
			{
				visitedPortParent.VisitedPorts.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedToParent()
		{
			var visitedPortParent = VisitedPortParent;
			if (visitedPortParent != null)
			{
				visitedPortParent.VisitedPorts.SequenceNumberCalculator.RecalculateWhenAdded(this);
			}
		}

		public IVisitedPortParent VisitedPortParent
		{
			get { return Parent as IVisitedPortParent; }
		}

		#endregion
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.TRVisitedPort;
		}
	}
}
