using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Rating.Business
{
	[UniversalCopyWithExtendedEntities]
	public class RateOneOffContainers : AutoRateOneOffContainers
	{
		public RateOneOffContainers(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region TC_RC

		[List("Lookups.Containers")]
		public override ZGuid TC_RC
		{
			get { return base.TC_RC; }
			set { base.TC_RC = value; }
		}

		#endregion

		#region TC_ContainerCount

		public override ZShort TC_ContainerCount
		{
			get { return base.TC_ContainerCount; }
			set
			{
				if (base.TC_ContainerCount != value)
				{
					base.TC_ContainerCount = value;
				}
			}
		}

		#endregion

		#endregion

		#region New Properties

		public RefContainer RefContainer
		{
			get { return Factory.Load<RefContainer>(TC_RC); }
		}

		[DecimalPlaces(2)]
		public ZDecimal Calc_TEUCount
		{
			get
			{
				return RefContainer != null ? TC_ContainerCount * RefContainer.RC_TEU : 0;
			}
		}

		#endregion

		#region Parent One Off Shipment

		public RateOneOffShipment Parent
		{
			get { return fParent ?? Factory.Load<RateOneOffShipment>(TC_TT); }
			internal set { fParent = value; }
		}

		RateOneOffShipment fParent;

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion
	}
}

