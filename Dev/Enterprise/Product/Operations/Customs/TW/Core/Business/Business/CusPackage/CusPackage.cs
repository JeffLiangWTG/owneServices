using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusPackage : Customs.Business.CusPackage, Integration.Customs.TW.ICusPackage
	{
		public CusPackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusPackageJob CusPackageJob => Factory.Load<CusPackageJob>(KP_KJ_ParentPackageJob);

		public override PkgPackageJob PackageJob => CusPackageJob;

		public override ZString KP_MarksAndNumbers
		{
			get => base.KP_MarksAndNumbers;
			set
			{
				var oldValue = KP_MarksAndNumbers;
				base.KP_MarksAndNumbers = value;
				if (CusPackageJob.IsCalculatePackQtyFromPack && !IsCopying && oldValue != KP_MarksAndNumbers)
				{
					CalculatePackQtyFromPack(value);
				}
			}
		}

		void CalculatePackQtyFromPack(ZString marksAndNumbers)
		{
			var firstTwoSplit = marksAndNumbers.Split(new string[] { "\r\n-", "-\r\n", "\r\n", "-" }).Take(2);
			if (firstTwoSplit.Count() == 2)
			{
				var packageRegex = new Regex(@"[0-9]+", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.RightToLeft);
				var firstMatch = packageRegex.Match(firstTwoSplit.ElementAt(0));
				var secondMatch = packageRegex.Match(firstTwoSplit.ElementAt(1));
				if (secondMatch.Success && firstMatch.Success)
				{
					var packQty = int.Parse(secondMatch.Value) - int.Parse(firstMatch.Value) + 1;
					KP_PackageQty = packQty > 0 ? packQty : 1;
				}
				else
				{
					KP_PackageQty = 1;
				}
			}
			else
			{
				KP_PackageQty = 1;
			}
		}

		protected override Customs.Business.CusPackageCusPackableItemRelationCollection GetNewPackableItemRelataionsCollection() => new CusPackageCusPackableItemRelationCollection(this);
	}
}
