using System.Collections;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgLandedCostingPrefs : AutoOrgLandedCostingPrefs, ILandedCostPreference
	{
		public OrgLandedCostingPrefs(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log reference")]
		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString logReference = (NoResString)"Landed Costing";
				logReference += (NoResString)" Group Name: " + O9_LandedCostGroupName + (O9_LandedCostGroupNameInfo.HasChanges ? (NoResString)"(" + O9_LandedCostGroupNameInfo.OriginalValue + (NoResString)")" : (NoResString)"");
				logReference += " ID: " + O9_LandedCostGroup;

				return logReference;
			}
		}

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				ArrayList objects = new ArrayList();
				objects.AddRange(base.BusinessObjectsWithRelatedEventsCore);
				objects.AddRange(Charges);
				return (BusinessObject[])objects.ToArray(typeof(BusinessObject));
			}
		}

		#endregion

		#region Properties

		[List("Lookups.CostDistributionMechanisms")]
		public override ZString O9_DistributeCostBy
		{
			get
			{
				return base.O9_DistributeCostBy;
			}
			set
			{
				base.O9_DistributeCostBy = value;
			}
		}

		#region Cost Distribution Description

		public ZString CostDistributionDesc
		{
			get { return Lookups.CostDistributionMechanisms.GetDescriptionFromCode(O9_DistributeCostBy); }
		}

		public ZPropertyInfo CostDistributionDescInfo
		{
			get { return GetZPropertyInfo(nameof(CostDistributionDesc)); }
		}

		#endregion

		#endregion

		#region Charges

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public OrgLandedCostingPrefChargesCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new OrgLandedCostingPrefChargesCollection(this);
					fCharges.Load();
					RegisterEditableChildObject(fCharges);
					if (Header != null)
					{
						fCharges.SetReadOnlyIncludingChildren(!Header.SecurityProvider.HasModifyConsigneeLandedCostingSecurity);
					}
				}

				return fCharges;
			}
		}
		OrgLandedCostingPrefChargesCollection fCharges;

		#endregion

		#region Delete

		public override void Delete()
		{
			Charges.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region ILandedCostPreference Members

		ZByte ILandedCostPreference.LCGroupID
		{
			get { return O9_LandedCostGroup; }
		}

		ZString ILandedCostPreference.LCGroupName
		{
			get { return O9_LandedCostGroupName; }
		}

		ZString ILandedCostPreference.DistributionBy
		{
			get { return O9_DistributeCostBy; }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Header != null && Header.SecurityProvider != null &&
				!Header.SecurityProvider.HasModifyConsigneeLandedCostingSecurity) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
