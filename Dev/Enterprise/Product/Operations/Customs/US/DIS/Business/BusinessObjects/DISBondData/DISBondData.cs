using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DIS.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class DISBondData : AutoDISBondData
	{
		public DISBondData(DISDocument disDocument)
			: base(disDocument.Factory)
		{
			this.disDocument = disDocument;
		}

		readonly DISDocument disDocument;

		[List(nameof(DefaultBondDataList))]
		public override ZString DefaultBondCode
		{
			get { return base.DefaultBondCode; }
			set
			{
				var oldValue = DefaultBondCode;
				base.DefaultBondCode = value;

				if (oldValue != value)
				{
					DefaultBondDataFromBondCode();
				}
			}
		}

		public CodeDescriptionPairList DefaultBondDataList
		{
			get { return disDocument.DefaultBondDataList; }
		}

		void DefaultBondDataFromBondCode()
		{
			var defaultData = disDocument.GetDefaultBondData(DefaultBondCode);
			if (defaultData != null)
			{
				BondName = BondNameTypeList.GetCodeFrom(defaultData.BondName);
				BondAmount = defaultData.BondAmount;
				BondNumber = defaultData.BondNumber;
				SuretyCode = defaultData.SuretyCode;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "used by ProvideMetaDataProperty attribute")]
		bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			if (property.Name == Schema.DefaultBondCode)
			{
				return false;
			}
			return !DefaultBondCode.IsEmpty;
		}

		[List(nameof(BondNameList))]
		public override ZString BondName
		{
			get { return base.BondName; }
			set { base.BondName = value; }
		}

		public CodeDescriptionPairList BondNameList
		{
			get { return Factory.GetCachedValue<BondNameTypeList>(); }
		}
	}
}
