
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyPackLine : CFSPackLine
	{
		public TallyPackLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Related Business Objects

		public new PackUnpackShipment Shipment
		{
			get { return (PackUnpackShipment)base.Shipment; }
		}

		public new TallyPackLocationCollection PackLocations
		{
			get { return (TallyPackLocationCollection)base.PackLocations; }
		}

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<PackUnpackShipment>(JL_JS);
		}

		protected TallyContainer CurrentContainer
		{
			get
			{
				TallyContainer container = null;
				if (Shipment != null)
				{
					container = Shipment.CurrentContainer;
				}
				return container;
			}
		}

		protected override CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			return new TallyContainerManyToManyCollection(this);
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<TallyContainer>(containerPK);
		}

		protected override PackLocationCollection GetNewPackLocationCollection()
		{
			return new TallyPackLocationCollection(this, Factory);
		}

		#endregion

		#region BusinessObject Overrides

		protected override void RunPreSaveValidationCore()
		{
			int total = 0;

			foreach (PackLocation location in PackLocations)
			{
				total += location.JQ_NoPackages;
				location.ClearAllNotifications();
			}

			if (total != JL_Outturn)
			{
				foreach (PackLocation location in PackLocations)
				{
					location.JQ_NoPackagesInfo.AddError(
						Res.GetString("1d14b0e2-e669-47a2-975a-8bf6be566dff", "The total number of warehoused packages is not equal to the number of packages outturned."));
				}
			}
		}

		public override ZInt JL_Outturn
		{
			get
			{
				return base.JL_Outturn;
			}
			set
			{
				if (value != JL_Outturn)
				{
					base.JL_Outturn = value;
					if (CurrentContainer != null && CurrentContainer.JC_LCLUnpack.IsEmpty && value != 0)
					{
						CurrentContainer.JC_LCLUnpack = ZDateTime.Now;
					}
					UpdateCustomsLink();
				}
			}
		}

		public override ZInt JL_Pillaged
		{
			get
			{
				return base.JL_Pillaged;
			}
			set
			{
				base.JL_Pillaged = value;
				UpdateCustomsLink();
			}
		}

		public override ZInt JL_Damaged
		{
			get
			{
				return base.JL_Damaged;
			}
			set
			{
				base.JL_Damaged = value;
				UpdateCustomsLink();
			}
		}

		public override ZString JL_F3_NKPackType
		{
			get
			{
				return base.JL_F3_NKPackType;
			}
			set
			{
				base.JL_F3_NKPackType = value;
				UpdateCustomsLink();
			}
		}

		public override ZGuid JL_JS
		{
			get
			{
				return base.JL_JS;
			}
			set
			{
				var hasChanged = JL_JS != value;
				base.JL_JS = value;
				if (hasChanged)
				{
					Containers.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		void UpdateCustomsLink()
		{
			if (Shipment != null)
			{
				Shipment.NotifyCustomsListener();
			}
		}

		#endregion

		protected override bool JL_UnitOfDimension_ReadOnly
		{
			get { return true; }
		}

		protected override bool JL_ActualVolumeUQ_ReadOnly
		{
			get { return true; }
		}

		protected override bool JL_ActualWeightUQ_ReadOnly
		{
			get { return true; }
		}
	}
}
