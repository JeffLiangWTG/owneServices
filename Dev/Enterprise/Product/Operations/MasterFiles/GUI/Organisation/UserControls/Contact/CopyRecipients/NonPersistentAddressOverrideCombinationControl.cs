using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Grid.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public class NonPersistentAddressOverrideCombinationControl<TCopyRecipientOwner> : ZMultiCombinationControl where TCopyRecipientOwner : NonPersistentBusinessObject, ILinkable
	{
		public NonPersistentAddressOverrideCombinationControl(NonPersistentAddressOverrideColumnStyleInfo<TCopyRecipientOwner> columnInfo)
		{
			ColumnInfo = columnInfo;
			CharacterCasing = CharacterCasing.Normal;
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
		}
		protected NonPersistentAddressOverrideColumnStyleInfo<TCopyRecipientOwner> ColumnInfo { get; }

		protected override IGridControl CreateGridControl(FieldType typeToCreate)
		{
			if (typeToCreate == FieldType.TextCodeFindBox)
			{
				var findBox = new NonPersistentCopyRecipientsFindBox
				{
					CodeBox = { CharacterCasing = CharacterCasing.Lower }
				};

				return findBox;
			}

			return base.CreateGridControl(typeToCreate);
		}

		protected override void EditCore(CurrencyManager source, string mappingName)
		{
			if (CurrentEditor is NonPersistentCopyRecipientsFindBox copyRecipientFindBox)
			{
				var parentBusinessObject = source?.GetCurrent();
				if (parentBusinessObject != null)
				{
					copyRecipientFindBox.CopyRecipients = ColumnInfo.GetCopyRecipients((TCopyRecipientOwner)parentBusinessObject);
					copyRecipientFindBox.EmailAddressPropertyName = ColumnInfo.EmailAddressPropertyName;
					MaybeBindTextTemplatesFactory(copyRecipientFindBox.CodeBox, source, mappingName);
				}
			}
		}
	}
}
