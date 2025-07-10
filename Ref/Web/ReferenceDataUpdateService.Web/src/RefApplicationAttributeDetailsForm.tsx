import React from "react";
import { IValidationResults } from "./ValidationResults";
import { IParentProps } from "./IParentProps";
import { CodeInput } from "./CodeInput";
import { TextInput } from "./TextInput";
import { CheckBox } from "./CheckBox";
import { ComboBox, IComboBoxOptions } from "./ComboBox";
import { ProducerStatus } from "./ProducerStatus";
import { DateTimeInput } from "./DateTimeInput";
import { IEntityManager } from "./EntityManager";
import RefApplicationAttributeTypeEnum from "./models/RefApplicationAttributeTypeEnum"
import { IRefApplicationAttribute, RefApplicationAttributeWrapper } from "./models/IRefApplicationAttribute";
import IRefApplicationAttributeType from "./models/IRefApplicationAttributeType";
import { PasswordInput } from "./PasswordInput";
import Button from "./Button";

export interface IRefApplicationAttributeDetailsProps extends IParentProps {
	attributes: RefApplicationAttributeWrapper[],
	attributeTypes: IRefApplicationAttributeType[],
	onAttributeValueChange: (entity: any, name: string, value: object) => void,
	onAttributeValueChanged: (entity: any, name: string) => void,
	save: () => void,
	validationResults: {
		[pk: string]: IValidationResults
	},
	entityManager: IEntityManager
}

interface IRefApplicationAttributeDetailsStates {
	currentId: string,
}

export class RefApplicationAttributeDetailsForm extends React.Component<IRefApplicationAttributeDetailsProps, IRefApplicationAttributeDetailsStates> {
	constructor(props: IRefApplicationAttributeDetailsProps) {
		super(props);
		this.readFile = this.readFile.bind(this);
		this.updateContent = this.updateContent.bind(this);
		this.arrayBufferToBase64 = this.arrayBufferToBase64.bind(this);
		this.downloadFile = this.downloadFile.bind(this);
		this.state = {
			currentId: "",
		}

		this.reader = new FileReader();
		this.reader.addEventListener("loadend", this.updateContent);
		this.comBoxOptions = new ProducerStatus();
	}

	comBoxOptions: IComboBoxOptions;

	reader: FileReader;

	readFile(id: string, event: React.ChangeEvent<HTMLInputElement>): void {
		if (event.target.files != null && event.target.files.length > 0) {
			let file: File = event.target.files[0];
			let fileName = file.name;
			this.reader.readAsArrayBuffer(file);
			this.setState({ currentId: id });
			let index = this.props.attributes.findIndex(x => x.RAA_PK == id);
			if (index >= 0) {
				this.props.onAttributeValueChange(this.props.attributes[index], "RAA_Value", fileName as any);
			}
		}
	}

	updateContent(): void {
		if (this.reader.readyState == FileReader.DONE && this.reader.result != null) {
			let buffer: ArrayBuffer = this.reader.result as ArrayBuffer;
			let base64Content = this.arrayBufferToBase64(buffer);
			let index = this.props.attributes.findIndex(x => x.RAA_PK == this.state.currentId);
			if (index >= 0) {
				this.props.onAttributeValueChange(this.props.attributes[index], "RAA_Content", base64Content as any);
			}
		}
	}

	arrayBufferToBase64(buffer: ArrayBuffer): string {
		let binary = "";
		let bytes = new Uint8Array(buffer);
		for (let i = 0; i < bytes.byteLength; i++) {
			binary += String.fromCharCode(bytes[i]);
		}
		return window.btoa(binary);
	}

	downloadFile(fileName: string, content: string | null): void {
		if (fileName && content != null) {
			if (fileName.includes("\\") || fileName.includes("/")) {
				let index = fileName.lastIndexOf("\\");
				if (index < 0) {
					index = fileName.lastIndexOf("/");
				}
				fileName = fileName.substr(index + 1);
			}
			let a = document.createElement("a");
			a.setAttribute("href", "data:text/plain;base64," + content);
			a.setAttribute("download", fileName);
			a.style.display = "none";
			document.body.appendChild(a);
			a.click();
			document.body.removeChild(a);
		}
	}

	addControlByType(attribute: RefApplicationAttributeWrapper) {
		let property: string = "RAA_Value";
		switch (attribute.RAA_RAT_NKType) {
			case RefApplicationAttributeTypeEnum.StatusFlag: {
				return <ComboBox options={this.comBoxOptions} propertyName={property} readOnly={attribute.RAA_IsDefault} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} />
			}
			case RefApplicationAttributeTypeEnum.String: {
				return <TextInput inputType="Text" propertyName={property} readOnly={attribute.RAA_IsDefault} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} maxLength={400} />
			}
			case RefApplicationAttributeTypeEnum.Number: {
				return <TextInput inputType="number" propertyName={property} readOnly={attribute.RAA_IsDefault} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} maxLength={10} />
			}
			case RefApplicationAttributeTypeEnum.DateTime: {
				return <DateTimeInput entity={attribute} propertyName={property} readOnly={attribute.RAA_IsDefault} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResult={this.props.validationResults[attribute.RAA_PK]} format="YYYY-MM-DD HH:mm" />
			}
			case RefApplicationAttributeTypeEnum.Boolean: {
				return this.addCheckBoxes(attribute)
			}
			case RefApplicationAttributeTypeEnum.File: {
				return <TextInput inputType="Text" propertyName={property} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} maxLength={200} />
			}
			case RefApplicationAttributeTypeEnum.Credential: {
				return <PasswordInput propertyName={property} readOnly={attribute.RAA_IsDefault} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} maxLength={400} />
			}
			case RefApplicationAttributeTypeEnum.SecretFile: {
				return <TextInput inputType="Text" propertyName={property} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} maxLength={200} />
			}
			default: {
				return <TextInput inputType="Text" propertyName={property} readOnly={attribute.RAA_IsDefault} entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} maxLength={200} />
			}
		}
	}

	addDefaultCheckBox(attribute: RefApplicationAttributeWrapper) {
		if (this.hasDefault(attribute)) {
			return <div className="form-group row">
				<div className="col-sm-2"></div>
				<div className="col-sm-4">
					<CheckBox propertyName="RAA_IsDefault" entity={attribute} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[attribute.RAA_PK]} />
				</div>
			</div>
		}
	}

	hasDefault(attribute: RefApplicationAttributeWrapper): boolean {
		return attribute.defaultRecord !== null;
	}

	addCheckBoxes(attribute: RefApplicationAttributeWrapper) {
		return <div className="form-group row">
			<div className="col-sm-2"></div>
			<div className="col-sm-4">
				<CheckBox propertyName="RAA_Value" entity={attribute} onValueChange={this.props.onAttributeValueChange} />
			</div>
		</div>
	}

	sortArray(item1: IRefApplicationAttribute, item2: IRefApplicationAttribute) {
		if (item1.RAA_AttributeName > item2.RAA_AttributeName) {
			return 1;
		}
		if (item1.RAA_AttributeName < item2.RAA_AttributeName) {
			return -1;
		}
		return 0;
	}

	render() {
		return <div>
			<div className="form-group row">
				<div className="col-sm-1">
					<Button type='button' className="btn btn-info" disabled={this.props.readOnly} onClick={this.props.save}>Save</Button>
				</div>
			</div>
			<div className="container">
				<div className="form-group row">
					<div className="col-sm-1">Default</div>
					<div className="col-sm-2">Type</div>
					<div className="col-sm-3">Attribute Name</div>
					<div className="col-sm-3">Attribute Value</div>
					<div ></div>
					<div></div>
				</div>
				{this.props.attributes.sort((a, b) => { return this.sortArray(a, b); }).map((r) => <div key={r.RAA_PK}>
					<div className="form-group row">
						<div className="col-sm-1">
							{this.addDefaultCheckBox(r)}
						</div>
						<div className="col-sm-2">
							<CodeInput propertyName="RAA_RAT_NKType" entity={r} readOnly={r.RAA_IsDefault} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[r.RAA_PK]} listCodePropertyName="RAT_Type"
								listDescriptionPropertyName="RAT_Description" listEntityTypeName="RefApplicationAttributeType" maxLength={10} entityManager={this.props.entityManager} prevEntity={this.props.attributeTypes} />
						</div>
						<div className="col-sm-3">
							<TextInput inputType="Text" propertyName="RAA_AttributeName" readOnly={this.hasDefault(r)} entity={r} onValueChange={this.props.onAttributeValueChange} onValueChanged={this.props.onAttributeValueChanged} validationResults={this.props.validationResults[r.RAA_PK]} maxLength={200} />
						</div>
						<div className="col-sm-3">
							{this.addControlByType(r)}
						</div>
						<div className="form-group row">
							<div className="col-sm-1">
								<Button type='button' className="btn btn-info" title="Download" hidden={r.RAA_RAT_NKType != RefApplicationAttributeTypeEnum.File || r.RAA_Content == null} onClick={() => this.downloadFile(r.RAA_Value, r.RAA_Content)} >↓</Button>
							</div>
						</div>
						<div className="col-sm-1">
							<input type="file" id={"file-selector_" + r.RAA_PK} hidden={r.RAA_RAT_NKType != RefApplicationAttributeTypeEnum.File && r.RAA_RAT_NKType != RefApplicationAttributeTypeEnum.SecretFile} onChange={e => this.readFile(r.RAA_PK, e)}></input>
						</div>
					</div>
				</div>
				)}
			</div>
		</div>
	}
}
