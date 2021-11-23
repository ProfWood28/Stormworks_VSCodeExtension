-- developed by nameouschangey (Gordon Mckendrick) for use with LifeBoat Modding framework
-- please see: https://github.com/nameouschangey/STORMWORKS for updates


-- combines multiple scripts into one by following the require tree.
-- resulting script can then be passed through luamin to minify it (or alternate tools)

require("LifeBoatAPI.Missions.Utils.LBTable")
require("LifeBoatAPI.Missions.Utils.LBString")
require("LifeBoatAPI.Tools.Utils.LBFilesystem")

---@class LBCombiner : LBBaseClass
---@field filesByRequire table<string,string> table of require names -> filecontents
---@field systemRequires string[] list of system libraries that are OK to import, but should be stripped
LBCombiner = {

    ---@param cls LBCombiner
    ---@return LBCombiner
    new = function(cls)
        local this = LBBaseClass.new(cls)
        this.filesByRequire = {}
        this.systemRequires = {"table", "math", "string"}
        return this;
    end;

    ---@param this LBCombiner
    ---@param rootDirectory LBFilepath sourcecode root folder, to load files from
    addRootFolder = function(this, rootDirectory)
        local filesByRequire = this:_getDataByRequire(rootDirectory)
        LBTable_addRange(this.filesByRequire, filesByRequire)
    end;

    ---@param this LBCombiner
    ---@param entryPointFile LBFilepath
    ---@param outputFile LBFilepath
    combineFile = function (this, entryPointFile, outputFile)   
        local outputFileHandle = LBFileSystem_openForWrite(outputFile)
        local data = LBFileSystem_readAllText(entryPointFile)
        data = this:combine(data)
        outputFileHandle:write(data)
        outputFileHandle:close()
    end;

    ---@param this LBCombiner
    ---@param data string
    combine = function (this, data)   
        local requiresSeen = {}

        local keepSearching = true
        while keepSearching do
            keepSearching = false
            local requires = data:gmatch("%s-require%(\"(..-)\"%)")
            for require in requires do
                local fullstring = "%s-require%(\""..require.."\"%)%s-"
                if(requiresSeen[require]) then
                    -- already seen this, so we just cut it from the file
                    data = data:gsub(fullstring, "")
                else
                    -- valid require to be replaced with the file contents
                    keepSearching = true
                    requiresSeen[require] = true

                    if(this.filesByRequire[require]) then
                        data = data:gsub(fullstring, LBString_escapeSub(this.filesByRequire[require]), 1) -- only first instance

                    elseif (LBTable_containsValue(this.systemRequires, require)) then
                        data = data:gsub(fullstring, "") -- remove system requires, without error, as long as they are allowed in the game
                    else
                        error("Require " .. require .. " was not found.")
                    end
                end
            end
        end
        return data
    end;

    ---@param this LBCombiner
    ---@param rootDirectory LBFilepath
    ---@param outputFile LBFilepath
    _getDataByRequire = function(this, rootDirectory, outputFile)
        local requiresToFilecontents = {}
        local files = LBFileSystem_findFilesRecursive(rootDirectory)

        for _, filename in ipairs(files) do
            local requireName = filename:linux():gsub(rootDirectory:linux() .. "/", "")
            requireName = requireName:gsub("/", ".")
            requireName = requireName:gsub(".lua", "")

            if(requireName ~= outputFile) then
                requiresToFilecontents[requireName] = LBFileSystem_readAllText(filename)
            end
        end

        return requiresToFilecontents
    end;
}
LBClass(LBCombiner)

