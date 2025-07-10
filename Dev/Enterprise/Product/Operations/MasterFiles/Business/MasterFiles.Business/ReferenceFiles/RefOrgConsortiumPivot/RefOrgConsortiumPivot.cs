using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for RefOrgConsortiumPivot.
	/// </summary>
	public class RefOrgConsortiumPivot : AutoRefOrgConsortiumPivot
	{
		public RefOrgConsortiumPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return !IsNewAndAllPropertiesDuplicatesOfExistingObject; }
		}
	}
}
