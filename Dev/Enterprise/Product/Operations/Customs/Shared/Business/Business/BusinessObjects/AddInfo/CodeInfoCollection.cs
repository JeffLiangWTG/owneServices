using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public abstract class CodeInfoCollection : NonPersistentBusinessObjectCollection<CodeInfo>
	{
		protected CodeInfoCollection(ZPropertyInfo addInfoStringInfo)
			: base(addInfoStringInfo.BizObj.Factory)
		{
			this.Parent = addInfoStringInfo.BizObj;
			this.addInfoStringInfo = addInfoStringInfo;
			LoadFromString((ZString)addInfoStringInfo.Value);
		}
		internal readonly ZPropertyInfo addInfoStringInfo;
		public readonly BusinessObject Parent;

		public CodeInfo GetElementWithThisCode(string code)
		{
			foreach (CodeInfo codeInfo in this)
			{
				if (codeInfo.ZO_Code == code)
				{
					return codeInfo;
				}
			}
			return null;
		}

		public bool ContainsCode(ZString zO_Code)
		{
			return GetElementWithThisCode(zO_Code) != null;
		}

		public ZString AggregatedCodes
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (CodeInfo codeInfo in this)
				{
					result.Append(codeInfo.ZO_Code + CodeSeparatorInAggregated);
				}
				return result.ToString().Trim(CodeSeparatorInAggregated);
			}
		}

		public ZString AggregatedDatas
		{
			get
			{
				StringBuilder result = new StringBuilder();
				foreach (CodeInfo codeInfo in this)
				{
					result.Append(codeInfo.ZO_Data + CodeSeparatorInAggregated);
				}
				return result.ToString().Trim(CodeSeparatorInAggregated);
			}
		}

		#region Serialisation/Deserialisation

		internal void LoadFromString(ZString addInfoString)
		{
			isInitialised = false;
			RemoveAll();
			if (!addInfoString.IsEmpty)
			{
				ZString[] pairs = addInfoString.Split(CodeInfoSeparator);
				foreach (ZString pair in pairs)
				{
					ZString[] codeValue = pair.Split(BaseAddInfo.CodeValueSeparator);
					if (codeValue.Length > 0)
					{
						CodeInfo element = GetElementWithThisCode(codeValue[0]) ?? AddNew();
						element.LoadFromString(pair);
					}
				}
			}
			isInitialised = true;
		}
		bool isInitialised;

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			foreach (CodeInfo codeInfo in this)
			{
				result.Append(codeInfo.ToString() + CodeInfoSeparator);
			}
			return result.ToString().Trim(CodeInfoSeparator);
		}

		#endregion

		#region Overrides

		public const char CodeInfoSeparator = '^';
		const char CodeSeparatorInAggregated = '/';

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (!IsValidationSuspended)
			{
				RunPreSaveValidation();//Duplicate Code validation
			}

			CodeInfo @object = bizO as CodeInfo;
			if (@object != null)
			{
				@object.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(NewElement_HasChangesChanged);
			}
			UpdateAddInfoStringInfo();
		}

		protected sealed override BusinessObject CreateNonPersistentBusinessObject()
		{
			CodeInfo newElement = CreateNewCodeInfo();
			newElement.Collection = this;
			newElement.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(NewElement_HasChangesChanged);
			return newElement;
		}

		protected abstract CodeInfo CreateNewCodeInfo();

		void NewElement_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			UpdateAddInfoStringInfo();
		}

		protected void UpdateAddInfoStringInfo()
		{
			if (addInfoStringInfo != null && isInitialised)
			{
				addInfoStringInfo.Value = (ZString)this.ToString();
			}
		}

		#endregion
	}
}
