using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CargoWise.eHub.DataAccess.Integration;
using System.Collections;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
	public static class TransformAccessor
	{
		public static string CallActionProcedure(string procedure, string outputParm, params string[] inputParms)
		{
			return transformAccessor.CallActionProcedure(procedure, outputParm, inputParms);
		}

		static ITransformAccessor transformAccessor = CargoWise.eHub.DataAccess.Integration.DataAccessFactories.NewTransformAccessorInstance();
	}
}
