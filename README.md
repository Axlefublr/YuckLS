Highly experimental partially functional LSP implementation for the yuck configuration language. See https://www.github.com/elkowar/eww.
## Building.
Install dotnet-sdk-8.0 on your computer from your package manager.

# clone and compile
```
git clone https://www.github.com/eugenenoble2005/yuckls.git
cd yuckls/YuckLS
dotnet build --output dist
```

Note where you cloned the repo to and where the compiled binaries are (/dist). You can now set it up for an editor

# Neovim
```
-- put this in your init.lua
vim.api.nvim_create_autocmd({'BufEnter', 'BufWinEnter'}, {
		pattern = {"*.yuck"}, 
		callback = function(event)
				print(string.format("starting yuck;s for %s", vim.inspect(event)))
				vim.lsp.start {
						name = "YuckLs",
						cmd = {"dotnet" ,"/home/gitrepos/yuckls/YuckLS/dist/YuckLS.dll"}, --this must be where you cloned this repo to.
						root_dir = vim.fn.getcwd(),
				}
		end
})
```

# Helix
```
# put this in your languages.toml
[language-server]
yuckls = {command="dotnet" , args = ["/home/gitrepos/yuckls/YuckLS/dist/YuckLS.dll"]} #must be where you cloned this repo to

[[language]]
name="yuck"
scope="source.yuck"
injection-regex="yuck"
file-types=["yuck"]
language-servers = ["yuckls"]
```
