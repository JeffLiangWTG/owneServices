using System;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V38Transform : ITransformStrategy
	{
		static V38Transform v38Transform;

		V38Transform() { }

		public static V38Transform Instance()
		{
			if (v38Transform == null)
			{
				v38Transform = new V38Transform();
			}
			return v38Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 38 && (dataSetType == typeof(RefDocOrgCusCode));
		}

		public T Transform<T>(T data)
		{
			if (data is RefDocOrgCusCode docOrgCusCode)
			{
				if (docOrgCusCode.DOC_DocumentType == "HBL")
				{
					docOrgCusCode.Deleted = true;
				}
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefDocOrgCusCode)
				};
			}
		}
	}
}
