using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class OfficeCodeCollection : EuOfficeCodeCollection
	{
		public OfficeCodeCollection(BusinessObject master) : base(master)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(OfficeCode);

		public new OfficeCode AddNew() => (OfficeCode)base.AddNew();

		public new OfficeCode AddNew(ZString code) => (OfficeCode)base.AddNew(code);

		public new OfficeCode this[int index] => (OfficeCode)base[index];
	}
}
