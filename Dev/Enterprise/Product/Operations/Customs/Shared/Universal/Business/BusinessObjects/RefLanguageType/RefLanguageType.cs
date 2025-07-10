using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	[CodeProperty(Schema.ZX6_Language)]
	public class RefLanguageType : AutoRefLanguageType
	{
		public RefLanguageType(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
