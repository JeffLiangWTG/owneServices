using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ComparisonPackageWrapperHelper
	{
		public static HashSet<string> CompareSharedAttributes(this IComparisonPackageWrapper packageWrapper, IComparisonPackageWrapper comparisonPackageWrapper, bool ignoreContainer = false)
		{
			Argument.NotNull(packageWrapper, nameof(packageWrapper));
			Argument.NotNull(comparisonPackageWrapper, nameof(comparisonPackageWrapper));

			var result = new HashSet<string>();

			if (packageWrapper.PackTypeCode != comparisonPackageWrapper.PackTypeCode)
			{
				result.Add(Res.GetString("2a97434f-db4c-9390-4e2a-75f28f292fb9", "Pack Type"));
			}

			if (packageWrapper.InspectionTypeCode != comparisonPackageWrapper.InspectionTypeCode)
			{
				result.Add(Res.GetString("575d12cb-8bf4-4360-82ce-aa92a33a2537", "Inspection"));
			}

			if (packageWrapper.IsHighRisk != comparisonPackageWrapper.IsHighRisk)
			{
				result.Add(Res.GetString("d56b4786-8a7d-4180-88d6-6ec789911d7f", "Is High Risk"));
			}

			if (packageWrapper.AdditionalInspectionTypeCode != comparisonPackageWrapper.AdditionalInspectionTypeCode &&
				(comparisonPackageWrapper.AdditionalInspectionTypeCode != ZString.Empty || packageWrapper.AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code)
				&& (comparisonPackageWrapper.AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code || packageWrapper.AdditionalInspectionTypeCode != ZString.Empty))
			{
				result.Add(Res.GetString("6f9781ec-7422-4f09-9b4d-fffd6f4753a6", "Additional Inspection"));
			}

			if (packageWrapper.CommodityCode != comparisonPackageWrapper.CommodityCode)
			{
				result.Add(Res.GetString("8ae074f5-08cd-12af-448f-e58fe87046b2", "Commodity"));
			}

			if (!ignoreContainer && packageWrapper.ContainerNumber != comparisonPackageWrapper.ContainerNumber)
			{
				result.Add(Res.GetString("bc7eaf2f-ab3d-3298-45b5-85a8a8585ba6", "Container Number"));
			}

			if (packageWrapper.HarmonisedCode != comparisonPackageWrapper.HarmonisedCode)
			{
				result.Add(Res.GetString("d56facbc-15fa-be92-44ef-1a335144a630", "Harmonized Code"));
			}

			if (packageWrapper.MarksAndNumbers != comparisonPackageWrapper.MarksAndNumbers)
			{
				result.Add(Res.GetString("303becd4-4752-50b5-4427-db3c6b2f6f19", "Marks & Numbers"));
			}

			if (GetPackageInfoHelper.GetCleanSingleLineText(packageWrapper.GoodsDescription) != comparisonPackageWrapper.GoodsDescription)
			{
				result.Add(Res.GetString("854f0da9-9c26-52bf-4e71-7599c2543f88", "Goods Description"));
			}

			if (packageWrapper.Height != comparisonPackageWrapper.Height)
			{
				result.Add(Res.GetString("77ddb520-f404-858f-4c67-1a77fb35671b", "Height"));
			}

			if (packageWrapper.Length != comparisonPackageWrapper.Length)
			{
				result.Add(Res.GetString("dac05c46-1d2f-0081-4b07-a8400ad4aa09", "Length"));
			}

			if (packageWrapper.Width != comparisonPackageWrapper.Width)
			{
				result.Add(Res.GetString("105585ee-4136-4faf-440e-eefb93062ade", "Width"));
			}

			if (packageWrapper.UnitOfLength != comparisonPackageWrapper.UnitOfLength)
			{
				result.Add(Res.GetString("88640b9c-e632-0c9b-45d0-f5ac49ee7022", "Dimension Unit"));
			}

			result.UnionWith(CompareRequiresTemperatureControl(packageWrapper, comparisonPackageWrapper));

			return result;
		}

		static HashSet<string> CompareRequiresTemperatureControl(IComparisonPackageWrapper packageWrapper, IComparisonPackageWrapper comparisonPackageWrapper)
		{
			var result = new HashSet<string>();

			Argument.NotNull(packageWrapper, nameof(packageWrapper));
			Argument.NotNull(comparisonPackageWrapper, nameof(comparisonPackageWrapper));

			if (packageWrapper.RequiresTemperatureControl != comparisonPackageWrapper.RequiresTemperatureControl)
			{
				result.Add(Res.GetString("7025d157-b776-329d-435e-c3dbd8d008b3", "Requires Temperature Control"));
			}

			if (packageWrapper.RequiresTemperatureControl)
			{
				if (packageWrapper.RequiredTemperatureMaximum != comparisonPackageWrapper.RequiredTemperatureMaximum)
				{
					result.Add(Res.GetString("700b4768-804d-148d-4659-3250a6edb35b", "Required Temperature Maximum"));
				}

				if (packageWrapper.RequiredTemperatureMinimum != comparisonPackageWrapper.RequiredTemperatureMinimum)
				{
					result.Add(Res.GetString("1bb2b346-dd29-eeb7-4a90-cfbb044752dd", "Required Temperature Minimum"));
				}

				if (packageWrapper.RequiredTemperatureUnit != comparisonPackageWrapper.RequiredTemperatureUnit)
				{
					result.Add(Res.GetString("7e7fce77-3c07-f09b-4f05-ac858f4c85d5", "Required Temperature Unit"));
				}
			}

			return result;
		}
	}
}
