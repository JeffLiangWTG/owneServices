using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[DescriptionProperty(AutoGlbDepartment.Schema.GE_Desc)]
	[AllowAllObjectsToBeLoaded]
	[DebuggerDisplay("Department ({" + GlbDepartment.Schema.GE_Code + "})")]
	public class GlbDepartment : AutoGlbDepartment, Enterprise.Integration.IGlbDepartment, IDocManagerSupport, IDepartment
	{
		public GlbDepartment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Default Values / On Loaded

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			GE_SystemCode = false;
		}

		bool AttributesInitialised;
		void InitialiseAttributes()
		{
			if (!AttributesInitialised)
			{
				AttributesInitialised = true;
				if (ParentDepartment.Count != 0) // current department has a parent
				{
					GlbDepartment parent = ParentDepartment[0];
					SetAttributes(parent);

					if (!IsInDatabase)
					{
						SynchroniseWorkTimes(parent);
					}
				}
				else
				{
					SetAttributes(this);
				}
			}
		}

		#endregion

		#region Implementation

		void SetAttributes(GlbDepartment department)
		{
			string code = department.GE_Code;
			if (code.Length > 0)
			{
				GE_Activity = Lookups.ActivityList.GetDescriptionFromCode(code[0].ToString());
				if (GE_Activity.IsEmpty)
				{
					GE_Activity = Res.GetString("9cf80928-c630-4b6d-85d0-e5030e010aa6", "Miscellaneous");
				}
			}

			if (code.Length > 1)
			{
				GE_Direction = Lookups.DirectionList.GetDescriptionFromCode(code[1].ToString());
				if (GE_Direction.IsEmpty)
				{
					GE_Direction = Res.GetString("f7c4fdda-18ef-49d6-8c8c-f4bc8d664006", "Other");
				}
			}

			if (code.Length > 2)
			{
				GE_Mode = Lookups.ModeListByActivity(code[0].ToString()).GetDescriptionFromCode(code[2].ToString());
				if (GE_Mode.IsEmpty)
				{
					GE_Mode = Res.GetString("f7c4fdda-18ef-49d6-8c8c-f4bc8d664006", "Other");
				}
			}
		}

		#endregion

		#region Properties

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region GE_GE

		[ReadOnlyMember(GlbDepartmentSchema.Constants.GE_SystemCode)]
		[List("GE_GE_List")]
		public override ZGuid GE_GE
		{
			get { return base.GE_GE; }
			set
			{
				base.GE_GE = value;
				if (ParentDepartment.Count != 0) // current department has a parent
				{
					GlbDepartment parent = ParentDepartment[0];
					SetAttributes(parent);

					GE_InternationalFreight = parent.GE_InternationalFreight;
					GE_CustomsBrokerage = parent.GE_CustomsBrokerage;
					GE_LocalTransport = parent.GE_LocalTransport;
					GE_LineHaul = parent.GE_LineHaul;
					GE_DepotCFS = parent.GE_DepotCFS;
					GE_Warehouse = parent.GE_Warehouse;
					GE_Misc = parent.GE_Misc;

					GE_Export = parent.GE_Export;
					GE_Import = parent.GE_Import;
					GE_Domestic = parent.GE_Domestic;
					GE_NonDirectional = parent.GE_NonDirectional;

					GE_Air = parent.GE_Air;
					GE_Sea = parent.GE_Sea;
					GE_Road = parent.GE_Road;
					GE_Rail = parent.GE_Rail;
					GE_Post = parent.GE_Post;
					GE_NonTransport = parent.GE_NonTransport;

					SynchroniseWorkTimes(parent);
				}
			}
		}

		void SynchroniseWorkTimes(GlbDepartment parent)
		{
			WorkTimes.MondayWorkingHours = parent.WorkTimes.MondayWorkingHours;
			WorkTimes.TuesdayWorkingHours = parent.WorkTimes.TuesdayWorkingHours;
			WorkTimes.WednesdayWorkingHours = parent.WorkTimes.WednesdayWorkingHours;
			WorkTimes.ThursdayWorkingHours = parent.WorkTimes.ThursdayWorkingHours;
			WorkTimes.FridayWorkingHours = parent.WorkTimes.FridayWorkingHours;
			WorkTimes.SaturdayWorkingHours = parent.WorkTimes.SaturdayWorkingHours;
			WorkTimes.SundayWorkingHours = parent.WorkTimes.SundayWorkingHours;
		}

		#endregion

		#region Activity

		protected ZString fActivity;
		[BusinessObjectTestExclude()]
		[List("Lookups.ActivityList")]
		public ZString GE_Activity
		{
			get
			{
				InitialiseAttributes();
				return fActivity;
			}
			set
			{
				fActivity = value;
				GE_ActivityInfo.RefreshBinding();
				ActiveBusinessObjectCollection<GlbDepartment>.RefreshAll(Factory);
			}
		}

		public ZPropertyInfo GE_ActivityInfo
		{
			get { return GetZPropertyInfo(nameof(GE_Activity)); }
		}

		#endregion

		public bool IsGatewayDepartment
		{
			get { return GE_Code.StartsWith("G"); }
		}

		public bool IsCartageDepartment
		{
			get { return GE_Code.StartsWith("T"); }
		}

		#region Direction

		protected ZString fDirection;
		[BusinessObjectTestExclude()]
		[List("Lookups.DirectionList")]
		public ZString GE_Direction
		{
			set
			{
				fDirection = value;
				GE_DirectionInfo.RefreshBinding();
			}
			get
			{
				InitialiseAttributes();
				return fDirection;
			}
		}

		public ZPropertyInfo GE_DirectionInfo
		{
			get { return GetZPropertyInfo(nameof(GE_Direction)); }
		}

		#endregion

		#region Mode

		protected ZString fMode;
		[BusinessObjectTestExclude()]
		[List("Lookups.ModesList")]
		public ZString GE_Mode
		{
			set
			{
				fMode = value;
				GE_ModeInfo.RefreshBinding();
			}
			get
			{
				InitialiseAttributes();
				return fMode;
			}
		}

		public ZPropertyInfo GE_ModeInfo
		{
			get { return GetZPropertyInfo(nameof(GE_Mode)); }
		}

		#endregion

		#region System Department

		public ZString SystemDept
		{
			get
			{
				return GE_SystemCode ? Res.GetString("6af4942d-1cab-4343-901a-d91edfcd1742", "(System Department)") : "";
			}
		}

		public ZPropertyInfo SystemDeptInfo
		{
			get { return GetZPropertyInfo(nameof(SystemDept)); }
		}

		#endregion

		#region WorkTimes

		[ChildEditable]
		public GlbWorkTimeCollection WorkTimes
		{
			get
			{
				if (fWorkTimes == null)
				{
					ZQuery query = new ZQuery(GlbWorkTimeSchema.GW_ParentTableCode, GlbDepartmentSchema.Constants.Prefix);
					query.AddToFilter(GlbWorkTimeSchema.GW_ParentID, PK);
					fWorkTimes = new GlbWorkTimeCollection(Factory, query);

					RegisterEditableChildObject(fWorkTimes);
				}
				InitialiseAttributes();
				return fWorkTimes;
			}
		}
		GlbWorkTimeCollection fWorkTimes;

		public GlbWorkTimeViewModel WorkTimeViewModel
		{
			get
			{
				if (fWorkTimeViewModel == null)
				{
					fWorkTimeViewModel = new GlbWorkTimeViewModel(WorkTimes, false);
				}
				return fWorkTimeViewModel;
			}
		}
		GlbWorkTimeViewModel fWorkTimeViewModel;

		#endregion

		#region Transport Mode

		public string TransportMode
		{
			get
			{
				string result = "";

				if (GE_Air)
				{
					result = Enterprise.Core.Constants.TransportModes.Air;
				}
				else if (GE_Rail)
				{
					result = Enterprise.Core.Constants.TransportModes.Rail;
				}
				else if (GE_Road)
				{
					result = Enterprise.Core.Constants.TransportModes.Road;
				}
				else if (GE_Sea)
				{
					result = Enterprise.Core.Constants.TransportModes.Sea;
				}

				return result;
			}
		}

		#endregion

		#region CurrentDepartment

		[TestExcludeDetectStaticBusinessObjectsCollectionsAndFactories]
		public static GlbDepartment CurrentDepartment
		{
			get { return (GlbDepartment)Env.CurrentDepartment; }
		}

		public static GlbDepartment GetCurrentDepartment(BusinessObjectFactory factory)
		{
			var department = CurrentDepartment;
			return department != null && factory != department.Factory ? factory.Load<GlbDepartment>(department.PK) : department;
		}

		#endregion

		[ReadOnly(true)]
		public override ZBool GE_SystemCode
		{
			get { return base.GE_SystemCode; }
			set { base.GE_SystemCode = value; }
		}

		[ReadOnlyMember(GlbDepartmentSchema.Constants.GE_SystemCode)]
		public override ZString GE_Code
		{
			get { return base.GE_Code; }
			set
			{
				base.GE_Code = value;
				DeptCharges.MarkAsNeedingValidation();
			}
		}

		[GlbDepartmentTranslatableDataField(Schema.GE_Desc, Asmid = ResString.AssemblyId)]
		[ReadOnlyMember(GlbDepartmentSchema.Constants.GE_SystemCode)]
		public override ZString GE_Desc
		{
			get { return base.GE_Desc; }
			set { base.GE_Desc = value; }
		}

		public MultilingualString GE_DescMultilingual
		{
			get { return GetMultilingual(GE_DescInfo); }
		}

		#endregion

		#region Deleting

		public override void Delete()
		{
			DeleteUnusedStmData();
			base.Delete();
		}

		void DeleteUnusedStmData()
		{
			var datas = new StmDataCollection(Factory, new ZQuery(StmDataSchema.SD_DepartmentGuid, this.PK));
			datas.DeleteAll();
		}

		#endregion

		#region Lists

		GlbDepartmentCollection fGE_GE_List;

		public GlbDepartmentCollection GE_GE_List
		{
			get
			{
				if (fGE_GE_List == null)
				{
					ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_SystemCode, "Y");
					fGE_GE_List = new GlbDepartmentCollection(Factory, filter);
				}
				return fGE_GE_List;
			}
		}

		#endregion

		#region Related Business Objects

		#region ParentDepartment

		protected GlbDepartmentCollection fDepartments;
		public GlbDepartmentCollection ParentDepartment
		{
			get
			{
				if (fDepartments == null)
				{
					fDepartments = new GlbDepartmentCollection(Factory);
				}
				ZQuery filter = new ZQuery(GlbDepartmentSchema.PK, GE_GE);
				fDepartments.AdditionalFilter = filter;
				return fDepartments;
			}
		}

		#endregion

		#region This Department

		protected GlbDepartmentCollection gDepartments;
		public GlbDepartmentCollection ThisDepartment
		{
			get
			{
				if (gDepartments == null)
				{
					gDepartments = new GlbDepartmentCollection(Factory);
				}
				ZQuery filter = new ZQuery(GlbDepartmentSchema.GE_Code, GE_Code);
				gDepartments.AdditionalFilter = filter;
				return gDepartments;
			}
		}

		#endregion

		#region GlbDeptCharges

		[ChildEditable(true)]
		public GlbDeptChargesDependentCollection DeptCharges
		{
			get
			{
				if (fGlbDeptCharges == null)
				{
					ZQuery filter = new ZQuery(GlbDeptChargesSchema.GD_GC, GlbCompany.CurrentCompany.PK);
					fGlbDeptCharges = new GlbDeptChargesDependentCollection(this);
					fGlbDeptCharges.AdditionalFilter = filter;
					fGlbDeptCharges.ApplySort(GlbDeptChargesSchema.GD_SequenceNumber.Name, ListSortDirection.Ascending);
					RegisterEditableChildObject(fGlbDeptCharges);
				}
				return fGlbDeptCharges;
			}
		}
		protected GlbDeptChargesDependentCollection fGlbDeptCharges;

		#endregion

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Department);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				return !GE_SystemCode && !IsLastActiveDepartment;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString reason = ResString.GetMultilingualString("0d7a9f8e-f8fc-4d18-afd6-e0fba71c46ac", "Cannot delete a System department");
				if (IsLastActiveDepartment)
				{
					reason = ResString.GetMultilingualString("12032088-991c-4fe0-9e0f-f492b584fbd8", "Cannot delete department. This is the last active department and cannot be deleted.");
				}
				return reason;
			}
		}

		bool IsLastActiveDepartment
		{
			get
			{
				ZQuery activeDeptsQuery = new ZQuery(GlbDepartmentSchema.GE_IsActive, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueString);
				activeDeptsQuery.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, PK);

				return Factory.LoadTop1<GlbDepartment>(activeDeptsQuery) == null;
			}
		}

		#endregion

		#region IDepartment Members

		string IDepartment.Code
		{
			get { return GE_Code; }
		}

		string IDepartment.Description
		{
			get { return GE_DescMultilingual; }
		}

		Guid IDepartment.PK
		{
			get { return PK.ToGuid(); }
		}

		#endregion
	}
}
