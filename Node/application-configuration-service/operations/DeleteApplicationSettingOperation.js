export default class {
	constructor(settingEntityFactory) {
		this.settingEntityFactory = settingEntityFactory;
	}
 
    get name() {
        return "DeleteApplicationSetting";
	}
	
	get requestParameters() {
		return ["applicationName", "settingName"];
	}
 
    async execute(requestBody) {
		const deleted = await this.settingEntityFactory.deleteSetting(requestBody.applicationName, requestBody.settingName);
		return Promise.resolve(deleted ? "AlreadyDeleted" : "Deleted");
    }
};