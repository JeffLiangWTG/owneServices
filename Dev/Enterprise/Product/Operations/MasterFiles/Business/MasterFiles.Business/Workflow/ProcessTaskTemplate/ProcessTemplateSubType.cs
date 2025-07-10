using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTemplateSubType
	{
		public ProcessTemplateSubType(string description, ICodeDescriptionPairList list, bool isListRequired = false)
			: this(description, () => list, null, isListRequired, false)
		{
		}

		public ProcessTemplateSubType(string description, Func<ICodeDescriptionPairList> pullListDelegate, bool isListRequired = false)
			: this(description, pullListDelegate, null, isListRequired, false)
		{
		}

		public ProcessTemplateSubType(string description, IActiveBusinessObjectCollection collection, bool isListRequired = false)
			: this(description, null, collection, isListRequired, true)
		{
		}

		protected ProcessTemplateSubType(string description, Func<ICodeDescriptionPairList> pullListDelegate, IActiveBusinessObjectCollection collection, bool isListRequired, bool useCollection)
		{
			this.Description = description;
			if (useCollection)
			{
				this.Collection = collection;
			}
			else
			{
				this.lazyList = new Lazy<ICodeDescriptionPairList>(pullListDelegate);
			}

			this.IsListRequired = isListRequired;
			this.UseCollection = useCollection;
		}

		public string Description { get; private set; }

		public ICodeDescriptionPairList List
		{
			get { return lazyList == null ? null : lazyList.Value; }
		}
		readonly Lazy<ICodeDescriptionPairList> lazyList;

		public IActiveBusinessObjectCollection Collection { get; private set; }
		public bool IsListRequired { get; private set; }
		public bool UseCollection { get; private set; }
	}
}
