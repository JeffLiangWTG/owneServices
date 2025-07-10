using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContainerDetentionCollection : ActiveBusinessObjectCollection<OrgContainerDetention>
	{
		public enum ParentType
		{
			Consignee,
			Consignor,
			ServiceIMP,
			ServiceEXP,
			Carrier
		}

		readonly ParentType parentType;
		readonly ZString penaltyType;

		public OrgContainerDetentionCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public OrgContainerDetentionCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter) { }

		public OrgContainerDetentionCollection(OrgHeader parent, ParentType parentType, ZString penaltyType, ZString creditorType)
			: this(parent, parentType, penaltyType, creditorType, new ZQuery()) { }

		public OrgContainerDetentionCollection(OrgHeader parent, ParentType parentType, ZString penaltyType, ZString creditorType, ZQuery filter)
			: base(parent.Factory, GetRelationship(parent, parentType, penaltyType, creditorType, filter))
		{
			this.parentType = parentType;
			this.penaltyType = penaltyType;
		}

		static ICollectionRelationship GetRelationship(OrgHeader parent, ParentType parentType, ZString penaltyType, ZString creditorType, ZQuery filter)
		{
			if (parent == null)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			switch (parentType)
			{
				case ParentType.Consignee:
					return new ParentDependantRelationship(parent, filter, OrgContainerDetentionSchema.PD_OH_Client, parentType, Constants.ContainerDetentionDirection.Import, penaltyType, creditorType);
				case ParentType.Consignor:
					return new ParentDependantRelationship(parent, filter, OrgContainerDetentionSchema.PD_OH_Client, parentType, Constants.ContainerDetentionDirection.Export, penaltyType, creditorType);
				case ParentType.ServiceIMP:
					return new ParentDependantRelationship(parent, filter, OrgContainerDetentionSchema.PD_OH_CTO, parentType, Constants.ContainerDetentionDirection.Import, penaltyType, creditorType);
				case ParentType.ServiceEXP:
					return new ParentDependantRelationship(parent, filter, OrgContainerDetentionSchema.PD_OH_CTO, parentType, Constants.ContainerDetentionDirection.Export, penaltyType, creditorType);
				case ParentType.Carrier:
					return new ParentDependantRelationship(parent, filter, OrgContainerDetentionSchema.PD_OH_Carrier, parentType, string.Empty, string.Empty, creditorType);
				default:
					throw new ArgumentOutOfRangeException(nameof(parentType), parentType, "Unrecognised parent type");
			}
		}

		#region ClientDependantRelationship

		class ParentDependantRelationship : DependentRelationship
		{
			public ParentDependantRelationship(OrgHeader master, ZQuery filter, SchemaGuidColumn fkColumn, ParentType parentType, ZString direction, ZString penaltyType, ZString creditorType)
				: base(master, typeof(OrgContainerDetention), filter, fkColumn)
			{
				this.parentType = parentType;
				this.direction = direction;
				this.penaltyType = penaltyType;
				this.creditorType = creditorType;
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				OrgContainerDetention detention = (OrgContainerDetention)businessObject;
				base.AddToRelationship(detention);
				detention.PD_Direction = direction;
				detention.PD_PenaltyType = penaltyType;
				detention.PD_CreditorType = creditorType;
			}

			protected override void RemoveFromRelationship(BusinessObject businessObject)
			{
				OrgContainerDetention detention = (OrgContainerDetention)businessObject;
				base.RemoveFromRelationship(detention);

				detention.PD_Direction = "";
				detention.PD_PenaltyType = "";
				detention.PD_CreditorType = "";
			}

			protected override ZQuery RelationshipFilterCore
			{
				get
				{
					ZQuery result = new ZQuery();

					if (!this.direction.IsEmpty)
					{
						result.AddToFilter(OrgContainerDetentionSchema.PD_Direction, direction);
					}

					if (!this.penaltyType.IsEmpty)
					{
						result.AddToFilter(OrgContainerDetentionSchema.PD_PenaltyType, penaltyType);
					}

					if (!this.creditorType.IsEmpty)
					{
						result.AddToFilter(OrgContainerDetentionSchema.PD_CreditorType, creditorType);
					}

					if ((this.parentType == ParentType.Consignor || this.parentType == ParentType.Consignee)
						&& (this.penaltyType != Constants.ContainerDetentionPenaltyType.STO
							|| this.creditorType != Constants.ContainerPenaltyCreditorType.Codes.CTO))
					{
						result.AddToFilter(OrgContainerDetentionSchema.PD_CreditorType, SQLComparisonOperator.NotEqual, Constants.ContainerPenaltyCreditorType.Codes.CTO);
					}

					result.AddToFilter(base.RelationshipFilterCore);

					return result;
				}
			}

			readonly ParentType parentType;
			readonly ZString direction;
			readonly ZString penaltyType;
			readonly ZString creditorType;
		}

		#endregion

		#region PD_CreditorType Defaulting

		protected override void OnAdded(OrgContainerDetention businessObject)
		{
			base.OnAdded(businessObject);

			businessObject.ParentType = parentType;

			HookEvents(businessObject);
		}

		public override void Delete(OrgContainerDetention businessObject)
		{
			UnhookEvents(businessObject);

			base.Delete(businessObject);
		}

		void HookEvents(OrgContainerDetention businessObject)
		{
			businessObject.PD_PenaltyTypeInfo.ValueChanged += PD_PenaltyTypeInfo_ValueChanged;
		}

		void UnhookEvents(OrgContainerDetention businessObject)
		{
			businessObject.PD_PenaltyTypeInfo.ValueChanged -= PD_PenaltyTypeInfo_ValueChanged;
		}

		void PD_PenaltyTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			if ((this.parentType == ParentType.Consignor || this.parentType == ParentType.Consignee)
				&& this.penaltyType != Constants.ContainerDetentionPenaltyType.STO)
			{
				var detention = sender as OrgContainerDetention;

				if (detention.PD_CreditorType.IsEmpty)
				{
					detention.PD_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				}
			}
		}

		#endregion
	}
}
