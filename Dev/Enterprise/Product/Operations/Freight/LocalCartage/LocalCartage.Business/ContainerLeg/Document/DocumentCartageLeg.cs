using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class DocumentCartageLeg : NonPersistentBusinessObject
	{
		public DocumentCartageLeg(CommonCartageLeg cartageLeg)
			: this(new[] { cartageLeg })
		{
		}

		public DocumentCartageLeg(CommonCartageLeg[] cartageLegsWithSameInfo)
			: base(cartageLegsWithSameInfo[0].Factory)
		{
			this.cartageLegs = cartageLegsWithSameInfo;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string PrintCartageLeg = "PrintCartageLeg";
		}

		public CommonCartageLeg CartageLeg
		{
			get { return GetCartageLegs().First(); }
		}

		public CommonCartageLeg[] GetCartageLegs()
		{
			return cartageLegs;
		}

		readonly CommonCartageLeg[] cartageLegs;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PrintCartageLeg = true;
		}

		public ZInt TotalPackCount
		{
			get { return GetCartageLegs().Sum(l => l.TotalPackages); }
		}

		public ZString TotalPackType
		{
			get
			{
				var packageUnits = GetCartageLegs().Select(l => l.TotalPackagesUnit).Distinct();
				return packageUnits.Count() == 1 ? packageUnits.First() : new ZString(Core.Constants.PkgUnit.Piece);
			}
		}

		public ZBool PrintCartageLeg
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return printCartageLeg; }
			set { SetNonPersistentPropertyValue(PrintCartageLegInfo, ref printCartageLeg, value); }
		}
		ZBool printCartageLeg;

		public ZPropertyInfo PrintCartageLegInfo
		{
			get { return GetZPropertyInfo(Schema.PrintCartageLeg); }
		}
	}
}
