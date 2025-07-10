using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingDeclarationCollection : NonPersistentBusinessObjectCollection<TrackingDeclaration>
	{
		public TrackingDeclarationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			declarations = new BaseJobDeclarationCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("The method or operation is not supported.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1194:Usafe BusinessObjectCollection Creation", Justification = "Field set in ctor")]
		public override void Load(ZQuery filter)
		{
			RemoveAll();

			declarations.Load(filter);
			foreach (BaseJobDeclaration declaration in declarations)
			{
				Add(new TrackingDeclaration(declaration));
			}
		}

		readonly BaseJobDeclarationCollection declarations;
	}
}
