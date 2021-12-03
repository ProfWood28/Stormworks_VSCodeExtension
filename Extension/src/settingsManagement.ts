import * as vscode from 'vscode';
import * as path from 'path';
import { Func } from 'mocha';
import { TextEncoder } from 'util';
import { settings } from 'cluster';
import * as utils from "./utils";



export function beginUpdateWorkspaceSettings(context: vscode.ExtensionContext) {
	var lifeboatConfig = vscode.workspace.getConfiguration("lifeboatapi.stormworks", utils.getCurrentWorkspaceFile());
	
    // setup library paths
    // copy global (user) and workspace paths, rather than overwriting
    var lifeboatLibraryPaths : string[] = lifeboatConfig.get("projectSpecificLibraryPaths") ?? [];
    var wslifeboatLibraryPaths : string[] = lifeboatConfig.get("workspacelibraryPaths") ?? [];
    var userlifeboatLibraryPaths : string[] = lifeboatConfig.get("globalLibraryPaths") ?? [];

    for (var path of wslifeboatLibraryPaths)
    {
        lifeboatLibraryPaths.push(path);
    }

    for(var path of userlifeboatLibraryPaths)
    {
        lifeboatLibraryPaths.push(path);
    }

	// add lifeboatAPI to the library path
	if(utils.isMicrocontrollerProject())
    {
		lifeboatLibraryPaths.push(context.extensionPath + "/assets/LifeBoatAPI/Microcontroller/");
        lifeboatLibraryPaths.push(context.extensionPath + "/assets/LifeBoatAPI/Tools/Simulator/");
	}
	else
	{
		lifeboatLibraryPaths.push(context.extensionPath + "/assets/LifeBoatAPI/Addons");
	}


    // setup ignore paths
	var lifeboatIgnorePaths : string[]= lifeboatConfig.get("ignorePaths") ?? [];

	// add standard ignores
	lifeboatIgnorePaths.push(".vscode");
	lifeboatIgnorePaths.push("/out/");


	var luaDiagnosticsConfig = vscode.workspace.getConfiguration("Lua.diagnostics");
	var luaRuntimeConfig = vscode.workspace.getConfiguration("Lua.runtime");
	var luaLibWorkspace = vscode.workspace.getConfiguration("Lua.workspace");
	var luaDebugConfig = vscode.workspace.getConfiguration("lua.debug.settings");

	return Promise.resolve()
	.then( () => {
		if(!utils.getCurrentWorkspaceFolder())
		{
			return Promise.reject("LifeBoatAPI: Can't update settings while no workspace is active");
		}

	}).then( () => {
		//Lua.diagnostics.disable
		var existing : string[] = luaLibWorkspace.get("disable") ?? [];
		if(existing.indexOf("lowercase-global") === -1)
		{
			existing.push("lowercase-global");
		}
		if(existing.indexOf("undefined-doc-name") === -1)
		{
			existing.push("undefined-doc-name");
		}
		return luaDiagnosticsConfig.update("disable", existing, vscode.ConfigurationTarget.Workspace);

	}).then( () => luaRuntimeConfig.update("version", "Lua 5.3", vscode.ConfigurationTarget.Workspace)
	
	).then( () => {
		//Lua.workspace.ignoreDir
		return luaLibWorkspace.update("ignoreDir", lifeboatIgnorePaths, vscode.ConfigurationTarget.Workspace);

	}).then( () => { 
		//Lua.workspace.library
		return luaLibWorkspace.update("library", lifeboatLibraryPaths, vscode.ConfigurationTarget.Workspace);

	}).then(() => {
		// lua.debug.cpath
		var existing : string[] = luaDebugConfig.get("cpath") ?? [];
		const defaultCPaths = [
			"abc"
		];
		for(const cPathElement of defaultCPaths)
		{
			if(existing.indexOf(cPathElement) === -1)
			{
				existing.push(cPathElement);
			}
		}
		return luaDebugConfig.update("cpath", existing, vscode.ConfigurationTarget.Workspace);

	}).then(() => {
		//lua.debug.path
		return luaDebugConfig.update("path", lifeboatLibraryPaths, vscode.ConfigurationTarget.Workspace);

	}).then( () => luaDebugConfig.update("luaVersion", "5.3", vscode.ConfigurationTarget.Workspace)

	).then( () => luaDebugConfig.update("luaArch", "x86", vscode.ConfigurationTarget.Workspace) );
}