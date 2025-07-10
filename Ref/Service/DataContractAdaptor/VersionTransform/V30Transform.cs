using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.Service.DataContractAdaptor.VersionTransform
{
	public class V30Transform : ITransformStrategy
	{
		static V30Transform v30Transform;

		V30Transform() { }

		public static V30Transform Instance()
		{
			if (v30Transform == null)
			{
				v30Transform = new V30Transform();
			}
			return v30Transform;
		}

		public bool RequireTransform(Type dataSetType, int version)
		{
			return version < 30 && dataSetType == typeof(RefCusProcedure);
		}

		public T Transform<T>(T data)
		{
			var result = data as RefCusProcedure;
			if (result != null)
			{
				result.ZZ6_TemporaryProcedure = false;
				if (!string.IsNullOrEmpty(result.ZZ6_Description) && result.ZZ6_Description.Length > 500)
				{
					result.ZZ6_Description = result.ZZ6_Description.Substring(0, 500);
				}
				if (!string.IsNullOrEmpty(result.ZZ6_ProcedureCode) && result.ZZ6_ProcedureCode.Length > 4)
				{
					result.Deleted = true;
				}
				if (!string.IsNullOrEmpty(result.ZZ6_PreviousProcedureCode) && result.ZZ6_PreviousProcedureCode.Length > 4)
				{
					result.Deleted = true;
				}
			}
			return data;
		}

		public Type[] ToBeTransformedTypes
		{
			get
			{
				return new Type[] {
					typeof(RefCusProcedure)
				};
			}
		}
	}
}
