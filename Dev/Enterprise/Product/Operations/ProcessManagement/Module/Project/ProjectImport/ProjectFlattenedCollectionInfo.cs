using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectFlattenedCollectionInfo : ImportCollectionInfoImpl
	{
		public ProjectFlattenedCollectionInfo(IBusinessObjectCollection collection)
			: base(collection)
		{
			AddProperty(WorkProjectSchema.Constants.WKP_Type, Res.GetString("259EE2DD-D62A-41E1-A43C-56C4F488B573", "Type"), ZCharacterCasing.Upper);
			AddProperty(WorkProjectSchema.Constants.WKP_SubType, Res.GetString("1AF9FFFE-CDC7-4BB1-BA57-9DC26CD6BA7F", "Subtype"), ZCharacterCasing.Upper);
			AddProperty(WorkProjectSchema.Constants.WKP_Module, Res.GetString("11EA1B18-7A4B-4BC2-98C4-D88B53A34648", "Module"), ZCharacterCasing.Upper);
			AddProperty(WorkProjectSchema.Constants.WKP_Priority, Res.GetString("9208F83F-C4B6-4034-B743-DE324630F568", "Priority"), ZCharacterCasing.Upper);
			AddProperty(WorkProjectSchema.Constants.WKP_Summary, Res.GetString("0173EFB8-D562-42FC-889D-DF2BD9C6986F", "Summary"), isMandatory: true);
			AddProperty(WorkProjectSchema.Constants.WKP_Details, Res.GetString("9CA1F954-6758-46A7-84D0-3D47868EABB0", "Details"));
		}

		void AddProperty(string flattenedPropertyName, string headerText, ZCharacterCasing characterCasing = ZCharacterCasing.Normal, bool isMandatory = false)
		{
			var property = new ImportPropertyInfoImpl<ProjectFlattened>(flattenedPropertyName, isMandatory)
			{
				HeaderText = headerText,
				CharacterCasing = characterCasing,
			};
			Add(property);
		}
	}
}
