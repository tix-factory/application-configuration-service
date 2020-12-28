import { dirname } from "path";
import { fileURLToPath } from 'url';
import { HttpServer } from "@tix-factory/http-service";
import { MongoConnection } from "@tix-factory/mongodb";
import SettingsGroupEntityFactory from "./entities/settingsGroupEntityFactory.js";
import SettingEntityFactory from "./entities/settingEntityFactory.js";
import ApplicationNameProvider from "./implementation/applicationNameProvider.js";

import DeleteApplicationSettingOperation from "./operations/DeleteApplicationSettingOperation.js";
import GetApplicationSettingsOperation from "./operations/GetApplicationSettingsOperation.js";
import SetApplicationSettingOperation from "./operations/SetApplicationSettingOperation.js";
import SetApplicationSettingValueOperation from "./operations/SetApplicationSettingValueOperation.js";

const workingDirectory = dirname(fileURLToPath(import.meta.url));

const service = new HttpServer({
    name: "TixFactory.ApplicationConfiguration.Service",
    logName: "TFACS2.TixFactory.ApplicationConfiguration.Service"
});

const init = () => {
	console.log(`Starting ${service.options.name}...\n\tWorking directory: ${workingDirectory}\n\tNODE_ENV: ${process.env.NODE_ENV}\n\tPort: ${service.options.port}`);

	return new Promise(async (resolve, reject) => {
		try {
			const mongoConnection = new MongoConnection(process.env.MongoConnectionString);
			const settingsGroupCollection = await mongoConnection.getCollection("application-configuration-service", "settings-groups");
			const settingCollection = await mongoConnection.getCollection("application-configuration-service", "settings");

			const applicationNameProvider = new ApplicationNameProvider(service.httpClient);
			const settingsGroupEntityFactory = new SettingsGroupEntityFactory(settingsGroupCollection);
			const settingEntityFactory = new SettingEntityFactory(settingsGroupEntityFactory, settingCollection);

			await Promise.all([
				settingsGroupEntityFactory.setup(),
				settingEntityFactory.setup()
			]);
			
			service.operationRegistry.registerOperation(new DeleteApplicationSettingOperation(settingEntityFactory));
			service.operationRegistry.registerOperation(new GetApplicationSettingsOperation(settingEntityFactory, applicationNameProvider));
			service.operationRegistry.registerOperation(new SetApplicationSettingOperation(settingEntityFactory));
			service.operationRegistry.registerOperation(new SetApplicationSettingValueOperation(settingEntityFactory, applicationNameProvider));

			resolve();
		} catch (e) {
			reject(e);
		}
	});
};

init().then(() => {
	service.listen();
}).catch(err => {
	service.logger.error(err);
	console.error(err);
	process.exit(1);
});
