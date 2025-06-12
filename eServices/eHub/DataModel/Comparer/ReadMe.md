This project implements the functionality to compare EF with SQL DB schema. If you wanna add additional logics for existing DB comparer's behaviors, the following classes are the best place to be overridden:

1. EntityPropertiesDecoder: this class decodes Entity Data Model(EDM)'s properties to get EF schema infos (SQL type, nullable etc). You can also add new schema info behaviors by modifying this class, such as complex types of EDM, max length etc.
2. EFAndSQLDBComparer: this class compares sql type between SQL DB and EDM. You can implement additional functionalities such as comparing max length and EF relationships in this class. Please refer following Github project https://github.com/JonPSmith/EfSchemaCompare for EF SQL relationships compare. The original EfSchemaCompare has lots of bugs and bad structure. Our Comparer is enhanced on EfSchemaCompare, not just a copy.
3. ExceptionalClrTypes.Mappings: when EDM sql type does not match SQL DB type, EFAndSQLDBComparer uses this class for exceptional CLR types look up. Please adjust this look up based on your purposes.
4. Sample reference $/eServices/eHub/DataModel/IntegrationTests/eHubTransactions/eHubTransactionsSchemaComparisionTests.cs
5. Implementation UML Class Diagram

![UML CLASS DIAGRAM](https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/Shared%20Documents/eServices/Development/Integration%20tests/Data%20model%20compare/ClassDiagram.PNG "Class Diagram")