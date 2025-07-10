using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class EffectiveValueManager
	{
		public static void ClearValueIfSame<TBizObj>(IZType value, string fieldName, IEnumerable<TBizObj> bizObjs, Action<TBizObj> extraAction = null, bool isClearForciblyWhenIsDefaultValue = false)
			where TBizObj : BusinessObject, IEffectiveValueManagerSupporter
		{
			if (isClearForciblyWhenIsDefaultValue || !value.IsDefault)
			{
				foreach (var bizObj in bizObjs)
				{
					var bizObjValue = (IZType)bizObj[fieldName];

					if (bizObjValue.Equals(value))
					{
						using (bizObj.EffectiveValueManager.SuspendEffectiveValue(fieldName, value))
						{
							bizObj[fieldName] = bizObjValue.Default;
						}

						var infoToRefresh = bizObj.ZPropertyInfoHash[fieldName];
						if (infoToRefresh != null)
						{
							infoToRefresh.RefreshBinding();
						}

						if (extraAction != null)
						{
							extraAction(bizObj);
						}
					}
				}
			}
		}

		public IDisposable SuspendEffectiveValue(string fieldName, IZType value)
		{
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out var holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { Value = value };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		public IZType GetEffectiveValue(string fieldName)
		{
			return EffectiveValueSuspenders.TryGetValue(fieldName, out var holder) ? holder.Value : null;
		}

		public T GetEffectiveValueToReturn<T>(T baseValue, string fieldName, Func<T> getParentValue) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty)
			{
				IZType effectiveValue = GetEffectiveValue(fieldName);
				if (effectiveValue != null)
				{
					result = (T)effectiveValue;
				}
				else
				{
					result = getParentValue();
				}
			}

			return result;
		}

		public T GetEffectiveValueToSet<T>(T valuePassed, T parentValue) where T : IZType
		{
			T result = valuePassed;
			if (result.Equals(parentValue))
			{
				result = (T)valuePassed.Default;
			}
			return result;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(EffectiveValueManager manager, string fieldName)
			{
				this.manager = manager;
				this.fieldName = fieldName;
			}

			public IZType Value { get; set; }

			readonly EffectiveValueManager manager;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				manager.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}
	}
}
