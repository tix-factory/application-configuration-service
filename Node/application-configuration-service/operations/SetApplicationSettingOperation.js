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
 
    execute(requestBody) {
		return this.settingEntityFactory.setSettingValue(requestBody.applicationName, requestBody.settingName, requestBody.settingValue);
    }
};