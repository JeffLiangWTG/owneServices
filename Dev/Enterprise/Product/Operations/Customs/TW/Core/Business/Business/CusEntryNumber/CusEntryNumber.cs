using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class CusEntryNumber : Common.CusEntryNumber
	{
		public CusEntryNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static CusEntryNumber New(BusinessObject businessObject, ZString entryType, ZString countryCode)
		{
			return New<CusEntryNumber>(businessObject, entryType, countryCode);
		}

		public new static CusEntryNumber Load(BusinessObject parent, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
		{
			return Load<CusEntryNumber>(parent, entryType, countryCode, reLoadExistingRows);
		}

		public new static CusEntryNumber LoadOrCreate(BusinessObject businessObject, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
		{
			return LoadOrCreate<CusEntryNumber>(businessObject, entryType, countryCode, reLoadExistingRows);
		}

		public new static T LoadOrCreate<T>(BusinessObject businessObject, ZString entryType, ZString countryCode, bool reLoadExistingRows = false)
			where T : CusEntryNumber
		{
			return Load<T>(businessObject, entryType, countryCode, reLoadExistingRows) ?? New<T>(businessObject, entryType, countryCode);
		}

		public override ZString CE_EntryNum
		{
			get => base.CE_EntryNum;
			set
			{
				var oldValue = CE_EntryNum;
				base.CE_EntryNum = value;
				if (!IsCopying && oldValue != CE_EntryNum && Parent is IEntryNumberGeneratorProvider)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}
	}
}
