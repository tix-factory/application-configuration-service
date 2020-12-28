export default class {
	constructor(settingEntityFactory, applicationNameProvider) {
		this.settingEntityFactory = settingEntityFactory;
		this.applicationNameProvider = applicationNameProvider;
	}
 
    get name() {
        return "SetApplicationSettingValue";
    }
	
	get requestParameters() {
		return ["settingName", "settingValue"];
	}
 
    async execute(requestBody, request) {
		const applicationName = await this.applicationNameProvider.getApplicationName(request.apiKey);
		return this.settingEntityFactory.setSettingValue(applicationName, requestBody.settingName, requestBody.settingValue);
    }
};