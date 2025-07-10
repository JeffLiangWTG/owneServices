using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsCartonGroup : AutoWhsCartonGroup, IWhsCartonGroup
	{
		public WhsCartonGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : AutoWhsCartonGroup.Schema
		{
			public const string AttachedOrganisationsCodes = "AttachedOrganisationsCodes";
		}

		#endregion

		#region AttachedOrganisationsCodes

		[ResourceStringData("WhsCartonGroup|AttachedOrganisationsCodes", Caption = "Attached Organizations")]
		public ZString AttachedOrganisationsCodes
		{
			get
			{
				const int MaxNumberOfOrgsToShowInColumn = 3;
				return ParentOrgMiscServs.Count > MaxNumberOfOrgsToShowInColumn
					? Res.GetString("ea58a66d-ce43-4f71-8f65-1b065337dac5", "<Many>")
					: string.Join(", ", ParentOrgMiscServs.Select(o => o.Header.OH_Code));
			}
		}

		#endregion

		#region OptimizationMode

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(WhsCartonGroupLookups.OptimizationModes))]
		[ResourceStringData("WhsCartonGroup|OptimizationMode", ShortCaption = "Mode", MediumCaption = "Opt. Mode", Caption = "Optimization Mode")]
		public ZString OptimizationMode
		{
			get
			{
				if (!optimizationMode.HasValue)
				{
					optimizationMode = CalculateOptimizationMode();
				}

				return optimizationMode.Value;
			}
			set
			{
				var previousOptimizationMode = optimizationMode ?? ZString.Empty;
				SetNonPersistentPropertyValue(OptimizationModeInfo, ref optimizationMode, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOptimizationMode();
				}

				if (previousOptimizationMode != value)
				{
					SetLinkCostsBasedOnMode();
					CartonGroupSizeLinks.RefreshBinding();
				}
			}
		}

		ZString? optimizationMode;

		public ZPropertyInfo OptimizationModeInfo => GetZPropertyInfo(nameof(OptimizationMode));

		ZString CalculateOptimizationMode()
		{
			ZString calculatedMode;

			if (CartonGroupSizeLinks.Count == 0 || CartonGroupSizeLinks.All(l => l.WCV_OptimizationCost == 1))
			{
				calculatedMode = CartonizationOptimizationModes.Codes.MinimizeCartons;
			}
			else if (CartonGroupSizeLinks.All(l => l.WCV_OptimizationCost == l.GetOptimizationCostBasedOnCartonSizeVolume()))
			{
				calculatedMode = CartonizationOptimizationModes.Codes.MinimizeVolume;
			}
			else
			{
				calculatedMode = CartonizationOptimizationModes.Codes.CustomOptimizationCosts;
			}

			return calculatedMode;
		}

		void SetLinkCostsBasedOnMode()
		{
			if (OptimizationMode == CartonizationOptimizationModes.Codes.MinimizeCartons)
			{
				CartonGroupSizeLinks.ForEach(l => l.WCV_OptimizationCost = 1);
			}
			else if (OptimizationMode == CartonizationOptimizationModes.Codes.MinimizeVolume)
			{
				CartonGroupSizeLinks.ForEach(l => l.WCV_OptimizationCost = l.GetOptimizationCostBasedOnCartonSizeVolume());
			}
		}

		#endregion

		#region ParentOrgMiscServs

		[ChildEditable]
		public CartonGroupParentOrgMiscServCollection ParentOrgMiscServs
		{
			get
			{
				if (parentOrgMiscServs == null)
				{
					parentOrgMiscServs = new CartonGroupParentOrgMiscServCollection(this);
					RegisterEditableChildObject(parentOrgMiscServs);
				}

				return parentOrgMiscServs;
			}
		}

		CartonGroupParentOrgMiscServCollection parentOrgMiscServs;

		#endregion

		#region CartonSizes

		[ChildEditable]
		public WhsCartonSizeCollection CartonSizes
		{
			get
			{
				if (cartonSizes == null)
				{
					cartonSizes = new WhsCartonSizeCollection(this);
					RegisterEditableChildObject(cartonSizes);
				}

				return cartonSizes;
			}
		}

		WhsCartonSizeCollection cartonSizes;

		#endregion

		#region CartonGroupSizeLinks

		[ChildEditable]
		public ReadOnlyWhsCartonGroupSizeLinkCollection CartonGroupSizeLinks
		{
			get
			{
				if (cartonGroupSizeLinks == null)
				{
					cartonGroupSizeLinks = new ReadOnlyWhsCartonGroupSizeLinkCollection(this);
					RegisterEditableChildObject(cartonGroupSizeLinks);
				}

				return cartonGroupSizeLinks;
			}
		}

		ReadOnlyWhsCartonGroupSizeLinkCollection cartonGroupSizeLinks;

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsCartonGroupFetchStrategy(this);
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region Delete

		public override void Delete()
		{
			foreach (var cartonSize in CartonSizes)
			{
				CartonSizes.RemoveFromRelationship(cartonSize);
			}

			foreach (var orgMiscServ in ParentOrgMiscServs.ToArray())
			{
				ParentOrgMiscServs.RemoveFromRelationship(orgMiscServ);
			}

			var linkedProductRelations = Factory.Load<OrgPartRelation>(new ZQuery(OrgPartRelationSchema.OU_WCG_CartonGroup, PK));
			foreach (var relation in linkedProductRelations)
			{
				relation.OU_WCG_CartonGroup = ZGuid.Empty;
			}

			base.Delete();
		}

		#endregion
	}
}
