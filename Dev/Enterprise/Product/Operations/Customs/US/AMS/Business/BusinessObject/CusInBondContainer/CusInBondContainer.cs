using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondContainer : Customs.Business.CusInBondContainer,
		IContainer,
		IACEContainer,
		IVINOrEmptyContainer,
		ICanDelete,
		ICusInBondCargoDescTypeProvider,
		ISailingSynchronisationTarget<BillOfLadingContainer>
	{
		public CusInBondContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region New Properties

		public bool IsRail
		{
			get
			{
				var header = Header;
				return header != null && header.IsRail;
			}
		}

		[ChildEditable]
		public new UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		#endregion

		#region Override Properties

		public override ZGuid BC_ParentID
		{
			get { return base.BC_ParentID; }
			set
			{
				base.BC_ParentID = value;
				Vehicles.MarkAsNeedingValidation();
				Commodities.MarkAsNeedingValidation();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ForeignPorts))]
		[RelatedBusinessObject("ForeignPort")]
		[ReadOnlyMember(nameof(BC_RL_NKForeignPort_ReadOnly))]
		public override ZString BC_RL_NKForeignPort
		{
			get { return base.BC_RL_NKForeignPort; }
			set
			{
				var oldValue = BC_RL_NKForeignPort;
				base.BC_RL_NKForeignPort = value;
				if (!IsCopying && oldValue != BC_RL_NKForeignPort)
				{
					BC_ForeignPortKCode = USScheduleResolver.GetScheduleCode(Schedule.K, BC_RL_NKForeignPort, USLocoMapSystemUsageList.Codes.Sea, Factory).Left(BC_ForeignPortKCodeInfo.MaxLength);
				}
			}
		}

		bool BC_RL_NKForeignPort_ReadOnly
		{
			get { return !BC_IsEmpty; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ScheduleKList))]
		[RelatedBusinessObject("ForeignPortKCode")]
		[ReadOnlyMember(nameof(BC_ForeignPortKCode_ReadOnly))]
		public override ZString BC_ForeignPortKCode
		{
			get { return base.BC_ForeignPortKCode; }
			set { base.BC_ForeignPortKCode = value; }
		}

		bool BC_ForeignPortKCode_ReadOnly
		{
			get { return !BC_IsEmpty; }
		}

		public ZZRefCusCodeListCombined ForeignPortKCode
		{
			get
			{
				return Factory.GetCachedValue("CusInBondContainer|" + BC_ForeignPortKCode, () =>
				{
					return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndAttributes(Factory, BC_ForeignPortKCode,
						Core.Constants.CountryCodes.UnitedStates,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port,
						ZDateTime.Today,
						attributeFilters: new[] {
							new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.PortValidType, SQLComparisonOperator.Equal, ForeignPortTypeList.Codes.Common)
						});
				});
			}
		}

		#region BC_RC

		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ContainerTypes))]
		public override ZGuid BC_RC
		{
			get { return base.BC_RC; }
			set { base.BC_RC = value; }
		}

		#endregion

		#region BC_Seal1

		#endregion

		#region BC_Seal2

		#endregion

		#region BC_ContainerNum

		public override ZString BC_ContainerNum
		{
			get { return base.BC_ContainerNum; }
			set
			{
				var oldValue = BC_ContainerNum;
				base.BC_ContainerNum = value;
				if (!IsCopying && oldValue != BC_ContainerNum && (MoveHeader is CusInBondMoveHeader moveheader && !moveheader.IsUpdatingContainerDetailSuspended))
				{
					UpdateContainerDetails();
				}
			}
		}

		public bool IsContainerNumberStartsWithSCAC
		{
			get
			{
				return !BC_ContainerNum.StartsWith(NonContainerizedNumber) ||
							 !BC_ContainerNum.SubstringSafe(2, 2).IsEmpty && BC_ContainerNum.SubstringSafe(2, 2).IsLettersOnlyOrEmpty &&
							 Factory.Load<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, BC_ContainerNum.SubstringSafe(0, 4))).Length > 0;
			}
		}

		#endregion

		#region BC_TypeOfService

		[List(nameof(Lookups) + "." + nameof(CusInBondContainerLookups.ServiceTypes))]
		public override ZString BC_TypeOfService
		{
			get { return base.BC_TypeOfService; }
			set { base.BC_TypeOfService = value; }
		}

		#endregion

		#region BC_IsEmpty

		public override ZBool BC_IsEmpty
		{
			get { return base.BC_IsEmpty; }
			set
			{
				base.BC_IsEmpty = value;
				if (!BC_IsEmpty)
				{
					BC_RL_NKForeignPort = ZString.Empty;
					BC_ForeignPortKCode = ZString.Empty;
				}
			}
		}

		#endregion

		public new CusInBondContainerLookups Lookups
		{
			get { return (CusInBondContainerLookups)base.Lookups; }
		}

		public new CusInBondContainerValidation Validation
		{
			get { return (CusInBondContainerValidation)base.Validation; }
		}

		#endregion

		#region Related Objects

		public new CusInBondHeader Header
		{
			get { return (CusInBondHeader)base.Header; }
		}

		public new CusInBondBill Bill
		{
			get { return (CusInBondBill)base.Bill; }
		}

		public new CusInBondMoveHeader MoveHeader
		{
			get { return (CusInBondMoveHeader)base.MoveHeader; }
		}

		public new CusInBondMoveDetail MoveDetail
		{
			get { return (CusInBondMoveDetail)base.MoveDetail; }
		}

		[ChildEditable]
		public new CusInBondCargoDescCollection Commodities
		{
			get { return (CusInBondCargoDescCollection)base.Commodities; }
		}

		protected override ICusInBondCargoDescCollection<Customs.Business.CusInBondCargoDesc> GetNewCommoditiesCollection()
		{
			return new CusInBondCargoDescCollection(this);
		}

		[ChildEditable]
		public CusInBondVehicleCollection Vehicles
		{
			get
			{
				if (vehicles == null)
				{
					vehicles = new CusInBondVehicleCollection(this);
					RegisterEditableChildObject(vehicles);
				}
				return vehicles;
			}
		}
		CusInBondVehicleCollection vehicles;

		#endregion

		#region Implementation

		protected override Customs.Business.CusInBondMoveDetail MoveDetailCore
		{
			get { return Factory.Load<CusInBondMoveDetail>(BC_ParentID); }
		}

		void UpdateContainerDetails()
		{
			if (IsNonContainerized)
			{
				BC_RC = ZGuid.Empty;
				BC_Seal1 = ZString.Empty;
				BC_Seal2 = ZString.Empty;
				BC_IsEmpty = ZBool.False;
				BC_TypeOfService = ZString.Empty;
			}
			else if (!BC_ContainerNum.IsEmpty)
			{
				var moveHeader = MoveHeader;
				if (moveHeader != null)
				{
					var foundContainer = false;
					foreach (var moveDetail in moveHeader.MovementDetails)
					{
						if (moveDetail.PK != BC_ParentID)
						{
							foreach (var otherContainer in moveDetail.Containers)
							{
								if (otherContainer.BC_ContainerNum == BC_ContainerNum)
								{
									foundContainer = true;
									BC_RC = otherContainer.BC_RC;
									BC_Seal1 = otherContainer.BC_Seal1;
									BC_Seal2 = otherContainer.BC_Seal2;
									BC_IsEmpty = otherContainer.BC_IsEmpty;
									BC_TypeOfService = otherContainer.BC_TypeOfService;
									break;
								}
							}
						}
						if (foundContainer)
						{
							break;
						}
					}
				}
			}
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override Customs.Business.CusInBondContainerLookups GetNewLookups()
		{
			return new CusInBondContainerLookups(this);
		}

		protected override Customs.Business.CusInBondContainerValidation GetNewValidation()
		{
			return new CusInBondContainerValidation(this);
		}

		#endregion

		#region IContainer Members

		ZString ICommonContainer.ContainerEquipmentNo
		{
			get { return BC_ContainerNum; }
		}

		ZString ICommonContainer.SealNumber1
		{
			get { return MessageBlockStringDataCorrector.ReplaceInvalidCharacters(BC_Seal1, AMSCharacterTypeString.Constants.Special, '?'); }
		}

		ZString ICommonContainer.SealNumber2
		{
			get { return MessageBlockStringDataCorrector.ReplaceInvalidCharacters(BC_Seal2, AMSCharacterTypeString.Constants.Special, '?'); }
		}

		ZString ICommonContainer.ContainerEquipmentDescriptionCode
		{
			get
			{
				var container = Container;
				return container == null ? ZString.Empty : container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
			}
		}

		ZInt ICommonContainer.ContainerEquipmentLength
		{
			get { return ZInt.Zero; } // TODO
		}

		ZString ICommonContainer.Height
		{
			get { return ZString.Empty; } // TODO
		}

		ZString ICommonContainer.Width
		{
			get { return ZString.Empty; } // TODO
		}

		ZString ICommonContainer.ContainerEquipmentType
		{
			get
			{
				var result = ZString.Empty;
				var container = Container;
				if (container != null)
				{
					result = container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates, USContainerUsageList.Codes.AMS);
					if (result.IsEmpty)
					{
						result = container.RC_ISOType;
					}
				}
				return result;
			}
		}

		ZString ICommonContainer.LoadEmptyStatusCode
		{
			get { return BC_IsEmpty ? ContainerLoadConditionStatusList.Codes.Empty : ContainerLoadConditionStatusList.Codes.Loaded; }
		}

		ZString ICommonContainer.TypeOfServiceCode
		{
			get { return BC_TypeOfService; }
		}

		IEnumerable<IVINOrEmptyContainer> IContainer.VINsOrEmptyContainers
		{
			get
			{
				if (BC_IsEmpty)
				{
					yield return this;
				}
			}
		}

		IEnumerable<ICargoDescription> ICommonContainer.Commondities
		{
			get
			{
				foreach (ICargoDescription commodity in Commodities)
				{
					yield return commodity;
				}
			}
		}

		IEnumerable<IHazardousMaterial> ICommonContainer.HazardousMaterials
		{
			get
			{
				foreach (var undg in UNDGs)
				{
					yield return new HazardousMaterial(undg);
				}
			}
		}

		IEnumerable<IVehicleDetails> IACEContainer.VehicleDetails
		{
			get
			{
				foreach (var vehicle in Vehicles)
				{
					yield return vehicle;
				}
			}
		}

		#endregion

		#region IVINOrEmptyContainer Members

		ZString IVINOrEmptyContainer.VIN
		{
			get { return ZString.Empty; }
		}

		ZString IVINOrEmptyContainer.ForeignPort
		{
			get { return BC_ForeignPortKCode; }
		}

		ZString IVINOrEmptyContainer.FactoryCarOrderNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !ShouldSynchronise; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return ReasonForCannotDelete; }
		}

		public static MultilingualString ReasonForCannotDelete
		{
			get { return ResString.GetMultilingualString("AMS|CusInBondContainer|966AA33A-F4B1-45C5-B040-D06DE91537E5", "Container values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'."); }
		}

		#endregion

		#region ISailingSynchronisationTarget Members

		bool ISailingSynchronisationTarget<BillOfLadingContainer>.IsMatched(BillOfLadingContainer sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(BillOfLadingContainer sailingContainer)
		{
			var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
			return IsMatched(sailingContainer, sailingBill);
		}

		bool IsMatched(BillOfLadingContainer sailingContainer, BillOfLading sailingBill)
		{
			return sailingBill != null && sailingBill.IsContainerised && sailingBill.RealContainers.Contains(sailingContainer) && this.BC_ContainerNum == sailingContainer.JC_ContainerNum;
		}

		void ISailingSynchronisationTarget<BillOfLadingContainer>.Set(BillOfLadingContainer sailingTarget)
		{
			using (this.GetValidationSuspender())
			{
				this.BC_ContainerNum = sailingTarget.JC_ContainerNum;
			}
		}

		void ISailingSynchronisationTarget<BillOfLadingContainer>.Synchronise()
		{
			var sailingContainer = SailingSynchronisationSource;
			if (sailingContainer != null)
			{
				using (this.GetValidationSuspender())
				{
					this.BC_RC = sailingContainer.JC_RC;
					this.BC_Seal1 = sailingContainer.JC_SealNum;
					this.BC_Seal2 = sailingContainer.JC_AdditionalSealNum;
					var sailingPackLines = sailingContainer.PackLines.Cast<BillOfLadingPackLine>();
					this.Commodities.Synchronise<BillOfLadingPackLine, CusInBondCargoDesc>(sailingPackLines);
					this.UNDGs.Synchronise(sailingPackLines.SelectMany(x => x.UNDGs));
				}
			}
		}

		BillOfLadingContainer ISailingSynchronisationTarget<BillOfLadingContainer>.Source
		{
			get { return SailingSynchronisationSource; }
		}

		BillOfLadingContainer SailingSynchronisationSource
		{
			get
			{
				if (fSailingSynchronisationSource != null && IsMatched(fSailingSynchronisationSource))
				{
					return fSailingSynchronisationSource;
				}
				fSailingSynchronisationSource = null;
				var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
				if (sailingBill != null)
				{
					fSailingSynchronisationSource = sailingBill.RealContainers.Cast<BillOfLadingContainer>().FirstOrDefault(x => IsMatched(x, sailingBill));
				}
				return fSailingSynchronisationSource;
			}
		}
		BillOfLadingContainer fSailingSynchronisationSource;

		public bool HasSailingLinkage
		{
			get
			{
				var bill = Bill;
				return bill != null && bill.HasSailingLinkage;
			}
		}

		#endregion

		public override void Delete()
		{
			Vehicles.DeleteAll();
			Vehicles.Deactivate();
			base.Delete();
		}

		#region ICusInBondCargoDescTypeProvider Members
		Type ICusInBondCargoDescTypeProvider.CusInBondCargoDescType => typeof(CusInBondCargoDesc);
		#endregion
	}
}

