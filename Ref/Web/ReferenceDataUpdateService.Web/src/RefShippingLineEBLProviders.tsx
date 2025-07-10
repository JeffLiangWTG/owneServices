import React from "react";
import { RefShippingLineEBLProviderItem } from "./RefShippingLineEBLProviderItem";
import {
	RefShippingLineEBLProviderName,
	RefShippingLineEBLProviderWrapper,
} from "./models/IRefShippingLineEBLProvider";
import { IValidationResults } from "./ValidationResults";

interface IRefShippingLineEBLProvidersProps {
	providerItems: RefShippingLineEBLProviderWrapper[];
	onValueChange(
		entity: RefShippingLineEBLProviderWrapper,
		name: string,
		value: object
	): void;
	onValueChanged(entity: RefShippingLineEBLProviderWrapper, name: string): void;
	handleAddEBLProviderItem(): void;
	handleDeleteEBLProviderItem(providerItemPk: string): void;
	isAddButtonDisabled: boolean;
	eblProviderValidationResults?: {
		[pk: string]: IValidationResults;
	};
	eblProviderDistinctNames: RefShippingLineEBLProviderName[];
}

export const RefShippingLineEBLProviders = ({
	providerItems,
	onValueChange,
	onValueChanged,
	handleAddEBLProviderItem,
	handleDeleteEBLProviderItem,
	isAddButtonDisabled,
	eblProviderValidationResults,
	eblProviderDistinctNames,
}: IRefShippingLineEBLProvidersProps) => {
	const onEblItemProviderIsDeleted = (providerItemPk: string) => {
		//in case we are deleting a IsDefault item, we should show other defaults.
		let itemToBeDeleted = providerItems.find((x) => x.RSE_PK == providerItemPk);
		if (itemToBeDeleted && itemToBeDeleted.RSE_IsDefault) {
			providerItems
				.filter((x) => x.RSE_IsAvailable)
				.map((x) => (x.IsDefaultHidden = false));
		}

		handleDeleteEBLProviderItem(providerItemPk);
	};

	const onEblItemProviderChange = (
		entity: RefShippingLineEBLProviderWrapper,
		name: string,
		value: object
	) => {
		switch (name) {
			case "RSE_IsAvailable":
				if (
					providerItems.filter(
						(x) => x.RSE_IsDefault && x.RSE_PK != entity.RSE_PK
					).length == 0
				) {
					entity.IsDefaultHidden = !value;
				}
				entity.RSE_IsAvailable = value as any;

				//IsDefault is ticked but not available anymore
				//set default to false.
				if (entity.RSE_IsDefault && !value) {
					onEblItemProviderChange(entity, "RSE_IsDefault", false as any);
					entity.RSE_IsDefault = false;
				}
				break;
			case "RSE_IsDefault":
				if (!value) {
					//show all default checkboxes if their IsAvailable is ticked
					providerItems
						.filter((x) => x.RSE_IsAvailable)
						.map((x) => (x.IsDefaultHidden = false));
					break;
				}

				//hide all defaults when ticking one of them
				providerItems
					.filter((x) => x.RSE_PK != entity.RSE_PK)
					.map((x) => (x.IsDefaultHidden = value as any));
				break;
		}

		onValueChange(entity, name, value);
	};

	return (
		<>
			<div className="row">
				<div className="col-sm-4">
					<span>eBL Provider</span>
				</div>
				<div className="col-sm-3">
					<span>Is Available</span>
				</div>
				<div className="col-sm-3">
					<span>Default</span>
				</div>
			</div>
			<div className="row">
				<div className="col-sm-10">
					{providerItems.length > 0
						? providerItems.map((x) => (
								<div key={x.RSE_PK} className="row form-group mt-2">
									<RefShippingLineEBLProviderItem
										onValueChange={onEblItemProviderChange}
										onValueChanged={onValueChanged}
										handleDeleteEBLProviderItem={onEblItemProviderIsDeleted}
										providerItem={x}
										validationResults={
											eblProviderValidationResults
												? eblProviderValidationResults[x.RSE_PK]
												: undefined
										}
										eblProviderDistinctNames={eblProviderDistinctNames}
									/>
								</div>
						  ))
						: ""}
				</div>
			</div>
			<div className="row form-group ml-1">
				<input
					type="button"
					className="btn btn-info"
					id="refShippingLineEBLProviderItemAdd"
					value="Add"
					onClick={handleAddEBLProviderItem}
					disabled={isAddButtonDisabled}
					title={
						isAddButtonDisabled
							? "Requires '(BPL) Electronic Bill of Lading Provider Mandatory' for 'Shipping Instructions' to be selected in 'Messaging Requirements' section."
							: ""
					}
				/>
			</div>
		</>
	);
};
