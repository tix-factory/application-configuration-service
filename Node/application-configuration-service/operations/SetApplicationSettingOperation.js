export default class {
	constructor(settingEntityFactory) {
		this.settingEntityFactory = settingEntityFactory;
	}

    get name() {
        return "SetApplicationSetting";
    }
	
	get requestParameters() {
		return ["applicationName", "settingName", "settingValue"];
	}
 
    async execute(requestBody) {
		const updated = await this.settingEntityFactory.setSettingValue(requestBody.applicationName, requestBody.settingName, requestBody.settingValue);
		return Promise.resolve(updated ? "Changed" : "Unchanged");
    }
};