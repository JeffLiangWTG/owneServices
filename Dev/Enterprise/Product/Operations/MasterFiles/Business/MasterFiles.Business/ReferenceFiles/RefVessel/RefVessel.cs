using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoRefVessel.Schema.RV_LloydsNumber)]
	public class RefVessel : AutoRefVessel, IDocManagerSupport, IScreeningPartyProvider, IRefVessel, IDpsEntityProvider
	{
		public RefVessel(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static RefVessel New(BusinessObjectFactory factory)
		{
			return factory.New<RefVessel>();
		}

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory) : base(factory)
			{
			}

			public RefVessel LoadUnique(ZString name, ZString lloydsNumber, ZString radioCallSign, ZString conveyanceCountry)
			{
				RefVessel result = null;
				if (!name.IsEmpty || !lloydsNumber.IsEmpty || !radioCallSign.IsEmpty)
				{
					var query = new ZQuery { IgnoreActiveFilter = true };
					if (!name.IsEmpty)
					{
						query.AddToFilter(RefVesselSchema.RV_Code, name);
					}

					if (!lloydsNumber.IsEmpty)
					{
						query.AddToFilter(RefVesselSchema.RV_LloydsNumber, lloydsNumber);
					}

					if (!radioCallSign.IsEmpty)
					{
						query.AddToFilter(RefVesselSchema.RV_RadioCallSign, radioCallSign);
					}

					if (!conveyanceCountry.IsEmpty)
					{
						query.AddToFilter(RefVesselSchema.RV_RN_NKCountryOfReg, conveyanceCountry);
					}
					var items = Factory.Load<RefVessel>(query);
					if (items.Length == 1)
					{
						result = items[0];
					}
					else if (items.Length > 1)
					{
						var activeItems = items.Where(x => x.RV_IsActive).Take(2).ToArray();
						if (activeItems.Length == 1)
						{
							result = activeItems[0];
						}
					}
				}
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad() => typeof(RefVessel);
		}

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			if (RV_CodeInfo.HasChanges || RV_RadioCallSignInfo.HasChanges || RV_LloydsNumberInfo.HasChanges || RV_RN_NKCountryOfRegInfo.HasChanges || (RV_IsActiveChanged && !RV_IsActive) || ShippingProviderChangedAndScreenStatusNotSame)
			{
				InvalidateScreeningStatuses();
			}
		}

		bool ShippingProviderChangedAndScreenStatusNotSame
		{
			get
			{
				var result = false;

				if (RV_OHInfo.HasChanges)
				{
					if (RV_OH.IsValid)
					{
						var shippingProvider = ShippingProvider;
						result = shippingProvider == null || shippingProvider.OH_ScreeningStatus != RV_ScreeningStatus;
					}
					else
					{
						result = true;
					}
				}

				return result;
			}
		}

		OrgHeader ShippingProvider => Factory.Load<OrgHeader>(RV_OH);

		public void InvalidateScreeningStatuses()
		{
			ScreeningStatusUpdater.ProcessInvalidateLocalDataChanges(this, () =>
			{
				shouldDeactivateEntity = RV_IsActiveChanged && !RV_IsActive;
			});
		}

		bool shouldDeactivateEntity;

		public ZBool ShouldUpdateRelatedJobs { get; set; }

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (shouldDeactivateEntity)
			{
				RV_IsActiveChanged = false;
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			if (RV_CodeInfo.HasError(RefVesselValidation.VesselAlreadyExists))
			{
				throw new ZCannotSaveException("A Vessel with this Vessel Code already exists.", "Duplicate Vessel Code");
			}
			base.OnFactorySaving();
		}

		#region Properties

		#region RV_Code

		public override ZString RV_Code
		{
			get { return base.RV_Code; }
			set
			{
				var valueChanged = !RV_CodeInfo.Value.Equals(value);
				base.RV_Code = value;
				if (Factory != null && valueChanged)
				{
					Factory.UpdateNaturalKeyCache(this, RefVesselSchema.RV_Code, RV_CodeInfo.Value, value);
				}
			}
		}

		/// <summary>
		/// Please replace RV_Code with either RV_Name or RV_FK
		/// </summary>
		public ZString RV_FK
		{
			get { return RV_Code; }
			set { RV_Code = value; }
		}

		/// <summary>
		/// Please replace RV_Code with either RV_Name or RV_FK
		/// </summary>
		public ZString RV_Name
		{
			get { return RV_Code; }
			set { RV_Code = value; }
		}

		public bool HasSystemVessel
		{
			get
			{
				if (hasSystemVessel == null)
				{
					hasSystemVessel = SystemVessel != null;
				}
				return hasSystemVessel.Value;
			}
		}
		bool? hasSystemVessel;

		#endregion

		public RefVesselZZ SystemVessel => RefVesselZZ.GetCachedVesselByCode(RV_Code, Factory);

		public RefVesselZZ[] RelatedSystemVessels => RefVesselZZ.GetCachedVesselsByCode(RV_Code, Factory);

		#region RV_VesselType

		[List("Lookups.RV_VesselType_List")]
		public override ZString RV_VesselType
		{
			get
			{
				return base.RV_VesselType;
			}
			set
			{
				base.RV_VesselType = value;
			}
		}

		#endregion

		#region RV_OH

		[List("Lookups.RV_ShippingLine_List")]
		public override ZGuid RV_OH
		{
			get
			{
				return base.RV_OH;
			}
			set
			{
				base.RV_OH = value;
			}
		}

		#endregion

		#region RV_RN_NKCountryOfReg

		[List("Lookups.RV_RN_List")]
		public override ZString RV_RN_NKCountryOfReg
		{
			get { return base.RV_RN_NKCountryOfReg; }
			set
			{
				CheckMaximumLength(RV_RN_NKCountryOfRegInfo, value);
				base.RV_RN_NKCountryOfReg = value;
			}
		}

		#endregion

		#region RV_ScreeningStatus

		[ReadOnly(true)]
		[List("Lookups.ScreeningStatusesList")]
		public override ZString RV_ScreeningStatus
		{
			get { return base.RV_ScreeningStatus; }
			set
			{
				base.RV_ScreeningStatus = value;
			}
		}

		#endregion

		#region RV_IsActive

		public override ZBool RV_IsActive
		{
			get
			{
				return base.RV_IsActive;
			}
			set
			{
				var oldValue = RV_IsActive;
				base.RV_IsActive = value;
				if (!IsCopying && oldValue != RV_IsActive)
				{
					RV_IsActiveChanged = true;
				}
			}
		}

		public bool RV_IsActiveChanged { get; set; }

		#endregion

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Custom Fields
		public bool CustomAttribute1Used
		{
			get { return !String.IsNullOrEmpty(CustomAttribute1Caption); }
		}

		public string CustomAttribute1Caption
		{
			get { return FreightDataRegistry.Instance.VesselCustomAttribute1Caption.Value; }
		}

		public bool CustomAttribute2Used
		{
			get { return !String.IsNullOrEmpty(CustomAttribute2Caption); }
		}

		public string CustomAttribute2Caption
		{
			get { return FreightDataRegistry.Instance.VesselCustomAttribute2Caption.Value; }
		}

		public bool CustomAttribute3Used
		{
			get { return !String.IsNullOrEmpty(CustomAttribute3Caption); }
		}

		public string CustomAttribute3Caption
		{
			get { return FreightDataRegistry.Instance.VesselCustomAttribute3Caption.Value; }
		}

		public bool CustomFlag1Used
		{
			get { return !String.IsNullOrEmpty(CustomFlag1Caption); }
		}

		public string CustomFlag1Caption
		{
			get { return FreightDataRegistry.Instance.VesselCustomFlag1Caption.Value; }
		}

		public bool CustomDecimal1Used
		{
			get { return !String.IsNullOrEmpty(CustomDecimal1Caption); }
		}

		public string CustomDecimal1Caption
		{
			get { return FreightDataRegistry.Instance.VesselCustomDecimal1Caption.Value; }
		}

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("720ae65c-1f85-42f3-ba29-2986abf91167", "Vessel {0}", RV_Code); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RV_VesselType = "CV";
		}

		[RelatedBusinessObject("CarrierConsortium")]
		[List("Lookups.RV_RG_List")]
		public override ZGuid RV_RG
		{
			get { return base.RV_RG; }
			set
			{
				base.RV_RG = value;
				fCarrierConsortium = null;
			}
		}

		RefCarrierConsortium fCarrierConsortium;
		public override RefCarrierConsortium CarrierConsortium
		{
			get
			{
				if (fCarrierConsortium == null)
				{
					fCarrierConsortium = base.CarrierConsortium;
				}

				return fCarrierConsortium;
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Vessel);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDeniedPartyScreeningPartyProvider Members

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties
		{
			get
			{
				List<ScreeningParty> result = new List<ScreeningParty>();
				var shippingProvider = ShippingProvider;
				if (shippingProvider != null)
				{
					result.Add(new ScreeningParty(this, Res.GetString("7cb94d25-bd03-4e63-a9da-2aeee343d5bf", "Shipping Provider"), shippingProvider));
				}

				result.Add(new ScreeningParty(this, Res.GetString("c27cc53a-f304-4e42-91ab-d06667d271b2", "Vessel"), this));

				return result.ToArray();
			}
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatus()
		{
			var statuses = new List<ZString>() { this.RV_ScreeningStatus };

			var shippingProvider = ShippingProvider;
			if (shippingProvider != null)
			{
				statuses.Add(shippingProvider.OH_ScreeningStatus);
			}

			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared()
		{
			return (this as IScreeningPartyProvider).GetWorstScreeningStatus();
		}

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get { return RV_ScreeningStatus; }
			set { RV_ScreeningStatus = value; }
		}

		ZBool IDpsEntityProvider.ShouldUpdateRelatedJobs
		{
			get { return ShouldUpdateRelatedJobs; }
			set { ShouldUpdateRelatedJobs = value; }
		}

		ZString IDpsEntityProvider.OriginalScreeningStatus => RV_ScreeningStatusInfo.OriginalValue.ToString();

		ZBool IDpsEntityProvider.NeedsScreening => RV_ScreeningStatus != ScreeningStatusesList.Codes.NotScreened || !IsInDatabase;

		public void InvalidateByLocalDataChanges() => ScreeningLogCollection.InvalidateByLocalDataChanges();

		#endregion

		#region Screening Log Collection

		[ChildEditable]
		[ChildEditableTestExclude]
		public IStmEntityScreeningLogCollection ScreeningLogCollection
		{
			get
			{
				if (screeningLogCollection == null)
				{
					screeningLogCollection = (IStmEntityScreeningLogCollection)Activator.CreateInstance(ObjectFactory.GetType<IStmEntityScreeningLogCollection>(), this);
					RegisterEditableChildObject((IBusiness)screeningLogCollection);
				}
				return screeningLogCollection;
			}
		}
		IStmEntityScreeningLogCollection screeningLogCollection;

		#endregion

		#region IRelatedOrgDeniedPartyScreenable Members

		public IRelatedOrgPartyScreeningStatusCollection RelatedShippingLineScreeningStatusCollection
		{
			get
			{
				return ObjectFactory.Get<IRelatedShippingProviderScreeningStatusHelper>().GetRelatedOrgPartyScreeningStatusCollection(Factory, ((IScreeningPartyProvider)this).ScreeningParties);
			}
		}

		#endregion

		#region Lookups

		/// <summary>
		/// If an operational job has FK to RefVessel, please use this method. Refactor all LookupVesselByCode by,
		/// for example, LookupVesselByFK(jobVoyage, JobVoyageSchema.JV_RV_NKVessel). Later this method 
		/// will be refactored to become LookupVesselByFK(jobVoyage, JobVoyageSchema.JV_RV_Vessel)
		/// </summary>
		/// <param name="bizObj"></param>
		/// <param name="fkColumn"></param>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static RefVessel LookupVesselByFK(BusinessObject bizObj, SchemaColumn fkColumn)
		{
			if (fkColumn.ColumnType == SchemaColumnType.String)
			{
				return LookupVesselByCode((ZString)bizObj[fkColumn], bizObj.Factory, true);
			}
			throw new InvalidOperationException("Only string column type is supported.");
		}

		/// <summary>
		/// If an operational job has FK to RefVessel, please use this method. Refactor all LookupVesselByCode by,
		/// for example, LookupVesselByFK(jobVoyage, JobVoyageSchema.JV_RV_NKVessel, jobVoyage.Factory). Later this method 
		/// will be refactored to become LookupVesselByFK(jobVoyage, JobVoyageSchema.JV_RV_Vessel, jobVoyage.Factory)
		/// </summary>
		/// <param name="bizObj"></param>
		/// <param name="fkColumn"></param>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static RefVessel LookupVesselByFK(BusinessObject bizObj, SchemaColumn fkColumn, BusinessObjectFactory factory)
		{
			if (fkColumn.ColumnType == SchemaColumnType.String)
			{
				return LookupVesselByCode((ZString)bizObj[fkColumn], factory, true);
			}
			throw new InvalidOperationException("Only string column type is supported.");
		}

		public static RefVessel LookupVesselByFK(ZString vesselPK, BusinessObjectFactory factory)
		{
			return vesselPK.IsEmpty ? null : LookupVesselByCode(vesselPK, factory, true);
		}

		/// <summary>
		/// Look up vessels by name, might return more than 1 matches.
		/// </summary>
		/// <param name="vesselName"></param>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static RefVessel[] LookupVesselByName(ZString vesselName, BusinessObjectFactory factory, bool ignoreActiveFilter = false)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ZQuery filter = new ZQuery(RefVesselSchema.RV_Code, vesselName.ToUpper().Trim());
			if (ignoreActiveFilter)
			{
				filter.IgnoreActiveFilter = true;
			}
			else
			{
				filter.AddToFilter(RefVesselSchema.RV_IsActive, true);
			}

			return factory.Load<RefVessel>(filter);
		}

		/// <summary>
		/// This method is going to be obseleted soon, please use either LookupVesselByName or LookupVesselByFK
		/// </summary>
		/// <param name="vesselName"></param>
		/// <param name="factory"></param>
		/// <returns></returns>
		public static RefVessel LookupVesselByCode(ZString vesselName, BusinessObjectFactory factory, bool ignoreActiveFilter = false)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ZQuery filter = new ZQuery(RefVesselSchema.RV_Code, vesselName.ToUpper().Trim());
			if (ignoreActiveFilter)
			{
				filter.IgnoreActiveFilter = true;
			}
			else
			{
				filter.AddToFilter(RefVesselSchema.RV_IsActive, true);
			}

			return factory.LoadTop1<RefVessel>(filter);
		}

		public static RefVessel LookupVesselByLloyds(ZString lloydsNumber, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ZQuery filter = new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber.ToUpper().Trim());
			filter.AddToFilter(RefVesselSchema.RV_IsActive, true);

			return factory.LoadTop1<RefVessel>(filter);
		}

		public static RefVessel[] LookupVesselsByLloyds(ZString lloydsNumber, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			ZQuery filter = new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber.ToUpper().Trim());
			filter.AddToFilter(RefVesselSchema.RV_IsActive, true);

			return factory.Load<RefVessel>(filter);
		}

		public static RefVessel[] LookupVesselsByNameAndLloyds(ZString vesselName, ZString lloydsNumber, BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			var filter = new ZQuery(RefVesselSchema.RV_Code, vesselName.ToUpper().Trim());
			filter.AddToFilter(RefVesselSchema.RV_LloydsNumber, lloydsNumber.ToUpper().Trim());
			filter.AddToFilter(RefVesselSchema.RV_IsActive, true);

			return factory.Load<RefVessel>(filter);
		}

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(RefVesselSchema.RV_IsActive, SQLComparisonOperator.Equal, true); }
		}

		#endregion
	}
}
