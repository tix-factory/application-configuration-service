const CacheExpiry = 60 * 1000;
const MaxGroupNameLength = 128;
const ValidationRegex = /^\s+$/;

const isSettingsGroupNameValid = (groupName) => {
	if (typeof(groupName) !== "string") {
		return false;
	}

	return groupName && !ValidationRegex.test(groupName) && groupName.length <= MaxGroupNameLength;
};

export default class {
	constructor(settingsGroupCollection) {
		this.settingsGroupCollection = settingsGroupCollection;
		this.idByNameCache = {};
	}

	async setup() {
		await this.settingsGroupCollection.createIndex({
			"name": 1
		}, {
			unique: true
		});
	}

	async getOrCreateSettingsGroupIdByName(groupName) {
		if (!isSettingsGroupNameValid(groupName)) {
			return Promise.reject("InvalidSettingsGroupName");
		}

		let settingsGroupId = await this.getSettingsGroupIdByName(groupName);
		if (settingsGroupId) {
			return Promise.resolve(settingsGroupId);
		}

		const cacheKey = groupName.toLowerCase();
		this.idByNameCache[cacheKey] = settingsGroupId = await this.settingsGroupCollection.insert({
			name: groupName
		});

		return Promise.resolve(settingsGroupId);
	}

	async getSettingsGroupIdByName(groupName) {
		if (!isSettingsGroupNameValid(groupName)) {
			return Promise.resolve(null);
		}

		const cacheKey = groupName.toLowerCase();
		if (this.idByNameCache.hasOwnProperty(cacheKey)) {
			return Promise.resolve(this.idByNameCache[cacheKey]);
		}

		const settingsGroup = await this.settingsGroupCollection.findOne({
			"name": groupName
		});
		
		const id = settingsGroup ? settingsGroup.id : null;
		this.idByNameCache[cacheKey] = id;

		if (!id) {
			setTimeout(() => {
				if (!this.idByNameCache[cacheKey]) {
					delete this.idByNameCache[cacheKey];
				}
			}, CacheExpiry);
		}

		return Promise.resolve(id);
	}
};
