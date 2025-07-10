using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	static class PackageJobStructureComparer
	{
		public static bool IsPackageJobSameStructureAsXMLPackageStructure(UniversalObjectFactory factory, IEnumerable<Container> pickUpContainers, List<PackingLine> pickUpPackages, IColumnIndexer packageJobRow)
		{
			// essentially this will serialize the package structure to a string representation in the following format:
			// 1xPLT->(2xCTN)
			// 1xPLT->(1xBOX,1xKEG->(1xCTN))
			// 2xPLT
			// 4xBOX
			//
			// The structure represented in the XML and the structure on the existing Consignment will be compared and return false if they are not the same.

			var outerPackageDataObjects =
				from p in pickUpPackages
				where !p.ContainerLink.HasValue
				let childPackageStructure = GetChildPackageStructure(p)
				select new
				{
					PackType = p.PackType.GetCodeAsUpperCase(),
					PackQty = p.PackQty.GetValueOrDefault(),
					ChildPackageStructure = childPackageStructure,
					HasChildren = !string.IsNullOrEmpty(childPackageStructure)
				};

			var containerDataObjects =
				from c in pickUpContainers
				let childPackageStructure = GetChildPackageStructure(c, pickUpPackages)
				select new
				{
					PackType = (ZString)Constants.PkgUnit.Container,
					PackQty = new ZLong(c.ContainerCount.GetValueOrDefault()),
					ChildPackageStructure = childPackageStructure,
					HasChildren = !string.IsNullOrEmpty(childPackageStructure)
				};

			var packageStructureInXML =
				from data in outerPackageDataObjects.Concat(containerDataObjects)
				let key = data.HasChildren ? data.PackQty + data.PackType + data.ChildPackageStructure : data.PackType.ToString()
				group data by key into groupedPacks
				select new { groupedPacks.Key, PackQty = groupedPacks.Sum(p => p.PackQty) };

			var existingStructure = GetExistingPackageStructure(factory, packageJobRow);

			foreach (var packageSubSet in packageStructureInXML)
			{
				int packQty;
				if (!existingStructure.TryGetValue(packageSubSet.Key, out packQty) || packQty != packageSubSet.PackQty)
				{
					return false;
				}
				else
				{
					existingStructure.Remove(packageSubSet.Key);
				}
			}

			return true;
		}

		static Dictionary<string, int> GetExistingPackageStructure(UniversalObjectFactory factory, IColumnIndexer packageJobRow)
		{
			var packageQuery = new ZQuery();
			packageQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobRow.GetValue(PkgPackageJobSchema.PK));
			packageQuery.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);
			var outerPackages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, packageQuery);

			var existingStructure =
				from p in outerPackages
				let row = DataObjectReader.GetColumnIndexerFromRow(p)
				let childPackageStructure = GetChildPackageStructure(factory, row)
				let data = new
				{
					PackType = row.GetValue(PkgPackageSchema.KP_F3_NKPackType).ToUpper(),
					PackQty = row.GetValue(PkgPackageSchema.KP_PackageQty),
					ChildPackageStructure = childPackageStructure,
					HasChildren = !string.IsNullOrEmpty(childPackageStructure)
				}
				let key = data.HasChildren ? data.PackQty + data.PackType + data.ChildPackageStructure : data.PackType.ToString()
				group data by key into groupedPacks
				select new { groupedPacks.Key, PackQty = groupedPacks.Sum(p => p.PackQty) };

			return existingStructure.ToDictionary(g => g.Key, g => g.PackQty);
		}

		static string GetChildPackageStructure(UniversalObjectFactory factory, IColumnIndexer packageRow)
		{
			string result = "";

			var query = new ZQuery();
			query.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, packageRow.GetValue(PkgPackageSchema.PK));
			query.OrderBy = PkgPackageSchema.Constants.KP_F3_NKPackType;

			var childPackages = factory.RowFactory.Load(PkgPackageSchema.Constants.TableName, query);
			if (childPackages.Length > 0)
			{
				result = GetChildPackageStructure(childPackages.Select(DataObjectReader.GetColumnIndexerFromRow),
					p => new ZLong(p.GetValue(PkgPackageSchema.KP_PackageQty)),
					p => p.GetValue(PkgPackageSchema.KP_F3_NKPackType).ToUpper(),
					p => GetChildPackageStructure(factory, p));
			}

			return result;
		}

		static string GetChildPackageStructure(Container container, IEnumerable<PackingLine> packages)
		{
			var childPackages = container.Link.HasValue && packages != null
				? packages.Where(p => p.ContainerLink == container.Link).OrderBy(p => p.PackType.GetCodeAsUpperCase()).ToArray()
				: Array.Empty<PackingLine>();

			return GetChildPackageStructure(childPackages);
		}

		static string GetChildPackageStructure(PackingLine packingLine)
		{
			var childPackages = packingLine.PackingLineCollection != null
				? packingLine.PackingLineCollection.OrderBy(p => p.PackType.GetCodeAsUpperCase()).ToArray()
				: Array.Empty<PackingLine>();

			return GetChildPackageStructure(childPackages);
		}

		static string GetChildPackageStructure(PackingLine[] childPackages)
		{
			string result = "";

			if (childPackages != null && childPackages.Length > 0)
			{
				result = GetChildPackageStructure(childPackages, p => p.PackQty.GetValueOrDefault(), p => p.PackType.GetCodeAsUpperCase(), GetChildPackageStructure);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String is empty or contains only symbols.")]
		static string GetChildPackageStructure<T>(IEnumerable<T> childPackages, Func<T, ZLong> getPackQty, Func<T, ZString> getPackType, Func<T, string> getChildPackageStructure)
		{
			var builder = new ZStringBuilder();
			builder.Append("->(");

			builder.Append(string.Join(",", childPackages
				.Select(p => new { PackQty = getPackQty(p), PackType = getPackType(p), ChildStructure = getChildPackageStructure(p) })
				.OrderBy(p => p.PackType + p.ChildStructure)
				.Select(p => string.Format(Culture.Invariant, "{0}x{1}{2}", p.PackQty, p.PackType, p.ChildStructure))));

			builder.Append(")");
			return builder.ToString();
		}
	}
}


