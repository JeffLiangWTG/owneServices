using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefVesselFindBoxListProvider : FindBoxListProvider, ICodePropertyNameProvider
	{
		public RefVesselFindBoxListProvider(RefVesselCollection collection) : base(collection)
		{
		}

		public new RefVesselCollection List => (RefVesselCollection)base.List;

		protected override string GetCodePropertyName(ZGuid pK) => List.UseLloyds ? RefVessel.Schema.RV_LloydsNumber : base.GetCodePropertyName(pK);
		protected override string GetCodePropertyName(Type elementType) => List.UseLloyds ? RefVessel.Schema.RV_LloydsNumber : base.GetCodePropertyName(elementType);
		protected override string GetCodePropertyName(string code) => List.UseLloyds ? RefVessel.Schema.RV_LloydsNumber : base.GetCodePropertyName(code);
		protected override string GetDescriptionPropertyName(Type typeOfElements) => List.UseLloyds ? RefVessel.Schema.RV_Code : base.GetDescriptionPropertyName(typeOfElements);

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			var bizObjs = Enumerable.Empty<BusinessObject>();

			if (!string.IsNullOrEmpty(code))
			{
				ZQuery query = new ZQuery();
				query.IgnoreActiveFilter = true;
				AddCodeEqualsFilter(query, code);

				bizObjs = List.Factory.Load(GetTypeOfElements(code), query);
			}

			return bizObjs;
		}

		#region ICodePropertyNameProvider
		string ICodePropertyNameProvider.GetCodePropertyName(Type type) => GetCodePropertyName(type);
		#endregion

	}
}
