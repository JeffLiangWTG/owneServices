using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class Package : TypeSafePackage
		, Integration.Customs.US.IPackage
		, IInBondContainerMarksAndNumbers
	{
		public Package(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : TypeSafePackage.Schema
		{
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		#region Overriden properties

		protected override bool ShouldDeleteIfPackQtyIsEmpty
		{
			get { return false; }
		}

		#endregion

		#region IInBondPackageLineDetails Members

		ZString IInBondContainerMarksAndNumbers.MarksAndNumbers
		{
			get { return CW_MarksAndNos; }
		}

		#endregion
	}
}
