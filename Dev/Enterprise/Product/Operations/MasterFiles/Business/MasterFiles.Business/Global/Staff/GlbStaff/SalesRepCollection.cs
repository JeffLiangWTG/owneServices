using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class SalesRepCollection : GlbStaffCollection
	{
		public SalesRepCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AddFilterBusinessObjectDefaults();
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(GlbStaffSchema.GS_IsSalesRep, true);

			return result;
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);

			if (!((GlbStaff)selectedBusinessObject).GS_IsSalesRep)
			{
				errors.Add(Res.GetString("630b8508-a8a7-4956-9a61-734d73ed70e4", "A staff member selected from here must be a sales representative"));
			}
		}

		#endregion

		#region New

		protected override void SetDefaultsForNewElementCore(GlbStaff newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.GS_IsSalesRep = true;
		}

		#endregion

		#region Filter Business Object

		void AddFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Sales Rep", "Property", new ZString("SALES"), false));
		}

		#endregion
	}
}
