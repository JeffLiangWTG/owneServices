using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business
{
	[RegistryEditor("Enterprise.ProcessManagement.GUI.CodeDescriptionBoolTreeRegistryEditor, Enterprise.ProcessManagement.GUI")]
	public class CodeDescriptionBoolTreeRegistryEditorInfo : CodeDescriptionBoolRegistryEditorInfo, IRegistryEditorInfo
	{
		public CodeDescriptionBoolTreeRegistryEditorInfo(
			MultilingualString[] captions,
			MultilingualString boolColumnCaption,
			ICondition isBoolColumnVisibleCondition,
			bool isBoolColumnVisible,
			bool isOnlyBoolColumnEditable)
			: base(boolColumnCaption, isBoolColumnVisibleCondition, isBoolColumnVisible, isOnlyBoolColumnEditable)
		{
			Captions = captions;
		}

		public IEnumerable<MultilingualString> Captions { get; }
		Type IRegistryEditorInfo.BaseDataTypeToBeEdited
		{
			get { return typeof(CodeDescriptionBoolTreeNodeCollection); }
		}
	}
}
