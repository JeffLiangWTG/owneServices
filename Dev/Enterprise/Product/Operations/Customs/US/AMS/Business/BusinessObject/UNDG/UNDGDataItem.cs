using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class UNDGDataItem : MasterFiles.Business.UNDGDataItem,
		ISailingSynchronisationTarget<MasterFiles.Business.UNDGDataItem>
	{
		public UNDGDataItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		CusInBondContainer Container
		{
			get { return fContainer ?? (fContainer = Factory.Load<CusInBondContainer>(this.DI_ParentID)); }
		}
		CusInBondContainer fContainer;

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return "Hazardous Detail"; }
		}

		public new UNDGDataItemValidation Validation
		{
			get { return (UNDGDataItemValidation)base.Validation; }
		}

		protected override MasterFiles.Business.UNDGDataItemValidation GetNewValidation()
		{
			return new UNDGDataItemValidation(this);
		}

		#endregion

		#region ISailingSynchronisationTarget<UNDGDataItem> Members

		bool IsMatched(MasterFiles.Business.UNDGDataItem sailingUNDG)
		{
			var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Container.Bill).Source;
			if (sailingBill != null)
			{
				if (sailingBill.IsContainerised)
				{
					var sailingContainer = ((ISailingSynchronisationTarget<BillOfLadingContainer>)Container).Source;
					return sailingContainer != null && sailingContainer.PackLines.Cast<BillOfLadingPackLine>().Any(x => x.UNDGs.Contains(sailingUNDG))
						&& this.DI_DG == sailingUNDG.DI_DG;
				}
				else if (sailingBill.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
				{
					return sailingBill.Vehicles.Cast<AgencyShipmentContainer>().Any(x => x.UNDGs.Contains(sailingUNDG))
						&& this.DI_DG == sailingUNDG.DI_DG;
				}
				else
				{
					return sailingBill.TopLevelPacks.Cast<AgencyShipmentContainer>().Any(x => x.UNDGs.Contains(sailingUNDG))
						&& this.DI_DG == sailingUNDG.DI_DG;
				}
			}
			return false;
		}

		bool ISailingSynchronisationTarget<MasterFiles.Business.UNDGDataItem>.IsMatched(MasterFiles.Business.UNDGDataItem sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		void ISailingSynchronisationTarget<MasterFiles.Business.UNDGDataItem>.Set(MasterFiles.Business.UNDGDataItem sailingTarget)
		{
			this.DI_DG = sailingTarget.DI_DG;
		}

		void ISailingSynchronisationTarget<MasterFiles.Business.UNDGDataItem>.Synchronise()
		{
			var sailingUNDG = SailingSynchronisationSource;
			if (sailingUNDG != null)
			{
				this.DI_DGFlashPoint = sailingUNDG.DI_DGFlashPoint;
				this.DI_OC_DGContact = sailingUNDG.DI_OC_DGContact;
				this.DI_TechnicalName = sailingUNDG.DI_TechnicalName;
			}
		}

		MasterFiles.Business.UNDGDataItem ISailingSynchronisationTarget<MasterFiles.Business.UNDGDataItem>.Source
		{
			get { return SailingSynchronisationSource; }
		}

		MasterFiles.Business.UNDGDataItem SailingSynchronisationSource
		{
			get
			{
				if (fSailingSynchronisationSource != null && IsMatched(fSailingSynchronisationSource))
				{
					return fSailingSynchronisationSource;
				}
				fSailingSynchronisationSource = null;
				var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Container.Bill).Source;
				if (sailingBill != null)
				{
					if (sailingBill.IsContainerised)
					{
						var sailingContainer = ((ISailingSynchronisationTarget<BillOfLadingContainer>)Container).Source;
						if (sailingContainer != null)
						{
							fSailingSynchronisationSource = sailingContainer.PackLines.Cast<BillOfLadingPackLine>().SelectMany(x => x.UNDGs).FirstOrDefault(x => IsMatched(x));
						}
					}
					else if (sailingBill.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
					{
						fSailingSynchronisationSource = sailingBill.Vehicles.Cast<AgencyShipmentContainer>().SelectMany(x => x.UNDGs).FirstOrDefault(x => IsMatched(x));
					}
					else
					{
						fSailingSynchronisationSource = sailingBill.TopLevelPacks.Cast<AgencyShipmentContainer>().SelectMany(x => x.UNDGs).FirstOrDefault(x => IsMatched(x));
					}
				}
				return fSailingSynchronisationSource;
			}
		}
		MasterFiles.Business.UNDGDataItem fSailingSynchronisationSource;

		internal bool HasSailingLinkage
		{
			get
			{
				var container = Container;
				return container != null && container.HasSailingLinkage;
			}
		}

		#endregion
	}
}
