using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.TrolleyPicking
{
	public class WhsPickTrolleySlot : AutoWhsPickTrolleySlot, IWhsPickTrolleySlot
	{
		public WhsPickTrolleySlot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		public WhsPickTrolleyJob TrolleyJob
		{
			get { return Factory.Load<WhsPickTrolleyJob>(WTS_WTJ_TrolleyJob); }
		}

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(WTS_KP_Package); }
		}

		// in case PickLines property required later.
		//public IEnumerable<WhsPickLine> PickLines
		//{
		//	get
		//	{
		//		IEnumerable<WhsPickLine> result;
		//		var package = Package;
		//		if (package != null)
		//		{
		//			result = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, package.PackedItems.Select(i => i.KI_ParentID)));
		//		}
		//		else
		//		{
		//			result = new WhsPickLine[0];
		//		}

		//		return result;
		//	}
		//}

		#endregion

		#region Properties

		[RelatedBusinessObject("TrolleyJob")]
		public override ZGuid WTS_WTJ_TrolleyJob
		{
			get { return base.WTS_WTJ_TrolleyJob; }
			set
			{
				base.WTS_WTJ_TrolleyJob = value;
				Package?.ClearActionStrategyCacheIncludingChildren();
			}
		}

		[RelatedBusinessObject("Package")]
		public override ZGuid WTS_KP_Package
		{
			get { return base.WTS_KP_Package; }
			set
			{
				base.WTS_KP_Package = value;
				Package?.ClearActionStrategyCacheIncludingChildren();
			}
		}

		#endregion

		public override void Delete()
		{
			var package = Package;
			if (package != null && package.GetIsTote())
			{
				UnpackPackagesAndDeleteFromTrolleySlot(package);
			}

			base.Delete();
		}

		void UnpackPackagesAndDeleteFromTrolleySlot(PkgPackage package)
		{
			var wrappers = package.PackedItems.Typed.ToArray();
			foreach (var wrapper in wrappers)
			{
				package.Unpack(wrapper, wrapper.PackedQty);
			}

			package.SetIsTote(false);
			WTS_KP_Package = ZGuid.Empty; // unassign tote before deleting

			if (!package.IsDeletingPackage)
			{
				package.Delete();
			}
		}

		public override void OnSaving()
		{
			if ((!IsInDatabase || HasChanges) && WTS_WTJ_TrolleyJob.IsValid)
			{
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsPickTrolleyJob>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WTS_WTJ_TrolleyJob);
			}

			base.OnSaving();
		}
	}
}

// Add tests to TrolleyPicking.Testing project.
