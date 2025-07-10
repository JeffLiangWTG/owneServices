using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Common.Business
{
	public class CommonCartageType : AutoLocalCartageJobType, ICommonCartageType
	{
		public CommonCartageType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		#region Delete

		public override void Delete()
		{
			if (E3_IsSystem)
			{
				throw new NotSupportedException("Shouldn't be trying to delete System Defined Cartage Type");
			}

			AllCartageLegTypes.DeleteAll();
			CommonCartageOrganisations.DeleteAll();
			base.Delete();
		}

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CartageTypeFetchStrategy(this);
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return E3_JobType.IsEmpty ?
					Res.GetString("3cfa88e0-94b0-4a68-9d3f-7cd1f0ad1c89", "Cartage Job Type") :
					Res.GetString("a4e157c8-80b7-473b-8610-7f71437b90e4", "Cartage Job Type {0}", E3_JobType);
			}
		}

		#endregion

		#endregion

		#region Property Overrides

		[ReadOnlyMember(Schema.E3_IsSystem)]
		public override ZString E3_JobType
		{
			get { return base.E3_JobType; }
			set
			{
				if (!isSettingJobType)
				{
					try
					{
						isSettingJobType = true;

						base.E3_JobType = value;

						ContainerMode = ContainerMode;
						Direction = Direction;
						ZString oneCharCode = E3_JobType.SubstringSafe(1, 1);
						if (oneCharCode.Length > 0)
						{
							E3_ShippingTransportMode = BindToLists.OneCharConnectingFreightModes.GetDescriptionFromCode(oneCharCode);
						}

						Validation.ValidateE3_JobType();
					}
					finally
					{
						isSettingJobType = false;
					}
				}
			}
		}
		bool isSettingJobType;

		[ReadOnly(true)]
		public override ZBool E3_IsSystem
		{
			get { return base.E3_IsSystem; }
			set { base.E3_IsSystem = value; }
		}

		[TranslatableDataField(Schema.TableName, Schema.E3_Description, LocalCartageXMLDataFilePath, MaxLength = Schema.E3_DescriptionMaxLength, Type = typeof(CommonCartageType), SecurityCheckpoint = "LocalCartageJobTypeEdit", Asmid = ResString.AssemblyId)]
		public override ZString E3_Description
		{
			get { return base.E3_Description; }
			set { base.E3_Description = value; }
		}
		const string LocalCartageXMLDataFilePath = @"Database\Odyssey\Data\Public\LocalCartageJobType\LocalCartage.xml";

		public MultilingualString E3_DescriptionMultilingual
		{
			get { return GetMultilingual(E3_DescriptionInfo); }
		}

		[ReadOnlyMember(Schema.E3_IsSystem)]
		[List("BindToLists+ShippingTransportModeList")]
		public override ZString E3_ShippingTransportMode
		{
			get { return base.E3_ShippingTransportMode; }
			set
			{
				base.E3_ShippingTransportMode = value;
				ZString oneCharCode = BindToLists.OneCharConnectingFreightModes.GetCodeFromDescription(value);
				if (oneCharCode.Length > 0)
				{
					E3_JobType = E3_JobType.SubstringSafe(0, 1).PadRight(1, '_') + oneCharCode + E3_JobType.SubstringSafe(2);
				}
			}
		}

		#endregion

		#region RelatedObjects

		public CommonCartageLegType ContainerizedBooking
		{
			get
			{
				if (containerizedBooking != null && containerizedBooking.IsDeleted)
				{
					containerizedBooking = null;
				}

				if (containerizedBooking == null)
				{
					if (ContainerizedBookedMoveTypes.Count > 0)
					{
						containerizedBooking = ContainerizedBookedMoveTypes[0];
					}
					else if (AllowContainerBookings)
					{
						containerizedBooking = ContainerizedBookedMoveTypes.AddNew();
					}
				}
				return containerizedBooking;
			}
		}
		CommonCartageLegType containerizedBooking;

		public CommonCartageLegType LooseBooking
		{
			get
			{
				if (looseBooking != null && looseBooking.IsDeleted)
				{
					looseBooking = null;
				}

				if (looseBooking == null)
				{
					if (LooseBookedMoveTypes.Count > 0)
					{
						looseBooking = LooseBookedMoveTypes[0];
					}
					else if (AllowLooseBookings)
					{
						looseBooking = LooseBookedMoveTypes.AddNew();
					}
				}
				return looseBooking;
			}
		}
		CommonCartageLegType looseBooking;

		#endregion

		#region Dependent Collections

		#region Organisations

		[ChildEditable(true)]
		public CommonCartageOrgCollection CommonCartageOrganisations
		{
			get
			{
				if (cartageJobOrganisations == null)
				{
					cartageJobOrganisations = new CommonCartageOrgCollection(this);
					//Commented out for now, should be included before launched.
					//fCartageJobOrganisations.ApplySort(new LocalCartageJobOrgComparer(this));
					RegisterEditableChildObject(cartageJobOrganisations);
					return cartageJobOrganisations;
				}

				return cartageJobOrganisations;
			}
		}
		CommonCartageOrgCollection cartageJobOrganisations;

		#endregion

		#region Leg Types

		[ChildEditable(true)]
		public CommonCartageLegTypeCollection LooseCartageLegTypes
		{
			get
			{
				if (looseCartageLegTypes == null)
				{
					looseCartageLegTypes = new CommonCartageLegTypeCollection(this, new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.NonBooking, LegTypeCollectionStrategy.LegContainerMode.Loose));
					looseCartageLegTypes.ApplySort(CommonCartageLegType.Schema.E4_DisplayOrder, ListSortDirection.Ascending);
					RegisterEditableChildObject(looseCartageLegTypes);
				}
				return looseCartageLegTypes;
			}
		}
		CommonCartageLegTypeCollection looseCartageLegTypes;

		[ChildEditable(true)]
		public CommonCartageLegTypeCollection ContainerizedCartageLegTypes
		{
			get
			{
				if (containerizedCartageLegTypes == null)
				{
					containerizedCartageLegTypes = new CommonCartageLegTypeCollection(this, new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.NonBooking, LegTypeCollectionStrategy.LegContainerMode.Containerized));
					containerizedCartageLegTypes.ApplySort(CommonCartageLegType.Schema.E4_DisplayOrder, ListSortDirection.Ascending);
					RegisterEditableChildObject(containerizedCartageLegTypes);
				}
				return containerizedCartageLegTypes;
			}
		}
		CommonCartageLegTypeCollection containerizedCartageLegTypes;

		[ChildEditable(true)]
		public CommonCartageLegTypeCollection AllCartageLegTypes
		{
			get
			{
				if (commonCartageLegTypes == null)
				{
					commonCartageLegTypes = new CommonCartageLegTypeCollection(this, new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.NonBooking, LegTypeCollectionStrategy.LegContainerMode.All));
					commonCartageLegTypes.ApplySort(CommonCartageLegType.Schema.E4_DisplayOrder, ListSortDirection.Ascending);
					RegisterEditableChildObject(commonCartageLegTypes);
				}
				return commonCartageLegTypes;
			}
		}
		CommonCartageLegTypeCollection commonCartageLegTypes;

		#endregion

		#region Booked Move Types

		[ChildEditable(true)]
		public CommonCartageLegTypeCollection LooseBookedMoveTypes
		{
			get
			{
				if (looseBookedMoveTypes == null)
				{
					looseBookedMoveTypes = new CommonCartageLegTypeCollection(this, new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.Booking, LegTypeCollectionStrategy.LegContainerMode.Loose));
					looseBookedMoveTypes.ApplySort(CommonCartageLegType.Schema.E4_DisplayOrder, ListSortDirection.Ascending);
					RegisterEditableChildObject(looseBookedMoveTypes);
				}
				return looseBookedMoveTypes;
			}
		}
		CommonCartageLegTypeCollection looseBookedMoveTypes;

		[ChildEditable(true)]
		public CommonCartageLegTypeCollection ContainerizedBookedMoveTypes
		{
			get
			{
				if (containerizedBookedMoveTypes == null)
				{
					containerizedBookedMoveTypes = new CommonCartageLegTypeCollection(this, new LegTypeCollectionStrategy(LegTypeCollectionStrategy.LegBookingType.Booking, LegTypeCollectionStrategy.LegContainerMode.Containerized));
					containerizedBookedMoveTypes.ApplySort(CommonCartageLegType.Schema.E4_DisplayOrder, ListSortDirection.Ascending);
					RegisterEditableChildObject(containerizedBookedMoveTypes);
				}
				return containerizedBookedMoveTypes;
			}
		}
		CommonCartageLegTypeCollection containerizedBookedMoveTypes;

		#endregion

		#endregion

		#region New Properties

		#region Direction

		[ReadOnlyMember(Schema.E3_IsSystem)]
		[BusinessObjectTestExclude()]
		[List("BindToLists+Directions")]
		[MaxLength(3)]
		public ZString Direction
		{
			get { return GetDirecton3CharCode(E3_JobType.SubstringSafe(0, 1)); }
			set
			{
				ZString oneCharCode = GetDirecton1CharCode(value);
				if (oneCharCode.Length > 0)
				{
					E3_JobType = oneCharCode + E3_JobType.SubstringSafe(1);
				}
				DirectionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(nameof(Direction)); }
		}

		ZString GetDirecton3CharCode(ZString oneCharCode)
		{
			return BindToLists.OneCharDirections.GetDescriptionFromCode(oneCharCode);
		}

		ZString GetDirecton1CharCode(ZString threeCharCode)
		{
			return BindToLists.OneCharDirections.GetCodeFromDescription(threeCharCode);
		}

		#endregion

		#region ContainerMode

		[ReadOnlyMember(Schema.E3_IsSystem)]
		[BusinessObjectTestExclude()]
		[List("BindToLists+ContainerModes")]
		[MaxLength(3)]
		public ZString ContainerMode
		{
			get { return GetContainerMode3CharCode(E3_JobType.SubstringSafe(2, 1)); }
			set
			{
				ZString oneCharCode = GetContainerMode1CharCode(value);
				if (oneCharCode.Length > 0)
				{
					E3_JobType = E3_JobType.SubstringSafe(0, 2).PadRight(2, '_') + oneCharCode + E3_JobType.SubstringSafe(3);
				}

				if (!AllowLooseBookings)
				{
					LooseBookedMoveTypes.DeleteAll();
					LooseCartageLegTypes.DeleteAll();
				}

				if (!AllowContainerBookings)
				{
					ContainerizedBookedMoveTypes.DeleteAll();
					ContainerizedCartageLegTypes.DeleteAll();
				}

				ContainerModeInfo.RefreshBinding();
			}
		}

		public bool AllowLooseBookings
		{
			get
			{
				return ContainerMode == Constants.CartageContainerMode.Loose ||
				ContainerMode == Constants.CartageContainerMode.FTL ||
				ContainerMode == Constants.CartageContainerMode.Mixed;
			}
		}

		public bool AllowContainerBookings
		{
			get
			{
				return ContainerMode == Constants.CartageContainerMode.FCL ||
				ContainerMode == Constants.CartageContainerMode.EmptyContainer ||
				ContainerMode == Constants.CartageContainerMode.Containerized ||
				ContainerMode == Constants.CartageContainerMode.Mixed;
			}
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerMode)); }
		}

		ZString GetContainerMode3CharCode(ZString oneCharCode)
		{
			return BindToLists.OneCharContainerModes.GetDescriptionFromCode(oneCharCode);
		}

		ZString GetContainerMode1CharCode(ZString threeCharCode)
		{
			return BindToLists.OneCharContainerModes.GetCodeFromDescription(threeCharCode);
		}

		#endregion

		public bool IsContainerised
		{
			get { return ContainerizedCartageLegTypes.Count > 0; }
		}

		public bool IsLoose
		{
			get { return LooseCartageLegTypes.Count > 0; }
		}

		public bool IsAir
		{
			get { return E3_ShippingTransportMode == Core.Constants.TransportModes.Air; }
		}

		public bool IsSea
		{
			get { return E3_ShippingTransportMode == Core.Constants.TransportModes.Sea; }
		}

		public bool IsRoad
		{
			get { return E3_ShippingTransportMode == Core.Constants.TransportModes.Road; }
		}

		public bool IsRail
		{
			get { return E3_ShippingTransportMode == Core.Constants.TransportModes.Rail; }
		}

		public bool IsExport
		{
			get { return Direction == Core.Constants.CartageDirection.Export; }
		}

		public bool IsOrigin
		{
			get { return Direction == Core.Constants.CartageDirection.Origin; }
		}

		public bool IsExportOrOrigin
		{
			get { return IsExport || IsOrigin; }
		}

		public bool IsImport
		{
			get { return Direction == Core.Constants.CartageDirection.Import; }
		}

		public bool IsDestination
		{
			get { return Direction == Core.Constants.CartageDirection.Destination; }
		}

		public bool IsImportOrDestination
		{
			get { return IsImport || IsDestination; }
		}

		public bool IsDomestic
		{
			get { return Direction == Core.Constants.CartageDirection.Local || Direction == Core.Constants.CartageDirection.Origin || Direction == Core.Constants.CartageDirection.Destination; }
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !E3_IsSystem; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("ad74c636-ce7b-49e4-9705-6dd0aadd8239", "This Port Transport Job Type is System Defined and cannot be deleted."); }
		}

		#endregion

		#region BindToLists

		public BindToLists BindToLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			E3_GE = GlbDepartment.CurrentDepartment.PK;
			E3_Description = "Test Description";
			E3_JobType = "JOB";
		}
#endif
	}
}
