using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.NO.Business
{
	public class CusEntryNumberWrapper
	{
		public CusEntryNumberWrapper(BusinessObject parent, ZString entryType, Dictionary<ZString, ZInt> positionParts)
		: this(parent, entryType, Core.Constants.CountryCodes.Norway, positionParts)
		{
		}

		public CusEntryNumberWrapper(BusinessObject parent, ZString entryType, ZString countryCode, Dictionary<ZString, ZInt> positionParts, string partsSeparator = ";")
		{
			this.parent = parent;
			this.entryType = entryType;
			this.countryCode = countryCode;
			this.positionParts = positionParts;
			this.partsSeparator = partsSeparator;
		}

		public ZBool ExistsCusEntryNumber
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber != null;
			}
		}

		public ZString EntryNumber
		{
			get
			{
				SetupCusEntryNumber(false);
				return cusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			}
		}

		public void SetEntryNumberPart(ZString partValue, ZString partName, ZPropertyInfo propertyInfo)
		{
			var oldValue = EntryNumber;
			if (positionParts.ContainsKey(partName))
			{
				SetupCusEntryNumber(true);
				if (cusEntryNumber.CE_EntryNum.IsEmpty && !partValue.IsEmpty)
				{
					cusEntryNumber.CE_EntryNum = partsSeparator.IsEmpty ? ZString.Empty : new ZString(partsSeparator[0], positionParts.Keys.Count - 1);
				}

				var partIndex = positionParts[partName];
				var parts = cusEntryNumber.CE_EntryNum.Split(partsSeparator);
				parts[partIndex] = partValue;

				cusEntryNumber.CE_EntryNum = ZString.Join(partsSeparator, parts);

				propertyInfo.RefreshBinding(oldValue);
			}
		}

		public ZString GetEntryNumberPart(ZString partName)
		{
			var partValue = ZString.Empty;
			if (ExistsCusEntryNumber && positionParts.TryGetValue(partName, out var partIndex))
			{
				partValue = SplitCusEntryNum(partIndex);
			}
			return partValue;
		}

		void SetupCusEntryNumber(bool createIfNotExists)
		{
			if (cusEntryNumber == null || cusEntryNumber.IsDeleted)
			{
				cusEntryNumber = createIfNotExists ? CusEntryNumber.LoadOrCreate(parent, entryType, countryCode) : CusEntryNumber.Load(parent, entryType, countryCode);
				if (cusEntryNumber != null)
				{
					parent.RegisterEditableChildObject(cusEntryNumber);
				}
			}
		}

		ZString SplitCusEntryNum(int part)
		{
			var partValue = ZString.Empty;
			var splits = cusEntryNumber.CE_EntryNum.Split(partsSeparator);
			if (part >= 0 && part < splits.Length)
			{
				partValue = splits[part];
			}
			return partValue;
		}

		CusEntryNumber cusEntryNumber;
		readonly BusinessObject parent;
		readonly ZString entryType;
		readonly ZString countryCode;
		readonly ZString partsSeparator = ";";
		readonly Dictionary<ZString, ZInt> positionParts = new Dictionary<ZString, ZInt>();
	}
}
