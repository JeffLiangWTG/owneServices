using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerConditionValueParameter : Parameter
	{
#if DEBUG
		public TriggerConditionValueParameter() : base(Events.CustomisableEvent00Code) { }
#endif

		public TriggerConditionValueParameter(string eventCode) : base(eventCode) { }

		public TriggerConditionValueParameter(string eventCode, String code, String paramValue) : base(eventCode, code, paramValue) { }

		#region Is Unique Code In The List

		protected override bool IsUniqueCodeInTheList
		{
			get
			{
				var parentCollection = ParentCollections.FirstOrDefault() as TriggerConditionValueParameterCollection;

				return parentCollection == null
						|| parentCollection.Cast<TriggerConditionValueParameter>().All(parameter => parameter == this || parameter.Code != Code);
			}
		}

		#endregion

		#region GetCodeListCore

		protected override CodeDescriptionPairList GetCodeListCore()
		{
			if (parametersList == null)
			{
				parametersList = new CodeDescriptionPairList(base.GetCodeListCore());
				parametersList.Add(new CodeDescriptionPair(Constants.EventReferenceReservedParameters.Codes.Reference, Constants.EventReferenceReservedParameters.Descriptions.Reference));
			}

			return parametersList;
		}

		[ThreadStatic] static CodeDescriptionPairList parametersList;

		#endregion

	}
}
