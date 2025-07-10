## Release 23.8.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------------------------------------------------------------
CUS001  | Naming       | Warning  | SomeRefZZFieldsAreCaseInsensitiveAnalyzer
CW1013  | CargoWiseOne | Warning  | CargoWiseProgressBarAnalyzer
CW1014  | Reference    | Warning  | EmbeddedIconRuleAnalyzer
CW1015  | CargoWiseOne | Warning  | NoApplicationOpenFormsAnalyzer
CW1016  | CargoWiseOne | Warning  | DontUseFlexCelPdfExportAnalyzer
CW1017  | CargoWiseOne | Warning  | DpiAwareDevelopmentRuleAnalyzer
CW1018  | Inheritance  | Warning  | HttpApplicationRuleAnalyzer 
CW1018A | Naming       | Warning  | HttpApplicationDbAppSettingsRuleAnalyzer
CW1019  | CargoWiseOne | Warning  | NoSqlTransactionRollbackAnalyzer
CW1020  | CargoWiseOne | Warning  | DontUseCurrencyManagerCurrentAnalyzer
CW1021  | CargoWiseOne | Disabled | StaticFieldsAreThreadStaticRuleAnalyzer
CW1022  | Naming       | Warning  | ThreadStaticSetInStaticInitializerRuleAnalyzer
CW1023  | CargoWiseOne | Warning  | ImmutableRuleAnalyzer
CW1024  | Concurrency  | Warning  | BadConcurrentCollectionAccessRuleAnalyzer
CW1030A | Naming       | Warning  | FileModeOpenOrCreateRuleAnalyzer
CW1031  | Reference    | Warning  | DoNotUseGetVersionExOrEnvironmentDotOSVersionRuleAnalyzer
CW1032  | CargoWiseOne | Disabled | UseMoqForMockingAnalyzer
CW1040  | CargoWiseOne | Warning  | MixedDpiAwareAndUnawareRuleAnalyzer
CW1041  | CargoWiseOne | Warning  | DoubleScalingComponentsRuleAnalyzer
CW1042  | CargoWiseOne | Warning  | MixedDpiScalingAssignmentRuleAnalyzer
CW1043  | Naming       | Warning  | EAdaptorNamingAnalyzer
CW1044  | CargoWiseOne | Warning  | FactoryGetDatabaseCountCollectionCountRuleAnalyzer
CW1046  | CargoWiseOne | Warning  | DoNotSpecifyTooltipsManuallyAnalyzer
CW1049  | CargoWiseOne | Disabled | DontUseApplicationDoEventsAnalyzer
CW1050  | CargoWiseOne | Disabled | UseTimespanTypeForDurationAnalyzer
CW1051  | CargoWiseOne | Disabled | DoNotUseBaseSourcePath
CW1052  | CargoWiseOne | Disabled | DoNotCastFactoryMethodAnalyzer
CW1053  | CargoWiseOne | Warning  | DontUseSRDBNameConstantRuleAnalyzer
CW1054  | CargoWiseOne | Warning  | DoNotUseHardCodedPathsAnalyzer
CW1055  | CargoWiseOne | Disabled | DoNotUseProcessGetProcessesAnalyzer
CW1056  | CargoWiseOne | Disabled | DoNotUseGCCollectAnalyzer
CW1057  | CargoWiseOne | Disabled | DoNotHardcodeMailServerNameAnalyzer
CW1058  | CargoWiseOne | Disabled | DoNotUseDebuggerIsAttachedAnalyzer
CW1060  | CargoWiseOne | Disabled | DoNotUseDateTimeNowRuleAnalyzer
CW1061  | CargoWiseOne | Disabled | DoNotUseDateTimeUtcNowRuleAnalyzer
CW1062  | CargoWiseOne | Disabled | DoNotUseDateTimeTodayRuleAnalyzer
CW1063  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsScreenAnalyzer
CW1065  | CargoWiseOne | Disabled | EncryptSqlConnectionRuleAnalyzer
CW1066  | CargoWiseOne | Disabled | GotoDefaultOrCaseAnalyzer
CW1067  | CargoWiseOne | Disabled | ApplicationThreadExceptionAnalyzer
CW1068  | CargoWiseOne | Disabled | DoNotUseMathRoundAnalyzer
CW1069  | CargoWiseOne | Disabled | DoNotUseEventLogWriteEntryAnalyzer
CW1071  | CargoWiseOne | Disabled | DoNotUseGCWaitForPendingFinalizersOrGetTotalMemoryAnalyzer
CW1072  | CargoWiseOne | Warning  | DoNotUseDBNameInSQLCommandsAnalyzer
CW1073  | CargoWiseOne | Disabled | DoNotHardcodeSQLPasswordAnalyzer
CW1074  | CargoWiseOne | Disabled | DoNotHardcodeDeveloperDBUsernameAnalyzer
CW1075  | CargoWiseOne | Disabled | DoNotUseLoopToAddOrConditionsToFilterAnalyzer
CW1076  | CargoWiseOne | Disabled | DoNotUseMessageBoxShowAnalyzer
CW1077  | CargoWiseOne | Disabled | Do Not Use Microsoft.Office.Interop
CW1078  | CargoWiseOne | Disabled | DoNotUseProcessStartAnalyzer
CW1079  | CargoWiseOne | Disabled | DoNotCompareOnExceptionMessageAnalyzer
CW1080  | CargoWiseOne | Disabled | DoNotUseBusinessObjectCollectionIsAssignableFromAnalyzer
CW1081  | CargoWiseOne | Disabled | IsSubclassOfTypeofBusinessObjectCollectionAnalyzersss
CW1083  | CargoWiseOne | Disabled | DoNotUseStringLiteralsForEventCodesAnalyzer
CW1084  | CargoWiseOne | Disabled | VirtualNewAddNewAnalyzer
CW1086  | CargoWiseOne | Disabled | DoNotUseSystemWebMail
CW1087  | CargoWiseOne | Disabled | DoNotUsePrinterSettingsInstalledPrintersAnalyzer
CW1088  | CargoWiseOne | Disabled | DoNotUseClipboardAnalyzer
CW1089  | CargoWiseOne | Disabled | DoNotUseAssemblyGetEntryAssemblyAnalyzer
CW1090  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsSaveFileDialog
CW1091  | CargoWiseOne | Disabled | DoNotSetCausesValidationToFalseAnalyzer
CW1093  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsToolStripControlsAnalyzer
CW1094  | CargoWiseOne | Disabled | ToolTipTextShouldBeSetWithResGetStringAnalyzer
CW1095  | CargoWiseOne | Disabled | HttpParameterNamesShouldNotBeTranslatedAnalyzer
CW1097  | CargoWiseOne | Warning  | DoNotHardcodeTmpOrTempPathAnalyzer
CW1098  | CargoWiseOne | Disabled | DoNotInitializeStringFieldsWithResGetStringAnalyzer
CW1099  | CargoWiseOne | Disabled | ResGetStringDefaultTextMustBeStringLiteralAnalyzer
CW1100  | CargoWiseOne | Disabled | DoNotUseMenuItemOrKMenuItemAnalyzer
CW1102  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsTabPageAnalyzer
CW1103  | CargoWiseOne | Disabled | DoNotUseStringLiteralsForShortDateFormat
CW1104  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsTabControlAnalyzer
CW1105  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsUserControlOrKUserControlAnalyzer
CW1106  | CargoWiseOne | Disabled | DoNotLeaveInDebugMessagesAnalyzer
CW1107  | CargoWiseOne | Disabled | DoNotUseDbConnectionMethodsAnalyzer
CW1108  | CargoWiseOne | Disabled | DoNotUseDataSetAnalyzer
CW1109  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsFormOrKFormAnalyzer
CW1111  | CargoWiseOne | Disabled | DoNotUseSystemWindowsFormsTabDrawModeOwnerDrawFixedAnalyzer
CW1112  | CargoWiseOne | Disabled | DoNotBindToEnabledAnalyzer
CW1113  | CargoWiseOne | Disabled | DoNotShowMessageBoxFromBusinessLayerAnalyzer
CW1115  | CargoWiseOne | Disabled | UseSetTemporaryUserContextInsteadOfSetUserContextAnalyzer
CW1116  | CargoWiseOne | Disabled | UseEnterpriseCoreDataWrapperClassesAnalyzer
CW1117  | CargoWiseOne | Warning  | DuplicateForEventScriptsAnalyzer
CW1118  | CargoWiseOne | Disabled | UseKeysKeyCodeAndKeysModifersBitmaskAnalyzer
CW1119  | CargoWiseOne | Warning  | DoNotUseSLEventTimeTableColumnAnalyzer
CW1120  | CargoWiseOne | Disabled | DoNotGetIconsImagesFromRexOrResourcesFileAnalyzer
CW1121  | CargoWiseOne | Disabled | DoNotIncludeColumnValuesOrNamesInErrorReporterKeyAnalyzer
CW1122  | CargoWiseOne | Disabled | DoNotUseDateTimeParseAnalyzer
CW1123  | CargoWiseOne | Disabled | DoNotUseCachedValueAnalyzer
CW1124  | CargoWiseOne | Disabled | DoNotUseCachedPropertyAnalyzer
CW1125  | CargoWiseOne | Warning  | UseSystemViewsInsteadOfSystemTablesAnalyzer
CW1126  | CargoWiseOne | Warning  | DbAnsiNullsAnsiPaddingConcatNullYieldsNullAlwaysOnAnalyzer
CW1127  | CargoWiseOne | Warning  | DbSetOffsetsUnavailableAnalyzer
CW1128  | CargoWiseOne | Warning  | DbDbReindexDeprecatedAnalyzer
CW1129  | CargoWiseOne | Warning  | DbIndexDefragDeprecatedAnalyzer
CW1130  | CargoWiseOne | Warning  | DbIndexKeyPropertyDeprecatedAnalyzer
CW1131  | CargoWiseOne | Warning  | DbSpDbCmptLevelDeprecatedAnalyzer
CW1132  | CargoWiseOne | Warning  | DbSpLockDeprecatedAnalyzer
CW1133  | CargoWiseOne | Disabled | DoNotExceedBizOMaxLength
CW1134  | CargoWiseOne | Disabled | NonAbstractTestClassesShouldBeSealedAnalyzer
CW1135  | CargoWiseOne | Disabled | DoNotUseCountrySpecificBusinessRuleAnalyzer
CW1136  | CargoWiseOne | Disabled | DoNotUseSystemRuntimeSerializationFormattersBinary
CW1137  | CargoWiseOne | Disabled | DoNotUseInvalidTargetFrameworkOrTargetFrameworksAnalyzer
CW1138  | CargoWiseOne | Disabled | DoNotUseSystemRuntimeRemoting
CW1139  | CargoWiseOne | Disabled | DoNotUseNullLiteralAsReturnInCoalesceExpressionAnalyzer
CW1140  | CargoWiseOne | Disabled | ColumnNameCaseAnalyzer
CW1141  | CargoWiseOne | Disabled | DoNotUseUnity
CW1142  | CargoWiseOne | Disabled | AwaitExpressionsInAssertThatArgumentAnalyzer
CW1143  | CargoWiseOne | Disabled | DoNotUseInvalidCsprojRemovePropertyValueAnalyzer
CW1144  | CargoWiseOne | Disabled | DoNotUsePrivateTestMethodAnalyzer
EDI001  | CargoWiseOne | Disabled | ResourceStringStaticReferenceAnalyzer
EDI005  | Reference    | Warning  | WeakReferenceTargetRaceConditionRuleAnalyzer
EDI006  | Naming       | Warning  | RightToLeftRuleAnalyzer
EDI007  | Enterprise.Globalization | Warning | CustomizableDataTranslationRuleAnalyzer
EDI008  | CargoWiseOne | Disabled | LogReferenceValuesInEnglishOnlyAnalyzer
EDI010  | Reference    | Warning  | PredefinedNoteTypeDescriptionRuleAnalyzer
EDI011  | CargoWiseOne | Warning  | TempPathAnalyzer
EDI012  | CargoWiseOne | Warning  | UnmaintainableProductName_CSharpAnalyzer
EDI013  | Enterprise   | Warning  | UnmaintainableProductNameXMLAnalyzer

## Release 23.9.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
CW1082  | CargoWiseOne | Disabled | DoNotUseTooManyArgumentsAnalyzer
CW1082W | CargoWiseOne | Disabled | DoNotUseTooManyArgumentsAnalyzer
CW1145  | CargoWiseOne | Disabled | SingletonAnalyzer
CW1146  | CargoWiseOne | Disabled | NoDebugClassesEnterpriseApplicationConfigurationAnalyzer
CW1147  | CargoWiseOne | Disabled | DoNotUseWebControlsAnalyzer
CW1148  | CargoWiseOne | Disabled | SuppressCodeSmellAnalyzer
CW1149  | CargoWiseOne | Disabled | SingletonClassesShouldNotHavePublicCtorsAnalyzer
CW1150  | CargoWiseOne | Disabled | PublicFieldsAnalzyer
CW1152  | CargoWiseOne | Disabled | PublicVirtualAnalyzer
CW1153  | CargoWiseOne | Disabled | BaseNamingAnalyzer
CW1154  | CargoWiseOne | Disabled | PublicInternalClassesInSeperateFilesAnalyzer
CW1155	| CargoWiseOne | Disabled | NoMultiLineCommentsAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1024  | Concurrency  | Warning      | Unchanged    | Warning      | BadConcurrentCollectionAccessRuleAnalyzer improved performance
CW1051  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseBaseSourcePath now only triggers on compilation; allows where methods have SOURCE_CODE Dat capaibility attribute
CW1144  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUsePrivateTestMethodAnalyzer allows private methods used in other methods

## Release 23.10.3

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------------------------------------------------------------
CW1156  | CargoWiseOne | Disabled | CW1156_NoDebugClassesWithReleaseInterfacesAnalyzer
CW1157  | CargoWiseOne | Info     | CW1157_DoNotUseSystemAppDomainAnalyzer
CW1158  | CargoWiseOne | Disabled | CW1158 CsprojPackageReferenceAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1013  | CargoWiseOne | Warning  | Unchanged | Warning  | CargoWiseProgressBarAnalyzer - handle implicit object creation
CW1016  | CargoWiseOne | Warning  | Unchanged | Warning  | DontUseFlexCelPdfExportAnalyzer - handle implicit object creation
CW1090  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseSystemWindowsFormsSaveFileDialog - handle implicit object creation
CW1100  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseMenuItemOrKMenuItemAnalyzer - handle implicit object creation
CW1102  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseSystemWindowsFormsTabPageAnalyzer - handle implicit object creation
CW1104  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseSystemWindowsFormsTabControlAnalyzer - handle implicit object creation
CW1105  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseSystemWindowsFormsUserControlOrKUserControlAnalyzer - handle implicit object creation
CW1109  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseSystemWindowsFormsFormOrKFormAnalyzer - handle implicit object creation
CW1112  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotBindToEnabledAnalyzer - CodeFixProvider added
CW1123  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseCachedValueAnalyzer - handle implicit object creation
CW1124  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseCachedPropertyAnalyzer - handle implicit object creation
CW1137  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseInvalidTargetFrameworkOrTargetFrameworksAnalyzer - now includes location when reporting issues. Enables double-clicking on errors in VS to jump to the file.
CW1143  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseInvalidCsprojRemovePropertyValueAnalyzer - now includes location when reporting issues. Enables double-clicking on errors in VS to jump to the file.
CW1144  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUsePrivateTestMethodAnalyzer - enhance error message
CW1146  | CargoWiseOne | Disabled | Unchanged | Disabled | NoDebugClassesObjectFactoryIoCConfigurationAnalyzer - renaming, updating scope, tweaking performance
CW1147  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseWebControlsAnalyzer - handle implicit object creation, also checks base classes, also examines generated code
EDI001  | CargoWiseOne | Disabled | Unchanged | Disabled | ResourceStringStaticReferenceAnalyzer - handle implicit object creation expression
EDI013  | Enterprise   | Warning  | Unchanged | Disabled | UnmaintainableProductNameXMLAnalyzer - now includes location when reporting issues. Enables double-clicking on errors in VS to jump to the file. 

## Release 23.11.1

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1160 | CargoWiseOne | Disabled | FullyQualifySqlObjectsAnalyzer


### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1017  | CargoWiseOne | Warning  | Unchanged | Warning  | DpiAwareDevelopmentRule - remove handling of identifiers
CW1111  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotUseSystemWindowsFormsTabDrawModeOwnerDrawFixedAnalyzer - Implement codefix
CW1116  | CargoWiseOne | Disabled | Unchanged | Disabled | UseEnterpriseCoreDataWrapperClassesAnalyzer - corrected diagnostic title and message
CW1156  | CargoWiseOne | Disabled | Unchanged | Disabled | NoDebugClassesWithReleaseInterfacesAnalyzer - modified to look for invocation expressions
CW1158  | CargoWiseOne | Disabled | Unchanged | Disabled | CsprojPackageReferenceAnalyzer - allow net7.0 and package references to StrongNameSigner
EDI007  | Enterprise.Globalization | Warning | Unchanged | Warning | CustomizableDataTranslationRuleAnalyzer - caching bug fix which now handle overridden virtual properties, IsRelatedMethod bug fix to catch diagnostic of related methods in different modules

## Release 23.11.15

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1159 | CargoWiseOne | Disabled| ResGetStringShouldBeFullyQualifiedAnalyzer and new codefix for CS0104 to include Res and ResString type.

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CUS001  | Naming | Warning  | Unchanged | Warning  | SomeRefZZFieldsAreCaseInsensitiveRuleAnalyzer - Added code fix, handle not equals binary expression
CW1146  | CargoWiseOne | Disabled | Unchanged | Disabled | NoDebugClassesObjectFactoryIoCConfigurationAnalyzer - correct lazy initialization, make collection static
CW1156  | CargoWiseOne | Disabled | Unchanged | Disabled | NoDebugClassesWithReleaseInterfacesAnalyzer - correct lazy initialization, make collection static

## Release 23.12.8

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1162 | CargoWiseOne | Disabled | ComReferenceAnalyzer
CW1163 | CargoWiseOne | Disabled | DoNotInitializeLazyWithMethodIdentifierAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1156  | CargoWiseOne | Disabled | Unchanged | Disabled | NoDebugClassesWithReleaseInterfacesAnalyzer - Update to ignore test projects and debug preprocessor directive scopes.
CW1160  | CargoWiseOne | Disabled | Unchanged | Disabled | FullyQualifySqlObjectsAnalyzer - Update to support string concatenations.

## Release 23.12.14

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1133  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotExceedBizOMaxLength - Update to include ZString Schema based max length validation.

## Release 23.12.20

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1164 | CargoWiseOne | Disabled | DoNotUseStringLiteralsForListAttributeAnalyzer

## Release 24.02.01

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1166 | CargoWiseOne | Disabled | DoNotExtendNUnitAnalyzer
CW1167 | CargoWiseOne | Disabled | DbDisposableActionForDbConnectionAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1133  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotExceedBizOMaxLength - Update to include ZString Schema based and ZString function based and Binary and Local expression condition max length. Update to implement methodImplmentationDepth to avoid getting max length from the recursive call with over depth condition.
CW1155  | CargoWiseOne | Disabled | Unchanged | Disabled | NoMultiLineComments - Add a code fix for this analyzer

## Release 24.03.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1161 | CargoWiseOne | Disabled | ResGetStringAnalyzer and migration code fix for SuppressCodeSmell inline comments
CW1169 | CargoWiseOne | Disabled | DoNotUseOxyPlot
CW1170 | CargoWiseOne | Disabled | DoNotUseDbGeographyAnalyzer
CW1171 | CargoWiseOne | Disabled | DoNotUseMimeMessageToStringAnalyzer
CW1173 | CargoWiseOne | Disabled | DoNotInvokeResUnderscoreGetStringAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1095  | CargoWiseOne | Disabled | Unchanged | Disabled | HttpParameterNamesShouldNotBeTranslated - Analyzer adjusted to find all Res and ResString types
CW1098  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotInitializeStringFieldsWithResGetString - Analyzer adjusted to find all Res and ResString types
CW1099  | CargoWiseOne | Disabled | Unchanged | Disabled | ResGetStringDefaultTextMustBeStringLiteral - Analyzer adjusted to find all Res and ResString types
CW1133  | CargoWiseOne | Disabled | Unchanged | Disabled | DoNotExceedBizOMaxLength - Update to include string property, field, Res.GetString and MultilingualString function based max length
CW1167  | CargoWiseOne | Disabled | Unchanged | Disabled | DbDisposableActionForDbConnection - Update to include Task methods overloads, Thread.Start, BackgroundWorker, method attributes and standardized implementation
EDI001  | CargoWiseOne | Disabled | Unchanged | Disabled | ResourceStringsStaticReferenceAnalyzer - Analyzer adjusted to find all Res and ResString types
EDI008  | CargoWiseOne | Disabled | Unchanged | Disabled | LogReferenceValuesInEnglishOnly - Analyzer adjusted to find all Res and ResString types

## Release 24.04.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1172 | CargoWiseOne | Disabled | ResGetStringKeyValidationAnalyzer
CW1174 | CargoWiseOne | Disabled | ResGetStringMustBeInvokedWithinANamespace
EDI003 | CargoWiseOne | Disabled | DoNotExceedBizOMaxLength - Refactor and rename from CW1133
CW1175 | CargoWiseOne | Disabled | SystemColumnSqlAnalyzer
CW1175A | CargoWiseOne | Disabled | SqlSyntaxAnalyzer

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1133  | CargoWiseOne | Disabled | DoNotExceedBizOMaxLength - Renamed as EDI003

## Release 24.05.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1168  | CargoWiseOne | Disabled | NonAbstractTestClassesShouldBeInternalAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1164  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseStringLiteralsForListAttributeAnalyzer - Update to include all string literals

## Release 24.06.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1178  | CargoWiseOne | Disabled | DoNotInvokeOldResGetStringMethodsAnalyzer
CW1179  | CargoWiseOne | Disabled | DoNotDeclareInitializeComponentInNonDesignerFileAnalyzer
CW1176  | CargoWiseOne | Disabled | TestShouldContainMeaningfulAssertionAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1159  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | ResGetStringShouldBeFullyQualifiedAnalyzer renamed as ResGetStringShouldUseSourceGenMethodAnalyzer, codefix adds aliased using instead
CW1161  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | ResGetStringAnalyzer add code fix for (NoResString) cast and migration code fix
CW1167  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | 1.DbDisposableActionForDbConnectionAnalyzer handles event enlistment. 2.Diagnostic for invocations of Db.DisposableActionForDbConnection() in public methods.

## Release 24.07.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1180  | CargoWiseOne | Disabled | InternalClassesShouldNotHavePublicMethodsAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1161  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | ResGetStringAnalyzer adjust NoResString code fix provider to not fix object parameters(include ctor and method invocation), switch labels, improved handling of leading trivia other kinds of literals in binary expressions
CW1178  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotInvokeOldResGetStringMethodsAnalyzer code fix adds a using for the default namespace instead, using the CW1159 code fix provider

## Release 24.08.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1181  | CargoWiseOne | Disabled | IncorrectUseOfCryptographicLibraryAnalyzer
CW1182  | CargoWiseOne | Warning  | DoNotUseIdentityServerReferenceAnalyzer
CW1183  | CargoWiseOne | Warning  | DateAddNonIntegerArgumentAnalyzer

### Changed Rules
Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
EDI003  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | MeasureableMethodImplementationAnalyzer code fix stop performing length checks on the string passed to the exception-throwing statement.

## Release 24.09.01
### Changed Rules
Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1018A | Naming       | Warning      | Unchanged    | Warning      | HttpApplicationDbAppSettingsRule convert to operation action analyzer for performance
CW1032  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | UseMoqForMocking moved into DoNotUseNamespace combined analyzer for performance
CW1077  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseMicrosoftOfficeInterop combined analyzer now uses symbol action and operation action for performance
CW1086  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseSystemWebMail combined analyzer now uses symbol action and operation action for performance
CW1098  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotInitializeStringFieldsWithResGetString convert to operation action analyzer for performance
CW1136  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseSystemRuntimeSerializationFormattersBinary combined analyzer now uses symbol action and operation action for performance
CW1138  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseSystemRuntimeRemoting combined analyzer now uses symbol action and operation action for performance
CW1141  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseUnity combined analyzer now uses symbol action and operation action for performance
CW1144  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUsePrivateTestMethod convert to symbol and operation action analyzer for performance
CW1161  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | ResGetStringAnalyzer allows string literals in ElementBindingExpressions, in attribute ctors
CW1164  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseStringLiteralsForListAttribute convert to symbol action analyzer for performance
CW1169  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseOxyplot combined analyzer now uses symbol action and operation action for performance
CW1183  | CargoWiseOne | Warning      | Unchanged    | Warning      | DateAddNonIntegerArgumentAnalyzer non-numeric parameters are no longer catching
EDI005  | Reference    | Warning      | Unchanged    | Warning      | WeakReferenceTargetRaceCondition convert to operation block analyzer for performance
EDI006  | Naming       | Warning      | Unchanged    | Warning      | RightToLeftRule convert to operation action for performance
EDI007  | Enterprise.Globalization | Warning      | Unchanged    | Warning      | CustomizableDataTranslationRule use to operation action for performance

## Release 24.10.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1186  | CargoWiseOne | Disabled | DoNotUseNonBreakingSpaceAnalyzer
CW1189  | CargoWiseOne | Disabled | DoNotUseZTerminalServiceProperties

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1044  | CargoWiseOne | Warning      | Unchanged    | Warning      | FactoryGetDatabaseCountCollectionCountRuleAnalyzer - convert to operation action analyzer for performance
CW1056  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseGCCollectAnalyzer - remove check for test code
CW1118  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | UseKeysKeyCodeAndKeysModifersBitmaskAnalyzer - change to operation action analyzer for performance
CW1122  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseDateTimeParseAnalyzer - change to operation action analyzer for performance
CW1144  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUsePrivateTestMethodAnalyzer - add handling of nameof()
CW1148  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | SuppressCodeSmellAnalyzer - run Regex once per file instead of once per line for performance
CW1161  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | ResGetStringAnalyzer - now allows binary string comparisons
EDI003  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotExceedBizOMaxLength - fixed concurrency issue in zrs file reader

## Release 24.11.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1187 | CargoWiseOne | Disabled | DoNotTranslateExceptionMessageAnalyzer, [Documentation](https://devops.wisetechglobal.com/wtg/Content/_wiki/wikis/Content.wiki/13329/CW1187-DoNotTranslateExceptionMessage)
CW1190 | CargoWiseOne | Disabled | DoNotUseTypeOrNamespaceAnalayzer, [Documentation](https://devops.wisetechglobal.com/wtg/Content/_wiki/wikis/Content.wiki/14139/CW1190-DoNotUseNUnitAssertions)

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1112  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotBindToEnabledAnalyzer - change to operation action analyzer for performance
CW1148  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | SuppressCodeSmellAnalyzer - fixed out of bound substring
CW1176  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | TestShouldContainMeaningfulAssertionAnalyzer - fixed recursion
CW1052  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotCastFactoryMethodAnalyzer  - change to operation action analyzer for performance

## Release 24.12.01
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1191  | CargoWiseOne | Disabled | TestsShouldContainAssertionAnalyzer
CW1192  | CargoWiseOne | Disabled | CustomizableDataCaptionAsmidRequiredAnalyzer
CW1193  | CargoWiseOne | Disabled | DoNotUseCargoWiseOneNameInCodeAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1017  | CargoWiseOne | Warning      | Unchanged    | Warning      | DpiAwareDevelopmentRuleAnalyzer - check for helper name by type symbol name for performance
CW1079  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotCompareOnExceptionMessageAnalyzer - change to operation action analyzer for performance
CW1163  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotInitializeLazyWithMethodInvocationAnalyzer - correct behaviour, changed title
CW1176  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | TestShouldContainMeaningfulAssertionAnalyzer - also checks IMethodReferenceOperations

## Release 25.1.1
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1194  | CargoWiseOne | Disabled | UnsafeBusinessObjectCollectionCreationAnalyzer

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1017  | CargoWiseOne | Warning      | Unchanged    | Warning      | DpiAwareDevelopmentRuleAnalyzer - significantly more thorough checks for whether assignments to properties of control are DPI aware

## Release 25.2.1
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1196  | CargoWiseOne | Disabled | DoNotUseSystem.Data.SqlClientStatementAnalyzer, [Documentation](https://devops.wisetechglobal.com/wtg/Content/_wiki/wikis/Content.wiki/15471/CW1196-DoNotUseSystem.Data.SqlClientStatementAnalyzer)
CW1197  | CargoWiseOne | Disabled | DoNotUseCW1OrCargoWiseNext

### Changed Rules

Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
CW1172  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | ResGetStringKeyValidationAnalyzer - fix index out of range, null reference exceptions
CW1191  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | TestsShouldContainAssertionAnalyzer - handle AggregateException
CW1193  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | DoNotUseCargoWiseOneNameInCodeAnalyzer - added codefix
CW1194  | CargoWiseOne | Disabled     | Unchanged    | Disabled     | UnsafeBusinessObjectCollectionCreationAnalyzer - adjusted to trigger on Load() invocations for base class only (legacy BusinessObjectCollections)

## Release 25.3.1
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1198  | CargoWiseOne | Disabled | DoNotAddAuditEventsToStmALogAnalyzer

## Release 25.4.1
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1199  | CargoWiseOne | Disabled | DoNotUseUnnecessaryResourceStringInUnitTestsAnalyzer

## Release 25.6.1
### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CW1200  | CargoWiseOne | Disabled | ServiceTaskTargetsNetCoreAnalyzer
