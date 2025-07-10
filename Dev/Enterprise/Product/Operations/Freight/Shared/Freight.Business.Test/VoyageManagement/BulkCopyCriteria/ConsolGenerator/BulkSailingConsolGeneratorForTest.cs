using System;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	class BulkSailingConsolGeneratorForTest : BulkSailingConsolGenerator
	{
		public BulkSailingConsolGeneratorForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[BusinessObjectTestExclude]
		public Func<CommonConsol> GetTemplateConsolImplementation { get; set; }
		protected override CommonConsol GetTemplateConsol()
		{
			return GetTemplateConsolImplementation != null ? GetTemplateConsolImplementation() : Factory.Load<CommonConsol>(TemplateConsolPK);
		}

		[BusinessObjectTestExclude]
		public Func<CommonConsol> CreateConsolFromTemplateImplementation { get; set; }
		protected override CommonConsol CreateConsolFromTemplate(CommonConsol templateConsol)
		{
			return CreateConsolFromTemplateImplementation != null ?
				CreateConsolFromTemplateImplementation() : (templateConsol != null ? templateConsol.TemplateCopy(CopyShipments, false) : null);
		}

		protected override CommonConsol CreateNewConsol()
		{
			return Factory.New<CommonConsol>();
		}
	}
}
