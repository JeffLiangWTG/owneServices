using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentPackLine : PackLine, Integration.Agency.IAgencyPackLine
	{
		public AgencyShipmentPackLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fJL_JC = ZGuid.Missing;
		}

		#region Validation

		public new AgencyShipmentPackLineValidation Validation
		{
			get { return (AgencyShipmentPackLineValidation)base.Validation; }
		}

		protected override JobPackLinesValidation GetNewValidation()
		{
			return new AgencyShipmentPackLineValidation(this);
		}

		#endregion

		#region Properties

		#region UNDG PK

		// This is only used by ShippingBasePage in WebTracker
		public ZGuid FirstUNDGPK
		{
			get { return UNDGs.Count > 0 && UNDGs[0].Substance != null ? UNDGs[0].Substance.PK : ZGuid.Empty; }
			set
			{
				UNDGSubstance substance = Factory.Load<UNDGSubstance>(value);
				if (substance != null)
				{
					if (UNDGs.Count == 0)
					{
						UNDGs.AddNew().DI_DG = substance.PK;
					}
					else
					{
						UNDGs[0].DI_DG = substance.PK;
					}
				}

				FirstUNDGPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FirstUNDGPKInfo
		{
			get { return GetZPropertyInfo(nameof(FirstUNDGPK)); }
		}

		#endregion

		#region JL_JC

		[RelatedBusinessObject("Container")]
		[List("Lookups.Containers")]
		public override ZGuid JL_JC
		{
			get
			{
				if (fJL_JC.IsMissing)
				{
					if (Containers.Count > 0)
					{
						fJL_JC = Containers[0].PK;
					}
					else
					{
						fJL_JC = ZGuid.Empty;
					}
				}

				return fJL_JC;
			}
			set
			{
				if (fJL_JC != value)
				{
					fJL_JC = value;
					Containers.RemoveAll();
					Containers.AddFromDatabase(fJL_JC);
					JL_JCInfo.RefreshBinding();
					if (!IsValidationSuspended)
					{
						Validation.ValidateJL_JC();
					}
				}
			}
		}
		ZGuid fJL_JC;

		#endregion

		#region JL_JS

		public override ZGuid JL_JS
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JL_JS; }
			set
			{
				if (Shipment != null)
				{
					Shipment.MarkAsNeedingValidation();
				}

				base.JL_JS = value;

				if (Shipment != null)
				{
					Shipment.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#endregion

		#region Lookups

		public new AgencyShipmentPackLineLookups Lookups
		{
			get { return (AgencyShipmentPackLineLookups)base.Lookups; }
		}

		protected override JobPackLinesLookups GetNewLookups()
		{
			return new AgencyShipmentPackLineLookups(this);
		}

		#endregion

		#region Related BusinessObjects

		#region Shipment

		protected override CommonShipment GetParentShipment()
		{
			return Factory.Load<AgencyShipment>(JL_JS);
		}

		public new AgencyShipment Shipment
		{
			get { return (AgencyShipment)base.Shipment; }
		}

		#endregion

		#region Containers

		protected sealed override CommonContainerManyToManyCollection GetNewContainersCollection()
		{
			AgencyShipmentContainerManyToManyCollection result = GetNewContainersCollectionCore();
			result.CountChanged += new CollectionCountChangedEventHandler(PackContainers_CountChanged);
			return result;
		}

		protected virtual AgencyShipmentContainerManyToManyCollection GetNewContainersCollectionCore()
		{
			return new AgencyShipmentContainerManyToManyCollection(this);
		}

		public new AgencyShipmentContainerManyToManyCollection Containers
		{
			get { return (AgencyShipmentContainerManyToManyCollection)base.Containers; }
		}

		public AgencyShipmentContainer Container
		{
			get { return (AgencyShipmentContainer)LoadContainer(JL_JC); }
		}

		protected override CommonContainer LoadContainer(ZGuid containerPK)
		{
			return Factory.Load<AgencyShipmentContainer>(containerPK);
		}

		#endregion

		#region UNDG

		protected override UNDGDataItemCollection GetNewUNDGs()
		{
			return new AgencyUNDGDataItemCollection(this);
		}

		#endregion

		#endregion

		#region Disabled Relationships

		[Browsable(false)]
		[Bindable(false)]
		[EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		[Obsolete("This property is invalid for agency, you probably want Container instead", true)]
		[ActionFieldFollow(false)]
		public new CommonContainer ConsolOrShipmentContainer
		{
			get { return null; }
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateJL_JC();
		}

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			JL_PackageCount = 1;
		}

		#endregion

		#region Events

		void PackContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved && e.BizObject.PK == fJL_JC)
			{
				BusinessObject bO = e.BizObject;
				if (bO.IsDeleted || bO.PK == fJL_JC)
				{
					fJL_JC = ZGuid.Missing;
					JL_JCInfo.RefreshBinding();
				}
			}
			else if (e.ItemAdded && !fJL_JC.IsMissing && !fJL_JC.IsValid)
			{
				fJL_JC = ZGuid.Missing;
				JL_JCInfo.RefreshBinding();
			}
		}

		#endregion

		#region IDefaultNumberOfDecimalsSupporter Members

		protected override int GetDefaultNumberOfDecimalsCore(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForShipping.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		protected override ZDecimal GetRoundedValueCore(SchemaColumn column, PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterWithSchemaColumnHelperForShipping.GetRoundedValue(this, column, property, value);
		}

		#endregion
	}
}



