using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ProcessManagement.Business
{
	public class CodeDescriptionBoolTreeRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionBoolTreeNodeCollection, CodeDescriptionBoolTreeNodeCollection>
	{
		public CodeDescriptionBoolTreeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage,
			CodeDescriptionBoolTreeRegistryEditorInfo editorInfo,
			CodeDescriptionBoolTreeNodeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolTreeRegistryDataType(defaultValue), editorInfo, storage, RegistryOptions.Default, defaultValue), editorInfo)
		{
			this.defaultValue = defaultValue;

			if (defaultValue.MaxDepth <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(defaultValue), "MaxDepth must be set on the default value");
			}
		}

		protected readonly CodeDescriptionBoolTreeNodeCollection defaultValue;

		public override int MaxLength
		{
			get { return 250; }
		}
	}

	public class CodeDescriptionBoolTreeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolTreeNodeCollection>
	{
		public CodeDescriptionBoolTreeRegistryDataType(CodeDescriptionBoolTreeNodeCollection defaultValue)
			: base(defaultValue)
		{
		}

		protected override CodeDescriptionBoolTreeNodeCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);
			if (DefaultValue != null)
			{
				result.MaxDepth = DefaultValue.MaxDepth;
				result.AllDescriptions = DefaultValue.AllDescriptions;
				result.CodeLists = DefaultValue.CodeLists;

				if (result.AllDescriptions != null || result.CodeLists != null)
				{
					var resultAllDescriptions = result.AllDescriptions?.ToArray();
					var resultCodeLists = result.CodeLists?.ToArray();

					foreach (CodeDescriptionBoolTreeNode node in result)
					{
						int depthIndex = result.GetDepth(node) - 1;
						if (result.AllDescriptions != null)
						{
							if (node.IsSystemAll)
							{
								node.Description = resultAllDescriptions[depthIndex];
							}
						}

						if (result.CodeLists != null)
						{
							node.CodeList = resultCodeLists[depthIndex];
						}
					}
				}
			}
			return result;
		}
	}
}
