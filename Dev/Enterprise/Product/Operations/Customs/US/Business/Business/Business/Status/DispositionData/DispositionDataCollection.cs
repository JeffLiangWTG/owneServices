using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class DispositionDataCollection : DependentCusAddInfoCollection<DispositionData, BusinessObject>
	{
		public DispositionDataCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USDisposition)
		{
		}

		public DispositionData AddNewIfNotExist(ZString dispositionCode, ZDateTime dispositionDate, string source = "")
		{
			DispositionData result = null;

			foreach (DispositionData one in this)
			{
				if (one.US_Code == dispositionCode && one.US_DispositionDate == dispositionDate)
				{
					result = one;
					break;
				}
			}

			if (result == null)
			{
				result = AddNew();
				result.US_Code = dispositionCode;
				result.US_DispositionDate = dispositionDate;
				result.US_Source = source;
			}

			result.US_Order = GetMaxOrderNumber() + 1;

			return result;
		}

		public DispositionData AddNewIfNotExist(ZString dispositionCode, ZDateTime dispositionDate, ZString fTZIDType, ZString fTZNumber, string source = "")
		{
			DispositionData result = null;

			foreach (DispositionData one in this)
			{
				if (one.US_Code == dispositionCode && one.US_DispositionDate == dispositionDate)
				{
					result = one;
					break;
				}
			}

			if (result == null)
			{
				result = AddNew();
				result.US_Code = dispositionCode;
				result.US_DispositionDate = dispositionDate;
				result.US_Source = source;
				result.US_FTZIDType = fTZIDType;
				result.US_FTZNumber = fTZNumber;
			}

			result.US_Order = GetMaxOrderNumber() + 1;

			return result;
		}
		public bool HasCode(ZString code)
		{
			bool result = false;
			foreach (DispositionData disposition in this)
			{
				if (disposition.US_Code == code)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public IEnumerable<DispositionData> GetLatestDispositions(ZString source)
		{
			var maxDispositionDate = GetMaxDispositionDate();
			return !maxDispositionDate.IsEmpty ? this.Cast<DispositionData>().Where(x => x.US_DispositionDate == maxDispositionDate && x.US_Source == source) : Enumerable.Empty<DispositionData>();
		}

		public DispositionData GetLatestDisposition(ICodeDescriptionPairList list = null, Func<ZString, ZBool> shouldIgnoreDispositionCode = null)
		{
			DispositionData result = null;
			foreach (DispositionData data in this)
			{
				var dispositionCode = data.US_Code;
				if (shouldIgnoreDispositionCode == null || !shouldIgnoreDispositionCode(dispositionCode))
				{
					if (list == null || list.ContainsCode(dispositionCode))
					{
						if (result == null || data.US_DispositionDate > result.US_DispositionDate || (data.US_DispositionDate == result.US_DispositionDate && data.US_Order > result.US_Order))
						{
							result = data;
						}
					}
				}
			}
			return result;
		}

		public IEnumerable<DispositionData> GetLatestDispositions(ICodeDescriptionPairList list = null)
		{
			return this.OfType<DispositionData>().GetLatestDispositions(list);
		}

		ZInt GetMaxOrderNumber()
		{
			var result = ZInt.Zero;
			if (Count > 0)
			{
				foreach (DispositionData data in this)
				{
					if (data.US_Order > result)
					{
						result = data.US_Order;
					}
				}
			}
			return result;
		}

		public ZDateTime GetMaxDispositionDate()
		{
			var dispDate = ZDateTime.Empty;
			if (Count > 0)
			{
				foreach (DispositionData data in this)
				{
					if (dispDate.IsEmpty || data.US_DispositionDate > dispDate)
					{
						dispDate = data.US_DispositionDate;
					}
				}
			}
			return dispDate;
		}

		public ZString GetDispositionsFor1302Document(string endLine)
		{
			ZString result = ZString.Empty;

			DispositionDataCollection sortedDispositions = this;
			if (sortedDispositions.Count > 0)
			{
				sortedDispositions.Sort(USDispositionDataAddInfoSchema.US_DispositionDate.Name, ListSortDirection.Ascending);
				foreach (DispositionData disposition in sortedDispositions)
				{
					result += disposition.US_Code + " " +
						disposition.DispositionCodeDesc + " " +
						disposition.US_DispositionDate.ToString("MM/dd/yyyy h:mm:ss tt") // date time format string constant
						+ endLine;
				}
			}

			return result;
		}
	}
}
