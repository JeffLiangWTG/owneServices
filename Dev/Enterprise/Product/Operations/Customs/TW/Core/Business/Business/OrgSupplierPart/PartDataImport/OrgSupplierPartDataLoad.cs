using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class OrgSupplierPartDataLoad : GlobalOrgSupplierPartDataLoad, Integration.Customs.TW.IOrgSupplierPartDataLoad
	{
		public static class TWFieldNames
		{
			public const string ChineseDescription = "ChineseDescription";
			public const string UnitOfMeasure = "UnitOfMeasure";
			public const string AssignedNumber1 = "AssignedNumber1";
			public const string AssignedNumber2 = "AssignedNumber2";
			public const string AssignedNumber3 = "AssignedNumber3";
			public const string AssignedNumber4 = "AssignedNumber4";
			public const string AssignedNumber5 = "AssignedNumber5";
			public const string AssignedNumber6 = "AssignedNumber6";
			public const string AssignedNumber7 = "AssignedNumber7";
			public const string AssignedNumber8 = "AssignedNumber8";
			public const string AssignedNumber9 = "AssignedNumber9";
			public const string AssignedNumber10 = "AssignedNumber10";
			public const string PermitNumber1 = "PermitNumber1";
			public const string PermitLineNo1 = "PermitLineNo1";
			public const string PermitNumber2 = "PermitNumber2";
			public const string PermitLineNo2 = "PermitLineNo2";
			public const string PermitNumber3 = "PermitNumber3";
			public const string PermitLineNo3 = "PermitLineNo3";
			public const string PermitNumber4 = "PermitNumber4";
			public const string PermitLineNo4 = "PermitLineNo4";
			public const string PermitNumber5 = "PermitNumber5";
			public const string PermitLineNo5 = "PermitLineNo5";
		}

		protected override IEnumerable<string> GetFieldNames()
		{
			foreach (var property in base.GetFieldNames())
			{
				yield return property;
			}
			yield return TWFieldNames.ChineseDescription;
			yield return TWFieldNames.UnitOfMeasure;
			yield return TWFieldNames.AssignedNumber1;
			yield return TWFieldNames.AssignedNumber2;
			yield return TWFieldNames.AssignedNumber3;
			yield return TWFieldNames.AssignedNumber4;
			yield return TWFieldNames.AssignedNumber5;
			yield return TWFieldNames.AssignedNumber6;
			yield return TWFieldNames.AssignedNumber7;
			yield return TWFieldNames.AssignedNumber8;
			yield return TWFieldNames.AssignedNumber9;
			yield return TWFieldNames.AssignedNumber10;
			yield return TWFieldNames.PermitNumber1;
			yield return TWFieldNames.PermitLineNo1;
			yield return TWFieldNames.PermitNumber2;
			yield return TWFieldNames.PermitLineNo2;
			yield return TWFieldNames.PermitNumber3;
			yield return TWFieldNames.PermitLineNo3;
			yield return TWFieldNames.PermitNumber4;
			yield return TWFieldNames.PermitLineNo4;
			yield return TWFieldNames.PermitNumber5;
			yield return TWFieldNames.PermitLineNo5;
		}

		public class TWPartsDataToLoad : GlobalPartsDataToLoad
		{
			public ZString ChineseDescription;
			public ZString UnitOfMeasure;
			public List<ZString> AssignedNumbers;
			public List<(ZString, ZInt)> Permits;
		}

		TWPartsDataToLoad PartsDataToLoad => partsDataToLoad ?? (partsDataToLoad = new TWPartsDataToLoad());
		TWPartsDataToLoad partsDataToLoad;

		protected override void ProcessDataForThisLine(OCsvLine line)
		{
			PartsDataToLoad.AssignedNumbers = new List<ZString>();
			PartsDataToLoad.Permits = new List<(ZString, ZInt)>();
			AddAssignedNumber(line, TWFieldNames.AssignedNumber1);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber2);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber3);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber4);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber5);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber6);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber7);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber8);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber9);
			AddAssignedNumber(line, TWFieldNames.AssignedNumber10);
			AddPermit(line, TWFieldNames.PermitNumber1, TWFieldNames.PermitLineNo1);
			AddPermit(line, TWFieldNames.PermitNumber2, TWFieldNames.PermitLineNo2);
			AddPermit(line, TWFieldNames.PermitNumber3, TWFieldNames.PermitLineNo3);
			AddPermit(line, TWFieldNames.PermitNumber4, TWFieldNames.PermitLineNo4);
			AddPermit(line, TWFieldNames.PermitNumber5, TWFieldNames.PermitLineNo5);
			base.ProcessDataForThisLine(line);
		}

		protected override PartsDataToLoad GetPartsDataToLoad() => PartsDataToLoad;

		void AddAssignedNumber(OCsvLine line, string assignedNumberField)
		{
			var assignedNumber = TryGetStringValue(line, assignedNumberField);
			if (!assignedNumber.IsEmpty && !PartsDataToLoad.AssignedNumbers.Contains(assignedNumber))
			{
				PartsDataToLoad.AssignedNumbers.Add(assignedNumber);
			}
		}

		void AddPermit(OCsvLine line, string permitNumberField, string permitLineNoField)
		{
			var permitNumber = TryGetStringValue(line, permitNumberField);
			TryGetValue(line, permitLineNoField, out ZInt permitLineNo);
			if (!permitNumber.IsEmpty && !permitLineNo.IsEmpty)
			{
				PartsDataToLoad.Permits.Add((permitNumber, permitLineNo));
			}
		}

		protected override void AddDataToPivot(BaseCusClassPartPivot pivot, PartsDataToLoad partsData)
		{
			base.AddDataToPivot(pivot, partsData);
			AddCountrySpecificDataToPivot((CusClassPartPivot)pivot, (TWPartsDataToLoad)partsData);
		}

		void AddCountrySpecificDataToPivot(CusClassPartPivot pivot, TWPartsDataToLoad partsData)
		{
			SetValue(pivot.CI_NDescriptionInfo, partsData.ChineseDescription);
			SetValue(pivot.CI_PartPivotUOMInfo, partsData.UnitOfMeasure);
			UpdateAssignedNumbers(pivot.AssignedCusClassPartPivotRefCollection, partsData.AssignedNumbers);
			UpdatePermits(pivot.ProductPermitCusSupportingCollection, partsData.Permits);
		}

		void UpdateAssignedNumbers(AssignedCusClassPartPivotRefCollection assignedNumberCollection, IEnumerable<ZString> assignedNumbers)
		{
			foreach (var assignedNumber in assignedNumbers)
			{
				var pivotRef = assignedNumberCollection.Find(c => c.CIR_ReferenceNumber == assignedNumber).FirstOrDefault();
				if (pivotRef == null)
				{
					pivotRef = assignedNumberCollection.AddNew();
					SetValue(pivotRef.CIR_ReferenceNumberInfo, assignedNumber);
				}
			}
		}

		void UpdatePermits(ProductPermitCusSupportingCollection permitCollection, IEnumerable<(ZString, ZInt)> permits)
		{
			foreach (var permit in permits)
			{
				var permitNumber = permit.Item1;
				var permitLineNo = permit.Item2;
				var productPermit = permitCollection.Find(c => c.CSI_ReferenceNumber == permitNumber && c.CSI_LineNo == permitLineNo).FirstOrDefault();
				if (productPermit == null)
				{
					productPermit = permitCollection.AddNew();
					SetValue(productPermit.CSI_ReferenceNumberInfo, permitNumber);
					SetValue(productPermit.CSI_LineNoInfo, permitLineNo);
				}
			}
		}
	}
}
