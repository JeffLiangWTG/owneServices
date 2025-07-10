using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging
{
	public class CMDDataValueWrapperCollection : NonPersistentBusinessObjectCollection<CMDDataValueWrapper>
	{
		public CMDDataValueWrapperCollection(CMDShipmentWrapper shipmentWrapper)
			: base(shipmentWrapper.Factory)
		{
			this.ShipmentWrapper = shipmentWrapper;
			this.Shipment = shipmentWrapper.Shipment;
		}

		public override void Load()
		{
			IsLoaded = true;
			ResetListsAndFactory();
			foreach (CMDPermitNumber cMDData in CMDDataValues)
			{
				Add(new CMDDataValueWrapper(cMDData));
			}
		}

		public void CopyChangesToShipmentFactory()
		{
			foreach (CMDDataValueWrapper entryNumberWrapper in this)
			{
				entryNumberWrapper.CommitChangesToCMDData();
				if (!CMDDataValues.Contains(entryNumberWrapper.CMDDataValue.PK))
				{
					var cusCodeDataInShipmentFactory = Factory.ImportFromAnotherFactory(entryNumberWrapper.CMDDataValue, typeof(CMDPermitNumber));
					CMDDataValues.Add(cusCodeDataInShipmentFactory);
				}
			}

			foreach (CMDPermitNumber elementToBeDeleted in ElementsToDeleteOnCommit)
			{
				CMDDataValues.RemoveAndDelete(elementToBeDeleted);
			}

			Shipment.HasChanges = true;
		}

		internal CMDPermitNumberCollection CMDDataValues
		{
			get
			{
				ShipmentWrapper.CMDDataValues.Load();
				return ShipmentWrapper.CMDDataValues;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var cmdData = FactoryForNewCMDDataValues.New<CMDPermitNumber>();
			cmdData.CY_ParentID = Shipment.PK;
			cmdData.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			return new CMDDataValueWrapper(cmdData);
		}

		public override void Remove(BusinessObject elementToRemove)
		{
			RemoveOverride(elementToRemove as CMDDataValueWrapper, false);
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var element = elementToDelete as CMDDataValueWrapper;
			if (element != null)
			{
				base.RemoveAndDelete(element);
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			CMDDataValueWrapper wrapper = (CMDDataValueWrapper)bizO;
			if (CMDDataValues.Contains(wrapper.CMDDataValue.PK))
			{
				ElementsToDeleteOnCommit.Add(wrapper.CMDDataValue);
			}
		}

		#region Implementation

		void ResetListsAndFactory()
		{
			fFactoryForNewCMDDataValues = null;
			ForceRemoveAll();
			ElementsToDeleteOnCommit.Clear();
		}

		internal void ForceRemoveAll()
		{
			foreach (CMDDataValueWrapper bizO in this.ToArray(typeof(CMDDataValueWrapper)))
			{
				RemoveOverride(bizO, true);
			}
		}

		void RemoveOverride(CMDDataValueWrapper element, bool forceRemove)
		{
			if (forceRemove || element != null)
			{
				base.Remove(element);
			}
		}

		internal List<CMDPermitNumber> ElementsToDeleteOnCommit
		{
			get
			{
				if (fElementsToDeleteOnCommit == null)
				{
					fElementsToDeleteOnCommit = new List<CMDPermitNumber>();
				}
				return fElementsToDeleteOnCommit;
			}
		}

		internal BusinessObjectFactory FactoryForNewCMDDataValues
		{
			get
			{
				if (fFactoryForNewCMDDataValues == null)
				{
					fFactoryForNewCMDDataValues = new BusinessObjectFactory();
				}
				return fFactoryForNewCMDDataValues;
			}
		}

		List<CMDPermitNumber> fElementsToDeleteOnCommit;
		BusinessObjectFactory fFactoryForNewCMDDataValues;

		#endregion

		public readonly CMDShipmentWrapper ShipmentWrapper;
		public readonly ForwardingShipment Shipment;
	}
}
