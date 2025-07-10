using CargoWise.Types;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISBondDataWrapper : IDISBondData
	{
		public DISBondDataWrapper(DISBondData bondData, ZString filer)
		{
			this.bondData = bondData;
			this.filer = filer;
		}

		readonly DISBondData bondData;
		readonly ZString filer;

		ZString IDISBondData.AgentIDNumber
		{
			get { return bondData.AgentIDNumber; }
		}

		ZDecimal IDISBondData.BondAmount
		{
			get { return bondData.BondAmount; }
		}

		BondNameType IDISBondData.BondName
		{
			get { return BondNameTypeList.GetBondNameType(bondData.BondName); }
		}

		ZString IDISBondData.BondNumber
		{
			get { return bondData.BondNumber; }
		}

		ZString IDISBondData.BondType
		{
			get { return bondData.BondType; }
		}

		ZString IDISBondData.Filer
		{
			get { return filer; }
		}

		ZString IDISBondData.SuretyCode
		{
			get { return bondData.SuretyCode; }
		}
	}
}
