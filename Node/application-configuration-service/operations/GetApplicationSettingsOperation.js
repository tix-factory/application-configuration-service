export default class {
	constructor(settingEntityFactory, applicationNameProvider) {
		this.settingEntityFactory = settingEntityFactory;
		this.applicationNameProvider = applicationNameProvider;
	}

    get name() {
        return "GetApplicationSettings";
    }
 
    async execute(requestBody, request) {
		const applicationName = await this.applicationNameProvider.getApplicationName(request.apiKey);
		const settings = {};

		if (applicationName) {
			const settingsEntities = await this.settingEntityFactory.getSettings(applicationName);
			settingsEntities.forEach(setting => {
				settings[setting.Name] = setting.Value;
			});
		}

		return Promise.resolve(settings);
    }
};