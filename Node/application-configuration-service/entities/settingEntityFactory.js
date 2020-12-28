const MaxNameLength = 128;
const MaxValueLength = 32768;
const ValidationRegex = /^\s+$/;

const isSettingNameValid = (name) => {
	if (typeof(name) !== "string") {
		return false;
	}

	return name && !ValidationRegex.test(name) && name.length <= MaxNameLength;
};

const isSettingValueValid = (value) => {
	if (typeof(value) !== "string") {
		return false;
	}

	return value && value.length <= MaxValueLength;
};

export default class {
	constructor(settingsGroupEntityFactory, settingCollection) {
		this.settingsGroupEntityFactory = settingsGroupEntityFactory;
		this.settingCollection = settingCollection;
	}

	async setup() {
		await this.settingCollection.createIndex({
			"settingsGroupId": 1,
			"name": 1
		}, {
			unique: true
		});
	}

	async setSettingValue(settingsGroupName, name, value) {
		if (isSettingNameValid(name)) {
			return Promise.reject("InvalidSettingName");
		}

		if (isSettingValueValid(value)) {
			return Promise.reject("InvalidSettingValue");
		}

		const settingsGroupId = await this.settingsGroupEntityFactory.getOrCreateSettingsGroupIdByName(settingsGroupName);
		const updated = await this.settingCollection.findOneAndUpdate({
			settingsGroupId: settingsGroupId,
			name: name
		}, {
			value: value
		});

		if (!updated) {
			await this.settingCollection.insert({
				settingsGroupId: settingsGroupId,
				name: name,
				value: value
			});
		}

		return Promise.resolve();
	}

	async getSettings(settingsGroupName) {
		const settingsGroupId = await this.settingsGroupEntityFactory.getOrCreateSettingsGroupIdByName(settingsGroupName);
		const settings = await this.settingCollection.find({
			settingsGroupId: settingsGroupId
		});

		return Promise.resolve(settings.map(entity => {
			return {
				ID: entity.id,
				SettingsGroupID: entity.settingsGroupId,
				Name: entity.name,
				Value: entity.value,
				Created: entity.created,
				Updated: entity.updated
			};
		}));
	}

	async deleteSetting(settingsGroupName, name) {
		if (isSettingNameValid(name)) {
			return Promise.reject("InvalidSettingName");
		}

		const settingsGroupId = await this.settingsGroupEntityFactory.getOrCreateSettingsGroupIdByName(settingsGroupName);
		return this.settingCollection.deleteOne({
			settingsGroupId: settingsGroupId,
			name: name
		});
	}
};
