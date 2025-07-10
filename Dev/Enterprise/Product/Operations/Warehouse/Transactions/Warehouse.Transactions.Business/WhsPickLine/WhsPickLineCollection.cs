using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickLineCollection : ActiveBusinessObjectCollection<WhsPickLine>
	{
		public WhsPickLineCollection(WhsPickOrderedInventory orderedInventory)
			: base(GetFactoryWillNullCheck(orderedInventory), new OrderedInventoryRelationship(orderedInventory))
		{
		}

		static BusinessObjectFactory GetFactoryWillNullCheck(WhsPickOrderedInventory orderedInventory) => Argument.NotNull(orderedInventory, nameof(orderedInventory)).Factory;

		public WhsPickLineCollection(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(WhsPickLine)))
		{
		}

		public WhsPickLineCollection(WhsDocketLine master)
			: this(master, null, WhsPickLineSchema.WZ_WE_TransactionLine)
		{
		}

		public WhsPickLineCollection(WhsOrderLine master)
			: base(master.Factory, new OrderLinePickLineRelationship(master, null))
		{
		}

		protected WhsPickLineCollection(WhsDocketLine master, ZQuery filter, SchemaGuidColumn relationshipColumn)
			: base(master.Factory, master, filter, relationshipColumn)
		{
		}

		#region AllowNew

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion

		#region GetQtyCommitted

		public ZDecimal GetQtyCommitted() => this.Sum(p => p.WZ_Units);

		#endregion

		#region GetQtyPicked

		public ZDecimal GetQtyPicked() => this.Where(p => p.IsPickedFromPutawayLocation).Sum(p => p.WZ_Units);

		#endregion

		#region GetLinesWithUnitsGreaterThanZero

		internal WhsPickLineCollection GetLinesWithUnitsGreaterThanZero()
		{
			var picklinesHasUnits = Clone();
			picklinesHasUnits.AdditionalFilter = new ZQuery(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
			return (WhsPickLineCollection)picklinesHasUnits;
		}

		#endregion

		#region RunPreSaveValidation

		protected override bool RunPreSaveValidationCore()
		{
			return Relationship is AdhocCollectionRelationship || base.RunPreSaveValidationCore();
		}

		#endregion

		#region OrderedInventoryRelationship class

		class OrderedInventoryRelationship : CollectionRelationship
		{
			#region Constructor

			public OrderedInventoryRelationship(WhsPickOrderedInventory orderedInventory)
				: base(typeof(WhsPickLine))
			{
				OrderedInventory = Argument.NotNull(orderedInventory, nameof(orderedInventory));
				OrderedInventory.Owners.CountChanged += OrderedInventory_OwnersChanged;
			}

			readonly WhsPickOrderedInventory OrderedInventory;

			void OrderedInventory_OwnersChanged(object sender, EventArgs e)
			{
				OnRelationshipFilterChanged(e);
			}

			#endregion

			#region Relationship

			protected override ZQuery RelationshipFilterCore
			{
				get
				{
					return IsRelationshipValid
						? new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, OrderedInventory.Owners.Select(o => o.PK))
						: ZQuery.NoResultQuery;
				}
			}

			bool IsRelationshipValid => !OrderedInventory.IsDeleted;

			#endregion

			#region Add

			protected override bool SupportsAddToRelationshipCore() => false;

			#endregion

			#region Clear

			protected override void Clear()
			{
				OrderedInventory.Owners.CountChanged -= OrderedInventory_OwnersChanged;
				OnRelationshipFilterChanged(EventArgs.Empty);
			}

			#endregion

			public override int GetHashCode()
			{
				return base.GetHashCode() ^ OrderedInventory.PK.GetHashCode();
			}
		}

		#endregion

		#region OrderLinePickLineRelationship

		//pivotTableFKToElements must be a column on the pivot table
		class OrderLinePickLineRelationship : ManyToManyRelationship<SchemaGuidColumn, ZGuid, SchemaGuidColumn, ZGuid>
		{
			public OrderLinePickLineRelationship(WhsDocketLine master, ZQuery filter)
			 : base(master, typeof(WhsPickLine), typeof(WhsOrderLine), filter, WhsDocketLineSchema.WE_WE_ParentDocketLine, WhsDocketLineSchema.PK, WhsDocketLineSchema.PK, WhsPickLineSchema.WZ_WE_TransactionLine)
			{
			}

			protected override bool SupportsAddToRelationshipCore()
			{
				return false;
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				throw new InvalidOperationException();
			}

			protected override void RemoveFromRelationship(BusinessObject businessObject)
			{
				throw new InvalidOperationException();
			}

			protected override BusinessObject[] GetElementsByKey(BusinessObjectFactory factory, ZGuid elementFK)
			{
				return factory.Load(ElementType, new ZQuery(ElementTableFKToPivots, elementFK));
			}

			protected override void InitPivots(BusinessObjectFactory factory, SchemaGuidColumn pivotTableFKToMaster, SchemaGuidColumn pivotTableFKToElements)
			{
				base.InitPivots(factory, pivotTableFKToMaster, pivotTableFKToElements); // add Child Order Component Lines as a Pivot
				base.InitPivots(factory, pivotTableFKToElements, pivotTableFKToElements); // add Master Order Line as a Pivot
			}

			protected override string GetPivotDataViewRowFilter()
			{
				// load both master Order Line and Child Order Component Lines as Pivots into Data View
				return string.Format(ZArchitecture.Core.Culture.Invariant, (NoResString)"{0} OR {1} = '{2}'", base.GetPivotDataViewRowFilter(), PivotTableFKToElements.Name, Master.PK); // developer only to build condition.
			}
		}

		#endregion

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			IComparer comparer;
			if (property.Name == PickLineInventoryLocationPropertyName)
			{
				comparer = new LocationComparer<WhsPickLine>(property, direction, pickLine => pickLine.Inventory.Location);
			}
			else
			{
				comparer = base.GetSortComparerForProperty(property, direction);
			}

			return comparer;
		}

		internal static string PickLineInventoryLocationPropertyName => "Inventory+LocationString";
	}
}
