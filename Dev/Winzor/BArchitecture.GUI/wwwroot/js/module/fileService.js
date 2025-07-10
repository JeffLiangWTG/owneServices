export const openFileDialog = async (allowMultiSelect, fileTypes) => {
	if ('showOpenFilePicker' in window) {
		const { types } = processFileTypes(fileTypes);
		try {
			let args = {
				multiple: allowMultiSelect,
			};
			if (types !== null) {
				args.types = types;
			}
			await window.cargoWiseClient?.requestUserActivation();
			let fileHandles = await window.showOpenFilePicker(args);
			let files = await Promise.all(
				fileHandles.map(async (handle) => {
					const file = await handle.getFile();
					return {
						lastModified: new Date(file.lastModified).toISOString(),
						name: file.name,
						size: file.size,
						contentType: file.type,
						fileStream: getFileStream(file),
					};
				})
			);
			return files;
		} catch (e) {
			if (e.name !== 'AbortError') {
				throw e;
			}
		}
	} else {
		throw new Error('Uploading files is not supported in this browser.');
	}
	return [];
};

function getFileStream(file) {
	if (file.size > 0) {
		return DotNet.createJSStreamReference(file);
	}

	return null;
}

function parseFileName(fullPath) {
	if (!fullPath) {
		return {
			fileName: null,
			dir: null,
		};
	}

	let lastDirSplitterIndex = fullPath.lastIndexOf('\\');
	if (lastDirSplitterIndex === -1) {
		lastDirSplitterIndex = fullPath.lastIndexOf('/');
	}
	if (lastDirSplitterIndex === -1) {
		return {
			fileName: fullPath,
			dir: null,
		};
	} else {
		return {
			fileName: fullPath.substring(lastDirSplitterIndex + 1),
			dir: fullPath.substring(0, lastDirSplitterIndex),
		};
	}
}

export const saveFileToDirectory = async (suggestedName, fileTypes) => {
	let nameInfo = parseFileName(suggestedName);
	if (dirHandle && dirHandle.name === nameInfo.dir) {
		let fileHandle = await dirHandle.getFileHandle(nameInfo.fileName, { create: true });
		return {
			cancelled: false,
			fileName: fileHandle.name,
			fileReference: createFileReference(fileHandle),
		};
	} else {
		return saveFileDialog(suggestedName, fileTypes);
	}
};

function createFileReference(fileHandle) {
	return DotNet.createJSObjectReference({
		write: async (fileDataStream) => {
			const writableFileStream = await fileHandle.createWritable();
			const fileData = await fileDataStream.arrayBuffer();
			await writableFileStream.write(fileData);
			await writableFileStream.close();
		},
	});
}

export const saveFileDialog = async (suggestedName, fileTypes) => {
	if ('showSaveFilePicker' in window) {
		try {
			const { types, firstExt } = processFileTypes(fileTypes);
			if (firstExt && suggestedName.indexOf('.') < 0) {
				suggestedName += firstExt;
			}
			await window.cargoWiseClient?.requestUserActivation();
			const fileHandle = await window.showSaveFilePicker({
				suggestedName,
				types,
				excludeAcceptAllOption: types?.length > 0,
			});
			return {
				cancelled: false,
				fileName: fileHandle.name,
				fileReference: createFileReference(fileHandle),
			};
		} catch (e) {
			if (e.name !== 'AbortError') {
				throw e;
			}
		}
	} else {
		throw new Error('Saving files is not supported in this browser.');
	}
	return {
		cancelled: true,
		fileName: null,
		fileReference: DotNet.createJSObjectReference({}),
	};
};

function processFileTypes(fileTypes) {
	let firstExt;
	const types = fileTypes?.map((types, i) => {
		const key = Object.keys(types)[0];
		const extensions = types[key];
		const typeMap = {
			accept: getAcceptObject(extensions),
		};

		if (key) {
			typeMap.description = key;
		}

		if (i === 0) {
			firstExt = extensions[0];
		}

		return typeMap;
	});

	return { types, firstExt };
}

function getAcceptObject(extensions) {
	if (!Array.isArray(extensions)) {
		throw 'Expected extensions to be Array';
	}

	return extensions.reduce((accept, ext) => {
		accept[getCustomMimeType(ext)] = [ext];
		return accept;
	}, {});
}

function getCustomMimeType(ext) {
	if (!ext || ext[0] !== '.') {
		throw `Invalid extension ${ext}`;
	}

	return `application/x-wtg-${ext.substring(1)}`;
}

let dirHandle;
export const OpenDirectoryDialog = async (startIn, mode) => {
	if ('showDirectoryPicker' in window) {
		try {
			dirHandle = null;
			let args = {
				startIn: startIn,
				mode: mode,
			};
			await window.cargoWiseClient?.requestUserActivation();
			dirHandle = await window.showDirectoryPicker(args);
			return dirHandle.name;
		} catch (e) {
			if (e.name !== 'AbortError') {
				throw e;
			}
		}
	} else {
		throw new Error('Uploading directory is not supported in this browser.');
	}
	return '';
};

export const SaveFileByPath = async (fileName, data) => {
	if (!fileName || !data) {
		return false;
	}

	try {
		const pathArr = fileName.split('\\');
		if (dirHandle !== null && pathArr.length === 2 && pathArr[0] === dirHandle.name) {
			const fileHandle = await dirHandle.getFileHandle(pathArr[1], {
				create: true,
			});
			const writable = await fileHandle.createWritable();
			const encoder = new TextEncoder();
			const byteArr = encoder.encode('\ufeff');
			const mergedArray = new Uint8Array(byteArr.length + data.length);
			mergedArray.set(byteArr, 0);
			mergedArray.set(data, byteArr.length);
			await writable.write(mergedArray);
			await writable.close();
			return true;
		} else {
			const urlObj = window.URL || window.webkitURL || window;
			const blob = new Blob([data]);
			const url = urlObj.createObjectURL(blob);
			const link = document.createElementNS('http://www.w3.org/1999/xhtml', 'a');

			link.href = url;
			link.download = fileName;
			document.body.appendChild(link);
			link.click();
			link.remove();
			URL.revokeObjectURL(url);
			return true;
		}
	} catch {
		throw new Error('Save file failed. Please check path.');
	}
};

export const DownloadFile = async (fileName) => {
	if (!fileName) {
		return false;
	}

	const BuildALinkWithBlob = function (fileName_) {
		const a = document.createElement('a');
		a.href = `/download/${fileName_}`;
		a.download = fileName_;
		// remove after handler
		const clickHandler = () => {
			setTimeout(() => {
				removeEventListener('click', clickHandler);
			}, 150);
		};

		a.addEventListener('click', clickHandler, false);
		return a;
	};

	const downlLink = BuildALinkWithBlob(fileName);
	downlLink.click();
	return true;
};
