#if DEBUG
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	public static class PackingTestExtensions
	{
		/// <summary>
		/// This is used for Tests ONLY!!
		/// </summary>
		public static IPackableItemParent GetPackableItemParent(this PkgPackageItemDivot divot)
		{
			return divot.ParentPackage.PackageJob.PackableItemParents.GetPackableItemParentFromKey(divot.PackedItem.Key);
		}

		/// <summary>
		/// DO NOT use this for future tests. By Default DummyPackableItem contains a Single Packing Item,
		/// and this Method was added to make existing tests pass easily without much changes.
		/// </summary>
		public static PkgPackageItemDivotsWrapper Pack_ForTesting(this PkgPackage package, IPackableItemParent itemToPack, ZDecimal qtyToPack)
		{
			return package.Pack(itemToPack, qtyToPack).SingleOrDefault();
		}

		/// <summary>
		/// This is a simple way to Wrap one PackedItem into a PackedItem Group, this is for Tests ONLY!
		/// </summary>
		public static IGrouping<object, PkgPackageItemDivotsWrapper> AsGroup(this PkgPackageItemDivotsWrapper packedItem)
		{
			return new[] { packedItem }.GroupedPackedItems().Single();
		}

		#region AssertPackageTree

		/// <summary>
		/// This build a text version of the package tree and asserts the expected.
		/// Sample Layout for 2 Containers w/ 2 bottles in 1st, 3 boxes in 2nd:
		/// - 1 CNT 2280 KG (CONT123)
		///   - 1 BOT 10 KG 1.5 CF (ABC)
		///   - 1 BOT 10 KG 1.5 CF (DEF)
		/// - 1 CNT 2280 KG (CONT456)
		///   - 1 BOX 5 KG 2 CF (GHI)
		///   - 1 BOX 5 KG 2 CF (JKL)
		///   - 1 BOX 5 KG 2 CF (MNO)
		/// </summary>
		public static void AssertPackageTree(this PkgPackageJob packageJob, ZString message, ZString expected)
		{
			var builder = new ZStringBuilder();
			BuildTree(builder, packageJob.Packages);
			Assertion.AssertMultilineASCIIEquals(message, expected.Trim(), builder.ToString().Trim());
		}

		static void BuildTree(ZStringBuilder treeBuilder, IEnumerable<PkgPackage> packages, int level = 0)
		{
			var indent = ZString.Replicate(' ', level * 2);

			foreach (var p in packages.OrderBy(r => r.KP_PackageQty).ThenBy(r => r.KP_F3_NKPackType))
			{
				var packageBuilder = new ZStringBuilder($"{indent}- {p.KP_PackageQty} {p.KP_F3_NKPackType}");
				packageBuilder.AppendIfNotEmpty(p.KP_Weight.IsEmpty ? "" : $"{p.KP_Weight} {p.KP_WeightUQ}");
				packageBuilder.AppendIfNotEmpty(p.KP_Volume.IsEmpty ? "" : $"{p.KP_Volume} {p.KP_VolumeUQ}");
				packageBuilder.AppendIfNotEmpty(p.KP_PackageID.IsEmpty ? "" : $"({p.KP_PackageID})");
				treeBuilder.AppendLine(packageBuilder.ToStringWithDelimiterBetweenAppends(" "));
				BuildTree(treeBuilder, p.Packages, level + 1);
			}
		}

		#endregion
	}
}
#endif
