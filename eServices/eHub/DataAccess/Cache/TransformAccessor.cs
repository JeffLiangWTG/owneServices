using System;
using System.Collections.Generic;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;

namespace CargoWise.eHub.DataAccess.Cache
{
	[Serializable]
	class TransformAccessor : ITransformAccessor
	{
		private const string TRANSFORM_ACCESSOR_CACHE_KEY = "CargoWise.eHub.DataAccess.Cache.TransformAccessor, Version=3.0.0.0";

		#region ITransformAccessor Members

		public List<TransformSet> SelectTransformsByPartiesMessage(string senderID, string recipientID, string sourceMessageType, bool? tsDirection)
		{
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
			return dbAccessor.SelectTransformsByPartiesMessage(senderID, recipientID, sourceMessageType, tsDirection);
		}

        public string GetRecipientCode(string senderClientCode, string recipientClientCode, string transformationName, string codeSet, string resultField, string key1, string key2, string key3, string key4, string key5)
		{
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
			return dbAccessor.GetRecipientCode(senderClientCode, recipientClientCode, transformationName, codeSet, resultField, key1, key2, key3, key4, key5);
		}

		public bool IsFlatFile(string messageType, out string charset)
		{
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
			return dbAccessor.IsFlatFile(messageType, out charset);
		}

		public bool IsEDI(string messageType)
		{
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
			return dbAccessor.IsEDI(messageType);
		}

        public bool IsJson(string messageType)
        {
            var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
            return dbAccessor.IsJson(messageType);
        }

        public void SetCodeMapsTestingContext(CodeMapsTestingContext ctx)
        {
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
            dbAccessor.SetCodeMapsTestingContext(ctx);
        }

		public string CallActionProcedure(string procedure, string outputParm, string[] inputParms)
		{
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
			return dbAccessor.CallActionProcedure(procedure, outputParm, inputParms);
		}

		public bool IsPostAssembleMapping(string messageType, out string postAssembleWrapper)
		{
			var dbAccessor = DataAccessFactories.NewTransformAccessorInstance(TRANSFORM_ACCESSOR_CACHE_KEY);
			return dbAccessor.IsPostAssembleMapping(messageType, out postAssembleWrapper);
		}

		#endregion

	}
}
